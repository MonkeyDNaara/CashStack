import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { API_BASE } from '../config';
import { categories, summary, transactions } from './fixtures';

/** URL helper for handlers: api('/transactions') → http://localhost:5198/api/v1/transactions */
export const api = (path: string) => `${API_BASE}${path}`;

// Happy-path defaults; a test overrides single routes with server.use(...)
export const handlers = [
  http.get(api('/transactions'), () => HttpResponse.json(transactions)),
  http.get(api('/transactions/summary'), () => HttpResponse.json(summary)),
  http.get(api('/categories'), () => HttpResponse.json(categories)),
];

export const server = setupServer(...handlers);
