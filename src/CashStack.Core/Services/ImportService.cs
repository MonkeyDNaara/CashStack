using CashStack.Models;
using CashStack.Repositories;

namespace CashStack.Services;

// Copies categories and transactions from one storage to another (e.g. JSON files → SQLite).
// Works only with the repository interfaces, so source and target can be any implementation.
// Idempotent: the Id is the key, a second run skips everything that is already there.
public class ImportService
{
    private readonly ITransactionRepository _sourceTransactions;
    private readonly ICategoryRepository _sourceCategories;
    private readonly ITransactionRepository _targetTransactions;
    private readonly ICategoryRepository _targetCategories;

    public ImportService(
        ITransactionRepository sourceTransactions, ICategoryRepository sourceCategories,
        ITransactionRepository targetTransactions, ICategoryRepository targetCategories)
    {
        _sourceTransactions = sourceTransactions;
        _sourceCategories = sourceCategories;
        _targetTransactions = targetTransactions;
        _targetCategories = targetCategories;
    }

    public async Task<ImportResult> ImportAsync(CancellationToken ct = default)
    {
        (Dictionary<Guid, Category> categoryMap, int categoriesImported, int categoriesSkipped) =
            await ImportCategoriesAsync(ct);

        int transactionsImported = 0;
        int transactionsSkipped = 0;
        IReadOnlyList<Transaction> transactions =
            await _sourceTransactions.GetRangeAsync(DateOnly.MinValue, DateOnly.MaxValue, ct: ct);

        foreach (Transaction transaction in transactions)
        {
            if (await _targetTransactions.GetByIdAsync(transaction.Id, ct) is not null)
            {
                transactionsSkipped++;
                continue;
            }

            await _targetTransactions.AddAsync(WithTargetCategory(transaction, categoryMap), ct);
            transactionsImported++;
        }

        return new ImportResult(categoriesImported, categoriesSkipped, transactionsImported, transactionsSkipped);
    }

    // Returns: source category id → category in the target
    private async Task<(Dictionary<Guid, Category> Map, int Imported, int Skipped)> ImportCategoriesAsync(CancellationToken ct)
    {
        var map = new Dictionary<Guid, Category>();
        int imported = 0;
        int skipped = 0;

        foreach (Category category in await _sourceCategories.GetAllAsync(ct: ct))
        {
            Category? sameId = await _targetCategories.GetByIdAsync(category.Id, ct);
            if (sameId is not null)
            {
                map[category.Id] = sameId;
                skipped++;
                continue;
            }

            // Same name and type under another id: reuse it instead of breaking the unique name rule
            IReadOnlyList<Category> sameType = await _targetCategories.GetAllAsync(category.Type, ct);
            Category? sameName = sameType.FirstOrDefault(c =>
                string.Equals(c.Name, category.Name, StringComparison.OrdinalIgnoreCase));
            if (sameName is not null)
            {
                map[category.Id] = sameName;
                skipped++;
                continue;
            }

            await _targetCategories.AddAsync(category, ct);
            map[category.Id] = category;
            imported++;
        }

        return (map, imported, skipped);
    }

    // Unknown category or a category of the other type → "Other" of the transaction type
    private static Transaction WithTargetCategory(Transaction t, Dictionary<Guid, Category> categoryMap)
    {
        Guid categoryId = categoryMap.TryGetValue(t.CategoryId, out Category? target) && target.Type == t.Type
            ? target.Id
            : DefaultCategories.GetOtherId(t.Type);

        if (categoryId == t.CategoryId)
        {
            return t;
        }
        return new Transaction(t.Id, t.Timestamp, t.Type, categoryId, t.Description, t.Amount);
    }
}
