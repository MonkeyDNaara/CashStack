using CashStack.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ => new StorageService(builder.Configuration["Storage:DataDirectory"] ?? "data"));
builder.Services.AddSingleton<TransactionService>();
builder.Services.AddSingleton(_ => new LoggerService(builder.Configuration["Storage:LogDirectory"] ?? "logs"));

var app = builder.Build();

app.Run();