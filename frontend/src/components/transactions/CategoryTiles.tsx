import type { Category } from '../../api/types';
import { useCategoryColors } from '../../hooks/useCategoryColors';
import { CategoryChip } from '../common/CategoryChip';

type CategoryTilesProps = {
  categories: Category[];
  value: string;
  onChange: (categoryId: string) => void;
  error?: string;
  errorId?: string;
};

/**
 * Category picker as tiles. Real radio buttons underneath (visually hidden), so arrow keys,
 * Tab and screen readers work like in any radio group.
 */
export function CategoryTiles({ categories, value, onChange, error, errorId }: CategoryTilesProps) {
  const colorOf = useCategoryColors();

  return (
    <fieldset aria-describedby={error ? errorId : undefined}>
      <legend className="mb-1.5 text-sm font-semibold">Category</legend>
      <div className="grid max-h-60 grid-cols-3 gap-2 overflow-y-auto p-0.5">
        {categories.map((c) => {
          const checked = c.id === value;
          return (
            <label
              key={c.id}
              className={`flex cursor-pointer flex-col items-center gap-1 rounded-xl border bg-base-100 px-1 py-2.5 text-center text-xs transition has-[:focus-visible]:ring-2 has-[:focus-visible]:ring-primary ${checked ? 'border-primary ring-2 ring-primary/20' : 'border-base-300 hover:border-base-content/30'}`}
            >
              <input
                type="radio"
                name="categoryId"
                value={c.id}
                checked={checked}
                onChange={() => onChange(c.id)}
                className="sr-only"
              />
              <CategoryChip name={c.name} color={colorOf(c.id)} size="sm" />
              <span className="w-full truncate">{c.name}</span>
            </label>
          );
        })}
      </div>
      {error && (
        <p id={errorId} className="mt-1 text-xs text-error">
          {error}
        </p>
      )}
    </fieldset>
  );
}
