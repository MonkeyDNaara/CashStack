import { describe, expect, it } from 'vitest';
import { currentMonthRange, formatDateTime, formatMoney, toIsoDate } from './format';

// Intl puts a non-breaking space (U+00A0) between amount and "€"
const nbsp = '\u00a0';

describe('format helpers', () => {
  it('formats money in EUR with German separators', () => {
    expect(formatMoney(1234.5)).toBe(`1.234,50${nbsp}€`);
    expect(formatMoney(0)).toBe(`0,00${nbsp}€`);
    expect(formatMoney(-42.5)).toBe(`-42,50${nbsp}€`);
  });

  it('uses the local date, not UTC', () => {
    // 00:30 local time would be the previous day in UTC for time zones east of UTC
    expect(toIsoDate(new Date(2026, 0, 5, 0, 30))).toBe('2026-01-05');
  });

  it('formats a timestamp like the console app', () => {
    const local = new Date(2026, 10, 10, 9, 5);
    expect(formatDateTime(local.toISOString())).toBe('2026-11-10 09:05');
  });

  it('returns the current month up to today, like the API default', () => {
    expect(currentMonthRange(new Date(2026, 10, 10))).toEqual({
      from: '2026-11-01',
      to: '2026-11-10',
    });
  });
});
