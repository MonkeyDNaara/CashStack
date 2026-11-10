using CashStack.Exceptions;
using Microsoft.Extensions.Configuration;

namespace CashStack.Configuration;

// Storage settings read once from configuration, shared by API and Console (same keys in both appsettings.json)
public sealed class StorageSettings
{
    public const string DefaultDataDirectory = "data";
    public const string DefaultConnectionString = "Data Source=data/cashstack.db";

    public StorageProvider Provider { get; }
    public string DataDirectory { get; }
    public string ConnectionString { get; }

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
                $"Unknown Storage:Provider '{providerValue}'. Use '{StorageProvider.Sqlite}' or '{StorageProvider.Json}'.");
        }

        return new StorageSettings(
            provider,
            configuration["Storage:DataDirectory"] ?? DefaultDataDirectory,
            configuration.GetConnectionString("CashStack") ?? DefaultConnectionString);
    }
}
