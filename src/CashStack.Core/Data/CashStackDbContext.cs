using CashStack.Models;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Data;

public class CashStackDbContext : DbContext
{
    public CashStackDbContext(DbContextOptions<CashStackDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();

    // One IEntityTypeConfiguration class per entity keeps this method small
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashStackDbContext).Assembly);
    }

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
