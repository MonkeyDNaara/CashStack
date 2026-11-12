using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CashStack.Tests.Api;

public class DemoSeedApiTests : IDisposable
{
    private readonly CashStackApiFactory _factory = new();

    [Fact]
    public async Task SeedIfEmpty_FillsTheDashboardAtStartup()
    {
        using WebApplicationFactory<Program> seeded =
            _factory.WithWebHostBuilder(builder => builder.UseSetting("Demo:SeedIfEmpty", "true"));

        int count = await CountLastThreeMonthsAsync(seeded.CreateClient());

        Assert.True(count > 0);
    }

    [Fact]
    public async Task WithoutTheSetting_NothingIsSeeded()
    {
        int count = await CountLastThreeMonthsAsync(_factory.CreateClient());

        Assert.Equal(0, count);
    }

    private static async Task<int> CountLastThreeMonthsAsync(HttpClient client)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        DateOnly from = new DateOnly(today.Year, today.Month, 1).AddMonths(-2);
        string json = await client.GetStringAsync($"/api/v1/transactions?from={from:yyyy-MM-dd}&to={today:yyyy-MM-dd}");
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetArrayLength();
    }

    public void Dispose() => _factory.Dispose();
}
