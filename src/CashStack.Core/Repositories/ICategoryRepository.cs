using CashStack.Models;

namespace CashStack.Repositories;

public interface ICategoryRepository
{
    // Ordered by type, then name
    Task<IReadOnlyList<Category>> GetAllAsync(TransactionType? type = null, CancellationToken ct = default);

    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Category category, CancellationToken ct = default);

    // Saves the changed name of an existing category
    Task UpdateAsync(Category category, CancellationToken ct = default);

    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
