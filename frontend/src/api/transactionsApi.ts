import { apiClient } from './client';
import type {
  CreateTransactionRequest,
  DateRange,
  Summary,
  Transaction,
  TransactionFilters,
} from './types';

const PATH = '/transactions';

export const transactionsApi = {
  list: (filters: TransactionFilters = {}, signal?: AbortSignal) =>
    apiClient.get<Transaction[]>(PATH, { query: { ...filters }, signal }),

  summary: (range: DateRange = {}, signal?: AbortSignal) =>
    apiClient.get<Summary>(`${PATH}/summary`, { query: { ...range }, signal }),

  get: (id: string, signal?: AbortSignal) =>
    apiClient.get<Transaction>(`${PATH}/${encodeURIComponent(id)}`, { signal }),

  create: (request: CreateTransactionRequest) => apiClient.post<Transaction>(PATH, request),

  delete: (id: string) => apiClient.delete(`${PATH}/${encodeURIComponent(id)}`),
};
