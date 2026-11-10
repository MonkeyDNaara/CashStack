# CashStack – UML

Current design (after Epic 11, EF Core). Signatures are shortened: `CancellationToken`
parameters and `async` return types (`Task<…>`) are left out for readability.

## Class diagram: Core

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
            +Guid CategoryId
            +string Description
            +decimal Amount
            +Transaction(id, timestamp, type, categoryId, description, amount)
        }
        class Category {
            +int MaxNameLength = 50$
            +Guid Id
            +string Name
            +TransactionType Type
            +bool IsProtected
            +Rename(string newName) void
        }
        class DefaultCategories {
            <<static>>
            +Guid OtherIncomeId$
            +Guid OtherExpenseId$
            +GetOtherId(TransactionType) Guid$
            +IsProtected(Guid) bool$
            +CreateStarterSet() IReadOnlyList~Category~$
        }
        class TransactionSummary {
            +decimal Income
            +decimal Expenses
            +decimal Balance
            +IReadOnlyList~CategoryTotal~ Categories
        }
        class CategoryTotal {
            +Guid CategoryId
            +string Name
            +TransactionType Type
            +decimal Total
        }
    }

    namespace Repositories {
        class ITransactionRepository {
            <<interface>>
            +AddAsync(Transaction)
            +GetByIdAsync(Guid) Transaction?
            +GetRangeAsync(from, to, type?, categoryId?) IReadOnlyList~Transaction~
            +RemoveAsync(Guid) bool
            +AnyWithCategoryAsync(Guid) bool
        }
        class ICategoryRepository {
            <<interface>>
            +GetAllAsync(type?) IReadOnlyList~Category~
            +GetByIdAsync(Guid) Category?
            +AddAsync(Category)
            +UpdateAsync(Category)
            +RemoveAsync(Guid) bool
        }
        class JsonTransactionRepository
        class JsonCategoryRepository
        class EfRepositoryBase {
            <<abstract>>
            #CashStackDbContext Context
            #ExecuteAsync~T~(Func~Task~T~~ action) T
        }
        class EfTransactionRepository
        class EfCategoryRepository
    }

    namespace Services {
        class TransactionService {
            +event TransactionAdded
            +AddAsync(type, description, amount, categoryId?) Transaction
            +GetTransactionsAsync(from, to, type?, categoryId?) IReadOnlyList~Transaction~
            +GetSummaryAsync(from, to) TransactionSummary
            +GetByIdAsync(Guid) Transaction?
            +RemoveAsync(Guid) bool
        }
        class CategoryService {
            +GetAllAsync(type?) IReadOnlyList~Category~
            +CreateAsync(name, type) Category
            +RenameAsync(id, newName) Category?
            +DeleteAsync(id) bool
        }
        class ImportService {
            +ImportAsync() ImportResult
        }
        class LoggerService {
            +OnTransactionAdded(sender, TransactionAddedEventArgs) void
        }
    }

    namespace Exceptions {
        class DomainException {
            <<abstract>>
        }
        class BusinessRuleException
        class ConflictException
        class StorageException
        class ConfigurationException
    }

    Transaction --> TransactionType
    Category --> TransactionType
    Category ..> DefaultCategories : IsProtected
    TransactionSummary --> CategoryTotal

    JsonTransactionRepository ..|> ITransactionRepository
    EfTransactionRepository ..|> ITransactionRepository
    JsonCategoryRepository ..|> ICategoryRepository
    EfCategoryRepository ..|> ICategoryRepository
    EfTransactionRepository --|> EfRepositoryBase
    EfCategoryRepository --|> EfRepositoryBase

    TransactionService --> ITransactionRepository
    TransactionService --> ICategoryRepository
    CategoryService --> ICategoryRepository
    CategoryService --> ITransactionRepository : in-use check
    ImportService --> ITransactionRepository : source + target
    ImportService --> ICategoryRepository : source + target
    LoggerService ..> TransactionService : subscribes to TransactionAdded

    BusinessRuleException --|> DomainException
    ConflictException --|> DomainException
```

**Reading the arrows:** `-->` uses · `..|>` implements · `--|>` inherits · `..>` depends on ·
`+` public · `#` protected · `$` static.

**Key rule:** services only depend on the two repository interfaces. The JSON and the EF
Core implementations are exchangeable (chosen by `Storage:Provider`) and are verified by
the same contract tests.

## Database (SQLite)

```mermaid
erDiagram
    Categories ||--o{ Transactions : "CategoryId (Restrict)"
    Categories {
        TEXT Id PK
        TEXT Name "NOCASE, unique with Type"
        TEXT Type "Income | Expense"
    }
    Transactions {
        TEXT Id PK
        INTEGER Timestamp "binary ticks"
        TEXT Type
        TEXT CategoryId FK
        TEXT Description
        INTEGER Amount "whole cents"
        TEXT Day "shadow column, local date"
    }
```

## Class diagram: API

```mermaid
classDiagram
    direction LR

    class TransactionsController {
        +GetAll(range, type?, categoryId?) 200
        +GetSummary(range) 200
        +GetById(id) 200 / 404
        +Create(CreateTransactionRequest) 201 / 400
        +Delete(id) 204 / 404
    }
    class CategoriesController {
        +GetAll(type?) 200
        +GetById(id) 200 / 404
        +Create(CreateCategoryRequest) 201 / 409
        +Rename(id, RenameCategoryRequest) 200 / 404 / 409
        +Delete(id) 204 / 404 / 409
    }
    class GlobalExceptionHandler {
        <<IExceptionHandler>>
        +TryHandleAsync(context, exception) bool
    }
    class RequestTimingMiddleware {
        <<IMiddleware>>
    }
    class ValidateDateRangeAttribute {
        <<ActionFilter>>
    }
    class StorageServiceCollectionExtensions {
        <<static>>
        +AddCashStackStorage(services, configuration, auditLog)
    }
    class FrontendCorsExtensions {
        <<static>>
        +AddFrontendCors(services, configuration)
        +UseFrontendCors(app)
    }

    TransactionsController --> TransactionService
    TransactionsController --> CategoryService : category names
    CategoriesController --> CategoryService
    TransactionsController ..> ValidateDateRangeAttribute
    GlobalExceptionHandler ..> DomainException : 400 / 409
    GlobalExceptionHandler ..> StorageException : 500
```

## Sequence: POST /api/v1/transactions (SQLite)

```mermaid
sequenceDiagram
    actor Client as Front end
    participant MW as Middleware<br/>(timing, CORS, exception handler)
    participant C as TransactionsController
    participant TS as TransactionService
    participant CR as EfCategoryRepository
    participant TR as EfTransactionRepository
    participant DB as SQLite
    participant L as LoggerService

    Client->>MW: POST { type, categoryId, description, amount }
    MW->>C: model binding + DataAnnotations (400 on error)
    C->>TS: AddAsync(type, description, amount, categoryId)
    TS->>CR: GetByIdAsync(categoryId)
    CR->>DB: SELECT category
    alt unknown category or other type
        TS--)MW: throw BusinessRuleException
        MW-->>Client: 400 ProblemDetails (our message)
    else category ok
        TS->>TS: new Transaction(...) validates amount and description
        TS->>TR: AddAsync(transaction)
        TR->>DB: INSERT (Amount in cents, Day = local date)
        TS-)L: raise TransactionAdded
        L->>L: append line to logs/transactions.log
        TS-->>C: transaction
        C-->>Client: 201 Created + Location + TransactionResponse (with category name)
    end
```

## Sequence: DELETE /api/v1/categories/{id} (category in use)

```mermaid
sequenceDiagram
    actor Client as Front end
    participant C as CategoriesController
    participant CS as CategoryService
    participant CR as ICategoryRepository
    participant TR as ITransactionRepository
    participant H as GlobalExceptionHandler

    Client->>C: DELETE /categories/{id}
    C->>CS: DeleteAsync(id)
    CS->>CR: GetByIdAsync(id)
    alt not found
        CS-->>C: false
        C-->>Client: 404
    else protected "Other"
        CS--)H: throw ConflictException
        H-->>Client: 409 "Category 'Other' is protected …"
    else used by transactions
        CS->>TR: AnyWithCategoryAsync(id) = true
        CS--)H: throw ConflictException
        H-->>Client: 409 "Category 'Rent' is used by transactions …"
    else unused
        CS->>CR: RemoveAsync(id)
        C-->>Client: 204
    end
```
