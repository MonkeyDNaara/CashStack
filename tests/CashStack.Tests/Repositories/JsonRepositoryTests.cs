using CashStack.Exceptions;
using CashStack.Models;
using CashStack.Repositories;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Repositories;

// xUnit creates a new instance per test and calls Dispose afterwards → one temp folder per test
public sealed class JsonTransactionRepositoryTests : TransactionRepositoryContractTests, IDisposable
{
    private readonly TempDirectory _directory = new();

    protected override ITransactionRepository CreateRepository() => new JsonTransactionRepository(_directory.Path);

    [Fact]
    public async Task OldDayFileWithoutCategory_LoadsAsOther()
    {
        File.WriteAllText(_directory.File("2026-11-10.json"), """
            [{ "id": "11111111-1111-1111-1111-111111111111", "timestamp": "2026-11-10T08:00:00+01:00",
               "type": "Expense", "description": "legacy", "amount": 5 }]
            """);

        Transaction? legacy = await CreateRepository().GetByIdAsync(new Guid("11111111-1111-1111-1111-111111111111"));

        Assert.Equal(DefaultCategories.OtherExpenseId, legacy?.CategoryId);
    }

    [Fact]
    public async Task CorruptDayFile_ThrowsStorageException()
    {
        File.WriteAllText(_directory.File("2026-11-10.json"), "{ not json");

        await Assert.ThrowsAsync<StorageException>(() =>
            CreateRepository().GetRangeAsync(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)));
    }

    [Fact]
    public async Task AddAsync_WritesOneFilePerLocalDay()
    {
        await CreateRepository().AddAsync(TestData.Expense(1m, TestData.At(11, 10, 23, 30)));

        Assert.True(File.Exists(_directory.File("2026-11-10.json")));
    }

    public void Dispose() => _directory.Dispose();
}

public sealed class JsonCategoryRepositoryTests : CategoryRepositoryContractTests, IDisposable
{
    private readonly TempDirectory _directory = new();

    protected override ICategoryRepository CreateRepository() => new JsonCategoryRepository(_directory.Path);

    [Fact]
    public async Task FirstAccess_CreatesCategoriesFileWithTheStarterSet()
    {
        await CreateRepository().GetAllAsync();

        Assert.True(File.Exists(_directory.File("categories.json")));
    }

    public void Dispose() => _directory.Dispose();
}
