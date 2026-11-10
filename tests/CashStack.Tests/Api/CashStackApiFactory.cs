using CashStack.Tests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace CashStack.Tests.Api;

// Starts the real API in memory (no port, no files in the repo):
// SQLite shared in-memory database + audit log in a temp folder
public sealed class CashStackApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString = $"Data Source=cashstack-api-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
    private readonly SqliteConnection _keepAlive;
    private readonly TempDirectory _logs = new();

    public CashStackApiFactory()
    {
        // A shared in-memory database exists as long as one connection is open
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }

    public string AuditLogPath => _logs.File("transactions.log");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Storage:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:CashStack", _connectionString);
        builder.UseSetting("Storage:LogDirectory", _logs.Path);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAlive.Dispose();
            _logs.Dispose();
        }
    }
}
