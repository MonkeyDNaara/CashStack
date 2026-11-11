import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { api, server } from '../test/server';
import { categories, ids, problems, transactions } from '../test/fixtures';
import { ApiError } from './ApiError';
import { categoriesApi } from './categoriesApi';
import { buildUrl } from './client';
import { transactionsApi } from './transactionsApi';

/** Runs the call and returns the ApiError it must throw */
async function catchApiError(call: () => Promise<unknown>): Promise<ApiError> {
  try {
    await call();
  } catch (error) {
    expect(error).toBeInstanceOf(ApiError);
    return error as ApiError;
  }
  throw new Error('Expected the call to fail');
}

describe('buildUrl', () => {
  it('leaves out empty filter values', () => {
    expect(buildUrl('/transactions', { from: '2026-11-01', to: undefined, type: '' })).toBe(
      'http://localhost:5198/api/v1/transactions?from=2026-11-01',
    );
  });
});

describe('API client', () => {
  it('sends the filters as query parameters', async () => {
    let query: URLSearchParams | undefined;
    server.use(
      http.get(api('/transactions'), ({ request }) => {
        query = new URL(request.url).searchParams;
        return HttpResponse.json(transactions);
      }),
    );

    const result = await transactionsApi.list({
      from: '2026-11-01',
      to: '2026-11-30',
      type: 'Expense',
      categoryId: ids.groceries,
    });

    expect(result).toEqual(transactions);
    expect(Object.fromEntries(query!)).toEqual({
      from: '2026-11-01',
      to: '2026-11-30',
      type: 'Expense',
      categoryId: ids.groceries,
    });
  });

  it('posts JSON and returns the created resource', async () => {
    let body: unknown;
    server.use(
      http.post(api('/categories'), async ({ request }) => {
        body = await request.json();
        return HttpResponse.json(categories[0], { status: 201 });
      }),
    );

    const created = await categoriesApi.create({ name: 'Bonus', type: 'Income' });

    expect(body).toEqual({ name: 'Bonus', type: 'Income' });
    expect(created).toEqual(categories[0]);
  });

  it('handles 204 No Content on delete', async () => {
    server.use(
      http.delete(api('/transactions/:id'), () => new HttpResponse(null, { status: 204 })),
    );

    await expect(transactionsApi.delete(transactions[0].id)).resolves.toBeUndefined();
  });

  it('turns a 400 validation response into field errors', async () => {
    server.use(
      http.post(api('/transactions'), () =>
        HttpResponse.json(problems.validation, { status: 400 }),
      ),
    );

    const error = await catchApiError(() =>
      transactionsApi.create({
        type: 'Expense',
        categoryId: ids.groceries,
        description: '',
        amount: 0,
      }),
    );

    expect(error.status).toBe(400);
    expect(error.fieldError('amount')).toMatch(/between 0.01/);
    expect(error.fieldError('description')).toMatch(/required/);
    expect(error.traceId).toBe('00-abc-01');
  });

  it('keeps the detail of a 409 conflict', async () => {
    server.use(
      http.post(api('/categories'), () => HttpResponse.json(problems.conflict, { status: 409 })),
    );

    const error = await catchApiError(() =>
      categoriesApi.create({ name: 'rent', type: 'Expense' }),
    );

    expect(error.status).toBe(409);
    expect(error.userMessage).toBe("An Expense category named 'rent' already exists.");
  });

  it('reports 404 with the title of the status code page', async () => {
    server.use(
      http.get(api('/transactions/:id'), () =>
        HttpResponse.json(problems.notFound, { status: 404 }),
      ),
    );

    const error = await catchApiError(() =>
      transactionsApi.get('00000000-0000-0000-0000-000000000000'),
    );

    expect(error.status).toBe(404);
    expect(error.userMessage).toBe('Not Found');
  });

  it('hides server details on 500', async () => {
    server.use(
      http.get(api('/transactions/summary'), () =>
        HttpResponse.json(problems.serverError, { status: 500 }),
      ),
    );

    const error = await catchApiError(() => transactionsApi.summary());

    expect(error.isServerError).toBe(true);
    expect(error.userMessage).toMatch(/try again later/);
  });

  it('copes with a non-JSON error body', async () => {
    server.use(
      http.get(
        api('/categories'),
        () => new HttpResponse('<html>Bad gateway</html>', { status: 502 }),
      ),
    );

    const error = await catchApiError(() => categoriesApi.list());

    expect(error.status).toBe(502);
    expect(error.isUnexpected).toBe(true);
  });

  it('reports a network error when the API is not reachable', async () => {
    server.use(http.get(api('/categories'), () => HttpResponse.error()));

    const error = await catchApiError(() => categoriesApi.list('Expense'));

    expect(error.isNetworkError).toBe(true);
  });
});
