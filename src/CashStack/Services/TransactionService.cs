using CashStack.Models;
using CashStack.Events;

namespace CashStack.Services;

public class TransactionService
{
    private readonly StorageService _storage;
    public event EventHandler<TransactionAddedEventArgs>? TransactionAdded;

    public TransactionService(StorageService storage)
    {
        _storage = storage;
    }

    public Transaction Add(TransactionType type, string description, decimal amount)
    {
        var transaction = new Transaction(
            Guid.NewGuid(),
            DateTimeOffset.Now,
            type,
            description,
            amount
        );

        _storage.Save(transaction);
        TransactionAdded?.Invoke(this, new TransactionAddedEventArgs(transaction));

        return transaction;
    }

    public bool Remove(Guid id) => _storage.Remove(id);

    public List<Transaction> GetTransactions(DateOnly from, DateOnly to, TransactionType? type = null)
    {
        var transactions = _storage.Load(from, to);
        if (type == null)
        {
            return transactions;
        }
        return transactions.Where(t => t.Type == type).ToList();
    }

    public TransactionSummary GetSummary(DateOnly from, DateOnly to)
    {
        var transactions = _storage.Load(from, to);
        decimal income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        decimal expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        return new TransactionSummary(income, expenses);
    }
}