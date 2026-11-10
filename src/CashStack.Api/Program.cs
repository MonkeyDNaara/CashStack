using CashStack.Api.ErrorHandling;
using CashStack.Api.Middleware;
using CashStack.Data;
using CashStack.Exceptions;
using CashStack.Repositories;
using CashStack.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

string dataDirectory = builder.Configuration["Storage:DataDirectory"] ?? "data";
builder.Services.AddSingleton<ITransactionRepository>(_ => new JsonTransactionRepository(dataDirectory));
builder.Services.AddSingleton<ICategoryRepository>(_ => new JsonCategoryRepository(dataDirectory));
builder.Services.AddSingleton<TransactionService>();
builder.Services.AddSingleton<CategoryService>();
// Registered now for the startup migration; repositories use it from #81/#82 on
builder.Services.AddDbContext<CashStackDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CashStack") ?? "Data Source=data/cashstack.db"));
builder.Services.AddSingleton(_ => new LoggerService(builder.Configuration["Storage:LogDirectory"] ?? "logs"));
builder.Services
    .AddControllers(options => options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        options.AllowInputFormatterExceptionMessages = false;
    });
builder.Services.AddRequestTiming();
builder.Services.AddProblemDetails(options =>
{
    // Applies to every ProblemDetails response (exceptions, validation, status codes)
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions.TryAdd(
            "traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Storage is required: without it the API is useless → fail fast with a clear log entry
TransactionService transactionService;
try
{
    // DbContext is scoped: create a scope just for the migration
    using (IServiceScope scope = app.Services.CreateScope())
    {
        await DatabaseInitializer.InitializeAsync(scope.ServiceProvider.GetRequiredService<CashStackDbContext>());
    }
    transactionService = app.Services.GetRequiredService<TransactionService>();
}
catch (StorageException ex)
{
    app.Logger.LogCritical(ex, "Storage could not be initialized. Shutting down.");
    return 1;
}

// The audit log is optional: warn and keep running without it
try
{
    var logger = app.Services.GetRequiredService<LoggerService>();
    transactionService.TransactionAdded += logger.OnTransactionAdded;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    app.Logger.LogWarning(ex, "Transaction log could not be initialized. Continuing without audit log.");
}

app.UseRequestTiming();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

app.Run();
return 0;
