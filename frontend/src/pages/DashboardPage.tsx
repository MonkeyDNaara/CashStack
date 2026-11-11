import { PageHeader } from '../components/layout/PageHeader';

export function DashboardPage() {
  return (
    <>
      <PageHeader title="Dashboard" description="Income, expenses and balance per category." />
      <div className="card bg-base-100 shadow-sm">
        <div className="card-body text-base-content/70">
          Summary cards and chart follow in #106.
        </div>
      </div>
    </>
  );
}
