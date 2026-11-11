import type { CategoryTotal } from '../../api/types';
import { useCategoryColors } from '../../hooks/useCategoryColors';
import { formatMoney, formatPercent } from '../../lib/format';
import { CategoryChip } from './CategoryChip';

type CategoryListProps = {
  totals: CategoryTotal[];
  /** Highlighted row, shared with the donut */
  activeId?: string;
  onActiveChange?: (categoryId: string | undefined) => void;
  /** Bar color for all rows, e.g. green for income; default: the category color */
  barColor?: string;
};

export function CategoryList({ totals, activeId, onActiveChange, barColor }: CategoryListProps) {
  const colorOf = useCategoryColors();
  const sum = totals.reduce((s, t) => s + t.total, 0);
  const max = Math.max(...totals.map((t) => t.total), 0);

  return (
    <ul className="divide-y divide-base-200">
      {totals.map((t) => {
        const color = colorOf(t.categoryId);
        const dimmed = activeId !== undefined && activeId !== t.categoryId;
        return (
          <li
            key={t.categoryId}
            className={`flex items-center gap-3 py-2 transition-opacity ${dimmed ? 'opacity-40' : ''}`}
            onMouseEnter={() => onActiveChange?.(t.categoryId)}
            onMouseLeave={() => onActiveChange?.(undefined)}
          >
            <CategoryChip name={t.name} color={color} />
            <div className="min-w-0 flex-1">
              <div className="flex justify-between gap-2 text-sm">
                <span className="truncate">{t.name}</span>
                <span className="font-semibold tabular-nums">{formatMoney(t.total)}</span>
              </div>
              <div className="mt-1 h-1.5 overflow-hidden rounded-full bg-base-200">
                <div
                  className="h-full rounded-full"
                  style={{
                    width: `${max > 0 ? (t.total / max) * 100 : 0}%`,
                    backgroundColor: barColor ?? color,
                  }}
                />
              </div>
            </div>
            <span className="w-10 text-right text-xs text-base-content/60 tabular-nums">
              {formatPercent(t.total, sum)}
            </span>
          </li>
        );
      })}
    </ul>
  );
}
