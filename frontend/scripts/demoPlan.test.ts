import { describe, expect, it } from 'vitest';
import { buildDemoPlan, toRequests } from './demoPlan.ts';

describe('demo plan', () => {
  it('covers three months with about 45 transactions', () => {
    const plan = buildDemoPlan(new Date(2026, 10, 30));

    expect(plan.length).toBeGreaterThanOrEqual(40);
    expect(plan.length).toBeLessThanOrEqual(50);
    expect(plan[0].date).toBe('2026-09-01');
    expect(new Set(plan.map((p) => p.date.slice(0, 7)))).toEqual(
      new Set(['2026-09', '2026-10', '2026-11']),
    );
  });

  it('never plans a day after today', () => {
    const plan = buildDemoPlan(new Date(2026, 10, 11));

    expect(plan.every((p) => p.date <= '2026-11-11')).toBe(true);
    expect(plan.at(-1)?.date).toBe('2026-11-09');
  });

  it('is the same on every run and skips days a month does not have', () => {
    const today = new Date(2027, 1, 28); // February: no 30th
    expect(buildDemoPlan(today)).toEqual(buildDemoPlan(today));
    expect(buildDemoPlan(today).some((p) => p.date === '2027-02-30')).toBe(false);
  });

  it('crosses the year boundary', () => {
    expect(buildDemoPlan(new Date(2027, 0, 31))[0].date).toBe('2026-11-01');
  });

  it('resolves category names and fails clearly for a missing one', () => {
    const plan = buildDemoPlan(new Date(2026, 10, 30)).slice(0, 2);
    expect(toRequests(plan, (name) => `id-${name}`)[0]).toMatchObject({
      categoryId: 'id-Salary',
      date: '2026-09-01',
    });
    expect(() => toRequests(plan, () => undefined)).toThrow(
      'Category "Salary" (Income) does not exist.',
    );
  });
});
