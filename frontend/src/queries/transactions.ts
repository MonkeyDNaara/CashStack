import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { ApiError } from '../api/ApiError';
import { transactionsApi } from '../api/transactionsApi';
import type {
  CreateTransactionRequest,
  DateRange,
  Summary,
  Transaction,
  TransactionFilters,
} from '../api/types';
import { transactionKeys } from './queryKeys';

export function useTransactions(filters: TransactionFilters = {}) {
  return useQuery<Transaction[], ApiError>({
    queryKey: transactionKeys.list(filters),
    queryFn: ({ signal }) => transactionsApi.list(filters, signal),
  });
}

export function useSummary(range: DateRange = {}) {
  return useQuery<Summary, ApiError>({
    queryKey: transactionKeys.summary(range),
    queryFn: ({ signal }) => transactionsApi.summary(range, signal),
  });
}

export function useCreateTransaction() {
  const queryClient = useQueryClient();
  return useMutation<Transaction, ApiError, CreateTransactionRequest>({
    mutationFn: transactionsApi.create,
    // Lists and summaries of every range may contain the new transaction
    onSuccess: () => queryClient.invalidateQueries({ queryKey: transactionKeys.all }),
  });
}

export function useDeleteTransaction() {
  const queryClient = useQueryClient();
  return useMutation<void, ApiError, string>({
    mutationFn: transactionsApi.delete,
    // Also refresh after 404: the transaction is gone either way
    onSettled: () => queryClient.invalidateQueries({ queryKey: transactionKeys.all }),
  });
}
