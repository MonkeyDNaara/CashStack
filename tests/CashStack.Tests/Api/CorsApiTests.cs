using System.Net;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Api;

public class CorsApiTests : ApiTestBase
{
    private const string FrontendOrigin = "http://localhost:5173";

    [Fact]
    public async Task Preflight_FromTheFrontend_IsAllowed()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/categories");
        request.Headers.Add("Origin", FrontendOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");

        HttpResponseMessage response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(FrontendOrigin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Fact]
    public async Task Request_FromAnotherOrigin_GetsNoCorsHeaders()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
        request.Headers.Add("Origin", "http://evil.example");

        HttpResponseMessage response = await Client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Created_ExposesTheLocationHeader()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/transactions")
        {
            Content = JsonContent(new { type = "Expense", categoryId = TestData.Groceries, description = "x", amount = 1m })
        };
        request.Headers.Add("Origin", FrontendOrigin);

        HttpResponseMessage response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("Location", response.Headers.GetValues("Access-Control-Expose-Headers"));
    }

    [Fact]
    public async Task ErrorResponse_IsStillReadableByTheFrontend()
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/categories/{CashStack.Models.DefaultCategories.OtherExpenseId}");
        request.Headers.Add("Origin", FrontendOrigin);

        HttpResponseMessage response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(FrontendOrigin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    private static HttpContent JsonContent(object body) =>
        System.Net.Http.Json.JsonContent.Create(body, options: Json);
}
