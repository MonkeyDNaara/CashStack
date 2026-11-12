using CashStack.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Tests.Support;

// SQLite in-memory database: lives as long as this connection is open, no file on disk.
// One instance (= one database) per test; the real migrations create the schema and the seed.
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly DbContextOptions<SqliteCashStackDbContext> _options;

    public SqliteTestDatabase()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<SqliteCashStackDbContext>().UseSqlite(_connection).Options;

        using CashStackDbContext context = CreateContext();
        context.Database.Migrate();
    }

    // A new context per "request", like the scoped DbContext in the API
    public CashStackDbContext CreateContext() => new SqliteCashStackDbContext(_options);

    public void Dispose() => _connection.Dispose();
}
