using System.Data.Common;
using CashStack.Data;
using CashStack.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Repositories;

// Shared base for the EF Core repositories: holds the DbContext and translates
// database errors into our own exceptions, so services never see EF or provider types
public abstract class EfRepositoryBase
{
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
        // The DbContext subclass knows the error codes of its provider (SQLite, Postgres)
        catch (Exception ex) when (Context.IsConstraintViolation(ex))
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
}
