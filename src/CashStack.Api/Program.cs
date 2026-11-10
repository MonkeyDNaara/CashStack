using CashStack.Api.ErrorHandling;
using CashStack.Api.Middleware;
using CashStack.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ => new StorageService(builder.Configuration["Storage:DataDirectory"] ?? "data"));
builder.Services.AddSingleton<TransactionService>();
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

var transactionService = app.Services.GetRequiredService<TransactionService>();
var logger = app.Services.GetRequiredService<LoggerService>();
transactionService.TransactionAdded += logger.OnTransactionAdded;

app.UseRequestTiming();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

app.Run();
