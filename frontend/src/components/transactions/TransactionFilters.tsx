import type { TransactionType } from '../../api/types';
import type { TransactionFilterState } from '../../hooks/useTransactionFilterParams';
import type { Range } from '../../lib/dateRanges';
import { useCategories } from '../../queries/categories';
import { DateRangePicker } from '../common/DateRangePicker';

type TransactionFiltersProps = {
  range: Range;
  onRangeChange: (range: Range) => void;
  filters: TransactionFilterState;
  onChange: (patch: Partial<TransactionFilterState>) => void;
  onReset: () => void;
  fromError?: string;
  toError?: string;
};

const typeOptions: { value?: TransactionType; label: string }[] = [
  { value: undefined, label: 'All' },
  { value: 'Income', label: 'Income' },
  { value: 'Expense', label: 'Expenses' },
];

export function TransactionFilters({
  range,
  onRangeChange,
  filters,
  onChange,
  onReset,
  fromError,
  toError,
}: TransactionFiltersProps) {
  const { data: categories = [] } = useCategories();
  const visible = filters.type ? categories.filter((c) => c.type === filters.type) : categories;

  function changeType(type?: TransactionType) {
    // A category of the other type would return nothing → drop it
    const selected = categories.find((c) => c.id === filters.categoryId);
    const keepCategory = !type || !selected || selected.type === type;
    onChange({ type, ...(keepCategory ? {} : { categoryId: undefined }) });
  }

  return (
    <div className="card rounded-2xl bg-base-100 shadow-sm">
      <div className="card-body flex-row flex-wrap items-start gap-3 p-4">
        <DateRangePicker
          value={range}
          onChange={onRangeChange}
          fromError={fromError}
          toError={toError}
        />
        <div className="flex flex-wrap items-center gap-2 lg:ml-auto">
          <div className="inline-flex rounded-lg bg-base-200 p-0.5" role="group" aria-label="Type">
            {typeOptions.map((o) => (
              <button
                key={o.label}
                type="button"
                aria-pressed={filters.type === o.value}
                className={`btn btn-sm rounded-md border-0 shadow-none ${filters.type === o.value ? 'bg-base-100 shadow-sm' : 'btn-ghost text-base-content/60'}`}
                onClick={() => changeType(o.value)}
              >
                {o.label}
              </button>
            ))}
          </div>
          <select
            aria-label="Category"
            className="select select-sm w-44 rounded-lg"
            value={filters.categoryId ?? ''}
            onChange={(e) => onChange({ categoryId: e.target.value || undefined })}
          >
            <option value="">All categories</option>
            {(['Income', 'Expense'] as const).map((type) => {
              const group = visible.filter((c) => c.type === type);
              return group.length === 0 ? null : (
                <optgroup key={type} label={type}>
                  {group.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </optgroup>
              );
            })}
          </select>
          <input
            type="search"
            aria-label="Search description"
            placeholder="Search description…"
            className="input input-sm w-48 rounded-lg"
            value={filters.search}
            onChange={(e) => onChange({ search: e.target.value })}
          />
          <button type="button" className="btn btn-ghost btn-sm" onClick={onReset}>
            Reset
          </button>
        </div>
      </div>
    </div>
  );
}
