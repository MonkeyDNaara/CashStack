using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CashStack.Data;

// Used only by the dotnet-ef tool. Creating a migration never connects, so a placeholder server is enough.
// dotnet ef migrations add <Name> --context PostgresCashStackDbContext --output-dir Data/Migrations/Postgres
public class PostgresDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PostgresCashStackDbContext>
{
    public PostgresCashStackDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<PostgresCashStackDbContext> options = new DbContextOptionsBuilder<PostgresCashStackDbContext>()
            .UseNpgsql("Host=localhost;Database=cashstack")
            .Options;
        return new PostgresCashStackDbContext(options);
    }
}
