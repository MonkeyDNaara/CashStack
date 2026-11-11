import { describe, expect, it } from 'vitest';
import { validateCategoryName } from './categoryName';

describe('validateCategoryName', () => {
  it('requires a name with at most 50 characters after trimming', () => {
    expect(validateCategoryName('  ')).toBe('Enter a name.');
    expect(validateCategoryName('x'.repeat(51))).toContain('50');
    expect(validateCategoryName(`  ${'x'.repeat(50)}  `)).toBeUndefined();
  });
});
