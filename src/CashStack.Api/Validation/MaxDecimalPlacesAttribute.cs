using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CashStack.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class MaxDecimalPlacesAttribute : ValidationAttribute
{
    public int Places { get; }

    public MaxDecimalPlacesAttribute(int places)
        : base("The field {0} must not have more than {1} decimal places.")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(places);
        Places = places;
    }

    public override bool IsValid(object? value)
    {
        return value switch
        {
            null => true,
            decimal amount => decimal.Round(amount, Places) == amount,
            _ => throw new InvalidOperationException(
                $"{nameof(MaxDecimalPlacesAttribute)} can only be used on decimal values.")
        };
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Places);
    }
}
