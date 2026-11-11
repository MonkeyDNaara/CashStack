import { renderHook, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { toastStore } from '../lib/toast';
import { createWrapper } from '../test/createWrapper';
import { categories, ids, problems, summary, transactions } from '../test/fixtures';
import { api, server } from '../test/server';
import { useCategories, useRenameCategory } from './categories';
import { useCreateTransaction, useSummary, useTransactions } from './transactions';

describe('query hooks', () => {
  it('loads the summary for a date range', async () => {
    const { wrapper } = createWrapper();

    const { result } = renderHook(() => useSummary({ from: '2026-11-01', to: '2026-11-10' }), {
      wrapper,
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(summary);
  });

  it('exposes the ApiError of a failed load and shows no toast for it', async () => {
    server.use(http.get(api('/categories'), () => HttpResponse.error()));
    const { wrapper } = createWrapper();

    const { result } = renderHook(() => useCategories(), { wrapper });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error?.isNetworkError).toBe(true);
    // The page shows the first failed load as alert, not as toast
    expect(toastStore.getSnapshot()).toHaveLength(0);
  });

  it('refreshes lists and summaries after creating a transaction', async () => {
    let summaryCalls = 0;
    server.use(
      http.get(api('/transactions/summary'), () => {
        summaryCalls++;
        return HttpResponse.json(summary);
      }),
      http.post(api('/transactions'), () => HttpResponse.json(transactions[0], { status: 201 })),
    );
    const { wrapper } = createWrapper();

    const { result } = renderHook(
      () => ({ summary: useSummary(), list: useTransactions(), create: useCreateTransaction() }),
      { wrapper },
    );
    await waitFor(() => expect(result.current.summary.isSuccess).toBe(true));
    expect(summaryCalls).toBe(1);

    await result.current.create.mutateAsync({
      type: 'Expense',
      categoryId: ids.groceries,
      description: 'Weekly shopping',
      amount: 42.5,
    });

    await waitFor(() => expect(summaryCalls).toBe(2));
  });

  it('keeps a 409 in the mutation (for the form) without a toast', async () => {
    server.use(
      http.put(api('/categories/:id'), () => HttpResponse.json(problems.conflict, { status: 409 })),
    );
    const { wrapper } = createWrapper();

    const { result } = renderHook(() => useRenameCategory(), { wrapper });
    result.current.mutate({ id: categories[3].id, name: 'Groceries' });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error?.status).toBe(409);
    expect(toastStore.getSnapshot()).toHaveLength(0);
  });

  it('shows a toast when a mutation fails with a server error', async () => {
    server.use(
      http.post(api('/transactions'), () =>
        HttpResponse.json(problems.serverError, { status: 500 }),
      ),
    );
    const { wrapper } = createWrapper();

    const { result } = renderHook(() => useCreateTransaction(), { wrapper });
    result.current.mutate({ type: 'Income', categoryId: ids.salary, description: 'x', amount: 1 });

    await waitFor(() => expect(toastStore.getSnapshot()).toHaveLength(1));
    expect(toastStore.getSnapshot()[0]).toMatchObject({ kind: 'error' });
  });
});
