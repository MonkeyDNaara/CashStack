using CashStack.Models;
using CashStack.Services;
using CashStack.Tests.Fakes;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Services;

public class ImportServiceTests
{
    private readonly FakeTransactionRepository _sourceTransactions = new();
    private readonly FakeCategoryRepository _sourceCategories = new();
    private readonly FakeTransactionRepository _targetTransactions = new();
    private readonly FakeCategoryRepository _targetCategories = new();
    private readonly ImportService _importer;

    public ImportServiceTests()
    {
        _importer = new ImportService(_sourceTransactions, _sourceCategories, _targetTransactions, _targetCategories);
    }

    [Fact]
    public async Task ImportAsync_RunTwice_ImportsEveryTransactionExactlyOnce()
    {
        _sourceTransactions.Items.AddRange([
            TestData.Expense(10m, TestData.At(10, 1, 9)),
            TestData.Income(20m, TestData.At(11, 1, 9))
        ]);

        ImportResult first = await _importer.ImportAsync();
        ImportResult second = await _importer.ImportAsync();

        Assert.Equal(2, first.TransactionsImported);
        Assert.Equal(0, second.TransactionsImported);
        Assert.Equal(2, second.TransactionsSkipped);
        Assert.Equal(
            _sourceTransactions.Items.Select(t => t.Id).Order(),
            _targetTransactions.Items.Select(t => t.Id).Order());
    }

    [Fact]
    public async Task ImportAsync_NewCategory_IsCreatedWithItsId()
    {
        var gym = new Category(Guid.NewGuid(), "Gym", TransactionType.Expense);
        _sourceCategories.Items.Add(gym);
        _sourceTransactions.Items.Add(TestData.Expense(30m, TestData.At(11, 1, 9), gym.Id));

        ImportResult result = await _importer.ImportAsync();

        Assert.Equal(1, result.CategoriesImported);
        Assert.Contains(_targetCategories.Items, c => c.Id == gym.Id);
        Assert.Equal(gym.Id, Assert.Single(_targetTransactions.Items).CategoryId);
    }

    [Fact]
    public async Task ImportAsync_SameNameUnderAnotherId_MapsToTheExistingCategory()
    {
        var groceriesCopy = new Category(Guid.NewGuid(), "groceries", TransactionType.Expense);
        _sourceCategories.Items.Add(groceriesCopy);
        _sourceTransactions.Items.Add(TestData.Expense(20m, TestData.At(11, 1, 9), groceriesCopy.Id));

        ImportResult result = await _importer.ImportAsync();

        Assert.Equal(0, result.CategoriesImported);
        Assert.DoesNotContain(_targetCategories.Items, c => c.Id == groceriesCopy.Id);
        Assert.Equal(TestData.Groceries, Assert.Single(_targetTransactions.Items).CategoryId);
    }

    [Fact]
    public async Task ImportAsync_UnknownCategoryOrWrongType_FallsBackToOther()
    {
        _sourceTransactions.Items.AddRange([
            TestData.Expense(1m, TestData.At(11, 1, 9), Guid.NewGuid(), "unknown"),
            TestData.Income(2m, TestData.At(11, 1, 10), TestData.Groceries, "wrong type")
        ]);

        await _importer.ImportAsync();

        Assert.Equal(DefaultCategories.OtherExpenseId, _targetTransactions.Items.Single(t => t.Description == "unknown").CategoryId);
        Assert.Equal(DefaultCategories.OtherIncomeId, _targetTransactions.Items.Single(t => t.Description == "wrong type").CategoryId);
    }
}
