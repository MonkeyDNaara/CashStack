import type { CreateTransactionRequest, IsoDate, TransactionType } from '../../api/types';
import { decimalPlaces, MAX_AMOUNT, MIN_AMOUNT, parseAmount } from '../../lib/amount';
import { toIsoDate } from '../../lib/format';

export interface TransactionFormValues {
  type: TransactionType;
  categoryId: string;
  description: string;
  /** Raw text of the amount field */
  amount: string;
  date: IsoDate;
}

export type TransactionFormErrors = Partial<Record<keyof TransactionFormValues, string>>;

export const DESCRIPTION_MAX = 100;

export function emptyForm(
  type: TransactionType = 'Expense',
  today: Date = new Date(),
): TransactionFormValues {
  return { type, categoryId: '', description: '', amount: '', date: toIsoDate(today) };
}

/**
 * Same rules as the API (DataAnnotations + [NotInFuture]), so most mistakes are caught
 * before a request. The API stays the source of truth: its field errors are shown too.
 */
export function validateTransactionForm(
  values: TransactionFormValues,
  today: Date = new Date(),
): TransactionFormErrors {
  const errors: TransactionFormErrors = {};

  if (!values.categoryId) errors.categoryId = 'Choose a category.';

  const description = values.description.trim();
  if (!description) errors.description = 'Enter a description.';
  else if (description.length > DESCRIPTION_MAX)
    errors.description = `At most ${DESCRIPTION_MAX} characters.`;

  const amount = parseAmount(values.amount);
  if (values.amount.trim() === '') errors.amount = 'Enter an amount.';
  else if (amount === null) errors.amount = 'Enter a number, e.g. 42,50.';
  else if (decimalPlaces(amount) > 2) errors.amount = 'At most 2 decimal places.';
  else if (amount < MIN_AMOUNT || amount > MAX_AMOUNT)
    errors.amount = 'Enter an amount between 0,01 and 1.000.000.';

  if (!values.date) errors.date = 'Choose a date.';
  else if (values.date > toIsoDate(today)) errors.date = 'The date must not be in the future.';

  return errors;
}

/** Only call with values that passed validation */
export function toCreateRequest(values: TransactionFormValues): CreateTransactionRequest {
  return {
    type: values.type,
    categoryId: values.categoryId,
    description: values.description.trim(),
    amount: parseAmount(values.amount)!,
    date: values.date,
  };
}
