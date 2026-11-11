import type { Transaction } from '../../api/types';
import { useCategoryColors } from '../../hooks/useCategoryColors';
import { formatNetMoney } from '../../lib/format';
import { formatDayHeading, groupByDay } from '../../lib/groupByDay';
import { TransactionRow } from './TransactionRow';

type TransactionListProps = {
  transactions: Transaction[];
  onDelete: (transaction: Transaction) => void;
};

export function TransactionList({ transactions, onDelete }: TransactionListProps) {
  const colorOf = useCategoryColors();

  return (
    <div>
      {groupByDay(transactions).map((group) => (
        <section key={group.day} aria-label={group.day}>
          <h3 className="flex justify-between pt-4 pb-1 text-xs font-bold tracking-wide text-base-content/60 uppercase">
            <span>{formatDayHeading(group.day)}</span>
            <span
              className={`tabular-nums ${group.net < 0 ? 'text-rose-600' : 'text-emerald-600'}`}
            >
              {formatNetMoney(group.net)}
            </span>
          </h3>
          <ul className="divide-y divide-base-200">
            {group.transactions.map((t) => (
              <li key={t.id}>
                <TransactionRow
                  transaction={t}
                  color={colorOf(t.categoryId)}
                  meta={t.categoryName}
                  onDelete={onDelete}
                />
              </li>
            ))}
          </ul>
        </section>
      ))}
    </div>
  );
}
