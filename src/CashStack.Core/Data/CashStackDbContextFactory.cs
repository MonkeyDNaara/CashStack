using CashStack.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Data;

// Creates the DbContext subclass for the configured provider (used by the console;
// the API registers the same subclasses with dependency injection)
public static class CashStackDbContextFactory
{
    public static CashStackDbContext Create(StorageSettings settings) => settings.Provider switch
    {
        StorageProvider.Sqlite => new SqliteCashStackDbContext(
            new DbContextOptionsBuilder<SqliteCashStackDbContext>().UseSqlite(settings.ConnectionString).Options),
        StorageProvider.Postgres => new PostgresCashStackDbContext(
            new DbContextOptionsBuilder<PostgresCashStackDbContext>().UseNpgsql(settings.ConnectionString).Options),
        _ => throw new InvalidOperationException($"Storage provider '{settings.Provider}' does not use a database.")
    };
}
