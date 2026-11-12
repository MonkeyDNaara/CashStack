import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { API_BASE, HEALTH_URL } from '../config';
import { categories, summary, transactions } from './fixtures';

/** URL helper for handlers: api('/transactions') → http://localhost:5198/api/v1/transactions */
export const api = (path: string) => `${API_BASE}${path}`;

// Happy-path defaults; a test overrides single routes with server.use(...)
export const handlers = [
  // The layout pings /health on start (wake-up banner); the server is awake by default
  http.get(HEALTH_URL, () => HttpResponse.text('Healthy')),
  http.get(api('/transactions'), () => HttpResponse.json(transactions)),
  http.get(api('/transactions/summary'), () => HttpResponse.json(summary)),
  // Filters by ?type like the real API
  http.get(api('/categories'), ({ request }) => {
    const type = new URL(request.url).searchParams.get('type');
    return HttpResponse.json(type ? categories.filter((c) => c.type === type) : categories);
  }),
];

export const server = setupServer(...handlers);
