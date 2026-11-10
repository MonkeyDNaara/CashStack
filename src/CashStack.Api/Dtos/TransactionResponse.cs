using CashStack.Models;

namespace CashStack.Api.Dtos;

public record TransactionResponse(
    Guid Id, DateTimeOffset Timestamp, TransactionType Type, Guid CategoryId, string CategoryName, string Description, decimal Amount)
{
    // The category name is looked up by the controller (no navigation property in the domain)
    public static TransactionResponse From(Transaction t, string categoryName) =>
        new(t.Id, t.Timestamp, t.Type, t.CategoryId, categoryName, t.Description, t.Amount);
}
