import { PageHeader } from '../components/layout/PageHeader';

export function TransactionsPage() {
  return (
    <>
      <PageHeader title="Transactions" description="All income and expenses with filters." />
      <div className="card bg-base-100 shadow-sm">
        <div className="card-body text-base-content/70">
          List, filters and delete follow in #107.
        </div>
      </div>
    </>
  );
}
