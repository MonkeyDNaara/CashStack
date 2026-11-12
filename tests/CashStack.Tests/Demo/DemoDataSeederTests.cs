using CashStack.Demo;
using CashStack.Models;
using CashStack.Services;
using CashStack.Tests.Fakes;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Demo;

public class DemoDataSeederTests
{
    private readonly FakeTransactionRepository _transactions = new();
    private readonly FakeCategoryRepository _categories = new();
    private readonly DemoDataSeeder _seeder;

    // "Now": 2026-12-31 18:00 in Berlin → three full months (October to December)
    private static readonly DateTimeOffset Now = TestData.At(12, 31, 18);

    public DemoDataSeederTests()
    {
        var time = new FakeTimeProvider(Now);
        _seeder = new DemoDataSeeder(
            new TransactionService(_transactions, _categories, time),
            new CategoryService(_categories, _transactions),
            time);
    }

    [Fact]
    public async Task EmptyDemo_IsSeeded_WithTheExtraCategory()
    {
        DemoSeedResult result = await _seeder.SeedIfEmptyAsync();

        Assert.True(result.Seeded);
        Assert.Equal(42, result.Count);
        Assert.Equal(42, _transactions.Items.Count);
        Assert.Equal(["Subscriptions"], result.CreatedCategories);
        Assert.Contains(_categories.Items, c => c.Name == "Subscriptions" && c.Type == TransactionType.Expense);
    }

    [Fact]
    public async Task SeededTransactions_UseThePlannedDatesAndCategories()
    {
        await _seeder.SeedIfEmptyAsync();

        Transaction rent = Assert.Single(_transactions.Items, t => t.Description == "Rent November");
        Assert.Equal(new DateOnly(2026, 11, 3), DateOnly.FromDateTime(rent.Timestamp.DateTime));
        Assert.Equal(TestData.Rent, rent.CategoryId);
        Assert.Equal(850m, rent.Amount);
    }

    [Fact]
    public async Task SecondRun_IsSkipped()
    {
        await _seeder.SeedIfEmptyAsync();

        DemoSeedResult second = await _seeder.SeedIfEmptyAsync();

        Assert.False(second.Seeded);
        Assert.Equal(42, second.Count);
        Assert.Equal(42, _transactions.Items.Count);
    }

    [Fact]
    public async Task ExistingTransactionInTheRange_SkipsTheSeed()
    {
        _transactions.Items.Add(TestData.Expense(5m, TestData.At(12, 1, 9)));

        DemoSeedResult result = await _seeder.SeedIfEmptyAsync();

        Assert.False(result.Seeded);
        Assert.Single(_transactions.Items);
    }

    [Fact]
    public async Task OnlyOldTransactions_StillSeeds()
    {
        // Aged demo data from the summer is outside "the last three months"
        _transactions.Items.Add(TestData.Expense(5m, TestData.At(7, 1, 9, offset: TestData.Summer)));

        DemoSeedResult result = await _seeder.SeedIfEmptyAsync();

        Assert.True(result.Seeded);
        Assert.Equal(43, _transactions.Items.Count);
    }

    [Fact]
    public async Task DeletedStarterCategory_IsCreatedAgain()
    {
        _categories.Items.RemoveAll(c => c.Id == TestData.Rent);

        DemoSeedResult result = await _seeder.SeedIfEmptyAsync();

        Assert.Contains("Rent", result.CreatedCategories);
        Guid newRent = Assert.Single(_categories.Items, c => c.Name == "Rent").Id;
        Assert.All(_transactions.Items.Where(t => t.Description.StartsWith("Rent")), t => Assert.Equal(newRent, t.CategoryId));
    }

    [Fact]
    public async Task CategoryNames_AreMatchedIgnoringCase()
    {
        _categories.Items.Add(new Category(Guid.NewGuid(), "subscriptions", TransactionType.Expense));

        DemoSeedResult result = await _seeder.SeedIfEmptyAsync();

        Assert.Empty(result.CreatedCategories);
    }
}
