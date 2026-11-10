using CashStack.Exceptions;
using CashStack.Models;
using CashStack.Services;
using CashStack.Tests.Fakes;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Services;

public class CategoryServiceTests
{
    private readonly FakeTransactionRepository _transactions = new();
    private readonly FakeCategoryRepository _categories = new();
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _service = new CategoryService(_categories, _transactions);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNameOfSameType_IgnoresCaseAndSpaces()
    {
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync("  rENT ", TransactionType.Expense));
    }

    [Fact]
    public async Task CreateAsync_SameNameForTheOtherType_IsAllowed()
    {
        Category created = await _service.CreateAsync("Rent", TransactionType.Income);

        Assert.Equal(TransactionType.Income, created.Type);
        Assert.Contains(created, _categories.Items);
    }

    [Fact]
    public async Task RenameAsync_ToExistingName_ThrowsAndKeepsTheOldName()
    {
        await Assert.ThrowsAsync<ConflictException>(() => _service.RenameAsync(TestData.Rent, "groceries"));

        Category rent = Assert.Single(_categories.Items, c => c.Id == TestData.Rent);
        Assert.Equal("Rent", rent.Name);
    }

    [Fact]
    public async Task RenameAsync_OnlyCaseChanges_IsAllowed()
    {
        Category? renamed = await _service.RenameAsync(TestData.Rent, "RENT");

        Assert.Equal("RENT", renamed?.Name);
    }

    [Fact]
    public async Task RenameAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _service.RenameAsync(Guid.NewGuid(), "x"));
    }

    [Theory]
    [MemberData(nameof(ProtectedIds))]
    public async Task RenameAndDelete_ProtectedOther_Throw(Guid otherId)
    {
        await Assert.ThrowsAsync<ConflictException>(() => _service.RenameAsync(otherId, "Misc"));
        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(otherId));
    }

    public static TheoryData<Guid> ProtectedIds => [DefaultCategories.OtherIncomeId, DefaultCategories.OtherExpenseId];

    [Fact]
    public async Task DeleteAsync_CategoryInUse_ThrowsAndKeepsIt()
    {
        _transactions.Items.Add(TestData.Expense(10m, DateTimeOffset.Now, TestData.Rent));

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(TestData.Rent));

        Assert.Contains(_categories.Items, c => c.Id == TestData.Rent);
    }

    [Fact]
    public async Task DeleteAsync_UnusedCategory_RemovesIt()
    {
        Assert.True(await _service.DeleteAsync(TestData.Health));
        Assert.DoesNotContain(_categories.Items, c => c.Id == TestData.Health);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));
    }
}
