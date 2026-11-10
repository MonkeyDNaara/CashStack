using System.ComponentModel.DataAnnotations;
using CashStack.Models;

namespace CashStack.Api.Dtos;

public class CreateCategoryRequest
{
    // Required also rejects whitespace-only names; the Category constructor trims the rest
    [Required]
    [StringLength(Category.MaxNameLength)]
    public string Name { get; init; } = string.Empty;

    // Nullable, so a missing "type" fails [Required] instead of defaulting to Income
    [Required]
    public TransactionType? Type { get; init; }
}
