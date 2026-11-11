import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import type { Transaction } from '../api/types';
import { ids, problems, transactions } from '../test/fixtures';
import { renderRoute } from '../test/renderRoute';
import { api, server } from '../test/server';

const START = '/transactions?from=2026-11-01&to=2026-11-10';

/** Records the query string of every list request */
function recordListRequests(data: Transaction[] = transactions) {
  const seen: Record<string, string>[] = [];
  server.use(
    http.get(api('/transactions'), ({ request }) => {
      seen.push(Object.fromEntries(new URL(request.url).searchParams));
      return HttpResponse.json(data);
    }),
  );
  return seen;
}

describe('TransactionsPage', () => {
  it('lists transactions grouped by day with count and sum', async () => {
    renderRoute(START);

    const list = await screen.findByRole('region', { name: 'Transaction list' });
    expect(await within(list).findByRole('region', { name: '2026-11-10' })).toHaveTextContent(
      'Weekly shopping',
    );
    expect(within(list).getByRole('region', { name: '2026-11-01' })).toHaveTextContent('Salary');
    expect(list).toHaveTextContent('2 transactions');
    expect(list).toHaveTextContent('+2.457,50');
  });

  it('sends range, type and category to the API and keeps them in the URL', async () => {
    const user = userEvent.setup();
    const seen = recordListRequests();
    const { router } = renderRoute(START);
    await screen.findByText('Weekly shopping');

    await user.click(screen.getByRole('button', { name: 'Expenses' }));
    await user.selectOptions(await screen.findByLabelText('Category'), 'Groceries');

    await waitFor(() =>
      expect(seen.at(-1)).toEqual({
        from: '2026-11-01',
        to: '2026-11-10',
        type: 'Expense',
        categoryId: ids.groceries,
      }),
    );
    expect(router.state.location.search).toContain('type=Expense');
    expect(router.state.location.search).toContain(`categoryId=${ids.groceries}`);
  });

  it('drops a category of the other type when the type changes', async () => {
    const user = userEvent.setup();
    const { router } = renderRoute(`${START}&type=Expense&categoryId=${ids.groceries}`);
    await screen.findByText('Weekly shopping');

    await user.click(screen.getByRole('button', { name: 'Income' }));

    expect(router.state.location.search).toContain('type=Income');
    expect(router.state.location.search).not.toContain('categoryId');
  });

  it('searches the description in the browser', async () => {
    const user = userEvent.setup();
    const seen = recordListRequests();
    const { router } = renderRoute(START);
    await screen.findByText('Weekly shopping');
    const requests = seen.length;

    await user.type(screen.getByLabelText('Search description'), 'week');

    const list = screen.getByRole('region', { name: 'Transaction list' });
    expect(within(list).getByText('Weekly shopping')).toBeInTheDocument();
    expect(within(list).queryByText('Salary')).not.toBeInTheDocument();
    expect(router.state.location.search).toContain('q=week');
    expect(seen).toHaveLength(requests); // no extra API call
  });

  it('offers a reset when the filters match nothing', async () => {
    const user = userEvent.setup();
    const { router } = renderRoute(`${START}&q=nothing-like-this`);

    expect(await screen.findByText('No transactions match your filters.')).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Reset filters' }));

    expect(router.state.location.search).toBe('');
    expect(await screen.findByText('Weekly shopping')).toBeInTheDocument();
  });

  it('deletes a transaction after confirmation', async () => {
    const user = userEvent.setup();
    let deleted: string | undefined;
    let remaining = transactions;
    server.use(
      http.get(api('/transactions'), () => HttpResponse.json(remaining)),
      http.delete(api('/transactions/:id'), ({ params }) => {
        deleted = String(params.id);
        remaining = transactions.filter((t) => t.id !== deleted);
        return new HttpResponse(null, { status: 204 });
      }),
    );
    renderRoute(START);

    await user.click(await screen.findByRole('button', { name: 'Delete Weekly shopping' }));
    const dialog = screen.getByRole('dialog', { name: 'Delete transaction?' });
    expect(dialog).toHaveTextContent('-42,50'.replace('-', '−'));
    await user.click(within(dialog).getByRole('button', { name: 'Delete' }));

    expect(await screen.findByRole('status')).toHaveTextContent('Transaction deleted');
    expect(deleted).toBe(transactions[0].id);
    await waitFor(() => expect(screen.queryByText('Weekly shopping')).not.toBeInTheDocument());
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('does not delete when the dialog is cancelled', async () => {
    const user = userEvent.setup();
    let calls = 0;
    server.use(
      http.delete(api('/transactions/:id'), () => {
        calls++;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    renderRoute(START);

    await user.click(await screen.findByRole('button', { name: 'Delete Salary' }));
    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(calls).toBe(0);
  });

  it('tells the user when the transaction was already deleted (404)', async () => {
    const user = userEvent.setup();
    server.use(
      http.delete(api('/transactions/:id'), () =>
        HttpResponse.json(problems.notFound, { status: 404 }),
      ),
    );
    renderRoute(START);

    await user.click(await screen.findByRole('button', { name: 'Delete Salary' }));
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Delete' }));

    expect(await screen.findByRole('status')).toHaveTextContent('already deleted');
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('shows an error alert with retry when the list cannot be loaded', async () => {
    const user = userEvent.setup();
    server.use(
      http.get(api('/transactions'), () =>
        HttpResponse.json(problems.serverError, { status: 500 }),
      ),
    );
    renderRoute(START);

    const alert = await screen.findByRole('alert');
    expect(alert).toHaveTextContent('could not be loaded');

    server.resetHandlers();
    await user.click(within(alert).getByRole('button', { name: 'Try again' }));
    expect(await screen.findByText('Weekly shopping')).toBeInTheDocument();
  });

  it('shows a wrong range at the date fields', async () => {
    server.use(
      http.get(api('/transactions'), () =>
        HttpResponse.json(
          { ...problems.validation, errors: { from: ["'from' must not be after 'to'."] } },
          { status: 400 },
        ),
      ),
    );
    renderRoute('/transactions?from=2026-12-01&to=2026-11-01');

    expect(await screen.findByRole('alert')).toHaveTextContent("must not be after 'to'");
    expect(screen.getByLabelText('From')).toHaveAttribute('aria-invalid', 'true');
  });
});
