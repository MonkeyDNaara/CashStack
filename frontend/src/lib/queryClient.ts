import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query';
import { isApiError } from '../api/ApiError';
import { toast } from './toast';

/** Network and 5xx errors: the user cannot fix them in a form, so they go to a toast */
function toastIfUnexpected(error: unknown) {
  if (isApiError(error) && error.isUnexpected) {
    toast.error(error.userMessage);
  } else if (!isApiError(error)) {
    toast.error('An unexpected error occurred.');
  }
}

// One factory for the app and the tests, so both use the same defaults
export function createQueryClient(): QueryClient {
  return new QueryClient({
    queryCache: new QueryCache({
      // A failed first load is shown by the page itself (alert + retry).
      // Only a failed background refresh, when old data is still on screen, gets a toast.
      onError: (error, query) => {
        if (query.state.data !== undefined) toastIfUnexpected(error);
      },
    }),
    mutationCache: new MutationCache({
      // 4xx (validation, 404, 409) is shown in the form/dialog that sent the request
      onError: toastIfUnexpected,
    }),
    defaultOptions: {
      queries: {
        // Data of a budget tracker changes only through our own mutations,
        // which invalidate the affected queries explicitly
        staleTime: 30_000,
        // Retrying a 400/404 cannot succeed; network and 5xx errors get one retry
        retry: (failureCount, error) =>
          failureCount < 1 && (!isApiError(error) || error.isUnexpected),
        refetchOnWindowFocus: false,
      },
    },
  });
}
