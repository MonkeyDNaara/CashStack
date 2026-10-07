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