import { describe, expect, it } from 'vitest';
import { categories } from '../test/fixtures';
import { buildColorMap, hashColor, otherColor, palette } from './categoryColors';

describe('category colors', () => {
  it('gives every category its own color and "Other" gray', () => {
    const map = buildColorMap(categories);

    const colors = categories.filter((c) => !c.isProtected).map((c) => map.get(c.id));
    expect(new Set(colors).size).toBe(colors.length);
    expect(map.get(categories.find((c) => c.isProtected)!.id)).toBe(otherColor);
  });

  it('falls back to a stable palette color for unknown ids', () => {
    expect(hashColor('abc')).toBe(hashColor('abc'));
    expect(palette).toContain(hashColor('abc'));
  });
});
