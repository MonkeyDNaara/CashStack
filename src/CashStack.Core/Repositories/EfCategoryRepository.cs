using CashStack.Data;
using CashStack.Models;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Repositories;

public class EfCategoryRepository : EfRepositoryBase, ICategoryRepository
{
    public EfCategoryRepository(CashStackDbContext context) : base(context) { }

    // Read-only list → no change tracking
    public Task<IReadOnlyList<Category>> GetAllAsync(TransactionType? type = null, CancellationToken ct = default) =>
        ExecuteAsync<IReadOnlyList<Category>>(async () =>
        {
            IQueryable<Category> query = Context.Categories.AsNoTracking();
            if (type is not null)
            {
                query = query.Where(c => c.Type == type);
            }

            // Type is stored as text ("Expense" < "Income"), so sort by enum order explicitly,
            // like the JSON repository. Name uses the NOCASE collation of the column.
            return await query
                .OrderBy(c => c.Type == TransactionType.Income ? 0 : 1)
                .ThenBy(c => c.Name)
                .ToListAsync(ct);
        });

    // Tracked: the caller may rename the category and save it with UpdateAsync
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        ExecuteAsync(async () => await Context.Categories.FindAsync([id], ct));

    public Task AddAsync(Category category, CancellationToken ct = default) =>
        ExecuteAsync(async () =>
        {
            Context.Categories.Add(category);
            await Context.SaveChangesAsync(ct);
        });

    // Update works for tracked and detached objects
    public Task UpdateAsync(Category category, CancellationToken ct = default) =>
        ExecuteAsync(async () =>
        {
            Context.Categories.Update(category);
            await Context.SaveChangesAsync(ct);
        });

    // One DELETE statement; a category in use is refused by the foreign key (→ ConflictException)
    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default) =>
        ExecuteAsync(async () => await Context.Categories
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync(ct) > 0);
}
