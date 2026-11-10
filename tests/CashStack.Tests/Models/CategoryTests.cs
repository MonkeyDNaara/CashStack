using CashStack.Models;
using Xunit;

namespace CashStack.Tests.Models;

public class CategoryTests
{
    [Fact]
    public void Constructor_TrimsName()
    {
        var category = new Category(Guid.NewGuid(), "  Rent  ", TransactionType.Expense);

        Assert.Equal("Rent", category.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), name, TransactionType.Expense));
    }

    [Fact]
    public void Constructor_NameLongerThanMax_Throws()
    {
        string tooLong = new('a', Category.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), tooLong, TransactionType.Expense));
    }

    [Fact]
    public void IsProtected_OnlyForTheTwoOtherCategories()
    {
        List<Category> protectedOnes = DefaultCategories.CreateStarterSet().Where(c => c.IsProtected).ToList();

        Assert.Equal(2, protectedOnes.Count);
        Assert.All(protectedOnes, c => Assert.Equal("Other", c.Name));
    }
}
