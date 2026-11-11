import { useState } from 'react';
import type { CategoryTotal } from '../../api/types';
import { CategoryList } from './CategoryList';
import { ExpensesDonut } from './ExpensesDonut';

// Donut + list share one "active" category: hovering either highlights it in both
export function ExpensesByCategory({ totals }: { totals: CategoryTotal[] }) {
  const [activeId, setActiveId] = useState<string>();

  return (
    <section className="card rounded-2xl bg-base-100 shadow-sm" aria-labelledby="expenses-heading">
      <div className="card-body">
        <h2 id="expenses-heading" className="card-title text-base">
          Expenses by category
        </h2>
        {totals.length === 0 ? (
          <p className="py-8 text-center text-base-content/60">No expenses in this period.</p>
        ) : (
          <div className="grid items-center gap-6 md:grid-cols-[auto_1fr]">
            <ExpensesDonut totals={totals} activeId={activeId} onActiveChange={setActiveId} />
            <CategoryList totals={totals} activeId={activeId} onActiveChange={setActiveId} />
          </div>
        )}
      </div>
    </section>
  );
}
