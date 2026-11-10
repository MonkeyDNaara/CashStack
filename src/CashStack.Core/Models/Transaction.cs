namespace CashStack.Models;

public class Transaction
{
    public Guid Id {get;}
    public DateTimeOffset Timestamp {get;}
    public TransactionType Type {get;}
    public Guid CategoryId {get;}
    public string Description {get;}
    public decimal Amount {get;}

    public Transaction(Guid id, DateTimeOffset timestamp, TransactionType type, Guid categoryId, string description, decimal amount)
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
        // Guid.Empty is allowed: old day files have no category, the repository maps them to "Other"
        CategoryId = categoryId;
        Description = description;
        Amount = amount;
    }
}