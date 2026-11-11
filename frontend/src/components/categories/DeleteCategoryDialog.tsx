import { isApiError } from '../../api/ApiError';
import type { Category } from '../../api/types';
import { toast } from '../../lib/toast';
import { useDeleteCategory } from '../../queries/categories';
import { Modal } from '../common/Modal';
import { ErrorAlert } from '../feedback/ErrorAlert';

type DeleteCategoryDialogProps = {
  category?: Category;
  onClose: () => void;
};

export function DeleteCategoryDialog({ category, onClose }: DeleteCategoryDialogProps) {
  const remove = useDeleteCategory();

  function close() {
    remove.reset();
    onClose();
  }

  function confirm() {
    if (!category) return;
    remove.mutate(category.id, {
      onSuccess: () => {
        toast.success(`Category "${category.name}" deleted`);
        close();
      },
      onError: (error) => {
        if (error.status === 404) {
          toast.info('This category was already deleted.');
          close();
        }
      },
    });
  }

  // 409 "in use" (or protected) is explained in the dialog; network/5xx is a toast
  const conflict =
    isApiError(remove.error) && !remove.error.isUnexpected && remove.error.status !== 404
      ? remove.error
      : undefined;

  return (
    <Modal
      open={category !== undefined}
      onClose={close}
      title="Delete category?"
      actions={
        <>
          <button type="button" className="btn btn-ghost" onClick={close}>
            {conflict ? 'Close' : 'Cancel'}
          </button>
          {!conflict && (
            <button
              type="button"
              className="btn btn-error"
              disabled={remove.isPending}
              onClick={confirm}
            >
              {remove.isPending && <span className="loading loading-spinner loading-sm" />}
              Delete
            </button>
          )}
        </>
      }
    >
      {category && (
        <p className="text-sm text-base-content/70">
          The {category.type.toLowerCase()} category <strong>{category.name}</strong> will be
          removed. Categories that are used by transactions cannot be deleted.
        </p>
      )}
      {conflict && (
        <div className="mt-3">
          <ErrorAlert title="Could not delete" error={conflict} />
          {conflict.status === 409 && (
            <p className="mt-2 text-xs text-base-content/60">
              Delete its transactions first, or rename the category instead.
            </p>
          )}
        </div>
      )}
    </Modal>
  );
}
