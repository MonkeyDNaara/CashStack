namespace CashStack.Services;

// Counts of one import run; "skipped" = already in the target (idempotent re-run)
public class ImportResult
{
    public int CategoriesImported { get; }
    public int CategoriesSkipped { get; }
    public int TransactionsImported { get; }
    public int TransactionsSkipped { get; }

    public ImportResult(int categoriesImported, int categoriesSkipped, int transactionsImported, int transactionsSkipped)
    {
        CategoriesImported = categoriesImported;
        CategoriesSkipped = categoriesSkipped;
        TransactionsImported = transactionsImported;
        TransactionsSkipped = transactionsSkipped;
    }
}
