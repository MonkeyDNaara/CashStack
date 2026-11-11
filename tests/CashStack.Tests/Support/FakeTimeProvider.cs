namespace CashStack.Tests.Support;

// Fixed clock in a fixed time zone, so "now" and summer/winter offsets are predictable.
// (Hand-written instead of Microsoft.Extensions.TimeProvider.Testing: one class, no extra package.)
public sealed class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTimeOffset now, string timeZoneId = "Europe/Berlin")
    {
        _utcNow = now.ToUniversalTime();
        LocalTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    public override TimeZoneInfo LocalTimeZone { get; }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
