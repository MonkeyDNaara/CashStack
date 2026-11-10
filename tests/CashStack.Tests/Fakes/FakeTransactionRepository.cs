using CashStack.Models;
using CashStack.Repositories;

namespace CashStack.Tests.Fakes;

// In-memory ITransactionRepository: tests the services without disk or database
public class FakeTransactionRepository : ITransactionRepository
{
    public List<Transaction> Items { get; } = [];

    public Task AddAsync(Transaction transaction, CancellationToken ct = default)
    {
        Items.Add(transaction);
        return Task.CompletedTask;
    }

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Items.Find(t => t.Id == id));

    public Task<IReadOnlyList<Transaction>> GetRangeAsync(
        DateOnly from, DateOnly to, TransactionType? type = null, Guid? categoryId = null, CancellationToken ct = default)
    {
        IReadOnlyList<Transaction> result = Items
            .Where(t => DateOnly.FromDateTime(t.Timestamp.DateTime) >= from && DateOnly.FromDateTime(t.Timestamp.DateTime) <= to)
            .Where(t => type is null || t.Type == type)
            .Where(t => categoryId is null || t.CategoryId == categoryId)
            .OrderBy(t => t.Timestamp)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Items.RemoveAll(t => t.Id == id) > 0);

    public Task<bool> AnyWithCategoryAsync(Guid categoryId, CancellationToken ct = default) =>
        Task.FromResult(Items.Exists(t => t.CategoryId == categoryId));
}
