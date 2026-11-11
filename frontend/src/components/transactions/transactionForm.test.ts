import { describe, expect, it } from 'vitest';
import { emptyForm, toCreateRequest, validateTransactionForm } from './transactionForm';

const today = new Date(2026, 10, 11);
const valid = {
  ...emptyForm('Expense', today),
  categoryId: 'c1',
  description: '  Weekly shopping ',
  amount: '42,50',
};

describe('transaction form', () => {
  it('starts as expense without category, dated today', () => {
    expect(emptyForm(undefined, today)).toEqual({
      type: 'Expense',
      categoryId: '',
      description: '',
      amount: '',
      date: '2026-11-11',
    });
  });

  it('accepts a complete form and builds the API request', () => {
    expect(validateTransactionForm(valid, today)).toEqual({});
    expect(toCreateRequest(valid)).toEqual({
      type: 'Expense',
      categoryId: 'c1',
      description: 'Weekly shopping',
      amount: 42.5,
      date: '2026-11-11',
    });
  });

  it('reports every missing field', () => {
    expect(Object.keys(validateTransactionForm(emptyForm('Income', today), today)).sort()).toEqual([
      'amount',
      'categoryId',
      'description',
    ]);
  });

  it.each([
    ['abc', 'Enter a number'],
    ['42,505', 'At most 2 decimal places'],
    ['0', 'between 0,01'],
    ['1.000.001', 'between 0,01'],
  ])('rejects the amount %s', (amount, message) => {
    expect(validateTransactionForm({ ...valid, amount }, today).amount).toContain(message);
  });

  it('rejects a future date and a too long description', () => {
    const errors = validateTransactionForm(
      { ...valid, date: '2026-11-12', description: 'x'.repeat(101) },
      today,
    );
    expect(errors.date).toContain('future');
    expect(errors.description).toContain('100');
  });
});
