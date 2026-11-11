import type { DateRange, TransactionFilters, TransactionType } from '../api/types';

// Query key factory: one place for all keys, so invalidation can target a whole group.
// invalidateQueries({ queryKey: transactionKeys.all }) refreshes lists AND summaries.
export const transactionKeys = {
  all: ['transactions'] as const,
  lists: () => [...transactionKeys.all, 'list'] as const,
  list: (filters: TransactionFilters) => [...transactionKeys.lists(), filters] as const,
  summaries: () => [...transactionKeys.all, 'summary'] as const,
  summary: (range: DateRange) => [...transactionKeys.summaries(), range] as const,
};

export const categoryKeys = {
  all: ['categories'] as const,
  list: (type?: TransactionType) => [...categoryKeys.all, 'list', type ?? 'all'] as const,
};
