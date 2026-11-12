using CashStack.Configuration;
using CashStack.Data;
using Xunit;

namespace CashStack.Tests.Data;

public class CashStackDbContextFactoryTests
{
    [Theory]
    [InlineData(StorageProvider.Sqlite, typeof(SqliteCashStackDbContext))]
    [InlineData(StorageProvider.Postgres, typeof(PostgresCashStackDbContext))]
    public void Create_ReturnsTheSubclassOfTheProvider(StorageProvider provider, Type expected)
    {
        // Creating a context never opens a connection, so no server is needed
        using CashStackDbContext context = CashStackDbContextFactory.Create(
            new StorageSettings(provider, "data", "Host=localhost;Database=cashstack"));

        Assert.IsType(expected, context);
    }

    [Fact]
    public void Create_ForJson_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CashStackDbContextFactory.Create(new StorageSettings(StorageProvider.Json, "data", "")));
    }
}
