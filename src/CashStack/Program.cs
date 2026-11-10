using System.Globalization;
using CashStack.Services;
using CashStack.Models;
using CashStack.Exceptions;
using CashStack.Repositories;
using CashStack.Configuration;
using CashStack.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

const string DateFormat = "yyyy-MM-dd";

// Same keys as the API: Storage:Provider, Storage:DataDirectory, Storage:LogDirectory, ConnectionStrings:CashStack
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

ITransactionRepository repository;
ICategoryRepository categoryRepository;
// One DbContext for the whole session: the console has a single user and no parallel requests
CashStackDbContext? dbContext = null;
try
{
    StorageSettings settings = StorageSettings.FromConfiguration(configuration);
    if (settings.Provider == StorageProvider.Sqlite)
    {
        DbContextOptions<CashStackDbContext> dbOptions = new DbContextOptionsBuilder<CashStackDbContext>()
            .UseSqlite(settings.ConnectionString)
            .Options;
        dbContext = new CashStackDbContext(dbOptions);
        await DatabaseInitializer.InitializeAsync(dbContext);

        repository = new EfTransactionRepository(dbContext);
        categoryRepository = new EfCategoryRepository(dbContext);
    }
    else
    {
        repository = new JsonTransactionRepository(settings.DataDirectory);
        categoryRepository = new JsonCategoryRepository(settings.DataDirectory);
    }
    Console.WriteLine($"Storage: {settings.Provider}");
}
catch (StorageException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message} -> {ex.InnerException?.Message}");
    Environment.ExitCode = 1;
    return;
}
catch (ConfigurationException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.ExitCode = 1;
    return;
}
var transactions = new TransactionService(repository, categoryRepository);
try
{
    var logger = new LoggerService(configuration["Storage:LogDirectory"] ?? "logs");
    transactions.TransactionAdded += logger.OnTransactionAdded;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Warning: {ex.Message}");
}

bool running = true;
while (running)
{
    PrintMenu();
    string choice = Prompt("Choose an option: ");

    try
    {
        switch (choice)
        {
            case "1":
                await AddTransactionAsync();
                break;
            case "2":
                await RemoveTransactionAsync();
                break;
            case "3":
                await ShowReportAsync();
                break;
            case "0":
                running = false;
                break;
            default:
                Console.WriteLine("Invalid option. Please enter a number from 0 to 3.");
                break;
        }
    }
    catch (StorageException ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message} -> {ex.InnerException?.Message}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Unexpected Error: {ex.Message}: {ex.GetType().Name}");
    }
}

Console.WriteLine("Goodbye!");

if (dbContext is not null)
{
    await dbContext.DisposeAsync();
}

// ---------- Menu ----------

void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("===== CashStack =====");
    Console.WriteLine("1) Add transaction");
    Console.WriteLine("2) Remove transaction");
    Console.WriteLine("3) Report");
    Console.WriteLine("0) Exit");
}

// ---------- Flows ----------

async Task AddTransactionAsync()
{
    PrintHeader("Add transaction");

    TransactionType type = ReadType("Type (i = income, e = expense): ");
    string description = ReadText("Description: ");
    decimal amount = ReadAmount("Amount: ");

    Transaction transaction = await transactions.AddAsync(type, description, amount);
    Console.WriteLine($"Added: {FormatTransaction(transaction)}");
}

async Task RemoveTransactionAsync()
{
    PrintHeader("Remove transaction");

    var (from, to) = ReadDateRange();
    IReadOnlyList<Transaction> list = await transactions.GetTransactionsAsync(from, to);

    if (list.Count == 0)
    {
        Console.WriteLine("No transactions found in this period.");
        return;
    }

    Console.WriteLine();
    PrintTransactions(list);

    int number = ReadNumber("Number to remove (0 = cancel): ", 0, list.Count);
    if (number == 0)
    {
        Console.WriteLine("Cancelled.");
        return;
    }

    Transaction selected = list[number - 1];
    if (!ReadYesNo($"Remove \"{selected.Description}\" ({FormatSignedAmount(selected)})?"))
    {
        Console.WriteLine("Cancelled.");
        return;
    }

    bool removed = await transactions.RemoveAsync(selected.Id);
    Console.WriteLine(removed ? "Transaction removed." : "Transaction not found. It may have been removed already.");
}

async Task ShowReportAsync()
{
    PrintHeader("Report");

    var (from, to) = ReadDateRange();
    TransactionType? typeFilter = ReadTypeFilter("Type (i = income, e = expense, Enter = all): ");

    IReadOnlyList<Transaction> list = await transactions.GetTransactionsAsync(from, to, typeFilter);

    string filterText = typeFilter?.ToString() ?? "All types";
    Console.WriteLine();
    Console.WriteLine($"{FormatDate(from)} to {FormatDate(to)} | {filterText}");

    if (list.Count == 0)
    {
        Console.WriteLine("No transactions found.");
    }
    else
    {
        PrintTransactions(list);
    }

    TransactionSummary summary = await transactions.GetSummaryAsync(from, to);
    Console.WriteLine();
    Console.WriteLine("Summary for this period (all types):");
    Console.WriteLine($"  Income:   {FormatAmount(summary.Income),12}");
    Console.WriteLine($"  Expenses: {FormatAmount(summary.Expenses),12}");
    Console.WriteLine($"  Balance:  {FormatAmount(summary.Balance),12}");
}

// ---------- Output helpers ----------

void PrintHeader(string title)
{
    Console.WriteLine();
    Console.WriteLine($"--- {title} ---");
}

void PrintTransactions(IReadOnlyList<Transaction> list)
{
    for (int i = 0; i < list.Count; i++)
    {
        Console.WriteLine($"{i + 1,3}) {FormatTransaction(list[i])}");
    }
}

string FormatTransaction(Transaction t)
{
    string timestamp = t.Timestamp.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    return $"{timestamp} | {t.Type,-7} | {FormatSignedAmount(t),10} | {t.Description}";
}

string FormatSignedAmount(Transaction t)
{
    string sign = t.Type == TransactionType.Expense ? "-" : "+";
    return sign + FormatAmount(t.Amount);
}

string FormatAmount(decimal amount) => amount.ToString("F2", CultureInfo.InvariantCulture);

string FormatDate(DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

// ---------- Input helpers (retry until the input is valid) ----------

string Prompt(string message)
{
    Console.Write(message);
    string? input = Console.ReadLine();

    if (input is null)
    {
        // End of input (Ctrl+D): exit instead of looping forever.
        Console.WriteLine();
        Environment.Exit(0);
    }

    return input.Trim();
}

string ReadText(string message)
{
    while (true)
    {
        string input = Prompt(message);
        if (input.Length > 0)
        {
            return input;
        }

        Console.WriteLine("  Input must not be empty.");
    }
}

decimal ReadAmount(string message)
{
    while (true)
    {
        // Accept "42,50" and "42.50" (German and English style).
        string input = Prompt(message).Replace(',', '.');

        if (decimal.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount)
            && amount > 0
            && decimal.Round(amount, 2) == amount)
        {
            return amount;
        }

        Console.WriteLine("  Please enter a positive amount with max. 2 decimals, e.g. 42.50");
    }
}

TransactionType? ParseType(string input) => input.ToLowerInvariant() switch
{
    "i" or "income" => TransactionType.Income,
    "e" or "expense" => TransactionType.Expense,
    _ => null
};

TransactionType ReadType(string message)
{
    while (true)
    {
        TransactionType? type = ParseType(Prompt(message));
        if (type is not null)
        {
            return type.Value;
        }

        Console.WriteLine("  Please enter 'i' (income) or 'e' (expense).");
    }
}

TransactionType? ReadTypeFilter(string message)
{
    while (true)
    {
        string input = Prompt(message);
        if (input.Length == 0)
        {
            return null;
        }

        TransactionType? type = ParseType(input);
        if (type is not null)
        {
            return type;
        }

        Console.WriteLine("  Please enter 'i', 'e' or press Enter for all.");
    }
}

DateOnly ReadDate(string message, DateOnly defaultValue)
{
    while (true)
    {
        string input = Prompt($"{message} [{FormatDate(defaultValue)}]: ");
        if (input.Length == 0)
        {
            return defaultValue;
        }

        if (DateOnly.TryParseExact(input, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            return date;
        }

        Console.WriteLine($"  Please use the format {DateFormat}, e.g. 2026-10-10.");
    }
}

(DateOnly From, DateOnly To) ReadDateRange()
{
    DateOnly today = DateOnly.FromDateTime(DateTime.Today);
    DateOnly from = ReadDate("From", new DateOnly(today.Year, today.Month, 1));

    while (true)
    {
        DateOnly to = ReadDate("To  ", today);
        if (to >= from)
        {
            return (from, to);
        }

        Console.WriteLine("  'To' must not be before 'From'.");
    }
}

int ReadNumber(string message, int min, int max)
{
    while (true)
    {
        if (int.TryParse(Prompt(message), out int number) && number >= min && number <= max)
        {
            return number;
        }

        Console.WriteLine($"  Please enter a number from {min} to {max}.");
    }
}

bool ReadYesNo(string message)
{
    while (true)
    {
        string input = Prompt($"{message} (y/n): ").ToLowerInvariant();
        if (input is "y" or "yes")
        {
            return true;
        }

        if (input is "n" or "no")
        {
            return false;
        }

        Console.WriteLine("  Please enter 'y' or 'n'.");
    }
}
