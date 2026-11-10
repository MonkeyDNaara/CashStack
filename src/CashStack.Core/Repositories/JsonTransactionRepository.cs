using System.Globalization;
using System.Text.Json;
using CashStack.Exceptions;
using CashStack.Models;

namespace CashStack.Repositories;

// Stores transactions in one JSON file per day: data/yyyy-MM-dd.json
public class JsonTransactionRepository : ITransactionRepository
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly string _dataDirectory;

    public JsonTransactionRepository(string dataDirectory)
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

    // Interface methods
    public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
    {
        DateOnly day = DateOnly.FromDateTime(transaction.Timestamp.DateTime);
        string path = GetFilePath(day);
        List<Transaction> transactions = await ReadFileAsync(path, ct);
        transactions.Add(transaction);
        await WriteFileAsync(path, transactions, ct);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        foreach (string path in GetDayFiles())
        {
            List<Transaction> transactions = await ReadFileAsync(path, ct);
            Transaction? match = transactions.Find(t => t.Id == id);
            if (match is not null)
            {
                return match;
            }
        }
        return null;
    }

    public async Task<IReadOnlyList<Transaction>> GetRangeAsync(
        DateOnly from, DateOnly to, TransactionType? type = null, Guid? categoryId = null, CancellationToken ct = default)
    {
        var result = new List<Transaction>();
        foreach (string path in GetJsonFiles().Where(path => IsInRange(path, from, to)))
        {
            result.AddRange(await ReadFileAsync(path, ct));
        }
        return result
            .Where(t => type is null || t.Type == type)
            .Where(t => categoryId is null || t.CategoryId == categoryId)
            .OrderBy(t => t.Timestamp)
            .ToList();
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        foreach (string path in GetDayFiles())
        {
            List<Transaction> transactions = await ReadFileAsync(path, ct);
            if (transactions.RemoveAll(t => t.Id == id) > 0)
            {
                await WriteFileAsync(path, transactions, ct);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> AnyWithCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        foreach (string path in GetDayFiles())
        {
            List<Transaction> transactions = await ReadFileAsync(path, ct);
            if (transactions.Exists(t => t.CategoryId == categoryId))
            {
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
        return Path.Combine(_dataDirectory, $"{filename}.json");
    }

    private static async Task<List<Transaction>> ReadFileAsync(string path, CancellationToken ct)
    {
        if (!File.Exists(path)) return [];
        try
        {
            string jsonIn = await File.ReadAllTextAsync(path, ct);
            List<Transaction> transactions = JsonSerializer.Deserialize<List<Transaction>>(jsonIn, JsonStorage.Options) ?? [];
            return transactions.ConvertAll(WithDefaultCategory);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            throw new StorageException($"Could not read '{path}'", ex);
        }
    }

    // Day files written before categories existed have no categoryId (→ Guid.Empty):
    // they belong to "Other" of their type
    private static Transaction WithDefaultCategory(Transaction t)
    {
        if (t.CategoryId != Guid.Empty)
        {
            return t;
        }
        return new Transaction(t.Id, t.Timestamp, t.Type, DefaultCategories.GetOtherId(t.Type), t.Description, t.Amount);
    }

    private static async Task WriteFileAsync(string path, List<Transaction> transactions, CancellationToken ct)
    {
        try
        {
            string jsonPretty = JsonSerializer.Serialize(transactions, JsonStorage.Options);
            await File.WriteAllTextAsync(path, jsonPretty, ct);
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
