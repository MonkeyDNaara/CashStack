# CashStack – Project Plan

Console budget tracker in C# / .NET 10. Source of truth for scope: WBS project requirements FR001–FR016 + optional stretch goals.

- Board: https://github.com/users/MonkeyDNaara/projects/8
- UML: [UML.md](./UML.md)
- Issues are created with [`create_issues.py`](./create_issues.py)

---

## 1. Repository structure (target)

```
CashStack/
├── CashStack.slnx                 # solution file → lets app + test project live side by side
├── src/
│   └── CashStack/
│       ├── CashStack.csproj       # RootNamespace = CashStack
│       ├── Program.cs             # top-level statements: wiring + menu loop
│       ├── Models/                # namespace CashStack.Models
│       │   ├── Transaction.cs
│       │   └── TransactionType.cs
│       ├── Events/                # namespace CashStack.Events
│       │   └── TransactionAddedEventArgs.cs
│       ├── Services/              # namespace CashStack.Services
│       │   ├── StorageService.cs
│       │   ├── TransactionService.cs
│       │   └── LoggerService.cs
│       └── Exceptions/            # optional, namespace CashStack.Exceptions
│           └── StorageException.cs
├── tests/
│   └── CashStack.Tests/           # stretch goal (xUnit)
├── docs/
│   └── planning/                  # this plan, UML
├── .gitignore                     # + data/, logs/, course HTML
└── README.md
```

**Why `src/` + `tests/` now and not later?** The SDK compiles every `*.cs` below the `.csproj` folder. If the xUnit project ends up *inside* the app folder, its test files get compiled into the app → build errors. Setting this up on day 1 costs 5 minutes; fixing it later costs a messy refactor commit.

**Naming:** FR001/FR011 use `BudgetTracker` as an example. Using `CashStack` consistently (project, namespaces) is fine and looks more like a real product. Current `RootNamespace` is `_08_cash_stack` → change it.

---

## 2. Data structure

### Transaction (FR012)

| Property    | Type              | Rule                                                        |
|-------------|-------------------|-------------------------------------------------------------|
| `Id`        | `Guid`            | `Guid.NewGuid()` – set by `TransactionService`              |
| `Timestamp` | `DateTimeOffset`  | `DateTimeOffset.Now` – set by `TransactionService`          |
| `Type`      | `TransactionType` | `Income` or `Expense`, stored as text                       |
| `Description` | `string`        | not empty                                                   |
| `Amount`    | `decimal`         | always **> 0**; `Type` decides + / −                        |

Design decision to make: `record` vs `class`. A transaction never changes after it's created → `record` (immutable, value equality, nice `ToString()`) fits well. Look at both in the "Record, Classes and Interfaces" lesson and decide.

`decimal`, not `double`: money must be exact (`0.1 + 0.2` problem).

### Storage layout (FR004, FR009)

```
data/
├── 2026-03-14.json     # one file per day (yyyy-MM-dd of Timestamp)
└── 2026-03-15.json
logs/
└── transactions.log
```

### Day file – `data/2026-03-14.json`

```json
[
  {
    "id": "3f2b8c1e-6a4d-4e1b-9c2f-0d8e7a5b1c34",
    "timestamp": "2026-03-14T09:15:22.123+01:00",
    "type": "Expense",
    "description": "Groceries",
    "amount": 42.50
  },
  {
    "id": "a91c7f02-1b3e-4d55-8e6a-2c4f9b0d7e11",
    "timestamp": "2026-03-14T18:02:10.456+01:00",
    "type": "Income",
    "description": "Freelance invoice",
    "amount": 350.00
  }
]
```

- `"type": "Expense"` as text → `JsonStringEnumConverter` (FR012 hint).
- camelCase property names (`JsonNamingPolicy.CamelCase`) is optional but standard for JSON — and what a later web frontend would expect.
- `WriteIndented = true` makes the files readable while debugging.
- One shared `JsonSerializerOptions` instance inside `StorageService`.

### Log line – `logs/transactions.log` (FR016)

```
2026-03-14T09:15:22+01:00 | ADDED | Expense |   42.50 | Groceries | 3f2b8c1e-...
```

Pick your own format, but keep it one line per transaction and easy to grep.

### ⚠️ Culture trap (you're on a German-locale Mac)

`decimal.TryParse("42.50")` with `de-DE` culture → `4250`. `42.50m.ToString()` → `"42,50"`. Decide **once** how input/output is parsed and formatted (e.g. `CultureInfo.InvariantCulture`, or accept both `,` and `.`). Covered in the input-helpers issue.

---

## 3. Epics & issues

Order = dependency order. Each sub-issue is small enough for one branch + one PR (`Closes #N`).

### Epic 1 – Project Setup & Structure · FR001, FR009, FR011
1. Restructure repo: solution file, `src/CashStack`, rename project + `RootNamespace`
2. Folder skeleton `Models/`, `Events/`, `Services/` with matching namespaces
3. `.gitignore`: `data/`, `logs/`, course HTML in `docs/`
4. Directory bootstrap: create `data/` and `logs/` on startup

### Epic 2 – Domain Models · FR012, FR013
1. `TransactionType` enum (`Income`, `Expense`)
2. `Transaction` model (5 properties, record vs class decision)
3. `TransactionAddedEventArgs : EventArgs`

### Epic 3 – Persistence: StorageService · FR004, FR005, FR006, FR014
1. `StorageService` skeleton: constructor takes data directory, shared `JsonSerializerOptions`
2. Save transaction to its day file (read → deserialize → add → serialize)
3. Load transactions for a date range (parse file names with `DateOnly.TryParseExact`)
4. Remove transaction by Id across all day files → `bool`

### Epic 4 – Business Logic: TransactionService & Events · FR003, FR008, FR015
1. `TransactionService` with `StorageService` injected via constructor
2. Add transaction: create Id/Timestamp, validate amount > 0, save
3. `TransactionAdded` event raised after a successful save
4. Remove + query methods (date range, optional type filter)

### Epic 5 – Logging · FR016
1. `LoggerService` subscribes to `TransactionAdded`, appends a line via `File.AppendAllText`

### Epic 6 – Console UI · FR002, FR003, FR005, FR006
1. Main menu loop (`while` until Exit)
2. Input helpers with retry (`Enum.TryParse` ignoreCase, `decimal.TryParse`, `DateOnly.TryParseExact`, `Guid.TryParse`)
3. "Add transaction" flow
4. "Remove transaction" flow
5. "Report" flow: date range, type filter, list + total income / expenses / balance (LINQ)

### Epic 7 – Error Handling · FR007
1. Custom exception(s), e.g. `StorageException` wrapping `IOException` / `JsonException`
2. Handle errors at the UI level → clear message, program never crashes

### Epic 8 – Documentation · FR010
1. README: description, build/run commands, menu options, data format, architecture (UML)

### Epic 9 – Stretch Goals (optional, after MVP)
1. Extract `IStorageService`, xUnit project, `TransactionService` tests with a fake storage
2. `StorageService` tests against a temp folder
3. `Category` enum + report grouped by category (`GroupBy`)
4. Monthly summary (one line per month: income, expenses, balance)

**Milestones:** `MVP` (Epics 1–8) · `Stretch` (Epic 9)

---

## 4. Workflow

- Branch per issue: `feat/12-transaction-model`, `chore/3-gitignore`, `docs/27-readme`
- Commits: Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`, `test:`, `refactor:`)
- PR body: `Closes #N` → issue auto-closes, board moves to Done
- Before closing an epic: re-check its FRs against the requirements table

## 5. Suggested schedule (5 days FT / 10 days PT)

| Day (FT) | Focus                                  |
|----------|----------------------------------------|
| 1        | Epic 1 + Epic 2                        |
| 2        | Epic 3                                 |
| 3        | Epic 4 + Epic 5                        |
| 4        | Epic 6 + Epic 7                        |
| 5        | Epic 8, requirement check, stretch     |
