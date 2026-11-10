using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CashStack.Data;

// Used only by the dotnet-ef tool (migrations, dbcontext info), never at runtime.
// Lets the tool create the context from CashStack.Core without starting the API.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CashStackDbContext>
{
    public CashStackDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<CashStackDbContext> options = new DbContextOptionsBuilder<CashStackDbContext>()
            .UseSqlite("Data Source=cashstack.db")
            .Options;
        return new CashStackDbContext(options);
    }
}
