import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { useState } from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router';
import { Toaster } from './components/feedback/Toaster';
import { createQueryClient } from './lib/queryClient';
import { routes } from './routes';

const router = createBrowserRouter(routes);

export function App() {
  // useState keeps one QueryClient for the lifetime of the app (also with StrictMode)
  const [queryClient] = useState(createQueryClient);

  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
      <Toaster />
      {import.meta.env.DEV && <ReactQueryDevtools buttonPosition="bottom-left" />}
    </QueryClientProvider>
  );
}
