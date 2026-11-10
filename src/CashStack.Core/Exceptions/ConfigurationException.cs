namespace CashStack.Exceptions;

// Invalid application settings (e.g. unknown storage provider) → the app refuses to start
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
}
