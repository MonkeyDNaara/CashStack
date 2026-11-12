using System.Data.Common;
using CashStack.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CashStack.Data;

// Local default: one database file, no server needed. Migrations: Data/Migrations/Sqlite
public class SqliteCashStackDbContext : CashStackDbContext
{
    // SQLITE_CONSTRAINT: unique index or foreign key violated
    private const int ConstraintError = 19;

    public SqliteCashStackDbContext(DbContextOptions<SqliteCashStackDbContext> options) : base(options) { }

    // NOCASE: "Rent" and "rent" collide in the unique index of Category.
    // Safety net only (ASCII case folding); CategoryService checks first and returns a clear message
    protected override void ConfigureProvider(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .UseCollation("NOCASE");
    }

    protected override bool IsConstraintError(DbException exception) =>
        exception is SqliteException { SqliteErrorCode: ConstraintError };

    // SQLite creates the database file, but not a missing folder like "data/"
    public override void PrepareStorage()
    {
        string dataSource = new SqliteConnectionStringBuilder(Database.GetConnectionString()).DataSource;
        string? directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
