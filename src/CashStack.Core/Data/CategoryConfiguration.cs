using CashStack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashStack.Data;

// EF calls the validating Category constructor and uses the private setter of Name for Rename
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        // Case-insensitive collation per provider (see ConfigureProvider in the DbContext subclasses),
        // so "Rent" and "rent" collide in the unique index below
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(Category.MaxNameLength);

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.HasIndex(c => new { c.Type, c.Name })
            .IsUnique();

        // Computed from the Id, not stored
        builder.Ignore(c => c.IsProtected);

        // Same fixed ids as the JSON fallback, so both providers and the import agree
        builder.HasData(DefaultCategories.CreateStarterSet());
    }
}
