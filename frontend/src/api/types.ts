// Mirrors the DTOs of CashStack.Api (System.Text.Json: camelCase, enums as strings)

export type TransactionType = 'Income' | 'Expense';

/** Calendar date as the API expects it: yyyy-MM-dd */
export type IsoDate = string;

export interface Transaction {
  id: string;
  /** ISO 8601 with offset, e.g. 2026-11-10T10:04:28.29+01:00 */
  timestamp: string;
  type: TransactionType;
  categoryId: string;
  categoryName: string;
  description: string;
  /** Always positive; the type decides the sign in the UI */
  amount: number;
}

export interface CategoryTotal {
  categoryId: string;
  name: string;
  type: TransactionType;
  total: number;
}

export interface Summary {
  from: IsoDate;
  to: IsoDate;
  income: number;
  expenses: number;
  balance: number;
  categories: CategoryTotal[];
}

export interface Category {
  id: string;
  name: string;
  type: TransactionType;
  /** The "Other" categories cannot be renamed or deleted */
  isProtected: boolean;
}

export interface DateRange {
  from?: IsoDate;
  to?: IsoDate;
}

export interface TransactionFilters extends DateRange {
  type?: TransactionType;
  categoryId?: string;
}

export interface CreateTransactionRequest {
  type: TransactionType;
  categoryId: string;
  description: string;
  amount: number;
  /** Optional booking date (added by #105); missing = now */
  date?: IsoDate;
}

export interface CreateCategoryRequest {
  name: string;
  type: TransactionType;
}

export interface RenameCategoryRequest {
  name: string;
}

/** RFC 9457 problem details as written by the API (incl. our traceId extension) */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  /** Only on validation errors (400): field name → messages */
  errors?: Record<string, string[]>;
}
