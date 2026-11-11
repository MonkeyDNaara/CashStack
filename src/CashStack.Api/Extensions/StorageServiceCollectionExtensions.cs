using CashStack.Configuration;
using CashStack.Data;
using CashStack.Repositories;
using CashStack.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CashStack.Api.Extensions;

public static class StorageServiceCollectionExtensions
{
    // Registers storage and services for the provider from Storage:Provider.
    // Everything is scoped (one instance per request), because the DbContext is scoped:
    // a singleton holding a scoped DbContext would be a captive dependency.
    public static IServiceCollection AddCashStackStorage(
        this IServiceCollection services, IConfiguration configuration, LoggerService? auditLog)
    {
        // Parsed on first use, so an invalid provider is reported by the startup check in Program.cs
        services.AddSingleton(_ => StorageSettings.FromConfiguration(configuration));

        services.AddDbContext<CashStackDbContext>((sp, options) =>
            options.UseSqlite(sp.GetRequiredService<StorageSettings>().ConnectionString));

        services.AddScoped<ITransactionRepository>(sp =>
        {
            StorageSettings settings = sp.GetRequiredService<StorageSettings>();
            return settings.Provider switch
            {
                StorageProvider.Sqlite => new EfTransactionRepository(sp.GetRequiredService<CashStackDbContext>()),
                _ => new JsonTransactionRepository(settings.DataDirectory)
            };
        });

        services.AddScoped<ICategoryRepository>(sp =>
        {
            StorageSettings settings = sp.GetRequiredService<StorageSettings>();
            return settings.Provider switch
            {
                StorageProvider.Sqlite => new EfCategoryRepository(sp.GetRequiredService<CashStackDbContext>()),
                _ => new JsonCategoryRepository(settings.DataDirectory)
            };
        });

        // One clock for the services and the validation attributes; tests can replace it
        services.TryAddSingleton(TimeProvider.System);

        // Factory registration: every new TransactionService instance gets the audit log subscription
        services.AddScoped(sp =>
        {
            var service = new TransactionService(
                sp.GetRequiredService<ITransactionRepository>(),
                sp.GetRequiredService<ICategoryRepository>(),
                sp.GetRequiredService<TimeProvider>());

            if (auditLog is not null)
            {
                service.TransactionAdded += auditLog.OnTransactionAdded;
            }
            return service;
        });

        services.AddScoped<CategoryService>();

        return services;
    }
}
