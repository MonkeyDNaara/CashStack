using CashStack.Models;
using CashStack.Services;

namespace CashStack.Demo;

// Fills an empty demo with the DemoDataPlan. Goes through the services, so the demo data passes
// the same rules as an API request (no future dates, category must match the type) and the audit log sees it.
public class DemoDataSeeder
{
    private readonly TransactionService _transactions;
    private readonly CategoryService _categories;
    private readonly TimeProvider _time;

    public DemoDataSeeder(TransactionService transactions, CategoryService categories, TimeProvider time)
    {
        _transactions = transactions;
        _categories = categories;
        _time = time;
    }

    // Seeds only when the plan's range (start of the month two months ago until today) has no transactions.
    // So an old demo refreshes itself once its data has aged out of "Last 3 months".
    public async Task<DemoSeedResult> SeedIfEmptyAsync(CancellationToken ct = default)
    {
        DateOnly today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);
        IReadOnlyList<PlannedTransaction> plan = DemoDataPlan.Build(today);
        DateOnly from = plan.Count > 0 ? plan[0].Date : today;

        IReadOnlyList<Transaction> existing = await _transactions.GetTransactionsAsync(from, today, ct: ct);
        if (existing.Count > 0)
        {
            return new DemoSeedResult(Seeded: false, existing.Count, []);
        }

        (Dictionary<(string, TransactionType), Guid> ids, List<string> created) = await EnsureCategoriesAsync(plan, ct);

        // One after another: keeps the audit log in date order
        foreach (PlannedTransaction p in plan)
        {
            await _transactions.AddAsync(p.Type, p.Description, p.Amount, ids[Key(p.Category, p.Type)], p.Date, ct);
        }

        return new DemoSeedResult(Seeded: true, plan.Count, created);
    }

    // Every category the plan uses must exist: the extra ones (Subscriptions) and any starter category
    // a visitor deleted or renamed are created again
    private async Task<(Dictionary<(string, TransactionType), Guid>, List<string>)> EnsureCategoriesAsync(
        IReadOnlyList<PlannedTransaction> plan, CancellationToken ct)
    {
        var ids = (await _categories.GetAllAsync(ct: ct))
            .GroupBy(c => Key(c.Name, c.Type))
            .ToDictionary(g => g.Key, g => g.First().Id);
        var created = new List<string>();

        IEnumerable<(string Name, TransactionType Type)> needed = plan
            .Select(p => (p.Category, p.Type))
            .Concat(DemoDataPlan.ExtraCategories)
            .Distinct();

        foreach ((string name, TransactionType type) in needed)
        {
            if (!ids.ContainsKey(Key(name, type)))
            {
                Category category = await _categories.CreateAsync(name, type, ct);
                ids[Key(name, type)] = category.Id;
                created.Add(name);
            }
        }
        return (ids, created);
    }

    // Names compare like the database collation: case-insensitive
    private static (string, TransactionType) Key(string name, TransactionType type) =>
        (name.Trim().ToUpperInvariant(), type);
}
