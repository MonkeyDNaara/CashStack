using System.Data.Common;
using CashStack.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Data;

// Brings the database to the latest migration at startup, so a fresh clone works without manual steps
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(CashStackDbContext context, CancellationToken ct = default)
    {
        try
        {
            // SQLite creates the file, but not a missing folder like "data/"
            EnsureDirectoryExists(context.Database.GetConnectionString());

            // Creates the database if needed and applies all pending migrations (incl. the category seed)
            await context.Database.MigrateAsync(ct);
        }
        catch (Exception ex) when (ex is DbException or IOException or UnauthorizedAccessException)
        {
            // Same exception type as the JSON storage: the existing startup handling (exit code 1) applies
            throw new StorageException("Could not create or migrate the database", ex);
        }
    }

    private static void EnsureDirectoryExists(string? connectionString)
    {
        string dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        string? directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
