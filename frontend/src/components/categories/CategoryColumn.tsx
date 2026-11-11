import type { Category, TransactionType } from '../../api/types';
import { AddCategoryForm } from './AddCategoryForm';
import { CategoryItem } from './CategoryItem';

const titles: Record<TransactionType, string> = { Income: 'Income', Expense: 'Expenses' };

type CategoryColumnProps = {
  type: TransactionType;
  categories: Category[];
  onDelete: (category: Category) => void;
};

export function CategoryColumn({ type, categories, onDelete }: CategoryColumnProps) {
  // Own categories by name, the protected fallback "Other" last
  const sorted = [...categories].sort(
    (a, b) => Number(a.isProtected) - Number(b.isProtected) || a.name.localeCompare(b.name),
  );

  return (
    <section className="card rounded-2xl bg-base-100 shadow-sm" aria-labelledby={`${type}-heading`}>
      <div className="card-body p-5">
        <h2 id={`${type}-heading`} className="flex items-baseline gap-2 text-base font-semibold">
          {titles[type]}
          <span className="text-xs font-normal text-base-content/50">{categories.length}</span>
        </h2>
        <AddCategoryForm type={type} />
        <ul className="divide-y divide-base-200 border-t border-base-200">
          {sorted.map((c) => (
            <CategoryItem key={c.id} category={c} onDelete={onDelete} />
          ))}
        </ul>
      </div>
    </section>
  );
}
