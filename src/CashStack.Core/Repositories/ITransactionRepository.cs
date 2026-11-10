using CashStack.Models;

namespace CashStack.Repositories;

// Storage abstraction: TransactionService only knows this interface,
// not whether the data lives in JSON files or in a database
public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct = default);

    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Inclusive range by the local date of the timestamp, ordered by timestamp
    Task<IReadOnlyList<Transaction>> GetRangeAsync(
        DateOnly from, DateOnly to, TransactionType? type = null, CancellationToken ct = default);

    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
