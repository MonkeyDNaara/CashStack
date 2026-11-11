import type { Summary, Transaction } from '../../api/types';
import { formatMoney } from '../../lib/format';

type StatCardsProps = {
  summary: Summary;
  transactions: Transaction[];
};

function count(transactions: Transaction[], type: Transaction['type']): string {
  const n = transactions.filter((t) => t.type === type).length;
  return `${n} ${n === 1 ? 'transaction' : 'transactions'}`;
}

export function StatCards({ summary, transactions }: StatCardsProps) {
  const left =
    summary.income > 0
      ? `${Math.round((summary.balance / summary.income) * 100)} % of income left`
      : '';

  const cards = [
    {
      label: 'Income',
      value: formatMoney(summary.income),
      hint: count(transactions, 'Income'),
      icon: '↓',
      tone: 'text-emerald-600',
      iconBg: 'bg-emerald-50',
    },
    {
      label: 'Expenses',
      value: formatMoney(summary.expenses),
      hint: count(transactions, 'Expense'),
      icon: '↑',
      tone: 'text-rose-600',
      iconBg: 'bg-rose-50',
    },
    {
      label: 'Balance',
      value: formatMoney(summary.balance),
      hint: left,
      icon: '=',
      tone: summary.balance < 0 ? 'text-rose-600' : 'text-base-content',
      iconBg: 'bg-teal-50 text-teal-700',
    },
  ];

  return (
    <div className="grid gap-4 sm:grid-cols-3">
      {cards.map((c) => (
        <section
          key={c.label}
          className="card rounded-2xl bg-base-100 shadow-sm"
          aria-label={c.label}
        >
          <div className="card-body gap-1 p-5">
            <div className="flex items-center gap-2 text-sm text-base-content/60">
              <span
                aria-hidden="true"
                className={`grid h-7 w-7 place-items-center rounded-lg ${c.iconBg} ${c.tone}`}
              >
                {c.icon}
              </span>
              {c.label}
            </div>
            <p className={`text-2xl font-bold tracking-tight tabular-nums ${c.tone}`}>{c.value}</p>
            <p className="text-xs text-base-content/60">{c.hint}</p>
          </div>
        </section>
      ))}
    </div>
  );
}
