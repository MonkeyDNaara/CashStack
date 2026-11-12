using System.Data.Common;
using CashStack.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CashStack.Data;

// Hosted database (Neon on the live demo). Migrations: Data/Migrations/Postgres
public class PostgresCashStackDbContext : CashStackDbContext
{
    // ICU collation that ignores case (strength level 2), also for umlauts ("Ärzte" = "ärzte").
    // Nondeterministic: equal comparisons and the unique index use it, unlike plain text equality.
    public const string CaseInsensitiveCollation = "case_insensitive";

    public PostgresCashStackDbContext(DbContextOptions<PostgresCashStackDbContext> options) : base(options) { }

    protected override void ConfigureProvider(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCollation(CaseInsensitiveCollation, locale: "und-u-ks-level2", provider: "icu", deterministic: false);

        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .UseCollation(CaseInsensitiveCollation);
    }

    protected override bool IsConstraintError(DbException exception) =>
        exception is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation };
}
