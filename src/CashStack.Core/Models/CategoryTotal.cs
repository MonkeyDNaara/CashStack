namespace CashStack.Models;

// Sum of all transactions of one category in a period (one bar/slice in a chart)
public class CategoryTotal
{
    public Guid CategoryId { get; }
    public string Name { get; }
    public TransactionType Type { get; }
    public decimal Total { get; }

    public CategoryTotal(Guid categoryId, string name, TransactionType type, decimal total)
    {
        CategoryId = categoryId;
        Name = name;
        Type = type;
        Total = total;
    }
}
