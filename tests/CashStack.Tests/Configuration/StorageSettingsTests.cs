using CashStack.Configuration;
using CashStack.Exceptions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CashStack.Tests.Configuration;

public class StorageSettingsTests
{
    [Theory]
    [InlineData("Sqlite", StorageProvider.Sqlite, true)]
    [InlineData("postgres", StorageProvider.Postgres, true)]
    [InlineData("Json", StorageProvider.Json, false)]
    public void Provider_IsParsedIgnoringCase(string value, StorageProvider expected, bool usesDatabase)
    {
        StorageSettings settings = FromValues(("Storage:Provider", value), ("ConnectionStrings:CashStack", "Host=db;Database=cashstack"));

        Assert.Equal(expected, settings.Provider);
        Assert.Equal(usesDatabase, settings.UsesDatabase);
    }

    [Fact]
    public void Defaults_AreSqliteWithTheLocalFile()
    {
        StorageSettings settings = FromValues();

        Assert.Equal(StorageProvider.Sqlite, settings.Provider);
        Assert.Equal(StorageSettings.DefaultConnectionString, settings.ConnectionString);
    }

    [Fact]
    public void UnknownProvider_IsRejected()
    {
        var ex = Assert.Throws<ConfigurationException>(() => FromValues(("Storage:Provider", "Oracle")));

        Assert.Contains("Postgres", ex.Message);
    }

    [Fact]
    public void Postgres_WithTheSqliteDefault_IsRejectedWithAHint()
    {
        // Forgot to set ConnectionStrings__CashStack on the server → the SQLite default from appsettings.json
        var ex = Assert.Throws<ConfigurationException>(() => FromValues(("Storage:Provider", "Postgres")));

        Assert.Contains("key-value format", ex.Message);
    }

    [Fact]
    public void Postgres_WithAUri_IsRejected_WithoutLeakingThePassword()
    {
        var ex = Assert.Throws<ConfigurationException>(() => FromValues(
            ("Storage:Provider", "Postgres"),
            ("ConnectionStrings:CashStack", "postgresql://user:s3cret@db.example/cashstack")));

        Assert.DoesNotContain("s3cret", ex.Message);
        Assert.Contains("URI", ex.Message);
    }

    [Fact]
    public void Postgres_WithoutHost_IsRejected()
    {
        var ex = Assert.Throws<ConfigurationException>(() => FromValues(
            ("Storage:Provider", "Postgres"),
            ("ConnectionStrings:CashStack", "Database=cashstack;Username=app")));

        Assert.Contains("Host", ex.Message);
    }

    private static StorageSettings FromValues(params (string Key, string Value)[] values) =>
        StorageSettings.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build());
}
