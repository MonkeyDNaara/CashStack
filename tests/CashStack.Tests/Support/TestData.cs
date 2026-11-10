using CashStack.Models;

namespace CashStack.Tests.Support;

// Fixed ids and factory methods, so tests read like the business case
public static class TestData
{
    public static readonly Guid Salary = new("6f0b8a52-1c3e-4c1a-9a51-000000000101");
    public static readonly Guid Groceries = new("6f0b8a52-1c3e-4c1a-9a51-000000000201");
    public static readonly Guid Rent = new("6f0b8a52-1c3e-4c1a-9a51-000000000202");
    public static readonly Guid Health = new("6f0b8a52-1c3e-4c1a-9a51-000000000205");

    public static readonly TimeSpan Winter = TimeSpan.FromHours(1);
    public static readonly TimeSpan Summer = TimeSpan.FromHours(2);

    public static Transaction Expense(decimal amount, DateTimeOffset timestamp, Guid? categoryId = null, string description = "expense") =>
        new(Guid.NewGuid(), timestamp, TransactionType.Expense, categoryId ?? Groceries, description, amount);

    public static Transaction Income(decimal amount, DateTimeOffset timestamp, Guid? categoryId = null, string description = "income") =>
        new(Guid.NewGuid(), timestamp, TransactionType.Income, categoryId ?? Salary, description, amount);

    public static DateTimeOffset At(int month, int day, int hour, int minute = 0, TimeSpan? offset = null) =>
        new(2026, month, day, hour, minute, 0, offset ?? Winter);
}
