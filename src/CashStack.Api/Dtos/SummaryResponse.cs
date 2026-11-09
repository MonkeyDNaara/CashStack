using CashStack.Models;

namespace CashStack.Api.Dtos;

public record SummaryResponse(DateOnly From, DateOnly To, decimal Income, decimal Expenses, decimal Balance)
{
    public static SummaryResponse Create(DateOnly from, DateOnly to, TransactionSummary summary) =>
        new(from, to, summary.Income, summary.Expenses, summary.Balance);
}
