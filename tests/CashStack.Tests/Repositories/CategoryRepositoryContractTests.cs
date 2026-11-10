using CashStack.Models;
using CashStack.Repositories;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Repositories;

// Same facts for JSON and EF Core category storage
public abstract class CategoryRepositoryContractTests
{
    protected abstract ICategoryRepository CreateRepository();

    [Fact]
    public async Task GetAllAsync_StartsWithTheStarterSet_IncomeFirstThenByName()
    {
        IReadOnlyList<Category> all = await CreateRepository().GetAllAsync();

        Assert.Equal(
            ["Freelance", "Other", "Salary", "Groceries", "Health", "Leisure", "Other", "Rent", "Transport"],
            all.Select(c => c.Name));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByType()
    {
        IReadOnlyList<Category> income = await CreateRepository().GetAllAsync(TransactionType.Income);

        Assert.All(income, c => Assert.Equal(TransactionType.Income, c.Type));
        Assert.Equal(3, income.Count);
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsIt()
    {
        var gym = new Category(Guid.NewGuid(), "Gym", TransactionType.Expense);
        await CreateRepository().AddAsync(gym);

        Category? loaded = await CreateRepository().GetByIdAsync(gym.Id);

        Assert.Equal("Gym", loaded?.Name);
        Assert.False(loaded?.IsProtected);
    }

    [Fact]
    public async Task UpdateAsync_PersistsTheNewName()
    {
        ICategoryRepository repository = CreateRepository();
        Category rent = (await repository.GetByIdAsync(TestData.Rent))!;
        rent.Rename("Housing");
        await repository.UpdateAsync(rent);

        Category? loaded = await CreateRepository().GetByIdAsync(TestData.Rent);

        Assert.Equal("Housing", loaded?.Name);
    }

    [Fact]
    public async Task RemoveAsync_RemovesOnce()
    {
        Assert.True(await CreateRepository().RemoveAsync(TestData.Health));
        Assert.False(await CreateRepository().RemoveAsync(TestData.Health));
        Assert.Null(await CreateRepository().GetByIdAsync(TestData.Health));
    }
}
