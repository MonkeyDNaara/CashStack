using CashStack.Data;
using CashStack.Models;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Repositories;

public class EfTransactionRepository : EfRepositoryBase, ITransactionRepository
{
    public EfTransactionRepository(CashStackDbContext context) : base(context) { }

    public Task AddAsync(Transaction transaction, CancellationToken ct = default) =>
        ExecuteAsync(async () =>
        {
            Context.Transactions.Add(transaction);
            await Context.SaveChangesAsync(ct);
        });

    // Transactions are immutable, so reads never need change tracking
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        ExecuteAsync(() => Context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct));

    public Task<IReadOnlyList<Transaction>> GetRangeAsync(
        DateOnly from, DateOnly to, TransactionType? type = null, CancellationToken ct = default) =>
        ExecuteAsync<IReadOnlyList<Transaction>>(async () =>
        {
            // "Day" = local date of the timestamp (shadow property), same rule as one JSON file per day
            IQueryable<Transaction> query = Context.Transactions
                .AsNoTracking()
                .Where(t => EF.Property<DateOnly>(t, TransactionConfiguration.DayProperty) >= from
                         && EF.Property<DateOnly>(t, TransactionConfiguration.DayProperty) <= to);

            if (type is not null)
            {
                query = query.Where(t => t.Type == type);
            }

            return await query.OrderBy(t => t.Timestamp).ToListAsync(ct);
        });

    // One DELETE statement, no need to load the transaction first
    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default) =>
        ExecuteAsync(async () => await Context.Transactions
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync(ct) > 0);

    public Task<bool> AnyWithCategoryAsync(Guid categoryId, CancellationToken ct = default) =>
        ExecuteAsync(() => Context.Transactions.AnyAsync(t => t.CategoryId == categoryId, ct));
}
