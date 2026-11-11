import { apiClient } from './client';
import type {
  Category,
  CreateCategoryRequest,
  RenameCategoryRequest,
  TransactionType,
} from './types';

const PATH = '/categories';

export const categoriesApi = {
  list: (type?: TransactionType, signal?: AbortSignal) =>
    apiClient.get<Category[]>(PATH, { query: { type }, signal }),

  get: (id: string, signal?: AbortSignal) =>
    apiClient.get<Category>(`${PATH}/${encodeURIComponent(id)}`, { signal }),

  create: (request: CreateCategoryRequest) => apiClient.post<Category>(PATH, request),

  rename: (id: string, request: RenameCategoryRequest) =>
    apiClient.put<Category>(`${PATH}/${encodeURIComponent(id)}`, request),

  delete: (id: string) => apiClient.delete(`${PATH}/${encodeURIComponent(id)}`),
};
