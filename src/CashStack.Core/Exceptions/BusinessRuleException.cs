namespace CashStack.Exceptions;

// The request breaks a business rule, e.g. unknown category or category of the wrong type (→ 400)
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
}
