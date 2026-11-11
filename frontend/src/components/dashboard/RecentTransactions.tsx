import { Link } from 'react-router';
import type { Transaction } from '../../api/types';
import type { Range } from '../../lib/dateRanges';
import { useCategoryColors } from '../../hooks/useCategoryColors';
import { formatDate, formatSignedMoney } from '../../lib/format';
import { CategoryChip } from './CategoryChip';

const LIMIT = 5;

type RecentTransactionsProps = {
  transactions: Transaction[];
  range: Range;
};

export function RecentTransactions({ transactions, range }: RecentTransactionsProps) {
  const colorOf = useCategoryColors();
  // Newest first; ISO timestamps with offset are compared as instants, not as strings
  const latest = [...transactions]
    .sort((a, b) => Date.parse(b.timestamp) - Date.parse(a.timestamp))
    .slice(0, LIMIT);

  return (
    <section className="card rounded-2xl bg-base-100 shadow-sm" aria-labelledby="recent-heading">
      <div className="card-body">
        <div className="flex items-center justify-between">
          <h2 id="recent-heading" className="card-title text-base">
            Recent transactions
          </h2>
          <Link
            to={`/transactions?from=${range.from}&to=${range.to}`}
            className="link link-primary text-sm no-underline"
          >
            View all →
          </Link>
        </div>
        {latest.length === 0 ? (
          <p className="py-6 text-center text-base-content/60">No transactions in this period.</p>
        ) : (
          <ul className="divide-y divide-base-200">
            {latest.map((t) => (
              <li key={t.id} className="flex items-center gap-3 py-2.5">
                <CategoryChip name={t.categoryName} color={colorOf(t.categoryId)} />
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm">{t.description}</p>
                  <p className="text-xs text-base-content/60">
                    {formatDate(t.timestamp)} · {t.categoryName}
                  </p>
                </div>
                <span
                  className={`text-sm font-semibold tabular-nums ${t.type === 'Income' ? 'text-emerald-600' : 'text-rose-600'}`}
                >
                  {formatSignedMoney(t.amount, t.type)}
                </span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  );
}
