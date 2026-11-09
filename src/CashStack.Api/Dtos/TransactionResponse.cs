using CashStack.Models;

namespace CashStack.Api.Dtos;

public record TransactionResponse(Guid Id, DateTimeOffset Timestamp, TransactionType Type, string Description, decimal Amount)
{
    public static TransactionResponse From(Transaction t) => new(t.Id, t.Timestamp, t.Type, t.Description, t.Amount);
}