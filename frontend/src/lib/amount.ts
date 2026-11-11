// Amount input in German or English notation: "42,50", "42.50", "1.234,56", "1,234.56", "1.234"

export const MIN_AMOUNT = 0.01;
export const MAX_AMOUNT = 1_000_000;

/**
 * Parses what the user typed into a number, or null if it is not a number.
 * - both "," and "." → the last one is the decimal separator ("1.234,56", "1,234.56")
 * - only "," → one comma is decimal ("42,50", "42,505" → 3 places, rejected later),
 *   several are thousands ("1,234,567")
 * - only "." → "1.234" (exactly 3 digits) is German thousands, else decimal ("42.50")
 */
export function parseAmount(input: string): number | null {
  const text = input.replace(/[\s€\u00a0+]/g, '');
  if (!/^\d[\d.,]*$/.test(text)) return null;

  const commas = text.split(',').length - 1;
  const dots = text.split('.').length - 1;
  let decimalSep: ',' | '.' | undefined;
  if (commas > 0 && dots > 0)
    decimalSep = text.lastIndexOf(',') > text.lastIndexOf('.') ? ',' : '.';
  else if (commas === 1) decimalSep = ',';
  else if (dots === 1 && !/^\d{1,3}\.\d{3}$/.test(text)) decimalSep = '.';

  let normalized: string;
  if (decimalSep) {
    const thousands = decimalSep === ',' ? '.' : ',';
    const parts = text.split(thousands).join('').split(decimalSep);
    if (parts.length !== 2) return null;
    normalized = `${parts[0]}.${parts[1]}`;
  } else {
    normalized = text.replace(/[.,]/g, '');
  }
  if (!/^\d+(\.\d+)?$/.test(normalized)) return null;
  return Number(normalized);
}

/** Number of decimal places the user typed (to reject 42,505 instead of rounding it) */
export function decimalPlaces(value: number): number {
  const [, fraction = ''] = String(value).split('.');
  return fraction.length;
}

/** 42.5 → "42,50" for the input field after blur */
export function formatAmountInput(value: number): string {
  return new Intl.NumberFormat('de-DE', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}
