/**
 * Fills a fresh CashStack database with demo data through the public API, so the
 * dashboard is not empty in a demo. Uses the API's own validation (no direct DB access).
 *
 *   npm run seed                      # API from VITE_API_URL or http://localhost:5198
 *   npm run seed -- --api=http://...  # other API
 *   npm run seed -- --force           # add the data even if transactions exist
 */
import type { Category, Transaction, TransactionType } from '../src/api/types.ts';
import { buildDemoPlan, extraCategories, isoDate, toRequests } from './demoPlan.ts';

const args = process.argv.slice(2);
const force = args.includes('--force');
const apiArg = args.find((a) => a.startsWith('--api='))?.slice('--api='.length);
const baseUrl = `${(apiArg ?? process.env.VITE_API_URL ?? 'http://localhost:5198').replace(/\/+$/, '')}/api/v1`;

async function api<T>(method: string, path: string, body?: unknown): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method,
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch {
    throw new Error(`The API is not reachable at ${baseUrl}. Start it with "npm run dev:api".`);
  }
  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as {
      title?: string;
      detail?: string;
    };
    throw new Error(
      `${method} ${path} failed (${response.status}): ${problem.detail ?? problem.title ?? ''}`,
    );
  }
  return (await response.json()) as T;
}

async function main() {
  const today = new Date();
  const plan = buildDemoPlan(today);
  const from = plan[0]?.date ?? isoDate(today);

  const existing = await api<Transaction[]>(
    'GET',
    `/transactions?from=${from}&to=${isoDate(today)}`,
  );
  if (existing.length > 0 && !force) {
    console.error(
      `Found ${existing.length} transactions between ${from} and today. Nothing was added.\n` +
        'Run "npm run seed -- --force" to add the demo data anyway.',
    );
    process.exitCode = 1;
    return;
  }

  let categories = await api<Category[]>('GET', '/categories');
  for (const extra of extraCategories) {
    const exists = categories.some(
      (c) => c.type === extra.type && c.name.toLowerCase() === extra.name.toLowerCase(),
    );
    if (!exists) {
      categories = [...categories, await api<Category>('POST', '/categories', extra)];
      console.log(`Created category ${extra.name} (${extra.type})`);
    }
  }

  const idOf = (name: string, type: TransactionType) =>
    categories.find((c) => c.type === type && c.name.toLowerCase() === name.toLowerCase())?.id;
  const requests = toRequests(plan, idOf);

  // One after another: keeps the audit log in date order and the output readable
  for (const request of requests) {
    await api('POST', '/transactions', request);
  }

  const income = requests.filter((r) => r.type === 'Income').reduce((s, r) => s + r.amount, 0);
  const expenses = requests.filter((r) => r.type === 'Expense').reduce((s, r) => s + r.amount, 0);
  console.log(
    `Added ${requests.length} transactions from ${from} to ${isoDate(today)} ` +
      `(income ${income.toFixed(2)}, expenses ${expenses.toFixed(2)}).`,
  );
}

main().catch((error: unknown) => {
  console.error(error instanceof Error ? error.message : error);
  process.exitCode = 1;
});
