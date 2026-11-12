using CashStack.Api.Middleware;
using CashStack.Tests.Support;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CashStack.Tests.Middleware;

public class RequestTimingMiddlewareTests
{
    private readonly ListLogger<RequestTimingMiddleware> _logger = new();

    [Fact]
    public async Task Request_IsLoggedWithMethodPathAndStatus()
    {
        await InvokeAsync("/api/v1/categories");

        string message = Assert.Single(_logger.Messages);
        Assert.StartsWith("GET /api/v1/categories -> 200", message);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/HEALTH")]
    public async Task HealthCheck_IsNotLogged(string path)
    {
        bool nextCalled = await InvokeAsync(path);

        Assert.True(nextCalled);
        Assert.Empty(_logger.Messages);
    }

    private async Task<bool> InvokeAsync(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = path;
        bool nextCalled = false;

        await new RequestTimingMiddleware(_logger).InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        return nextCalled;
    }
}
