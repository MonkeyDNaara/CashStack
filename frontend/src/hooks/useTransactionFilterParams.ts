import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router';
import type { TransactionType } from '../api/types';
import { useDateRangeParams } from './useDateRangeParams';

export interface TransactionFilterState {
  type?: TransactionType;
  categoryId?: string;
  /** Text search in the description, filtered in the browser */
  search: string;
}

const TYPES: TransactionType[] = ['Income', 'Expense'];

/** All filters of the transaction list in the URL: ?from&to&type&categoryId&q */
export function useTransactionFilterParams() {
  const [range, setRange] = useDateRangeParams();
  const [params, setParams] = useSearchParams();

  const typeParam = params.get('type');
  const categoryParam = params.get('categoryId');
  const searchParam = params.get('q') ?? '';

  const filters = useMemo<TransactionFilterState>(
    () => ({
      type: TYPES.find((t) => t === typeParam),
      categoryId: categoryParam || undefined,
      search: searchParam,
    }),
    [typeParam, categoryParam, searchParam],
  );

  const setFilters = useCallback(
    (patch: Partial<TransactionFilterState>) =>
      setParams(
        (current) => {
          const next = new URLSearchParams(current);
          const apply = (key: string, value: string | undefined) =>
            value ? next.set(key, value) : next.delete(key);
          if ('type' in patch) apply('type', patch.type);
          if ('categoryId' in patch) apply('categoryId', patch.categoryId);
          if ('search' in patch) apply('q', patch.search?.trim() ? patch.search : undefined);
          return next;
        },
        { replace: true },
      ),
    [setParams],
  );

  /** Back to this month without type, category and search */
  const reset = useCallback(() => setParams(new URLSearchParams(), { replace: true }), [setParams]);

  const isFiltered =
    filters.type !== undefined || filters.categoryId !== undefined || filters.search !== '';

  return { range, setRange, filters, setFilters, reset, isFiltered };
}
