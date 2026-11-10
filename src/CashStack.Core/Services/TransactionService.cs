using CashStack.Events;
using CashStack.Models;
using CashStack.Repositories;

namespace CashStack.Services;

public class TransactionService
{
    private readonly ITransactionRepository _repository;
    public event EventHandler<TransactionAddedEventArgs>? TransactionAdded;

    // Depends on the interface only: the caller decides which storage is used
    public TransactionService(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Transaction> AddAsync(
        TransactionType type, string description, decimal amount, CancellationToken ct = default)
    {
        var transaction = new Transaction(
            Guid.NewGuid(),
            DateTimeOffset.Now,
            type,
            description,
            amount
        );

        await _repository.AddAsync(transaction, ct);
        TransactionAdded?.Invoke(this, new TransactionAddedEventArgs(transaction));

        return transaction;
    }

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetByIdAsync(id, ct);

    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default) =>
        _repository.RemoveAsync(id, ct);

    public Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        DateOnly from, DateOnly to, TransactionType? type = null, CancellationToken ct = default)
    {
        EnsureValidRange(from, to);
        return _repository.GetRangeAsync(from, to, type, ct);
    }

    public async Task<TransactionSummary> GetSummaryAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        IReadOnlyList<Transaction> transactions = await GetTransactionsAsync(from, to, ct: ct);
        decimal income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        decimal expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        return new TransactionSummary(income, expenses);
    }

    // Business rule, checked once here instead of in every repository implementation
    private static void EnsureValidRange(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentException("From-date is after to-date", nameof(from));
        }
    }
}
