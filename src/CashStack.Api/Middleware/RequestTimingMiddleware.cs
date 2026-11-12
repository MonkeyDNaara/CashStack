using System.Diagnostics;
using CashStack.Api.Extensions;

namespace CashStack.Api.Middleware;

public class RequestTimingMiddleware : IMiddleware
{
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(ILogger<RequestTimingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // The platform pings the health check every few seconds: logging it would bury the real requests
        if (context.Request.Path.StartsWithSegments(HealthCheckExtensions.Path))
        {
            await next(context);
            return;
        }

        long start = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        catch (Exception)
        {
            // Unhandled exception: the response status is not set yet, so log 500 and pass the exception on
            LogRequest(context, StatusCodes.Status500InternalServerError, start);
            throw;
        }
        LogRequest(context, context.Response.StatusCode, start);
    }

    private void LogRequest(HttpContext context, int statusCode, long start)
    {
        TimeSpan elapsed = Stopwatch.GetElapsedTime(start);
        _logger.LogInformation(
            "{Method} {Path} -> {StatusCode} in {ElapsedMs:F1} ms",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            elapsed.TotalMilliseconds);
    }
}
