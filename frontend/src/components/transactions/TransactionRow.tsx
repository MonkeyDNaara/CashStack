import type { Transaction } from '../../api/types';
import { formatSignedMoney } from '../../lib/format';
import { CategoryChip } from '../common/CategoryChip';

type TransactionRowProps = {
  transaction: Transaction;
  color: string;
  /** Second line under the description, e.g. the category or "date · category" */
  meta: string;
  onDelete?: (transaction: Transaction) => void;
};

// One row, shared by the transaction list and the delete dialog preview
export function TransactionRow({ transaction: t, color, meta, onDelete }: TransactionRowProps) {
  return (
    <div className="group flex items-center gap-3 py-2.5">
      <CategoryChip name={t.categoryName} color={color} />
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm">{t.description}</p>
        <p className="text-xs text-base-content/60">{meta}</p>
      </div>
      <span
        className={`text-sm font-semibold whitespace-nowrap tabular-nums ${t.type === 'Income' ? 'text-emerald-600' : 'text-rose-600'}`}
      >
        {formatSignedMoney(t.amount, t.type)}
      </span>
      {onDelete && (
        <button
          type="button"
          aria-label={`Delete ${t.description}`}
          title="Delete"
          className="btn btn-ghost btn-sm btn-square text-base-content/40 hover:bg-rose-50 hover:text-rose-600 focus-visible:opacity-100 md:opacity-0 md:group-hover:opacity-100"
          onClick={() => onDelete(t)}
        >
          <TrashIcon />
        </button>
      )}
    </div>
  );
}

function TrashIcon() {
  return (
    <svg
      viewBox="0 0 24 24"
      className="h-4 w-4"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      aria-hidden="true"
    >
      <path d="M3 6h18M8 6V4h8v2m-9 0 1 14h8l1-14" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}
