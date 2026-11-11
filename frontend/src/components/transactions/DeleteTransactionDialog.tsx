import { isApiError } from '../../api/ApiError';
import type { Transaction } from '../../api/types';
import { useCategoryColors } from '../../hooks/useCategoryColors';
import { formatDate } from '../../lib/format';
import { toast } from '../../lib/toast';
import { useDeleteTransaction } from '../../queries/transactions';
import { ErrorAlert } from '../feedback/ErrorAlert';
import { Modal } from '../common/Modal';
import { TransactionRow } from './TransactionRow';

type DeleteTransactionDialogProps = {
  /** The transaction to delete; undefined = dialog closed */
  transaction?: Transaction;
  onClose: () => void;
};

export function DeleteTransactionDialog({ transaction, onClose }: DeleteTransactionDialogProps) {
  const colorOf = useCategoryColors();
  const remove = useDeleteTransaction();

  function close() {
    remove.reset();
    onClose();
  }

  function confirm() {
    if (!transaction) return;
    remove.mutate(transaction.id, {
      onSuccess: () => {
        toast.success('Transaction deleted');
        close();
      },
      onError: (error) => {
        // Already gone (deleted in another tab): the list refreshes anyway (onSettled)
        if (error.status === 404) {
          toast.info('This transaction was already deleted.');
          close();
        }
        // Network/5xx: the global toast is shown, the dialog stays open to try again
      },
    });
  }

  // 4xx other than 404 is shown in the dialog; unexpected errors only as toast
  const inlineError =
    isApiError(remove.error) && !remove.error.isUnexpected && remove.error.status !== 404
      ? remove.error
      : undefined;

  return (
    <Modal
      open={transaction !== undefined}
      onClose={close}
      title="Delete transaction?"
      actions={
        <>
          <button type="button" className="btn btn-ghost" onClick={close}>
            Cancel
          </button>
          <button
            type="button"
            className="btn btn-error"
            disabled={remove.isPending}
            onClick={confirm}
          >
            {remove.isPending && <span className="loading loading-spinner loading-sm" />}
            Delete
          </button>
        </>
      }
    >
      <p className="mb-3 text-sm text-base-content/70">This cannot be undone.</p>
      {transaction && (
        <div className="rounded-xl bg-base-200 px-3">
          <TransactionRow
            transaction={transaction}
            color={colorOf(transaction.categoryId)}
            meta={`${formatDate(transaction.timestamp)} · ${transaction.categoryName}`}
          />
        </div>
      )}
      {inlineError && (
        <div className="mt-3">
          <ErrorAlert title="Could not delete" error={inlineError} />
        </div>
      )}
    </Modal>
  );
}
