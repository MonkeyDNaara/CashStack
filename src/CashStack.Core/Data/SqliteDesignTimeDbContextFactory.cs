using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CashStack.Data;

// Used only by the dotnet-ef tool (migrations, dbcontext info), never at runtime.
// dotnet ef migrations add <Name> --context SqliteCashStackDbContext --output-dir Data/Migrations/Sqlite
public class SqliteDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqliteCashStackDbContext>
{
    public SqliteCashStackDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<SqliteCashStackDbContext> options = new DbContextOptionsBuilder<SqliteCashStackDbContext>()
            .UseSqlite("Data Source=cashstack.db")
            .Options;
        return new SqliteCashStackDbContext(options);
    }
}
