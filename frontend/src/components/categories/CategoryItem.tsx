import { useId, useState, type KeyboardEvent } from 'react';
import type { Category } from '../../api/types';
import { useCategoryColors } from '../../hooks/useCategoryColors';
import { toast } from '../../lib/toast';
import { useRenameCategory } from '../../queries/categories';
import { CategoryChip } from '../common/CategoryChip';
import { LockIcon, PencilIcon, TrashIcon } from '../common/icons';
import { CATEGORY_NAME_MAX, validateCategoryName } from './categoryName';

type CategoryItemProps = {
  category: Category;
  onDelete: (category: Category) => void;
};

export function CategoryItem({ category, onDelete }: CategoryItemProps) {
  const id = useId();
  const colorOf = useCategoryColors();
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(category.name);
  const [clientError, setClientError] = useState<string>();
  const rename = useRenameCategory();

  const apiError =
    rename.error && !rename.error.isUnexpected
      ? (rename.error.fieldError('name') ?? rename.error.userMessage)
      : undefined;
  const error = clientError ?? apiError;

  function startEdit() {
    setName(category.name);
    setClientError(undefined);
    rename.reset();
    setEditing(true);
  }

  function cancel() {
    setEditing(false);
    setClientError(undefined);
    rename.reset();
  }

  function save() {
    const trimmed = name.trim();
    if (trimmed === category.name) return cancel(); // nothing changed, no request
    const problem = validateCategoryName(trimmed);
    setClientError(problem);
    if (problem) return;

    rename.mutate(
      { id: category.id, name: trimmed },
      {
        onSuccess: (renamed) => {
          toast.success(`Renamed to "${renamed.name}"`);
          setEditing(false);
        },
      },
    );
  }

  function onKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === 'Enter') {
      event.preventDefault();
      save();
    } else if (event.key === 'Escape') {
      event.preventDefault();
      cancel();
    }
  }

  return (
    <li className="py-2">
      <div className="flex items-center gap-3">
        <CategoryChip name={category.name} color={colorOf(category.id)} />
        {editing ? (
          <>
            <input
              // Focus moves into the field the user just asked to edit
              autoFocus
              aria-label={`New name for ${category.name}`}
              maxLength={CATEGORY_NAME_MAX}
              className={`input input-sm h-9 min-w-0 flex-1 rounded-lg ${error ? 'input-error' : ''}`}
              aria-invalid={error ? true : undefined}
              aria-describedby={error ? `${id}-error` : undefined}
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                setClientError(undefined);
                if (rename.error) rename.reset();
              }}
              onKeyDown={onKeyDown}
            />
            <button
              type="button"
              className="btn btn-primary btn-sm h-9 rounded-lg"
              disabled={rename.isPending}
              onClick={save}
            >
              {rename.isPending && <span className="loading loading-spinner loading-xs" />}
              Save
            </button>
            <button type="button" className="btn btn-ghost btn-sm h-9" onClick={cancel}>
              Cancel
            </button>
          </>
        ) : (
          <>
            <span className="min-w-0 flex-1 truncate text-sm">{category.name}</span>
            {category.isProtected ? (
              <span
                className="flex items-center gap-1 text-xs text-base-content/50"
                title="Fallback category of this type: it cannot be renamed or deleted."
              >
                <LockIcon /> Protected
              </span>
            ) : (
              <>
                <button
                  type="button"
                  aria-label={`Rename ${category.name}`}
                  title="Rename"
                  className="btn btn-ghost btn-sm btn-square text-base-content/50"
                  onClick={startEdit}
                >
                  <PencilIcon />
                </button>
                <button
                  type="button"
                  aria-label={`Delete ${category.name}`}
                  title="Delete"
                  className="btn btn-ghost btn-sm btn-square text-base-content/50 hover:bg-rose-50 hover:text-rose-600"
                  onClick={() => onDelete(category)}
                >
                  <TrashIcon />
                </button>
              </>
            )}
          </>
        )}
      </div>
      {editing && error && (
        <p id={`${id}-error`} className="mt-1 ml-12 text-xs text-error">
          {error}
        </p>
      )}
    </li>
  );
}
