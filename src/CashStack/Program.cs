using CashStack.Services;
using CashStack.Models;

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
            Console.WriteLine("Coming soon.");
            break;
        case "2":
            Console.WriteLine("Coming soon.");
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
