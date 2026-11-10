using Microsoft.AspNetCore.Mvc;

namespace CashStack.Api.Dtos;

// Query parameters ?from=yyyy-MM-dd&to=yyyy-MM-dd with their defaults in one place
public class DateRangeQuery
{
    [FromQuery(Name = "from")]
    public DateOnly? From { get; init; }

    [FromQuery(Name = "to")]
    public DateOnly? To { get; init; }

    // Computed (get-only), so model binding ignores them
    public DateOnly End => To ?? DateOnly.FromDateTime(DateTime.Now);
    public DateOnly Start => From ?? new DateOnly(End.Year, End.Month, 1);
}
