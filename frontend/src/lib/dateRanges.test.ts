import { describe, expect, it } from 'vitest';
import { isIsoDate, matchPreset, presetRange } from './dateRanges';

const today = new Date(2026, 0, 15); // 2026-01-15: "last month" lies in the previous year

describe('date range presets', () => {
  it('computes every preset up to today', () => {
    expect(presetRange('thisMonth', today)).toEqual({ from: '2026-01-01', to: '2026-01-15' });
    expect(presetRange('lastMonth', today)).toEqual({ from: '2025-12-01', to: '2025-12-31' });
    expect(presetRange('last3Months', today)).toEqual({ from: '2025-11-01', to: '2026-01-15' });
    expect(presetRange('thisYear', today)).toEqual({ from: '2026-01-01', to: '2026-01-15' });
  });

  it('handles months with fewer days', () => {
    expect(presetRange('lastMonth', new Date(2026, 2, 31))).toEqual({
      from: '2026-02-01',
      to: '2026-02-28',
    });
  });

  it('recognises the preset of a range', () => {
    expect(matchPreset({ from: '2025-12-01', to: '2025-12-31' }, today)).toBe('lastMonth');
    expect(matchPreset({ from: '2025-12-02', to: '2025-12-31' }, today)).toBeUndefined();
  });

  it('accepts only real yyyy-MM-dd dates', () => {
    expect(isIsoDate('2026-11-01')).toBe(true);
    expect(isIsoDate('2026-13-01')).toBe(false);
    expect(isIsoDate('01.11.2026')).toBe(false);
    expect(isIsoDate(null)).toBe(false);
  });
});
