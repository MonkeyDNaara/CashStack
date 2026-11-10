using CashStack.Models;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Models;

public class TransactionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_AmountNotPositive_Throws(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Transaction(Guid.NewGuid(), DateTimeOffset.Now, TransactionType.Expense, TestData.Groceries, "x", amount));
    }

    [Fact]
    public void Constructor_MoreThanTwoDecimals_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Transaction(Guid.NewGuid(), DateTimeOffset.Now, TransactionType.Expense, TestData.Groceries, "x", 1.234m));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyDescription_Throws(string description)
    {
        Assert.Throws<ArgumentException>(() =>
            new Transaction(Guid.NewGuid(), DateTimeOffset.Now, TransactionType.Expense, TestData.Groceries, description, 1m));
    }
}
