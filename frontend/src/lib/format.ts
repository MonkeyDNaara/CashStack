import type { IsoDate } from '../api/types';

// de-DE: "1.234,50 €" (decision for the UI: English texts, German number format)
const money = new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' });

export function formatMoney(amount: number): string {
  return money.format(amount);
}

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

/** Local calendar date as yyyy-MM-dd (never toISOString: that would be UTC and can shift the day) */
export function toIsoDate(date: Date): IsoDate {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

/** API timestamp (ISO with offset) → "yyyy-MM-dd" in the browser's time zone */
export function formatDate(timestamp: string): string {
  return toIsoDate(new Date(timestamp));
}

/** API timestamp → "yyyy-MM-dd HH:mm" (same format as the console app) */
export function formatDateTime(timestamp: string): string {
  const date = new Date(timestamp);
  return `${toIsoDate(date)} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

/** First day of the month up to today: the same default range as the API */
export function currentMonthRange(today: Date = new Date()): { from: IsoDate; to: IsoDate } {
  const first = new Date(today.getFullYear(), today.getMonth(), 1);
  return { from: toIsoDate(first), to: toIsoDate(today) };
}
