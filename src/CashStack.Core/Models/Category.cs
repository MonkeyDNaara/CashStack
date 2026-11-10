using System.Text.Json.Serialization;

namespace CashStack.Models;

public class Category
{
    public const int MaxNameLength = 50;

    public Guid Id { get; }
    public string Name { get; private set; }
    public TransactionType Type { get; }

    // Derived from the Id, so it is never stored
    [JsonIgnore]
    public bool IsProtected => DefaultCategories.IsProtected(Id);

    public Category(Guid id, string name, TransactionType type)
    {
        Id = id;
        Name = ValidateName(name);
        Type = type;
    }

    // Only the name can change: a category never switches between Income and Expense
    public void Rename(string newName)
    {
        Name = ValidateName(newName);
    }

    private static string ValidateName(string name)
    {
        string trimmed = name?.Trim() ?? "";
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Category name must not be empty.", nameof(name));
        }

        if (trimmed.Length > MaxNameLength)
        {
            throw new ArgumentException($"Category name must not be longer than {MaxNameLength} characters.", nameof(name));
        }

        return trimmed;
    }
}
