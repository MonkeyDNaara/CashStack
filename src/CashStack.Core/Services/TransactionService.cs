using CashStack.Events;
using CashStack.Exceptions;
using CashStack.Models;
using CashStack.Repositories;

namespace CashStack.Services;

public class TransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly ICategoryRepository _categories;
    private readonly TimeProvider _time;
    public event EventHandler<TransactionAddedEventArgs>? TransactionAdded;

    // Depends on the interface only: the caller decides which storage is used.
    // TimeProvider instead of DateTimeOffset.Now, so tests can fix "now" and the time zone.
    public TransactionService(
        ITransactionRepository repository, ICategoryRepository categories, TimeProvider? timeProvider = null)
    {
        _repository = repository;
        _categories = categories;
        _time = timeProvider ?? TimeProvider.System;
    }

    public async Task<Transaction> AddAsync(
        TransactionType type, string description, decimal amount, Guid? categoryId = null, DateOnly? date = null,
        CancellationToken ct = default)
    {
        DateTimeOffset timestamp = CreateTimestamp(date);
        // No category chosen → "Other" of the type, so callers without category selection keep working
        Category category = await GetMatchingCategoryAsync(categoryId ?? DefaultCategories.GetOtherId(type), type, ct);

        var transaction = new Transaction(
            Guid.NewGuid(),
            timestamp,
            type,
            category.Id,
            description,
            amount
        );

        await _repository.AddAsync(transaction, ct);
        TransactionAdded?.Invoke(this, new TransactionAddedEventArgs(transaction));

        return transaction;
    }

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetByIdAsync(id, ct);

    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default) =>
        _repository.RemoveAsync(id, ct);

    public Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        DateOnly from, DateOnly to, TransactionType? type = null, Guid? categoryId = null, CancellationToken ct = default)
    {
        EnsureValidRange(from, to);
        return _repository.GetRangeAsync(from, to, type, categoryId, ct);
    }

    public async Task<TransactionSummary> GetSummaryAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        IReadOnlyList<Transaction> transactions = await GetTransactionsAsync(from, to, ct: ct);
        decimal income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        decimal expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        // Names come from the category repository, the totals from GroupBy over the period
        Dictionary<Guid, string> names = (await _categories.GetAllAsync(ct: ct)).ToDictionary(c => c.Id, c => c.Name);
        List<CategoryTotal> categories = transactions
            .GroupBy(t => new { t.CategoryId, t.Type })
            .Select(g => new CategoryTotal(
                g.Key.CategoryId,
                names.GetValueOrDefault(g.Key.CategoryId, "Unknown"),
                g.Key.Type,
                g.Sum(t => t.Amount)))
            .OrderBy(c => c.Type)
            .ThenByDescending(c => c.Total)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new TransactionSummary(income, expenses, categories);
    }

    // Business rule: a transaction can only use an existing category of the same type
    private async Task<Category> GetMatchingCategoryAsync(Guid categoryId, TransactionType type, CancellationToken ct)
    {
        Category category = await _categories.GetByIdAsync(categoryId, ct)
            ?? throw new BusinessRuleException($"Category '{categoryId}' does not exist.");

        if (category.Type != type)
        {
            throw new BusinessRuleException(
                $"Category '{category.Name}' belongs to {category.Type} and cannot be used for {type}.");
        }

        return category;
    }

    // Business rule: transactions can be booked for today or a past day, never for the future.
    // A past day gets the current time of day, so several entries of one day keep their order.
    private DateTimeOffset CreateTimestamp(DateOnly? date)
    {
        DateTimeOffset now = _time.GetLocalNow();
        DateOnly today = DateOnly.FromDateTime(now.DateTime);

        if (date is null || date == today)
        {
            return now;
        }

        if (date > today)
        {
            throw new BusinessRuleException($"The date {date:yyyy-MM-dd} must not be in the future.");
        }

        DateTime local = date.Value.ToDateTime(TimeOnly.FromDateTime(now.DateTime));
        // Offset of that day, not of today: a day in summer is +02:00, a day in winter +01:00
        return new DateTimeOffset(local, _time.LocalTimeZone.GetUtcOffset(local));
    }

    // Business rule, checked once here instead of in every repository implementation
    private static void EnsureValidRange(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentException("From-date is after to-date", nameof(from));
        }
    }
}
