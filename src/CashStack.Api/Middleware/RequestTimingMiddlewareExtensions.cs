namespace CashStack.Api.Middleware;

public static class RequestTimingMiddlewareExtensions
{
    public static IServiceCollection AddRequestTiming(this IServiceCollection services)
    {
        return services.AddTransient<RequestTimingMiddleware>();
    }

    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestTimingMiddleware>();
    }
}
