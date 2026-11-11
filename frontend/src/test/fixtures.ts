import type { Category, Summary, Transaction } from '../api/types';

// Same fixed ids as DefaultCategories in CashStack.Core
export const ids = {
  salary: '6f0b8a52-1c3e-4c1a-9a51-000000000101',
  groceries: '6f0b8a52-1c3e-4c1a-9a51-000000000201',
  rent: '6f0b8a52-1c3e-4c1a-9a51-000000000202',
  otherExpense: '6f0b8a52-1c3e-4c1a-9a51-000000000002',
};

export const categories: Category[] = [
  { id: ids.salary, name: 'Salary', type: 'Income', isProtected: false },
  { id: ids.groceries, name: 'Groceries', type: 'Expense', isProtected: false },
  { id: ids.otherExpense, name: 'Other', type: 'Expense', isProtected: true },
  { id: ids.rent, name: 'Rent', type: 'Expense', isProtected: false },
];

export const transactions: Transaction[] = [
  {
    id: 'a1d982e2-bf8d-4ee3-ab4d-64876d4826b0',
    timestamp: '2026-11-10T10:04:28.2942+01:00',
    type: 'Expense',
    categoryId: ids.groceries,
    categoryName: 'Groceries',
    description: 'Weekly shopping',
    amount: 42.5,
  },
  {
    id: 'b2e093f3-c09e-4ff4-bc5e-75987e5937c1',
    timestamp: '2026-11-01T08:00:00+01:00',
    type: 'Income',
    categoryId: ids.salary,
    categoryName: 'Salary',
    description: 'Salary',
    amount: 2500,
  },
];

export const summary: Summary = {
  from: '2026-11-01',
  to: '2026-11-10',
  income: 2500,
  expenses: 42.5,
  balance: 2457.5,
  categories: [
    { categoryId: ids.salary, name: 'Salary', type: 'Income', total: 2500 },
    { categoryId: ids.groceries, name: 'Groceries', type: 'Expense', total: 42.5 },
  ],
};

/** ProblemDetails bodies as the API writes them */
export const problems = {
  validation: {
    type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
    title: 'One or more validation errors occurred.',
    status: 400,
    errors: {
      amount: ['The field amount must be between 0.01 and 1000000.'],
      description: ['The description field is required.'],
    },
    instance: '/api/v1/transactions',
    traceId: '00-abc-01',
  },
  conflict: {
    type: 'https://tools.ietf.org/html/rfc9110#section-15.5.10',
    title: 'Conflict',
    status: 409,
    detail: "An Expense category named 'rent' already exists.",
    instance: '/api/v1/categories',
    traceId: '00-def-01',
  },
  notFound: {
    type: 'https://tools.ietf.org/html/rfc9110#section-15.5.5',
    title: 'Not Found',
    status: 404,
    instance: '/api/v1/transactions/00000000-0000-0000-0000-000000000000',
  },
  serverError: {
    type: 'https://tools.ietf.org/html/rfc9110#section-15.6.1',
    title: 'Storage error',
    status: 500,
    detail: 'The data could not be read or saved.',
  },
};
