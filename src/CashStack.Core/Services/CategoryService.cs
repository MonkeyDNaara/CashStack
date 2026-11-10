using CashStack.Exceptions;
using CashStack.Models;
using CashStack.Repositories;

namespace CashStack.Services;

// Business rules for categories; the repositories only store data
public class CategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;

    public CategoryService(ICategoryRepository categories, ITransactionRepository transactions)
    {
        _categories = categories;
        _transactions = transactions;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(TransactionType? type = null, CancellationToken ct = default) =>
        _categories.GetAllAsync(type, ct);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _categories.GetByIdAsync(id, ct);

    public async Task<Category> CreateAsync(string name, TransactionType type, CancellationToken ct = default)
    {
        // The constructor validates and trims the name before the uniqueness check
        var category = new Category(Guid.NewGuid(), name, type);
        await EnsureNameIsUniqueAsync(category.Name, type, excludeId: null, ct);

        await _categories.AddAsync(category, ct);
        return category;
    }

    // Returns null if the category does not exist
    public async Task<Category?> RenameAsync(Guid id, string newName, CancellationToken ct = default)
    {
        Category? category = await _categories.GetByIdAsync(id, ct);
        if (category is null)
        {
            return null;
        }

        EnsureNotProtected(category);
        // Check before Rename, so the object is never changed when a rule fails.
        // Exclude the category itself, so "rent" → "Rent" is allowed
        await EnsureNameIsUniqueAsync(newName.Trim(), category.Type, excludeId: id, ct);
        category.Rename(newName);

        await _categories.UpdateAsync(category, ct);
        return category;
    }

    // Returns false if the category does not exist
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Category? category = await _categories.GetByIdAsync(id, ct);
        if (category is null)
        {
            return false;
        }

        EnsureNotProtected(category);
        if (await _transactions.AnyWithCategoryAsync(id, ct))
        {
            throw new ConflictException($"Category '{category.Name}' is used by transactions and cannot be deleted.");
        }

        return await _categories.RemoveAsync(id, ct);
    }

    // Helper functions
    private static void EnsureNotProtected(Category category)
    {
        if (category.IsProtected)
        {
            throw new ConflictException($"Category '{category.Name}' is protected and cannot be changed or deleted.");
        }
    }

    private async Task EnsureNameIsUniqueAsync(string name, TransactionType type, Guid? excludeId, CancellationToken ct)
    {
        IReadOnlyList<Category> sameType = await _categories.GetAllAsync(type, ct);
        bool exists = sameType.Any(c =>
            c.Id != excludeId && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            throw new ConflictException($"An {type} category named '{name}' already exists.");
        }
    }
}
