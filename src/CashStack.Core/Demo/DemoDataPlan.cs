using System.Globalization;
using CashStack.Models;

namespace CashStack.Demo;

// About 40 realistic transactions over the current and the two previous months, relative to "today".
// Same data as frontend/scripts/demoPlan.ts (local seed script) — keep both in sync.
public static class DemoDataPlan
{
    // Fixed amounts, so every run (and every screenshot) looks the same.
    // Index 0 = two months ago, 1 = last month, 2 = this month.
    private static readonly decimal[][] Groceries =
    [
        [62.30m, 48.90m, 71.15m, 55.40m, 39.80m],
        [58.75m, 66.20m, 44.10m, 73.50m, 51.30m],
        [62.30m, 47.60m, 68.90m, 52.45m, 41.20m]
    ];

    private static readonly (int Day, string Description, decimal Amount)[][] Leisure =
    [
        [(8, "Cinema", 24m), (21, "Restaurant", 48.50m)],
        [(11, "Concert", 65.90m), (25, "Bowling", 32m)],
        [(9, "Cinema", 24m), (19, "Climbing gym", 18m)]
    ];

    private static readonly decimal[] Pharmacy = [18.50m, 9.95m, 23.40m];

    // Categories the plan needs that are not part of the starter set
    public static IReadOnlyList<(string Name, TransactionType Type)> ExtraCategories { get; } =
        [("Subscriptions", TransactionType.Expense)];

    // Days after today (current month) or beyond the month's length are skipped,
    // because the service rejects future dates
    public static IReadOnlyList<PlannedTransaction> Build(DateOnly today)
    {
        var plan = new List<PlannedTransaction>();

        for (int offset = 0; offset < 3; offset++)
        {
            DateOnly month = new DateOnly(today.Year, today.Month, 1).AddMonths(offset - 2);
            int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            string monthName = month.ToString("MMMM", CultureInfo.InvariantCulture);

            void Add(int day, TransactionType type, string category, string description, decimal amount)
            {
                if (day > daysInMonth)
                {
                    return;
                }
                var date = new DateOnly(month.Year, month.Month, day);
                if (date > today)
                {
                    return;
                }
                plan.Add(new PlannedTransaction(date, type, category, description, amount));
            }

            Add(1, TransactionType.Income, "Salary", $"Salary {monthName}", 2500m);
            Add(3, TransactionType.Expense, "Rent", $"Rent {monthName}", 850m);
            Add(5, TransactionType.Expense, "Transport", "Monthly ticket", 49m);
            Add(12, TransactionType.Expense, "Subscriptions", "Streaming", 12.99m);
            Add(15, TransactionType.Expense, "Subscriptions", "Mobile plan", 19.99m);
            Add(17, TransactionType.Expense, "Health", "Pharmacy", Pharmacy[offset]);
            for (int week = 0; week < Groceries[offset].Length; week++)
            {
                Add(2 + week * 7, TransactionType.Expense, "Groceries", week == 0 ? "Weekly shopping" : "Groceries", Groceries[offset][week]);
            }
            foreach ((int day, string description, decimal amount) in Leisure[offset])
            {
                Add(day, TransactionType.Expense, "Leisure", description, amount);
            }

            if (offset == 1) Add(20, TransactionType.Income, "Freelance", "Logo design", 450m);
            if (offset == 0) Add(24, TransactionType.Expense, "Other", "Birthday gift", 35m);
            if (offset == 2) Add(6, TransactionType.Expense, "Transport", "Train ticket", 29.90m);
        }

        // OrderBy is stable: bookings of one day keep their plan order
        return plan.OrderBy(p => p.Date).ToList();
    }
}
