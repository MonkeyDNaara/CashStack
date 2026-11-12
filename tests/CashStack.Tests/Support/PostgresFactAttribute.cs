using Xunit;

namespace CashStack.Tests.Support;

// Runs the test only when a Postgres server is configured, otherwise it is reported as skipped.
// Locally: export CASHSTACK_TEST_POSTGRES="Host=localhost;Username=postgres;Password=postgres"
public sealed class PostgresFactAttribute : FactAttribute
{
    public const string EnvironmentVariable = "CASHSTACK_TEST_POSTGRES";

    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvironmentVariable)))
        {
            Skip = $"Set {EnvironmentVariable} to a Postgres connection string to run this test.";
        }
    }

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable(EnvironmentVariable)
        ?? throw new InvalidOperationException($"{EnvironmentVariable} is not set.");
}
