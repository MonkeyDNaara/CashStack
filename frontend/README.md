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

React 19 · TypeScript (strict) · Vite · React Router · TanStack Query · Tailwind CSS v4 +
DaisyUI 5 · Vitest + React Testing Library + MSW

```
src/
├── api/                 # typed fetch client, ApiError (ProblemDetails), endpoint functions, DTO types
├── queries/             # TanStack Query hooks + query key factory (invalidation after mutations)
├── components/
│   ├── layout/          # AppLayout (navbar), PageHeader
│   └── feedback/        # Toaster, ErrorAlert
├── pages/               # one component per route
├── lib/                 # QueryClient factory, toast store, format helpers (EUR de-DE, dates)
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
