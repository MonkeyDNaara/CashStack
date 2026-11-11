// Same rules as the API (Category.MaxNameLength, trimmed, required)
export const CATEGORY_NAME_MAX = 50;

export function validateCategoryName(name: string): string | undefined {
  const trimmed = name.trim();
  if (!trimmed) return 'Enter a name.';
  if (trimmed.length > CATEGORY_NAME_MAX) return `At most ${CATEGORY_NAME_MAX} characters.`;
  return undefined;
}
