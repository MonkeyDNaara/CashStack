using CashStack.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CashStack.Tests.Support;

// A fresh Postgres database per test (random name), created by the real migrations and dropped afterwards.
// The server comes from CASHSTACK_TEST_POSTGRES, see PostgresFactAttribute.
public sealed class PostgresTestDatabase : IDisposable
{
    private readonly DbContextOptions<PostgresCashStackDbContext> _options;

    public PostgresTestDatabase()
    {
        var builder = new NpgsqlConnectionStringBuilder(PostgresFactAttribute.ConnectionString)
        {
            Database = $"cashstack_test_{Guid.NewGuid():N}"
        };
        ConnectionString = builder.ConnectionString;
        _options = new DbContextOptionsBuilder<PostgresCashStackDbContext>().UseNpgsql(ConnectionString).Options;

        // MigrateAsync creates the database, the schema and the category seed
        using CashStackDbContext context = CreateContext();
        context.Database.Migrate();
    }

    public string ConnectionString { get; }

    // A new context per "request", like the scoped DbContext in the API
    public CashStackDbContext CreateContext() => new PostgresCashStackDbContext(_options);

    public void Dispose()
    {
        using CashStackDbContext context = CreateContext();
        context.Database.EnsureDeleted();
    }
}
