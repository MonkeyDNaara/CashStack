using System.Text.Json;
using System.Globalization;
using System.Text.Json.Serialization;
using CashStack.Models;

namespace CashStack.Services;

public class StorageService
{
    private const string DateFormat = "yyyy-MM-dd";

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly string _dataDirectory;

    public StorageService(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        _dataDirectory = dataDirectory;
    }

    // Main methods
    public void Save(Transaction transaction)
    {
        DateOnly day = DateOnly.FromDateTime(transaction.Timestamp.DateTime);
        string path = GetFilePath(day);
        List<Transaction> transactions = ReadFile(path);
        transactions.Add(transaction);
        WriteFile(path, transactions);
    }

    public List<Transaction> Load(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentException("From-date is after to-date", nameof(from));
        }
        return Directory.GetFiles(_dataDirectory, "*.json")
            .Where(path => IsInRange(path, from, to))
            .SelectMany(ReadFile)
            .OrderBy(t => t.Timestamp)
            .ToList();
    }

    public bool Remove(Guid id)
    {
        foreach (var path in GetDayFiles())
        {
            List<Transaction> transactions = ReadFile(path);
            if (transactions.RemoveAll(t => t.Id == id) > 0)
            {
                WriteFile(path, transactions);
                return true;
            }
        }
        return false;
    }

    // Helper functions
    private static bool IsInRange(string path, DateOnly from, DateOnly to)
    {
        if (!TryGetDay(path, out DateOnly date))
        {
            return false;
        }
        return date >= from && date <= to;
    }

    private IEnumerable<string> GetDayFiles()
    {
        return Directory.GetFiles(_dataDirectory, "*.json")
            .Where(path => TryGetDay(path, out _));
    }

    private static bool TryGetDay(string path, out DateOnly day)
    {
        string filename = Path.GetFileNameWithoutExtension(path);
        return DateOnly.TryParseExact(filename, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
    }

    private string GetFilePath(DateOnly day)
    {
        string filename = day.ToString(DateFormat, CultureInfo.InvariantCulture);
        string fullPath = Path.Combine(_dataDirectory, $"{filename}.json");
        return fullPath;
    }

    private static List<Transaction> ReadFile(string path)
    {
        if (!File.Exists(path)) return [];
        string jsonIn = File.ReadAllText(path);
        List<Transaction> loaded = JsonSerializer.Deserialize<List<Transaction>>(jsonIn, _options) ?? [];
        return loaded;
    }

    private static void WriteFile(string path, List<Transaction> transactions)
    {
        string jsonPretty = JsonSerializer.Serialize(transactions, _options);
        File.WriteAllText(path, jsonPretty);
    }
}