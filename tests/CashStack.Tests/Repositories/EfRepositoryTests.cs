using CashStack.Data;
using CashStack.Exceptions;
using CashStack.Models;
using CashStack.Repositories;
using CashStack.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashStack.Tests.Repositories;

public sealed class EfTransactionRepositoryTests : TransactionRepositoryContractTests, IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    protected override ITransactionRepository CreateRepository() => new EfTransactionRepository(_database.CreateContext());

    [Fact]
    public async Task AmountIsStoredAsWholeCents()
    {
        await CreateRepository().AddAsync(TestData.Expense(12.34m, TestData.At(11, 1, 9)));

        await using CashStackDbContext context = _database.CreateContext();
        long cents = await context.Database.SqlQueryRaw<long>("SELECT Amount AS Value FROM Transactions").SingleAsync();

        Assert.Equal(1234, cents);
    }

    [Fact]
    public async Task UnknownCategory_IsRejectedByTheForeignKey()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateRepository().AddAsync(TestData.Expense(1m, TestData.At(11, 1, 9), Guid.NewGuid())));
    }

    [Fact]
    public async Task BrokenDatabase_ThrowsStorageException()
    {
        await using (CashStackDbContext context = _database.CreateContext())
        {
            await context.Database.ExecuteSqlRawAsync("DROP TABLE Transactions");
        }

        await Assert.ThrowsAsync<StorageException>(() =>
            CreateRepository().GetRangeAsync(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
    }

    public void Dispose() => _database.Dispose();
}

public sealed class EfCategoryRepositoryTests : CategoryRepositoryContractTests, IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    protected override ICategoryRepository CreateRepository() => new EfCategoryRepository(_database.CreateContext());

    // The services check these rules first; the database is the safety net (e.g. two requests at once)
    [Fact]
    public async Task DuplicateName_IsRejectedByTheUniqueIndex_IgnoringCase()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateRepository().AddAsync(new Category(Guid.NewGuid(), "rENT", TransactionType.Expense)));
    }

    [Fact]
    public async Task RemoveCategoryInUse_IsRejectedByTheForeignKey()
    {
        await new EfTransactionRepository(_database.CreateContext())
            .AddAsync(TestData.Expense(5m, TestData.At(11, 1, 9), TestData.Rent));

        await Assert.ThrowsAsync<ConflictException>(() => CreateRepository().RemoveAsync(TestData.Rent));
    }

    public void Dispose() => _database.Dispose();
}
