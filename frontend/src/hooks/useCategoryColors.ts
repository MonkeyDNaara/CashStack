import { useCallback, useMemo } from 'react';
import { buildColorMap, hashColor, type ColorOf } from '../lib/categoryColors';
import { useCategories } from '../queries/categories';

/** Returns colorOf(categoryId); uses the cached category list (no extra request per component) */
export function useCategoryColors(): ColorOf {
  const { data } = useCategories();
  const map = useMemo(() => buildColorMap(data ?? []), [data]);
  return useCallback((categoryId: string) => map.get(categoryId) ?? hashColor(categoryId), [map]);
}
