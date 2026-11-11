import { useState } from 'react';
import { isApiError } from '../api/ApiError';
import type { Transaction } from '../api/types';
import { ErrorAlert } from '../components/feedback/ErrorAlert';
import { PageHeader } from '../components/layout/PageHeader';
import { DeleteTransactionDialog } from '../components/transactions/DeleteTransactionDialog';
import { NewTransactionButton } from '../components/transactions/NewTransactionButton';
import { TransactionFilters } from '../components/transactions/TransactionFilters';
import { TransactionList } from '../components/transactions/TransactionList';
import { useTransactionFilterParams } from '../hooks/useTransactionFilterParams';
import { formatNetMoney } from '../lib/format';
import { signedAmount } from '../lib/groupByDay';
import { useTransactions } from '../queries/transactions';

export function TransactionsPage() {
  const { range, setRange, filters, setFilters, reset, isFiltered } = useTransactionFilterParams();
  const [toDelete, setToDelete] = useState<Transaction>();

  // Range, type and category go to the API; the text search runs on the loaded list
  const query = useTransactions({ ...range, type: filters.type, categoryId: filters.categoryId });
  const search = filters.search.trim().toLowerCase();
  const visible = (query.data ?? []).filter((t) => t.description.toLowerCase().includes(search));

  const rangeError =
    isApiError(query.error) && (query.error.fieldError('from') || query.error.fieldError('to'))
      ? query.error
      : undefined;

  function renderContent() {
    if (rangeError) return null;
    if (query.error) {
      return (
        <ErrorAlert
          title="The transactions could not be loaded"
          error={query.error}
          onRetry={() => void query.refetch()}
        />
      );
    }
    if (!query.data) {
      return (
        <div className="flex flex-col gap-3" aria-busy="true" aria-label="Loading transactions">
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="skeleton h-12 rounded-xl" />
          ))}
        </div>
      );
    }
    if (visible.length === 0) {
      return (
        <div className="py-10 text-center text-base-content/60">
          <p>
            {isFiltered ? 'No transactions match your filters.' : 'No transactions in this period.'}
          </p>
          {isFiltered && (
            <button type="button" className="btn btn-ghost btn-sm mt-2" onClick={reset}>
              Reset filters
            </button>
          )}
        </div>
      );
    }

    const net = Math.round(visible.reduce((s, t) => s + signedAmount(t), 0) * 100) / 100;
    return (
      <>
        <TransactionList transactions={visible} onDelete={setToDelete} />
        <div className="mt-3 flex justify-between border-t border-base-200 pt-3 text-sm text-base-content/60">
          <span>
            {visible.length} {visible.length === 1 ? 'transaction' : 'transactions'}
          </span>
          <span>
            Sum: <strong className="text-base-content tabular-nums">{formatNetMoney(net)}</strong>
          </span>
        </div>
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Transactions"
        description="All income and expenses with filters."
        actions={<NewTransactionButton defaultType={filters.type} />}
      />
      <div className="flex flex-col gap-4">
        <TransactionFilters
          range={range}
          onRangeChange={setRange}
          filters={filters}
          onChange={setFilters}
          onReset={reset}
          fromError={rangeError?.fieldError('from')}
          toError={rangeError?.fieldError('to')}
        />
        <section className="card rounded-2xl bg-base-100 shadow-sm" aria-label="Transaction list">
          <div className="card-body pt-2">{renderContent()}</div>
        </section>
      </div>
      <DeleteTransactionDialog transaction={toDelete} onClose={() => setToDelete(undefined)} />
    </>
  );
}
