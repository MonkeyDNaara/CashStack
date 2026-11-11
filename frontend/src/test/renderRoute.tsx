import { QueryClientProvider } from '@tanstack/react-query';
import { render } from '@testing-library/react';
import { createMemoryRouter, RouterProvider } from 'react-router';
import { Toaster } from '../components/feedback/Toaster';
import { createQueryClient } from '../lib/queryClient';
import { routes } from '../routes';

/** Renders the real app routes at the given URL, with a fresh QueryClient per test. */
export function renderRoute(url = '/') {
  const router = createMemoryRouter(routes, { initialEntries: [url] });
  const queryClient = createQueryClient();
  queryClient.setDefaultOptions({ queries: { retry: false } });

  return {
    router,
    ...render(
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
        <Toaster />
      </QueryClientProvider>,
    ),
  };
}
