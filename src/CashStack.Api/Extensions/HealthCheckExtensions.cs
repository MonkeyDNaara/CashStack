namespace CashStack.Api.Extensions;

public static class HealthCheckExtensions
{
    // Liveness for the platform (Render) and the front end's wake-up check
    public const string Path = "/health";

    // No checks registered on purpose: "Healthy" only means the process answers.
    // A database check here would wake the database on every platform ping.
    public static IServiceCollection AddCashStackHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }

    public static IEndpointRouteBuilder MapCashStackHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(Path);
        return endpoints;
    }
}
