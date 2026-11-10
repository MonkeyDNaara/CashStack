using CashStack.Api.ErrorHandling;
using CashStack.Api.Extensions;
using CashStack.Api.Middleware;
using CashStack.Configuration;
using CashStack.Data;
using CashStack.Exceptions;
using CashStack.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// The audit log is optional: created once here, a failure only disables it (warning after Build)
LoggerService? auditLog = null;
Exception? auditLogError = null;
try
{
    auditLog = new LoggerService(builder.Configuration["Storage:LogDirectory"] ?? "logs");
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    auditLogError = ex;
}

builder.Services.AddCashStackStorage(builder.Configuration, auditLog);
// Detect captive dependencies (singleton → scoped) in every environment, not only in Development
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
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
try
{
    using IServiceScope scope = app.Services.CreateScope();
    StorageSettings settings = scope.ServiceProvider.GetRequiredService<StorageSettings>();
    if (settings.Provider == StorageProvider.Sqlite)
    {
        await DatabaseInitializer.InitializeAsync(scope.ServiceProvider.GetRequiredService<CashStackDbContext>());
    }

    // Resolving once creates the repositories (JSON: data folder) before the first request
    scope.ServiceProvider.GetRequiredService<TransactionService>();
    app.Logger.LogInformation("Storage provider: {Provider}", settings.Provider);
}
catch (Exception ex) when (ex is StorageException or ConfigurationException)
{
    app.Logger.LogCritical(ex, "Storage could not be initialized. Shutting down.");
    return 1;
}

if (auditLogError is not null)
{
    app.Logger.LogWarning(auditLogError, "Transaction log could not be initialized. Continuing without audit log.");
}

app.UseRequestTiming();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

app.Run();
return 0;
