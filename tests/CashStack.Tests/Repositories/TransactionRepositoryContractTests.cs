using CashStack.Models;
using CashStack.Repositories;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Repositories;

// The same facts run against every ITransactionRepository implementation (JSON and EF Core):
// both storages must behave identically. The subclasses only say how to create the repository.
public abstract class TransactionRepositoryContractTests
{
    protected abstract ITransactionRepository CreateRepository();

    [Fact]
    public async Task AddAndGetById_ReturnsTheSameValues()
    {
        Transaction original = TestData.Expense(12.34m, TestData.At(7, 1, 8, offset: TestData.Summer), TestData.Rent, "Rent July");
        await CreateRepository().AddAsync(original);

        Transaction? loaded = await CreateRepository().GetByIdAsync(original.Id);

        Assert.NotNull(loaded);
        Assert.Equal(original.Amount, loaded.Amount);
        Assert.Equal(original.Timestamp, loaded.Timestamp);
        Assert.Equal(original.Timestamp.Offset, loaded.Timestamp.Offset);
        Assert.Equal(original.CategoryId, loaded.CategoryId);
        Assert.Equal(original.Description, loaded.Description);
        Assert.Equal(original.Type, loaded.Type);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await CreateRepository().GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetRangeAsync_UsesTheLocalDateOfTheTimestamp()
    {
        Transaction lateEvening = TestData.Expense(1m, TestData.At(11, 10, 23, 30), description: "late");
        Transaction afterMidnight = TestData.Expense(2m, TestData.At(11, 11, 0, 30), description: "next day");
        await AddAllAsync(lateEvening, afterMidnight);

        IReadOnlyList<Transaction> day = await CreateRepository().GetRangeAsync(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 10));

        Assert.Equal(lateEvening.Id, Assert.Single(day).Id);
    }

    [Fact]
    public async Task GetRangeAsync_IsOrderedByTimestamp_AlsoAcrossOffsets()
    {
        await AddAllAsync(
            TestData.Expense(1m, TestData.At(11, 2, 9), description: "november"),
            TestData.Expense(2m, TestData.At(7, 1, 9, offset: TestData.Summer), description: "july"),
            TestData.Expense(3m, TestData.At(11, 1, 9), description: "first of november"));

        IReadOnlyList<Transaction> all = await CreateRepository().GetRangeAsync(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        Assert.Equal(["july", "first of november", "november"], all.Select(t => t.Description));
    }

    [Fact]
    public async Task GetRangeAsync_FiltersByTypeAndCategory()
    {
        await AddAllAsync(
            TestData.Income(100m, TestData.At(11, 1, 9)),
            TestData.Expense(10m, TestData.At(11, 1, 10), TestData.Groceries, "groceries"),
            TestData.Expense(800m, TestData.At(11, 1, 11), TestData.Rent, "rent"));
        var (from, to) = (new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        IReadOnlyList<Transaction> expenses = await CreateRepository().GetRangeAsync(from, to, TransactionType.Expense);
        IReadOnlyList<Transaction> rent = await CreateRepository().GetRangeAsync(from, to, categoryId: TestData.Rent);

        Assert.Equal(2, expenses.Count);
        Assert.Equal("rent", Assert.Single(rent).Description);
    }

    [Fact]
    public async Task RemoveAsync_RemovesOnce()
    {
        Transaction transaction = TestData.Expense(5m, TestData.At(11, 1, 9));
        await CreateRepository().AddAsync(transaction);

        Assert.True(await CreateRepository().RemoveAsync(transaction.Id));
        Assert.False(await CreateRepository().RemoveAsync(transaction.Id));
        Assert.Null(await CreateRepository().GetByIdAsync(transaction.Id));
    }

    [Fact]
    public async Task AnyWithCategoryAsync_TrueOnlyForUsedCategories()
    {
        await CreateRepository().AddAsync(TestData.Expense(5m, TestData.At(11, 1, 9), TestData.Rent));

        Assert.True(await CreateRepository().AnyWithCategoryAsync(TestData.Rent));
        Assert.False(await CreateRepository().AnyWithCategoryAsync(TestData.Health));
    }

    private async Task AddAllAsync(params Transaction[] transactions)
    {
        ITransactionRepository repository = CreateRepository();
        foreach (Transaction transaction in transactions)
        {
            await repository.AddAsync(transaction);
        }
    }
}
