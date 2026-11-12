using System.Net;
using Xunit;

namespace CashStack.Tests.Api;

public class HealthApiTests : ApiTestBase
{
    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        HttpResponseMessage response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_IsReadableByTheFrontend()
    {
        // The front end pings /health cross-origin to detect a sleeping server
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:5173");

        HttpResponseMessage response = await Client.SendAsync(request);

        Assert.Equal("http://localhost:5173", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }
}
