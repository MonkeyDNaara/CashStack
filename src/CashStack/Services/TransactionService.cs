using CashStack.Models;

namespace CashStack.Services;

public class TransactionService
{
    private readonly StorageService _storage;

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

        return transaction;
    }
}