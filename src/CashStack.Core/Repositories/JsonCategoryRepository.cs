using System.Text.Json;
using CashStack.Exceptions;
using CashStack.Models;

namespace CashStack.Repositories;

// Stores all categories in one file: data/categories.json
// The file is created with the starter set on first access (JSON counterpart of the EF Core seed)
public class JsonCategoryRepository : ICategoryRepository
{
    private const string FileName = "categories.json";
    private readonly string _filePath;

    public JsonCategoryRepository(string dataDirectory)
    {
        try
        {
            Directory.CreateDirectory(dataDirectory);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Could not create directory '{dataDirectory}'", ex);
        }
        _filePath = Path.Combine(dataDirectory, FileName);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(TransactionType? type = null, CancellationToken ct = default)
    {
        List<Category> categories = await ReadFileAsync(ct);
        return categories
            .Where(c => type is null || c.Type == type)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        List<Category> categories = await ReadFileAsync(ct);
        return categories.Find(c => c.Id == id);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        List<Category> categories = await ReadFileAsync(ct);
        categories.Add(category);
        await WriteFileAsync(categories, ct);
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        List<Category> categories = await ReadFileAsync(ct);
        int index = categories.FindIndex(c => c.Id == category.Id);
        if (index < 0)
        {
            return;
        }
        categories[index] = category;
        await WriteFileAsync(categories, ct);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        List<Category> categories = await ReadFileAsync(ct);
        if (categories.RemoveAll(c => c.Id == id) == 0)
        {
            return false;
        }
        await WriteFileAsync(categories, ct);
        return true;
    }

    // Helper functions
    private async Task<List<Category>> ReadFileAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath))
        {
            List<Category> starterSet = DefaultCategories.CreateStarterSet().ToList();
            await WriteFileAsync(starterSet, ct);
            return starterSet;
        }

        try
        {
            string jsonIn = await File.ReadAllTextAsync(_filePath, ct);
            return JsonSerializer.Deserialize<List<Category>>(jsonIn, JsonStorage.Options) ?? [];
        }
        // ArgumentException: the Category constructor rejects invalid names in the file
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            throw new StorageException($"Could not read '{_filePath}'", ex);
        }
    }

    private async Task WriteFileAsync(List<Category> categories, CancellationToken ct)
    {
        try
        {
            string jsonPretty = JsonSerializer.Serialize(categories, JsonStorage.Options);
            await File.WriteAllTextAsync(_filePath, jsonPretty, ct);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Could not write '{_filePath}'", ex);
        }
    }
}
