using System.ComponentModel.DataAnnotations;
using CashStack.Models;

namespace CashStack.Api.Dtos;

// Only the name can change: a category never switches between Income and Expense
public class RenameCategoryRequest
{
    [Required]
    [StringLength(Category.MaxNameLength)]
    public string Name { get; init; } = string.Empty;
}
