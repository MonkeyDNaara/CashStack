using CashStack.Models;

namespace CashStack.Api.Dtos;

public record SummaryResponse(
    DateOnly From, DateOnly To, decimal Income, decimal Expenses, decimal Balance, List<CategoryTotalResponse> Categories)
{
    public static SummaryResponse Create(DateOnly from, DateOnly to, TransactionSummary summary) =>
        new(from, to, summary.Income, summary.Expenses, summary.Balance,
            summary.Categories.Select(CategoryTotalResponse.From).ToList());
}

// One entry per category with transactions in the period, ready for a chart
public record CategoryTotalResponse(Guid CategoryId, string Name, TransactionType Type, decimal Total)
{
    public static CategoryTotalResponse From(CategoryTotal c) => new(c.CategoryId, c.Name, c.Type, c.Total);
}
