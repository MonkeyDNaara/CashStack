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
DaisyUI 5 · Vitest + React Testing Library

```
src/
├── components/layout/   # AppLayout (navbar), PageHeader
├── pages/               # one component per route
├── lib/                 # QueryClient factory
├── test/                # test setup, renderRoute helper
├── routes.tsx           # route table, shared by the app and the tests
├── config.ts            # API base URL from VITE_API_URL
└── App.tsx / main.tsx   # providers and entry point
```
