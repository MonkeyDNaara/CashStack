# CashStack – UML

Design target for the MVP. Signatures are a guide — adjust names while implementing, then update this file so README and code stay in sync.

## Class diagram

```mermaid
classDiagram
    direction LR

    namespace Models {
        class TransactionType {
            <<enumeration>>
            Income
            Expense
        }
        class Transaction {
            +Guid Id
            +DateTimeOffset Timestamp
            +TransactionType Type
            +string Description
            +decimal Amount
        }
    }

    namespace Events {
        class EventArgs {
            <<.NET>>
        }
        class TransactionAddedEventArgs {
            +Transaction Transaction
        }
    }

    namespace Services {
        class StorageService {
            -string _dataDirectory
            -JsonSerializerOptions _jsonOptions
            +StorageService(string dataDirectory)
            +Save(Transaction transaction) void
            +LoadRange(DateOnly from, DateOnly to) IEnumerable~Transaction~
            +Remove(Guid id) bool
            -GetFilePath(DateOnly day) string
            -ReadFile(string path) List~Transaction~
            -WriteFile(string path, List~Transaction~ transactions) void
        }
        class TransactionService {
            -StorageService _storage
            +event EventHandler~TransactionAddedEventArgs~ TransactionAdded
            +TransactionService(StorageService storage)
            +Add(TransactionType type, string description, decimal amount) Transaction
            +Remove(Guid id) bool
            +GetTransactions(DateOnly from, DateOnly to, TransactionType? type) IEnumerable~Transaction~
            #OnTransactionAdded(Transaction transaction) void
        }
        class LoggerService {
            -string _logFilePath
            +LoggerService(string logFilePath)
            +Subscribe(TransactionService service) void
            -OnTransactionAdded(object? sender, TransactionAddedEventArgs e) void
        }
    }

    class Program {
        <<top-level statements>>
        menu loop
        input helpers
        report output
    }

    Transaction --> TransactionType : has
    TransactionAddedEventArgs --|> EventArgs : inherits
    TransactionAddedEventArgs --> Transaction : carries
    TransactionService --> StorageService : uses (constructor injection)
    TransactionService ..> TransactionAddedEventArgs : raises
    LoggerService ..> TransactionService : subscribes to TransactionAdded
    StorageService ..> Transaction : reads / writes JSON
    Program --> TransactionService : calls
    Program ..> LoggerService : creates + wires
    Program ..> StorageService : creates
```

**Reading the arrows:** `-->` = holds a reference / uses · `--|>` = inheritance · `..>` = depends on (creates, raises, subscribes) · `+` public · `-` private · `#` protected.

**Key rule (from the hints):** only `StorageService` touches files in `data/`. `TransactionService` has no `File.*` calls — that's what makes it testable with a fake storage later (stretch goal → `IStorageService`).

## Sequence: adding a transaction (FR003, FR004, FR008, FR016)

```mermaid
sequenceDiagram
    actor User
    participant P as Program (menu)
    participant TS as TransactionService
    participant SS as StorageService
    participant FS as data/yyyy-MM-dd.json
    participant L as LoggerService

    Note over P,L: Startup: create data/ + logs/, new StorageService, new TransactionService, logger.Subscribe(ts)

    User->>P: choose "Add"
    P->>User: ask type / description / amount
    User-->>P: "expense", "Groceries", "42.50"
    P->>P: Enum.TryParse + decimal.TryParse (retry on invalid)
    P->>TS: Add(Expense, "Groceries", 42.50)
    TS->>TS: validate amount > 0, Id = Guid.NewGuid(), Timestamp = DateTimeOffset.Now
    TS->>SS: Save(transaction)
    SS->>FS: read file (if exists) and deserialize List
    SS->>SS: list.Add(transaction)
    SS->>FS: serialize + write whole list
    SS-->>TS: ok
    TS-)L: raise TransactionAdded(e)
    L->>L: File.AppendAllText(logs/transactions.log)
    TS-->>P: transaction
    P->>User: "Added 3f2b... (Expense 42.50)"
```

## Sequence: report (FR006)

```mermaid
sequenceDiagram
    actor User
    participant P as Program
    participant TS as TransactionService
    participant SS as StorageService

    User->>P: choose "Report", from / to / type filter
    P->>TS: GetTransactions(from, to, type?)
    TS->>SS: LoadRange(from, to)
    SS->>SS: Directory.GetFiles(data, "*.json")<br/>keep files whose name parses to a date in range
    SS-->>TS: IEnumerable of Transaction (SelectMany)
    TS-->>P: filtered by type (Where)
    P->>P: Sum income, Sum expenses, balance = income - expenses
    P->>User: table + totals
```
