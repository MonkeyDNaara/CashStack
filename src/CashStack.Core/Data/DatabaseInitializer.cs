using System.Data.Common;
using CashStack.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Data;

// Brings the database to the latest migration at startup, so a fresh clone (or a fresh hosted database)
// works without manual steps
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(CashStackDbContext context, CancellationToken ct = default)
    {
        try
        {
            context.PrepareStorage();

            // Creates the database if needed and applies all pending migrations of this provider (incl. the category seed)
            await context.Database.MigrateAsync(ct);
        }
        catch (Exception ex) when (ex is DbException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            // Same exception type as the JSON storage: the existing startup handling (exit code 1) applies.
            // InvalidOperationException: e.g. Npgsql can't reach the server (no secrets in the message)
            throw new StorageException("Could not create or migrate the database", ex);
        }
    }
}
