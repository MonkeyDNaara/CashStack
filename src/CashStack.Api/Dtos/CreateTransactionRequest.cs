using System.ComponentModel.DataAnnotations;
using CashStack.Api.Validation;
using CashStack.Models;

namespace CashStack.Api.Dtos;

public class CreateTransactionRequest
{
    // Nullable, so a missing "type" stays null and [Required] fails (instead of defaulting to Income)
    [Required]
    public TransactionType? Type { get; init; }

    // Must belong to the same type (checked in TransactionService → 400)
    [Required]
    public Guid? CategoryId { get; init; }

    [Required]
    [StringLength(100)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000",
        ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    [MaxDecimalPlaces(2)]
    public decimal Amount { get; init; }
}
