namespace CashStack.Services;

public class LoggerService
{
    private const string LogFileName = "transactions.log";
    private readonly string _logFilePath;

    public LoggerService(string logDirectory)
    {
        _logFilePath = Path.Combine(logDirectory, LogFileName);
        Directory.CreateDirectory(logDirectory);
    }
    }