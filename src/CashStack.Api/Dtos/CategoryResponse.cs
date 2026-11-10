using CashStack.Models;

namespace CashStack.Api.Dtos;

// IsProtected lets the front end hide rename/delete for the "Other" categories
public record CategoryResponse(Guid Id, string Name, TransactionType Type, bool IsProtected)
{
    public static CategoryResponse From(Category c) => new(c.Id, c.Name, c.Type, c.IsProtected);
}
