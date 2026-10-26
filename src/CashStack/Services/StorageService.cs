using System.Text.Json;
using System.Globalization;
using System.Text.Json.Serialization;
using CashStack.Models;
using CashStack.Exceptions;

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
        try
        {
            Directory.CreateDirectory(dataDirectory);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Could not create directory '{dataDirectory}'", ex);
        }
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
        return GetJsonFiles()
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
        return GetJsonFiles()
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
        try
        {
            string jsonIn = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Transaction>>(jsonIn, _options) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Could not read '{path}'", ex);
        }
    }

    private static void WriteFile(string path, List<Transaction> transactions)
    {
        try
        {
            string jsonPretty = JsonSerializer.Serialize(transactions, _options);
            File.WriteAllText(path, jsonPretty);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Could not write '{path}'", ex);
        }
    }

    private string[] GetJsonFiles()
    {
        try
        {
            return Directory.GetFiles(_dataDirectory, "*.json");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Could not read directory '{_dataDirectory}'", ex);
        }
    }

}