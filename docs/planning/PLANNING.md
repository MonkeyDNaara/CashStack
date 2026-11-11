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

Decision: **class** (practice OOP: constructor, encapsulation, validation). Properties get-only / `init` so a transaction can't change after creation.

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
2. `Transaction` class (5 properties, constructor, immutable)
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

---

## 6. Epic 10 – REST API (ASP.NET Core)

Added 2026-10-26. Goal: expose the finished console MVP as a REST API and cover the course topics **REST API, Hosting, Middleware Pipeline, Endpoint Routing, Filters & Validation, Error Handling**. (The course list says "Hoisting" – that is a JavaScript concept; the ASP.NET topic is *Hosting*.) Milestone: `API`. Not part of FR001–FR016.

**Decisions**

- Shared `CashStack.Core` class library; the console app and the API both reference it.
- Controllers, not Minimal APIs: filters are an MVC feature, and the course topic is "Filters & Validation".
- Route prefix `/api/v1/transactions`; DTOs instead of exposing `Transaction`.
- Order: merge the console PR and finish Epic 7 first (`StorageException` is reused by the global exception handler).

**Target structure**

```
src/
├── CashStack.Core/      # Models, Events, Services, Exceptions (classlib)
├── CashStack/           # Console UI (Program.cs) -> references Core
└── CashStack.Api/       # ASP.NET Core Web API   -> references Core
tests/CashStack.Tests/
```

**Endpoints**

| Method + route | Purpose | Success | Errors |
|---|---|---|---|
| `GET /api/v1/transactions?from&to&type` | list | 200 | 400 |
| `GET /api/v1/transactions/summary?from&to` | totals | 200 | 400 |
| `POST /api/v1/transactions` | create | 201 + `Location` | 400 |
| `DELETE /api/v1/transactions/{id:guid}` | remove | 204 | 404 |

**Issues (in order)**

1. Extract `CashStack.Core` class library
2. Add `CashStack.Api`: hosting, DI and configuration
3. Transactions controller: REST endpoints, DTOs and routing
4. Custom middleware: request timing and logging
5. Request validation with DataAnnotations
6. Action filter: validate date range
7. Global error handling: `IExceptionHandler` and ProblemDetails
8. Make `StorageService` thread-safe (parallel requests)
9. Docs: update README and UML for Core and Api

Stretch additions (Epic 9): API integration tests with `WebApplicationFactory`, OpenAPI documentation with a UI (Scalar).

**Concept map:** Hosting = builder phase (services) + app phase (pipeline); Endpoint Routing = match, then execute; Middleware = ordered chain around the endpoint; Filters = MVC pipeline around the action; Validation = DataAnnotations + filter; Error Handling = `IExceptionHandler` + ProblemDetails.

## 7. Epic #102 – Front end (React + Vite + TypeScript)

Added 2026-11-10, finished 2026-11-11. Goal: a small, clean web front end for the existing REST API that can be demoed locally to employers (no deploy).

**Decisions**

| Topic | Decision | Why |
|---|---|---|
| Folder and tooling | `frontend/` next to `src/` and `tests/`; ESLint + Prettier + Vitest + React Testing Library + MSW; exact dependency versions | same quality bar as the back end (tests for every feature) |
| Libraries | React Router (data router), TanStack Query, Recharts, own typed `fetch` client | server state with caching and invalidation instead of hand-written `useEffect` fetching |
| Styling | Tailwind CSS v4 + DaisyUI 5, theme `corporate` with the brand teal | consistent, modern look with little custom CSS |
| Errors | `ProblemDetails` → `ApiError`; field errors at the field, `detail` (400/409) in the form or dialog, network/5xx as toast, failed page loads as alert with retry | the API's messages reach the user where they can act on them |
| Start | `npm run dev:all` (API + Vite with `concurrently`), `VITE_API_URL`; CORS for `http://localhost:5173` from Epic 10 | one command for a demo |
| Language and formats | English UI, EUR in `de-DE` (`1.234,50 €`), dates `yyyy-MM-dd` | matches the API and the console |
| Dashboard | summary cards, expenses as donut + list (hover highlights both), income list, latest 5; presets + date fields in the URL | chosen from HTML mockups |
| Transactions | grouped by day with daily totals; filters in the URL, text search in the browser; delete with a confirm dialog | the API has no restore, so no "undo" |
| New transaction | dialog with the amount first and category tiles (radio buttons underneath); defaults Expense, no category, today | quick to use, accessible |
| Categories | two columns (income, expenses), add field on top, inline rename, delete with a confirm dialog that explains 409 "in use" | both types visible at once |
| Demo data | `npm run seed` (TypeScript, `tsx`): fixed plan for three months relative to today, only through the public API | reproducible screenshots and demos |
| Back end change | optional `date` on `POST /api/v1/transactions` (not in the future), `TimeProvider` in `TransactionService` | needed for booking past days and for the seed |

**Issues**

| Issue | PR | Content |
|---|---|---|
| #103 | #112 | scaffold, tooling, layout and routing, `dev:all` |
| #104 | #113 | typed API client, `ApiError`, query hooks, toasts, format helpers |
| #105 | #114 | API: optional date when creating a transaction |
| #106 | #115 | dashboard |
| #107 | #116 | transaction list with filters and delete |
| #108 | #117 | create transaction dialog |
| #109 | #118 | manage categories |
| #110 | #119 | seed script for demo data |
| #111 | – | docs: README, screenshots, architecture, this section |

**Lessons learned**

- MSW 3 renamed `onUnhandledRequest` to `onUnhandledFrame`; the old option was ignored silently, only the type check found it.
- An open `<dialog>` lives in the browser's top layer and covers normal toasts → the toast container is a `popover`.
- Formatting the amount on blur rounded `42,505` to `42,51` → only valid input is formatted.
- `toLocaleDateString` differs between Node and browsers → day headings are built by hand.
- Recharts pushed the bundle over 500 kB → the dashboard route is lazy-loaded.
