namespace CashStack.Api.Extensions;

public static class FrontendCorsExtensions
{
    public const string PolicyName = "Frontend";
    private const string DefaultOrigin = "http://localhost:5173"; // Vite dev server

    // Allowed origins come from Cors:AllowedOrigins, so a new port needs no code change
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        string[] origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [DefaultOrigin];

        return services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type")
            // Browsers hide response headers by default; the front end reads the id of created resources from Location
            .WithExposedHeaders("Location")));
    }

    public static IApplicationBuilder UseFrontendCors(this IApplicationBuilder app) => app.UseCors(PolicyName);
}
