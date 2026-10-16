# CashStack

A small console budget tracker in **C# / .NET 10**: record income and expenses, remove
entries and get a report with totals for any date range. My first C# project, built
during the WBS Coding School "Software Engineering & AI" program (requirements
FR001–FR016).

## Features

- **Add** income or expenses with a description and amount (accepts `42,50` and `42.50`)
- **Remove** a transaction: pick a date range, choose from a numbered list, confirm
- **Report** for a date range with an optional type filter, plus income, expenses and balance
- **Persistent storage** as human-readable JSON, one file per day
- **Logging:** every added transaction is appended to a log file via a C# event
- **Input validation with retry:** invalid input never crashes the menu

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/MonkeyDNaara/CashStack.git
cd CashStack
dotnet run --project src/CashStack
```

The app creates `data/` and `logs/` in the folder you start it from (both are gitignored).

## Usage

```
===== CashStack =====
1) Add transaction
2) Remove transaction
3) Report
0) Exit
Choose an option: 3

--- Report ---
From [2026-10-01]:
To   [2026-10-16]:
Type (i = income, e = expense, Enter = all):

2026-10-01 to 2026-10-16 | All types
  1) 2026-10-16 16:29 | Expense |     -42.50 | Groceries
  2) 2026-10-16 16:29 | Income  |    +350.00 | Freelance invoice

Summary for this period (all types):
  Income:         350.00
  Expenses:        42.50
  Balance:        307.50
```

Dates use the format `yyyy-MM-dd`. Press Enter to accept the default shown in brackets.

## Data format

```
data/
├── 2026-10-15.json     # one file per day (date of the transaction)
└── 2026-10-16.json
logs/
└── transactions.log
```

A day file is a JSON array of transactions:

```json
[
  {
    "id": "3f2b8c1e-6a4d-4e1b-9c2f-0d8e7a5b1c34",
    "timestamp": "2026-10-16T16:29:22.123+02:00",
    "type": "Expense",
    "description": "Groceries",
    "amount": 42.50
  }
]
```

Each added transaction also writes one line to `logs/transactions.log`:

```
2026-10-16 16:29:22 +02:00 | ADDED | Expense |      42.50 | Groceries | 3f2b8c1e-...
```

## Architecture

```
src/CashStack/
├── Program.cs                 # menu loop, flows, input helpers
├── Models/                    # Transaction, TransactionType, TransactionSummary
├── Events/                    # TransactionAddedEventArgs
└── Services/
    ├── StorageService.cs      # JSON day files: save, load by date range, remove by id
    ├── TransactionService.cs  # business logic, raises TransactionAdded
    └── LoggerService.cs       # subscribes to TransactionAdded, appends to the log
```

- `Program.cs` wires everything together. `TransactionService` gets the `StorageService`
  through its constructor, and `LoggerService` subscribes to the `TransactionAdded` event.
- `Transaction` is immutable. Amounts are `decimal` (exact money math) and always positive;
  the type decides the sign.
- Parsing and formatting use `CultureInfo.InvariantCulture`, so the app behaves the same on
  German and English systems.

Class diagram: [docs/planning/UML.md](docs/planning/UML.md) · Plan and requirements:
[docs/planning/PLANNING.md](docs/planning/PLANNING.md)

## Status

| Epic | Requirements | Status |
|---|---|---|
| Project setup & structure | FR001, FR009, FR011 | ✅ |
| Domain models | FR012, FR013 | ✅ |
| Persistence (StorageService) | FR004, FR005, FR006, FR014 | ✅ |
| Business logic & events | FR003, FR008, FR015 | ✅ |
| Logging | FR016 | ✅ |
| Console UI | FR002, FR003, FR005, FR006 | ✅ |
| Error handling | FR007 | 🚧 in progress |
| Documentation | FR010 | ✅ this README |

**Next (stretch goals):** xUnit tests for `TransactionService` and `StorageService`,
categories with a grouped report, and a monthly summary.

## Workflow

Planned epic by epic on a [GitHub project board](https://github.com/users/MonkeyDNaara/projects/8):
one branch and one pull request per issue, with Conventional Commits.
