using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CashStack.Tests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CashStack.Tests.Api;

// The real API with Storage:Provider=Postgres, as on the live demo.
// Skipped unless CASHSTACK_TEST_POSTGRES is set (see PostgresFactAttribute).
public sealed class PostgresApiTests : IDisposable
{
    private readonly Lazy<PostgresTestDatabase> _database = new(() => new PostgresTestDatabase());
    private readonly TempDirectory _logs = new();
    private WebApplicationFactory<Program>? _factory;

    private HttpClient CreateClient()
    {
        string connectionString = _database.Value.ConnectionString;
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Storage:Provider", "Postgres");
            builder.UseSetting("ConnectionStrings:CashStack", connectionString);
            builder.UseSetting("Storage:LogDirectory", _logs.Path);
        });
        return _factory.CreateClient();
    }

    [PostgresFact]
    public async Task Categories_ComeFromThePostgresMigrationSeed()
    {
        HttpResponseMessage response = await CreateClient().GetAsync("/api/v1/categories");

        using JsonDocument all = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(9, all.RootElement.GetArrayLength());
    }

    [PostgresFact]
    public async Task CreateTransaction_ThenSummary_Works()
    {
        HttpClient client = CreateClient();

        HttpResponseMessage created = await client.PostAsJsonAsync("/api/v1/transactions",
            new { type = "Expense", categoryId = TestData.Rent, description = "Rent", amount = 850m });
        HttpResponseMessage summary = await client.GetAsync("/api/v1/transactions/summary");

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.OK, summary.StatusCode);
        Assert.Contains("850", await summary.Content.ReadAsStringAsync());
    }

    public void Dispose()
    {
        _factory?.Dispose();
        if (_database.IsValueCreated)
        {
            _database.Value.Dispose();
        }
        _logs.Dispose();
    }
}
