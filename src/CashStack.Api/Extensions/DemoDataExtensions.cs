using CashStack.Demo;

namespace CashStack.Api.Extensions;

public static class DemoDataExtensions
{
    // Off by default; the live demo sets Demo__SeedIfEmpty=true
    public const string SeedIfEmptyKey = "Demo:SeedIfEmpty";

    public static IServiceCollection AddDemoData(this IServiceCollection services)
    {
        services.AddScoped<DemoDataSeeder>();
        return services;
    }

    // Runs once at startup, before the first request, so the first visitor sees data.
    // A failed seed is not fatal: the API works without demo data, so it only logs a warning.
    public static async Task SeedDemoDataIfEnabledAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>(SeedIfEmptyKey))
        {
            return;
        }

        try
        {
            using IServiceScope scope = app.Services.CreateScope();
            DemoSeedResult result = await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedIfEmptyAsync();

            if (result.Seeded)
            {
                app.Logger.LogInformation("Seeded {Count} demo transactions (new categories: {Categories})",
                    result.Count, result.CreatedCategories.Count > 0 ? string.Join(", ", result.CreatedCategories) : "none");
            }
            else
            {
                app.Logger.LogInformation("Demo seed skipped: {Count} transactions exist in the last three months", result.Count);
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Demo data could not be seeded. Continuing without it.");
        }
    }
}
