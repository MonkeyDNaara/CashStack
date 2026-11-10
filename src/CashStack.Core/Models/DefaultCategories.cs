namespace CashStack.Models;

// Fixed ids for the starter categories.
// JSON fallback, EF Core seed and JSON import must use the same values.
public static class DefaultCategories
{
    public static readonly Guid OtherIncomeId = new("6f0b8a52-1c3e-4c1a-9a51-000000000001");
    public static readonly Guid OtherExpenseId = new("6f0b8a52-1c3e-4c1a-9a51-000000000002");

    public static Guid GetOtherId(TransactionType type) =>
        type == TransactionType.Income ? OtherIncomeId : OtherExpenseId;

    // The two "Other" categories are the fallback for transactions without category
    public static bool IsProtected(Guid id) => id == OtherIncomeId || id == OtherExpenseId;

    // New instances on every call: Category is mutable (Rename)
    public static IReadOnlyList<Category> CreateStarterSet() =>
    [
        new(new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000101"), "Salary", TransactionType.Income),
        new(new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000102"), "Freelance", TransactionType.Income),
        new(OtherIncomeId, "Other", TransactionType.Income),
        new(new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000201"), "Groceries", TransactionType.Expense),
        new(new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000202"), "Rent", TransactionType.Expense),
        new(new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000203"), "Transport", TransactionType.Expense),
        new(new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000204"), "Leisure", TransactionType.Expense),
        new(new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000205"), "Health", TransactionType.Expense),
        new(OtherExpenseId, "Other", TransactionType.Expense)
    ];
}
