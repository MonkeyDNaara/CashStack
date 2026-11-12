using CashStack.Exceptions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CashStack.Configuration;

// Storage settings read once from configuration, shared by API and Console (same keys in both appsettings.json)
public sealed class StorageSettings
{
    public const string DefaultDataDirectory = "data";
    public const string DefaultConnectionString = "Data Source=data/cashstack.db";

    public StorageProvider Provider { get; }
    public string DataDirectory { get; }
    public string ConnectionString { get; }

    // SQLite and Postgres go through EF Core (DbContext + migrations), JSON uses files
    public bool UsesDatabase => Provider is StorageProvider.Sqlite or StorageProvider.Postgres;

    public StorageSettings(StorageProvider provider, string dataDirectory, string connectionString)
    {
        Provider = provider;
        DataDirectory = dataDirectory;
        ConnectionString = connectionString;
    }

    public static StorageSettings FromConfiguration(IConfiguration configuration)
    {
        string providerValue = configuration["Storage:Provider"] ?? nameof(StorageProvider.Sqlite);

        // IsDefined: TryParse also accepts numbers like "7"
        if (!Enum.TryParse(providerValue, ignoreCase: true, out StorageProvider provider) || !Enum.IsDefined(provider))
        {
            throw new ConfigurationException(
                $"Unknown Storage:Provider '{providerValue}'. Use '{StorageProvider.Sqlite}', '{StorageProvider.Postgres}' or '{StorageProvider.Json}'.");
        }

        string connectionString = configuration.GetConnectionString("CashStack") ?? DefaultConnectionString;
        if (provider == StorageProvider.Postgres)
        {
            EnsurePostgresConnectionString(connectionString);
        }

        return new StorageSettings(
            provider,
            configuration["Storage:DataDirectory"] ?? DefaultDataDirectory,
            connectionString);
    }

    // Fails fast at startup instead of on the first request. The messages never contain
    // the connection string itself: it holds the database password.
    private static void EnsurePostgresConnectionString(string connectionString)
    {
        NpgsqlConnectionStringBuilder builder;
        try
        {
            builder = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            throw new ConfigurationException(
                "ConnectionStrings:CashStack is not a valid Npgsql connection string. " +
                "Use the key-value format Host=...;Database=...;Username=...;Password=...;SSL Mode=Require (not a postgresql:// URI).");
        }

        if (string.IsNullOrWhiteSpace(builder.Host))
        {
            throw new ConfigurationException("ConnectionStrings:CashStack needs a Host for Storage:Provider 'Postgres'.");
        }
    }
}
