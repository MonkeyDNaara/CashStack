namespace CashStack.Exceptions;

// The request conflicts with the current state, e.g. duplicate name or category in use (→ 409)
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
