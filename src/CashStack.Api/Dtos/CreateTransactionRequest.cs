using CashStack.Models;

namespace CashStack.Api.Dtos;

public class CreateTransactionRequest
{
    public TransactionType Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}