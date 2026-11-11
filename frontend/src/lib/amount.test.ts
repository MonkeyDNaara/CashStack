import { describe, expect, it } from 'vitest';
import { decimalPlaces, formatAmountInput, parseAmount } from './amount';

describe('parseAmount', () => {
  it.each([
    ['42,50', 42.5],
    ['42.50', 42.5],
    ['42', 42],
    ['1.234,56', 1234.56],
    ['1,234.56', 1234.56],
    ['1.234', 1234],
    ['1,234,567', 1234567],
    [' 2.500,00 € ', 2500],
    ['42,505', 42.505],
  ])('reads %s as %s', (input, expected) => {
    expect(parseAmount(input)).toBe(expected);
  });

  it.each(['', 'abc', '-5', '4,2,0.1.1', '12a', ','])('rejects %j', (input) => {
    expect(parseAmount(input)).toBeNull();
  });

  it('counts decimal places and formats for the input', () => {
    expect(decimalPlaces(42.505)).toBe(3);
    expect(decimalPlaces(42)).toBe(0);
    expect(formatAmountInput(1234.5)).toBe('1.234,50');
  });
});
