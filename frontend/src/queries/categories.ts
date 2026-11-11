import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { ApiError } from '../api/ApiError';
import { categoriesApi } from '../api/categoriesApi';
import type { Category, CreateCategoryRequest, TransactionType } from '../api/types';
import { categoryKeys, transactionKeys } from './queryKeys';

export function useCategories(type?: TransactionType) {
  return useQuery<Category[], ApiError>({
    queryKey: categoryKeys.list(type),
    queryFn: ({ signal }) => categoriesApi.list(type, signal),
    // Categories change rarely; the mutations below invalidate them anyway
    staleTime: 5 * 60_000,
  });
}

export function useCreateCategory() {
  const queryClient = useQueryClient();
  return useMutation<Category, ApiError, CreateCategoryRequest>({
    mutationFn: categoriesApi.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: categoryKeys.all }),
  });
}

export function useRenameCategory() {
  const queryClient = useQueryClient();
  return useMutation<Category, ApiError, { id: string; name: string }>({
    mutationFn: ({ id, name }) => categoriesApi.rename(id, { name }),
    // Lists and summaries show category names, so they need the new name too
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: categoryKeys.all }),
        queryClient.invalidateQueries({ queryKey: transactionKeys.all }),
      ]),
  });
}

export function useDeleteCategory() {
  const queryClient = useQueryClient();
  return useMutation<void, ApiError, string>({
    mutationFn: categoriesApi.delete,
    onSettled: () => queryClient.invalidateQueries({ queryKey: categoryKeys.all }),
  });
}
