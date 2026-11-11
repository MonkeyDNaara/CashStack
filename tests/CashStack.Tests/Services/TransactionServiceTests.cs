using CashStack.Events;
using CashStack.Exceptions;
using CashStack.Models;
using CashStack.Services;
using CashStack.Tests.Fakes;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Services;

public class TransactionServiceTests
{
    private readonly FakeTransactionRepository _transactions = new();
    private readonly FakeCategoryRepository _categories = new();
    private readonly TransactionService _service;

    // "Now" for all tests: Wednesday, 2026-11-11 14:30 in Berlin (winter time, +01:00)
    private static readonly DateTimeOffset Now = TestData.At(11, 11, 14, 30);

    public TransactionServiceTests()
    {
        _service = new TransactionService(_transactions, _categories, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task AddAsync_WithoutDate_UsesNow()
    {
        Transaction added = await _service.AddAsync(TransactionType.Expense, "Coffee", 3.50m, TestData.Groceries);

        Assert.Equal(Now, added.Timestamp);
    }

    [Fact]
    public async Task AddAsync_Today_UsesNow()
    {
        Transaction added = await _service.AddAsync(
            TransactionType.Expense, "Coffee", 3.50m, TestData.Groceries, new DateOnly(2026, 11, 11));

        Assert.Equal(Now, added.Timestamp);
    }

    [Fact]
    public async Task AddAsync_PastDate_KeepsTimeOfDayWithTheOffsetOfThatDay()
    {
        Transaction added = await _service.AddAsync(
            TransactionType.Expense, "Groceries", 20m, TestData.Groceries, new DateOnly(2026, 8, 15));

        // August is summer time in Berlin: +02:00, although "now" is +01:00
        Assert.Equal(TestData.At(8, 15, 14, 30, TestData.Summer), added.Timestamp);
        Assert.Equal(TestData.Summer, added.Timestamp.Offset);
    }

    [Fact]
    public async Task AddAsync_FutureDate_ThrowsAndSavesNothing()
    {
        int raised = 0;
        _service.TransactionAdded += (_, _) => raised++;

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.AddAsync(TransactionType.Expense, "x", 1m, TestData.Groceries, new DateOnly(2026, 11, 12)));

        Assert.Contains("2026-11-12", error.Message);
        Assert.Empty(_transactions.Items);
        Assert.Equal(0, raised);
    }

    [Fact]
    public async Task AddAsync_RaisesTransactionAddedOnce_WithTheAddedTransaction()
    {
        var raised = new List<TransactionAddedEventArgs>();
        _service.TransactionAdded += (_, e) => raised.Add(e);

        Transaction added = await _service.AddAsync(TransactionType.Expense, "Coffee", 3.50m, TestData.Groceries);

        TransactionAddedEventArgs e = Assert.Single(raised);
        Assert.Same(added, e.Transaction);
        Assert.Same(added, Assert.Single(_transactions.Items));
    }

    [Theory]
    [InlineData(TransactionType.Income)]
    [InlineData(TransactionType.Expense)]
    public async Task AddAsync_WithoutCategory_UsesOtherOfTheType(TransactionType type)
    {
        Transaction added = await _service.AddAsync(type, "x", 1m);

        Assert.Equal(DefaultCategories.GetOtherId(type), added.CategoryId);
    }

    [Fact]
    public async Task AddAsync_CategoryOfTheOtherType_ThrowsAndSavesNothing()
    {
        int raised = 0;
        _service.TransactionAdded += (_, _) => raised++;

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.AddAsync(TransactionType.Income, "x", 1m, TestData.Groceries));

        Assert.Empty(_transactions.Items);
        Assert.Equal(0, raised);
    }

    [Fact]
    public async Task AddAsync_UnknownCategory_Throws()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.AddAsync(TransactionType.Expense, "x", 1m, Guid.NewGuid()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task AddAsync_AmountNotPositive_IsRejected(decimal amount)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.AddAsync(TransactionType.Expense, "x", amount, TestData.Groceries));

        Assert.Empty(_transactions.Items);
    }

    [Fact]
    public async Task RemoveAsync_UnknownId_ReturnsFalse()
    {
        Assert.False(await _service.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetTransactionsAsync_FromAfterTo_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetTransactionsAsync(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public async Task GetSummaryAsync_TotalsPerCategory_AddUpToIncomeAndExpenses()
    {
        _transactions.Items.AddRange([
            TestData.Income(2500m, TestData.At(11, 1, 9)),
            TestData.Expense(800m, TestData.At(11, 2, 9), TestData.Rent),
            TestData.Expense(40m, TestData.At(11, 3, 9)),
            TestData.Expense(12.50m, TestData.At(11, 4, 9)),
            TestData.Expense(99m, TestData.At(10, 31, 9)) // outside the period
        ]);

        TransactionSummary summary = await _service.GetSummaryAsync(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        Assert.Equal(2500m, summary.Income);
        Assert.Equal(852.50m, summary.Expenses);
        Assert.Equal(1647.50m, summary.Balance);
        // Income first, then the largest total first
        Assert.Equal(["Salary", "Rent", "Groceries"], summary.Categories.Select(c => c.Name));
        Assert.Equal(summary.Income, summary.Categories.Where(c => c.Type == TransactionType.Income).Sum(c => c.Total));
        Assert.Equal(summary.Expenses, summary.Categories.Where(c => c.Type == TransactionType.Expense).Sum(c => c.Total));
    }
}
