import type { CreateTransactionRequest, IsoDate, TransactionType } from '../src/api/types.ts';

/** A demo booking before the category names are resolved to ids */
export interface PlannedTransaction {
  date: IsoDate;
  type: TransactionType;
  category: string;
  description: string;
  amount: number;
}

/** Categories the plan needs that are not part of the seeded defaults */
export const extraCategories: { name: string; type: TransactionType }[] = [
  { name: 'Subscriptions', type: 'Expense' },
];

const pad = (n: number) => String(n).padStart(2, '0');

/** Local yyyy-MM-dd (same rule as the app: never via toISOString/UTC) */
export function isoDate(date: Date): IsoDate {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

// Fixed amounts, so every run (and every screenshot) looks the same.
// Index 0 = two months ago, 1 = last month, 2 = this month.
const groceries = [
  [62.3, 48.9, 71.15, 55.4, 39.8],
  [58.75, 66.2, 44.1, 73.5, 51.3],
  [62.3, 47.6, 68.9, 52.45, 41.2],
];
const leisure = [
  [
    { day: 8, description: 'Cinema', amount: 24 },
    { day: 21, description: 'Restaurant', amount: 48.5 },
  ],
  [
    { day: 11, description: 'Concert', amount: 65.9 },
    { day: 25, description: 'Bowling', amount: 32 },
  ],
  [
    { day: 9, description: 'Cinema', amount: 24 },
    { day: 19, description: 'Climbing gym', amount: 18 },
  ],
];

/**
 * About 45 realistic transactions over the current and the two previous months.
 * Days after today (current month) or beyond the month's length are skipped,
 * because the API rejects future dates.
 */
export function buildDemoPlan(today: Date = new Date()): PlannedTransaction[] {
  const todayIso = isoDate(today);
  const plan: PlannedTransaction[] = [];

  for (let offset = 0; offset < 3; offset++) {
    const month = new Date(today.getFullYear(), today.getMonth() - 2 + offset, 1);
    const daysInMonth = new Date(month.getFullYear(), month.getMonth() + 1, 0).getDate();
    const monthName = month.toLocaleDateString('en-GB', { month: 'long' });

    const add = (
      day: number,
      type: TransactionType,
      category: string,
      description: string,
      amount: number,
    ) => {
      if (day > daysInMonth) return;
      const date = isoDate(new Date(month.getFullYear(), month.getMonth(), day));
      if (date > todayIso) return;
      plan.push({ date, type, category, description, amount });
    };

    add(1, 'Income', 'Salary', `Salary ${monthName}`, 2500);
    add(3, 'Expense', 'Rent', `Rent ${monthName}`, 850);
    add(5, 'Expense', 'Transport', 'Monthly ticket', 49);
    add(12, 'Expense', 'Subscriptions', 'Streaming', 12.99);
    add(15, 'Expense', 'Subscriptions', 'Mobile plan', 19.99);
    add(17, 'Expense', 'Health', 'Pharmacy', [18.5, 9.95, 23.4][offset]);
    groceries[offset].forEach((amount, week) =>
      add(
        2 + week * 7,
        'Expense',
        'Groceries',
        week === 0 ? 'Weekly shopping' : 'Groceries',
        amount,
      ),
    );
    leisure[offset].forEach((l) => add(l.day, 'Expense', 'Leisure', l.description, l.amount));

    if (offset === 1) add(20, 'Income', 'Freelance', 'Logo design', 450);
    if (offset === 0) add(24, 'Expense', 'Other', 'Birthday gift', 35);
    if (offset === 2) add(6, 'Expense', 'Transport', 'Train ticket', 29.9);
  }

  return plan.sort((a, b) => a.date.localeCompare(b.date));
}

/** Maps category names to ids ("Other" exists once per type) */
export function toRequests(
  plan: PlannedTransaction[],
  categoryId: (name: string, type: TransactionType) => string | undefined,
): CreateTransactionRequest[] {
  return plan.map((p) => {
    const id = categoryId(p.category, p.type);
    if (!id) throw new Error(`Category "${p.category}" (${p.type}) does not exist.`);
    return {
      type: p.type,
      categoryId: id,
      description: p.description,
      amount: p.amount,
      date: p.date,
    };
  });
}
