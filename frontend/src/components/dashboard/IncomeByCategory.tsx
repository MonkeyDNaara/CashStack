import type { CategoryTotal } from '../../api/types';
import { CategoryList } from './CategoryList';

export function IncomeByCategory({ totals }: { totals: CategoryTotal[] }) {
  return (
    <section className="card rounded-2xl bg-base-100 shadow-sm" aria-labelledby="income-heading">
      <div className="card-body">
        <h2 id="income-heading" className="card-title text-base">
          Income by category
        </h2>
        {totals.length === 0 ? (
          <p className="py-6 text-center text-base-content/60">No income in this period.</p>
        ) : (
          <CategoryList totals={totals} barColor="#059669" />
        )}
      </div>
    </section>
  );
}
