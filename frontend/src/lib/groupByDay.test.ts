import { describe, expect, it } from 'vitest';
import type { Transaction } from '../api/types';
import { transactions } from '../test/fixtures';
import { formatDayHeading, groupByDay } from './groupByDay';

const at = (
  timestamp: string,
  amount: number,
  type: Transaction['type'] = 'Expense',
): Transaction => ({
  ...transactions[0],
  id: timestamp,
  timestamp,
  amount,
  type,
});

describe('groupByDay', () => {
  it('groups by day, newest day and newest transaction first', () => {
    const groups = groupByDay([
      at('2026-11-01T08:00:00+01:00', 10),
      at('2026-11-03T09:00:00+01:00', 5),
      at('2026-11-03T18:00:00+01:00', 7),
    ]);

    expect(groups.map((g) => g.day)).toEqual(['2026-11-03', '2026-11-01']);
    expect(groups[0].transactions.map((t) => t.amount)).toEqual([7, 5]);
  });

  it('sums income minus expenses per day without floating point noise', () => {
    const [day] = groupByDay([
      at('2026-11-03T09:00:00+01:00', 0.1),
      at('2026-11-03T10:00:00+01:00', 0.2),
      at('2026-11-03T11:00:00+01:00', 1, 'Income'),
    ]);

    expect(day.net).toBe(0.7);
  });

  it('formats the heading with the year only for other years', () => {
    const today = new Date(2026, 10, 11);
    expect(formatDayHeading('2026-11-10', today)).toBe('Tue 10 Nov');
    expect(formatDayHeading('2025-12-24', today)).toBe('Wed 24 Dec 2025');
  });
});
