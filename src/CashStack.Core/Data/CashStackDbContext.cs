using System.Data.Common;
using CashStack.Models;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Data;

// Shared model for every database provider. Each provider has its own subclass
// (SqliteCashStackDbContext, PostgresCashStackDbContext) and its own migration set,
// because migrations contain provider-specific column types.
public abstract class CashStackDbContext : DbContext
{
    // Non-generic options: each subclass passes its own DbContextOptions<TSubclass>
    protected CashStackDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();

    // One IEntityTypeConfiguration class per entity keeps this method small,
    // then the provider adds what only it understands (e.g. the collation of Category.Name)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashStackDbContext).Assembly);
        ConfigureProvider(modelBuilder);
    }

    // Provider-specific mapping. Must make Category.Name case-insensitive ("Rent" = "rent")
    protected abstract void ConfigureProvider(ModelBuilder modelBuilder);

    // True when the database rejected a change because of a unique index or a foreign key.
    // SaveChanges wraps the provider error in a DbUpdateException, ExecuteDelete throws it directly.
    public bool IsConstraintViolation(Exception exception) =>
        (exception as DbException ?? exception.InnerException as DbException) is { } dbException
        && IsConstraintError(dbException);

    // Each provider reports constraint violations with its own error codes
    protected abstract bool IsConstraintError(DbException exception);

    // Called before migrating, e.g. to create the folder of a database file. Default: nothing to do
    public virtual void PrepareStorage() { }

    // The parameterless SaveChanges/SaveChangesAsync overloads call these two
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetTransactionDays();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetTransactionDays();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Fills the shadow property "Day" (local date of the timestamp) for new transactions.
    // Transactions are immutable, so only added entries need it.
    private void SetTransactionDays()
    {
        foreach (var entry in ChangeTracker.Entries<Transaction>().Where(e => e.State == EntityState.Added))
        {
            entry.Property<DateOnly>(TransactionConfiguration.DayProperty).CurrentValue =
                DateOnly.FromDateTime(entry.Entity.Timestamp.DateTime);
        }
    }
}
