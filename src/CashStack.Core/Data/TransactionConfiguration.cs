using CashStack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CashStack.Data;

// Mapping decisions for SQLite. EF calls the validating Transaction constructor
// (constructor binding: parameter names match the property names)
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    // Shadow property: exists in the table, not in the domain class
    public const string DayProperty = "Day";

    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        // SQLite can't compare or sort DateTimeOffset stored as text.
        // Binary (long) is sortable and keeps the offset.
        builder.Property(t => t.Timestamp)
            .HasConversion(new DateTimeOffsetToBinaryConverter());

        // Local date of the timestamp, set in CashStackDbContext.SaveChanges.
        // The date range filter uses it, so results match the JSON repository (one file per local day).
        // DateOnly is stored as "yyyy-MM-dd" text, which compares correctly.
        builder.Property<DateOnly>(DayProperty);
        builder.HasIndex(DayProperty);

        // Enums as text: readable in the database and safe if enum values are reordered
        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(t => t.Description)
            .IsRequired();

        // SQLite has no decimal type (EF would use text: wrong sorting, no SUM).
        // Whole cents as INTEGER are exact; the constructor guarantees max. 2 decimals.
        builder.Property(t => t.Amount)
            .HasConversion(
                amount => (long)(amount * 100m),
                cents => cents / 100m);

        // Category 1:n Transaction without navigation properties (domain classes stay free of EF).
        // Restrict: the database refuses to delete a category that is still in use
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
