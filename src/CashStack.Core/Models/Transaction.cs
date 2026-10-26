namespace CashStack.Models;

public class Transaction
{
    public Guid Id {get;}
    public DateTimeOffset Timestamp {get;}
    public TransactionType Type {get;}
    public string Description {get;}
    public decimal Amount {get;}

    public Transaction(Guid id, DateTimeOffset timestamp, TransactionType type, string description, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description must not be empty.", nameof(description));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");
        }

        Id = id;
        Timestamp = timestamp;
        Type = type;
        Description = description;
        Amount = amount;
    }
}