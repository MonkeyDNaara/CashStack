namespace CashStack.Services;

public class LoggerService
{
    private readonly string _logFilePath;
    const string LogFileName = "transactions.log";

    public LoggerService(string logDirectory)
    {
        _logFilePath = Path.Combine(logDirectory, LogFileName);    
        
        if (!Directory.Exists(logDirectory)){
            Directory.CreateDirectory(logDirectory);
        }
    }
    }