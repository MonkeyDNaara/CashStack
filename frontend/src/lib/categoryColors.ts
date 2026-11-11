import type { Category } from '../api/types';

// Fixed palette (Tailwind 600 tones), readable on white and distinct from each other
export const palette = [
  '#0f766e', // teal (brand)
  '#4f46e5', // indigo
  '#d97706', // amber
  '#db2777', // pink
  '#0284c7', // sky
  '#7c3aed', // violet
  '#65a30d', // lime
  '#dc2626', // red
  '#0891b2', // cyan
  '#c026d3', // fuchsia
] as const;

export const otherColor = '#94a3b8'; // slate: the protected "Other" categories

export type ColorOf = (categoryId: string) => string;

/**
 * One color per category, the same on every page (donut, lists, recent transactions).
 * Colors follow the order of the category list (Income first, then by name), so they are
 * unique as long as there are not more categories than palette entries; protected
 * "Other" categories are always gray.
 */
export function buildColorMap(categories: Category[]): Map<string, string> {
  const map = new Map<string, string>();
  let index = 0;
  for (const category of categories) {
    if (category.isProtected) {
      map.set(category.id, otherColor);
    } else {
      map.set(category.id, palette[index % palette.length]);
      index++;
    }
  }
  return map;
}

/** Fallback while categories are loading or for an unknown id: stable hash into the palette */
export function hashColor(categoryId: string): string {
  let hash = 0;
  for (const char of categoryId) {
    hash = (hash * 31 + char.charCodeAt(0)) >>> 0;
  }
  return palette[hash % palette.length];
}
