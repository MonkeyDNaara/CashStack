import { useId, useState, type FormEvent } from 'react';
import type { TransactionType } from '../../api/types';
import { toast } from '../../lib/toast';
import { useCreateCategory } from '../../queries/categories';
import { CATEGORY_NAME_MAX, validateCategoryName } from './categoryName';

const typeLabel: Record<TransactionType, string> = { Income: 'income', Expense: 'expense' };

export function AddCategoryForm({ type }: { type: TransactionType }) {
  const id = useId();
  const [name, setName] = useState('');
  const [clientError, setClientError] = useState<string>();
  const create = useCreateCategory();

  // 400 field error, or the API's message for 409 (duplicate name)
  const apiError =
    create.error && !create.error.isUnexpected
      ? (create.error.fieldError('name') ?? create.error.userMessage)
      : undefined;
  const error = clientError ?? apiError;

  function submit(event: FormEvent) {
    event.preventDefault();
    const problem = validateCategoryName(name);
    setClientError(problem);
    if (problem) return;

    create.mutate(
      { name: name.trim(), type },
      {
        onSuccess: (category) => {
          toast.success(`Category "${category.name}" added`);
          setName('');
        },
      },
    );
  }

  return (
    <form onSubmit={submit} noValidate className="mb-2">
      <div className="flex gap-2">
        <input
          aria-label={`New ${typeLabel[type]} category`}
          placeholder={`New ${typeLabel[type]} category…`}
          maxLength={CATEGORY_NAME_MAX}
          className={`input input-sm h-9 flex-1 rounded-lg ${error ? 'input-error' : ''}`}
          aria-invalid={error ? true : undefined}
          aria-describedby={error ? `${id}-error` : undefined}
          value={name}
          onChange={(e) => {
            setName(e.target.value);
            setClientError(undefined);
            if (create.error) create.reset();
          }}
        />
        <button
          type="submit"
          className="btn btn-primary btn-sm h-9 rounded-lg"
          disabled={create.isPending}
        >
          {create.isPending && <span className="loading loading-spinner loading-xs" />}
          Add
        </button>
      </div>
      {error && (
        <p id={`${id}-error`} className="mt-1 text-xs text-error">
          {error}
        </p>
      )}
    </form>
  );
}
