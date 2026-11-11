import { useState } from 'react';
import type { Category } from '../api/types';
import { CategoryColumn } from '../components/categories/CategoryColumn';
import { DeleteCategoryDialog } from '../components/categories/DeleteCategoryDialog';
import { ErrorAlert } from '../components/feedback/ErrorAlert';
import { PageHeader } from '../components/layout/PageHeader';
import { useCategories } from '../queries/categories';

export function CategoriesPage() {
  const query = useCategories();
  const [toDelete, setToDelete] = useState<Category>();

  function renderContent() {
    if (query.error) {
      return (
        <ErrorAlert
          title="The categories could not be loaded"
          error={query.error}
          onRetry={() => void query.refetch()}
        />
      );
    }
    if (!query.data) {
      return (
        <div className="grid gap-4 md:grid-cols-2" aria-busy="true" aria-label="Loading categories">
          <div className="skeleton h-64 rounded-2xl" />
          <div className="skeleton h-64 rounded-2xl" />
        </div>
      );
    }
    return (
      <div className="grid items-start gap-4 md:grid-cols-2">
        {(['Income', 'Expense'] as const).map((type) => (
          <CategoryColumn
            key={type}
            type={type}
            categories={query.data.filter((c) => c.type === type)}
            onDelete={setToDelete}
          />
        ))}
      </div>
    );
  }

  return (
    <>
      <PageHeader
        title="Categories"
        description="Income and expense categories. 'Other' is the protected fallback of each type."
      />
      {renderContent()}
      <DeleteCategoryDialog category={toDelete} onClose={() => setToDelete(undefined)} />
    </>
  );
}
