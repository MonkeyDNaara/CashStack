import type { IsoDate, Transaction } from '../api/types';
import { formatDate } from './format';

export interface DayGroup {
  day: IsoDate;
  transactions: Transaction[];
  /** Income minus expenses of that day */
  net: number;
}

/** Signed amount: income positive, expenses negative */
export function signedAmount(t: Transaction): number {
  return t.type === 'Income' ? t.amount : -t.amount;
}

/** Newest day first, newest transaction first within a day (local days, like the API filter) */
export function groupByDay(transactions: Transaction[]): DayGroup[] {
  const sorted = [...transactions].sort(
    (a, b) => Date.parse(b.timestamp) - Date.parse(a.timestamp),
  );
  const groups: DayGroup[] = [];
  for (const t of sorted) {
    const day = formatDate(t.timestamp);
    let group = groups.at(-1);
    if (!group || group.day !== day) {
      group = { day, transactions: [], net: 0 };
      groups.push(group);
    }
    group.transactions.push(t);
    group.net += signedAmount(t);
  }
  // Rounding: 0.1 + 0.2 must not show 0,30000000000000004
  for (const g of groups) g.net = Math.round(g.net * 100) / 100;
  return groups;
}

const WEEKDAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

/**
 * "Tue 10 Nov", with the year when it is not the current year.
 * Built by hand: toLocaleDateString differs between browsers and Node (commas).
 */
export function formatDayHeading(day: IsoDate, today: Date = new Date()): string {
  const [y, m, d] = day.split('-').map(Number);
  const weekday = WEEKDAYS[new Date(y, m - 1, d).getDay()];
  const text = `${weekday} ${d} ${MONTHS[m - 1]}`;
  return y === today.getFullYear() ? text : `${text} ${y}`;
}
