using System.Data.Common;
using CashStack.Data;
using CashStack.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Repositories;

// Shared base for the EF Core repositories: holds the DbContext and translates
// database errors into our own exceptions, so services never see EF or SQLite types
public abstract class EfRepositoryBase
{
    // SQLITE_CONSTRAINT: unique index or foreign key violated
    private const int SqliteConstraintError = 19;

    protected CashStackDbContext Context { get; }

    protected EfRepositoryBase(CashStackDbContext context)
    {
        Context = context;
    }

    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (IsConstraintViolation(ex))
        {
            // Normally the services check these rules first; this only happens in a race
            // (e.g. two requests create the same category at the same time)
            throw new ConflictException("The change conflicts with existing data.", ex);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            throw new StorageException("Could not access the database", ex);
        }
    }

    protected Task ExecuteAsync(Func<Task> action) =>
        ExecuteAsync(async () =>
        {
            await action();
            return true;
        });

    // SaveChanges wraps the SQLite error in a DbUpdateException, ExecuteDelete throws it directly
    private static bool IsConstraintViolation(Exception ex) =>
        (ex as SqliteException ?? ex.InnerException as SqliteException)?.SqliteErrorCode == SqliteConstraintError;
}
