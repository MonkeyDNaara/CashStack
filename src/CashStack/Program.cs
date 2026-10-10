using System.Globalization;
using CashStack.Services;
using CashStack.Models;

const string DateFormat = "yyyy-MM-dd";

var storage = new StorageService("data");
var transactions = new TransactionService(storage);
var logger = new LoggerService("logs");
transactions.TransactionAdded += logger.OnTransactionAdded;

bool running = true;
while (running)
{
    PrintMenu();
    string choice = Prompt("Choose an option: ");

    switch (choice)
    {
        case "1":
            AddTransaction();
            break;
        case "2":
            RemoveTransaction();
            break;
        case "3":
            Console.WriteLine("Coming soon.");
            break;
        case "0":
            running = false;
            break;
        default:
            Console.WriteLine("Invalid option. Please enter a number from 0 to 3.");
            break;
    }
}

Console.WriteLine("Goodbye!");

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

void AddTransaction()
{
    PrintHeader("Add transaction");

    TransactionType type = ReadType("Type (i = income, e = expense): ");
    string description = ReadText("Description: ");
    decimal amount = ReadAmount("Amount: ");

    Transaction transaction = transactions.Add(type, description, amount);
    Console.WriteLine($"Added: {FormatTransaction(transaction)}");
}

void RemoveTransaction()
{
    PrintHeader("Remove transaction");

    var (from, to) = ReadDateRange();
    List<Transaction> list = transactions.GetTransactions(from, to)
        .OrderBy(t => t.Timestamp)
        .ToList();

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

    bool removed = transactions.Remove(selected.Id);
    Console.WriteLine(removed ? "Transaction removed." : "Transaction not found. It may have been removed already.");
}

// ---------- Output helpers ----------

void PrintHeader(string title)
{
    Console.WriteLine();
    Console.WriteLine($"--- {title} ---");
}

void PrintTransactions(List<Transaction> list)
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
