using System.ComponentModel.DataAnnotations;

namespace CashStack.Api.Validation;

// Rejects days after today with a field error ("date"), so the front end can show it at the date field
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute()
        : base("The field {0} must not be in the future.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not DateOnly date)
        {
            throw new InvalidOperationException($"{nameof(NotInFutureAttribute)} can only be used on DateOnly values.");
        }

        // Same clock as the TransactionService (registered in DI), falls back to the system clock
        TimeProvider time = validationContext.GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
        DateOnly today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);

        return date <= today
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName), MemberNameList(validationContext));
    }

    private static string[]? MemberNameList(ValidationContext context) =>
        context.MemberName is null ? null : [context.MemberName];
}
