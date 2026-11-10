namespace CashStack.Exceptions;

// Base class for violations of our own business rules.
// The message is written by us (no internals), so it may be shown to the client.
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
