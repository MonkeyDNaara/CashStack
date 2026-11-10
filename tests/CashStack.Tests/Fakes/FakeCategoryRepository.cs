using CashStack.Models;
using CashStack.Repositories;

namespace CashStack.Tests.Fakes;

// In-memory ICategoryRepository, starts with the same starter set as the real storages
public class FakeCategoryRepository : ICategoryRepository
{
    public List<Category> Items { get; }

    public FakeCategoryRepository(bool withStarterSet = true)
    {
        Items = withStarterSet ? DefaultCategories.CreateStarterSet().ToList() : [];
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(TransactionType? type = null, CancellationToken ct = default)
    {
        IReadOnlyList<Category> result = Items
            .Where(c => type is null || c.Type == type)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Items.Find(c => c.Id == id));

    public Task AddAsync(Category category, CancellationToken ct = default)
    {
        Items.Add(category);
        return Task.CompletedTask;
    }

    // Objects are shared by reference, so the renamed instance is already "saved"
    public Task UpdateAsync(Category category, CancellationToken ct = default) => Task.CompletedTask;

    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Items.RemoveAll(c => c.Id == id) > 0);
}
