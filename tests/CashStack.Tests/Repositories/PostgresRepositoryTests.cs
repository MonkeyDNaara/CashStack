using CashStack.Data;
using CashStack.Exceptions;
using CashStack.Models;
using CashStack.Repositories;
using CashStack.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashStack.Tests.Repositories;

// The EF repositories against a real Postgres: the same mapping as SQLite, plus the parts that
// differ per provider (migrations, case-insensitive collation, constraint error codes).
// Skipped unless CASHSTACK_TEST_POSTGRES is set (see PostgresFactAttribute).
public sealed class PostgresRepositoryTests : IDisposable
{
    // Created lazily: a skipped test must not try to reach a server
    private readonly Lazy<PostgresTestDatabase> _database = new(() => new PostgresTestDatabase());

    private EfTransactionRepository Transactions() => new(_database.Value.CreateContext());
    private EfCategoryRepository Categories() => new(_database.Value.CreateContext());

    [PostgresFact]
    public async Task Migration_CreatesTheStarterSet_IncomeFirstThenByName()
    {
        IReadOnlyList<Category> all = await Categories().GetAllAsync();

        Assert.Equal(
            ["Freelance", "Other", "Salary", "Groceries", "Health", "Leisure", "Other", "Rent", "Transport"],
            all.Select(c => c.Name));
    }

    [PostgresFact]
    public async Task Transaction_RoundTripsAmountAndOffset()
    {
        Transaction original = TestData.Expense(12.34m, TestData.At(7, 1, 8, offset: TestData.Summer), TestData.Rent, "Rent July");
        await Transactions().AddAsync(original);

        Transaction? loaded = await Transactions().GetByIdAsync(original.Id);

        Assert.NotNull(loaded);
        Assert.Equal(12.34m, loaded.Amount);
        Assert.Equal(original.Timestamp, loaded.Timestamp);
        Assert.Equal(TestData.Summer, loaded.Timestamp.Offset);
    }

    [PostgresFact]
    public async Task AmountIsStoredAsWholeCents()
    {
        await Transactions().AddAsync(TestData.Expense(12.34m, TestData.At(11, 1, 9)));

        await using CashStackDbContext context = _database.Value.CreateContext();
        long cents = await context.Database.SqlQueryRaw<long>("""SELECT "Amount" AS "Value" FROM "Transactions" """).SingleAsync();

        Assert.Equal(1234, cents);
    }

    [PostgresFact]
    public async Task GetRangeAsync_UsesTheLocalDate_AndOrdersAcrossOffsets()
    {
        await Transactions().AddAsync(TestData.Expense(1m, TestData.At(11, 10, 23, 30), description: "late"));
        await Transactions().AddAsync(TestData.Expense(2m, TestData.At(11, 11, 0, 30), description: "next day"));
        await Transactions().AddAsync(TestData.Expense(3m, TestData.At(7, 1, 9, offset: TestData.Summer), description: "july"));

        IReadOnlyList<Transaction> tenth = await Transactions().GetRangeAsync(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 10));
        IReadOnlyList<Transaction> year = await Transactions().GetRangeAsync(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        Assert.Equal("late", Assert.Single(tenth).Description);
        Assert.Equal(["july", "late", "next day"], year.Select(t => t.Description));
    }

    [PostgresFact]
    public async Task DuplicateCategoryName_IsRejectedByTheUniqueIndex_IgnoringCase()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            Categories().AddAsync(new Category(Guid.NewGuid(), "rENT", TransactionType.Expense)));
    }

    [PostgresFact]
    public async Task DuplicateCategoryName_IgnoresCaseOfUmlauts()
    {
        // ICU collation: unlike SQLite's NOCASE (ASCII only), "Ä" and "ä" are equal
        await Categories().AddAsync(new Category(Guid.NewGuid(), "Ärzte", TransactionType.Expense));

        await Assert.ThrowsAsync<ConflictException>(() =>
            Categories().AddAsync(new Category(Guid.NewGuid(), "ärzte", TransactionType.Expense)));
    }

    [PostgresFact]
    public async Task UnknownCategory_IsRejectedByTheForeignKey()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            Transactions().AddAsync(TestData.Expense(1m, TestData.At(11, 1, 9), Guid.NewGuid())));
    }

    [PostgresFact]
    public async Task RemoveCategoryInUse_IsRejectedByTheForeignKey()
    {
        await Transactions().AddAsync(TestData.Expense(5m, TestData.At(11, 1, 9), TestData.Rent));

        await Assert.ThrowsAsync<ConflictException>(() => Categories().RemoveAsync(TestData.Rent));
    }

    public void Dispose()
    {
        if (_database.IsValueCreated)
        {
            _database.Value.Dispose();
        }
    }
}
