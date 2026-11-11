import { QueryClient } from '@tanstack/react-query';

// One factory for the app and the tests, so both use the same defaults
export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // Data of a budget tracker changes only through our own mutations,
        // which invalidate the affected queries explicitly
        staleTime: 30_000,
        retry: 1,
        refetchOnWindowFocus: false,
      },
    },
  });
}
