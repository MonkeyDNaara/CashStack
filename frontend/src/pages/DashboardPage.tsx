import { isApiError } from '../api/ApiError';
import { DateRangePicker } from '../components/common/DateRangePicker';
import { ExpensesByCategory } from '../components/dashboard/ExpensesByCategory';
import { IncomeByCategory } from '../components/dashboard/IncomeByCategory';
import { RecentTransactions } from '../components/dashboard/RecentTransactions';
import { StatCards } from '../components/dashboard/StatCards';
import { ErrorAlert } from '../components/feedback/ErrorAlert';
import { PageHeader } from '../components/layout/PageHeader';
import { useDateRangeParams } from '../hooks/useDateRangeParams';
import { useSummary, useTransactions } from '../queries/transactions';

export function DashboardPage() {
  const [range, setRange] = useDateRangeParams();
  const summary = useSummary(range);
  const transactions = useTransactions(range);

  // A wrong range (from after to) is shown at the date fields, not as a page error
  const error = summary.error ?? transactions.error;
  const rangeError =
    isApiError(error) && (error.fieldError('from') || error.fieldError('to')) ? error : undefined;

  function renderContent() {
    if (rangeError) return null;
    if (error) {
      return (
        <ErrorAlert
          title="The dashboard could not be loaded"
          error={error}
          onRetry={() => {
            void summary.refetch();
            void transactions.refetch();
          }}
        />
      );
    }
    if (!summary.data || !transactions.data) return <DashboardSkeleton />;

    const { categories } = summary.data;
    return (
      <div className="flex flex-col gap-4">
        <StatCards summary={summary.data} transactions={transactions.data} />
        <ExpensesByCategory totals={categories.filter((c) => c.type === 'Expense')} />
        <div className="grid gap-4 lg:grid-cols-[2fr_3fr]">
          <IncomeByCategory totals={categories.filter((c) => c.type === 'Income')} />
          <RecentTransactions transactions={transactions.data} range={range} />
        </div>
      </div>
    );
  }

  return (
    <>
      <PageHeader title="Dashboard" description="Income, expenses and balance per category." />
      <div className="mb-6">
        <DateRangePicker
          value={range}
          onChange={setRange}
          fromError={rangeError?.fieldError('from')}
          toError={rangeError?.fieldError('to')}
        />
      </div>

      {renderContent()}
    </>
  );
}

function DashboardSkeleton() {
  return (
    <div className="flex flex-col gap-4" aria-busy="true" aria-label="Loading dashboard">
      <div className="grid gap-4 sm:grid-cols-3">
        {[1, 2, 3].map((i) => (
          <div key={i} className="skeleton h-28 rounded-2xl" />
        ))}
      </div>
      <div className="skeleton h-72 rounded-2xl" />
    </div>
  );
}
