using CashStack.Models;
using CashStack.Events;
using System.Globalization;

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

    public void OnTransactionAdded(object? sender, TransactionAddedEventArgs e)
    {
        Transaction t = e.Transaction;

        string timestamp = t.Timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
        string amount = t.Amount.ToString("F2", CultureInfo.InvariantCulture);

        string line = $"{timestamp} | ADDED | {t.Type,-7} | {amount,10} | {t.Description} | {t.Id}";

        try
        {
            File.AppendAllText(_logFilePath, line + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Warning: Could not write log: {ex.Message}");
        }
    }
}