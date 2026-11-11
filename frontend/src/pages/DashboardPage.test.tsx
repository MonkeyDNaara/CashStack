import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import type { Transaction } from '../api/types';
import { ids, problems, transactions } from '../test/fixtures';
import { renderRoute } from '../test/renderRoute';
import { api, server } from '../test/server';

const START = '/?from=2026-11-01&to=2026-11-10';

describe('DashboardPage', () => {
  it('shows income, expenses and balance of the summary', async () => {
    renderRoute(START);

    const income = await screen.findByRole('region', { name: 'Income' });
    expect(income).toHaveTextContent('2.500,00');
    expect(income).toHaveTextContent('1 transaction');
    expect(screen.getByRole('region', { name: 'Expenses' })).toHaveTextContent('42,50');
    expect(screen.getByRole('region', { name: 'Balance' })).toHaveTextContent('2.457,50');
  });

  it('lists expenses and income per category', async () => {
    renderRoute(START);

    const expenses = await screen.findByRole('region', { name: 'Expenses by category' });
    expect(within(expenses).getByText('Groceries')).toBeInTheDocument();
    expect(within(expenses).getByText('100 %')).toBeInTheDocument();
    const income = screen.getByRole('region', { name: 'Income by category' });
    expect(within(income).getByText('Salary')).toBeInTheDocument();
  });

  it('sends the range from the URL to the API', async () => {
    const seen: string[] = [];
    server.use(
      http.get(api('/transactions/summary'), ({ request }) => {
        seen.push(new URL(request.url).search);
        return HttpResponse.json({
          from: '',
          to: '',
          income: 0,
          expenses: 0,
          balance: 0,
          categories: [],
        });
      }),
    );

    renderRoute('/?from=2026-10-01&to=2026-10-31');

    await waitFor(() => expect(seen).toContain('?from=2026-10-01&to=2026-10-31'));
  });

  it('switches the range with a preset and keeps it in the URL', async () => {
    const user = userEvent.setup();
    const { router } = renderRoute(START);
    await screen.findByRole('region', { name: 'Income' });

    await user.click(screen.getByRole('button', { name: 'This year' }));

    expect(router.state.location.search).toMatch(/from=\d{4}-01-01/);
    expect(screen.getByRole('button', { name: 'This year' })).toHaveAttribute(
      'aria-pressed',
      'true',
    );
  });

  it('shows the 5 newest transactions and links to the full list', async () => {
    const many: Transaction[] = Array.from({ length: 7 }, (_, i) => ({
      ...transactions[0],
      id: `id-${i}`,
      description: `Purchase ${i + 1}`,
      timestamp: `2026-11-0${i + 1}T10:00:00+01:00`,
    }));
    server.use(http.get(api('/transactions'), () => HttpResponse.json(many)));

    renderRoute(START);

    const recent = await screen.findByRole('region', { name: 'Recent transactions' });
    const items = within(recent).getAllByRole('listitem');
    expect(items).toHaveLength(5);
    expect(items[0]).toHaveTextContent('Purchase 7');
    expect(within(recent).getByRole('link', { name: /View all/ })).toHaveAttribute(
      'href',
      '/transactions?from=2026-11-01&to=2026-11-10',
    );
  });

  it('shows empty states for a period without transactions', async () => {
    server.use(
      http.get(api('/transactions'), () => HttpResponse.json([])),
      http.get(api('/transactions/summary'), () =>
        HttpResponse.json({ from: '', to: '', income: 0, expenses: 0, balance: 0, categories: [] }),
      ),
    );

    renderRoute(START);

    expect(await screen.findByText('No expenses in this period.')).toBeInTheDocument();
    expect(screen.getByText('No income in this period.')).toBeInTheDocument();
    expect(screen.getByText('No transactions in this period.')).toBeInTheDocument();
  });

  it('shows an error alert with retry when the API is down', async () => {
    const user = userEvent.setup();
    server.use(http.get(api('/transactions/summary'), () => HttpResponse.error()));

    renderRoute(START);

    const alert = await screen.findByRole('alert');
    expect(alert).toHaveTextContent('not reachable');

    server.resetHandlers();
    await user.click(within(alert).getByRole('button', { name: 'Try again' }));
    expect(await screen.findByRole('region', { name: 'Income' })).toBeInTheDocument();
  });

  it('shows a wrong range at the date fields instead of a page error', async () => {
    const rangeProblem = {
      ...problems.validation,
      errors: { from: ["'from' (2026-12-01) must not be after 'to' (2026-11-01)."] },
    };
    server.use(
      http.get(api('/transactions/summary'), () =>
        HttpResponse.json(rangeProblem, { status: 400 }),
      ),
      http.get(api('/transactions'), () => HttpResponse.json(rangeProblem, { status: 400 })),
    );

    renderRoute('/?from=2026-12-01&to=2026-11-01');

    expect(await screen.findByRole('alert')).toHaveTextContent("must not be after 'to'");
    expect(screen.getByLabelText('From')).toHaveAttribute('aria-invalid', 'true');
    expect(screen.queryByText('The dashboard could not be loaded')).not.toBeInTheDocument();
  });

  it('falls back to this month for invalid URL values', async () => {
    let search = '';
    server.use(
      http.get(api('/transactions/summary'), ({ request }) => {
        search = new URL(request.url).search;
        return HttpResponse.json({
          from: '',
          to: '',
          income: 0,
          expenses: 0,
          balance: 0,
          categories: [],
        });
      }),
    );

    renderRoute('/?from=yesterday&to=' + ids.groceries);

    await waitFor(() => expect(search).toMatch(/^\?from=\d{4}-\d{2}-01&to=\d{4}-\d{2}-\d{2}$/));
  });
});
