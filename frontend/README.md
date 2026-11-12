# CashStack front end

React + Vite + TypeScript front end for the CashStack REST API. Runs locally, no deploy.

## Requirements

- Node.js 22+ (`.nvmrc`) and npm
- .NET 10 SDK (for the API)

## Start

```bash
cd frontend
npm install
npm run dev:all      # API on http://localhost:5198 + front end on http://localhost:5173
```

Or in two terminals:

```bash
cd src/CashStack.Api && dotnet run --launch-profile http   # terminal 1
cd frontend && npm run dev                                   # terminal 2
```

The API URL comes from `VITE_API_URL` (default `http://localhost:5198`). To change it,
copy `.env.example` to `.env.local`. The dev server always uses port 5173, because the
API's CORS policy allows exactly that origin (`Cors:AllowedOrigins`).

`VITE_DEMO_MODE=true` (only in the live-demo build) adds a footer that the data is shared,
and explains a network error as a sleeping free server. The wake-up bar itself always works:
the app pings `GET /health` on start and when the tab comes back after 14 minutes, and shows
"Waking up the free demo server" when the answer takes longer than 2 seconds.

## Demo data

With the API running, fill an empty database with demo data for the last three months
(salary, rent, groceries, subscriptions, leisure, …):

```bash
npm run seed                          # API from VITE_API_URL or http://localhost:5198
npm run seed -- --force               # add the data although transactions exist
npm run seed -- --api=http://host:port
```

The script (`scripts/seed.ts`, run with `tsx`) only uses the public API, so every booking
goes through the API's validation and the audit log. The plan is fixed and relative to
today (same amounts on every run, nothing in the future): about 45 transactions when run
at the end of a month, fewer earlier in the month. It refuses to run twice unless
`--force` is given, and creates the category "Subscriptions" if it is missing.
To start over, stop the API and delete `src/CashStack.Api/data/`.

## Scripts

| Script                            | What it does                                                          |
| --------------------------------- | --------------------------------------------------------------------- |
| `npm run dev`                     | Vite dev server only                                                  |
| `npm run dev:api`                 | API only (`dotnet run` in `src/CashStack.Api`)                        |
| `npm run dev:all`                 | API + front end in one terminal (`concurrently`, stops both together) |
| `npm run lint`                    | ESLint (typescript-eslint, React hooks rules)                         |
| `npm run format` / `format:check` | Prettier                                                              |
| `npm test` / `test:watch`         | Vitest + React Testing Library                                        |
| `npm run build`                   | type check (`tsc -b`) + production build                              |
| `npm run check`                   | lint, format check, tests and build, like CI would run them           |

## Stack

React 19 · TypeScript (strict) · Vite · React Router · TanStack Query · Recharts · Tailwind CSS v4 +
DaisyUI 5 · Vitest + React Testing Library + MSW

```
src/
├── api/                 # typed fetch client, ApiError (ProblemDetails), endpoint functions, DTO types
├── queries/             # TanStack Query hooks + query key factory (invalidation after mutations)
├── components/
│   ├── layout/          # AppLayout (navbar), PageHeader
│   ├── categories/      # two columns with add field, inline rename, delete dialog
│   ├── common/          # Modal (<dialog>), DateRangePicker, CategoryChip, icons
│   ├── dashboard/       # stat cards, donut + category list, recent transactions
│   ├── transactions/    # filters, list grouped by day, create dialog (category tiles), delete dialog
│   └── feedback/        # Toaster, ErrorAlert
├── pages/               # one component per route (dashboard is lazy-loaded: Recharts only loads there)
├── hooks/               # URL state (useDateRangeParams, useTransactionFilterParams), useCategoryColors
├── lib/                 # QueryClient, toast store, format helpers, amount parsing, date ranges, category colors
├── test/                # test setup, MSW server + fixtures, render helpers
├── routes.tsx           # route table, shared by the app and the tests
├── config.ts            # API base URL from VITE_API_URL
└── App.tsx / main.tsx   # providers and entry point
```

## Error handling

Every failed call throws an `ApiError` built from the API's `ProblemDetails`:

| Case                          | `ApiError`                                 | Shown as                     |
| ----------------------------- | ------------------------------------------ | ---------------------------- |
| 400 validation                | `fieldErrors` (`amount`, `description`, …) | message under the form field |
| 400 business rule, 404, 409   | `detail` from the API                      | alert in the form / dialog   |
| 500, network error (API down) | generic `userMessage`                      | toast                        |
| first load of a page fails    | any of the above                           | alert with "Try again"       |

Tests mock the API with [MSW](https://mswjs.io); unhandled requests fail the test.
