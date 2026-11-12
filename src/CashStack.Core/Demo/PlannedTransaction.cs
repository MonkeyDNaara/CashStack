using CashStack.Models;

namespace CashStack.Demo;

// A demo booking before the category name is resolved to an id
public sealed record PlannedTransaction(
    DateOnly Date,
    TransactionType Type,
    string Category,
    string Description,
    decimal Amount);
