import { useId, useState, type FormEvent, type ReactNode } from 'react';
import type { ApiError } from '../../api/ApiError';
import type { TransactionType } from '../../api/types';
import { decimalPlaces, formatAmountInput, parseAmount } from '../../lib/amount';
import { toIsoDate } from '../../lib/format';
import { toast } from '../../lib/toast';
import { useCategories } from '../../queries/categories';
import { useCreateTransaction } from '../../queries/transactions';
import { Modal } from '../common/Modal';
import { ErrorAlert } from '../feedback/ErrorAlert';
import { CategoryTiles } from './CategoryTiles';
import {
  DESCRIPTION_MAX,
  emptyForm,
  toCreateRequest,
  validateTransactionForm,
  type TransactionFormErrors,
  type TransactionFormValues,
} from './transactionForm';

type CreateTransactionDialogProps = {
  open: boolean;
  onClose: () => void;
  defaultType?: TransactionType;
};

const FIELDS = ['type', 'categoryId', 'description', 'amount', 'date'] as const;

export function CreateTransactionDialog({
  open,
  onClose,
  defaultType,
}: CreateTransactionDialogProps) {
  return (
    <Modal open={open} onClose={onClose} title="New transaction">
      {/* Mounted only while open: every opening starts with a fresh, empty form */}
      {open && <TransactionForm onDone={onClose} defaultType={defaultType} />}
    </Modal>
  );
}

function TransactionForm({
  onDone,
  defaultType,
}: {
  onDone: () => void;
  defaultType?: TransactionType;
}) {
  const id = useId();
  const [values, setValues] = useState<TransactionFormValues>(() => emptyForm(defaultType));
  const [clientErrors, setClientErrors] = useState<TransactionFormErrors>({});
  const create = useCreateTransaction();
  const { data: categories = [] } = useCategories(values.type);

  // Shown error per field: own validation first, then what the API reported
  const serverError = create.error ?? undefined;
  const errorOf = (field: keyof TransactionFormValues) =>
    clientErrors[field] ?? serverError?.fieldError(field);

  function update(patch: Partial<TransactionFormValues>) {
    setValues((v) => ({ ...v, ...patch }));
    // Editing a field clears its error, so the message does not stay after the fix
    setClientErrors((e) => {
      const next = { ...e };
      for (const key of Object.keys(patch)) delete next[key as keyof TransactionFormValues];
      return next;
    });
    if (create.error) create.reset();
  }

  function submit(event: FormEvent) {
    event.preventDefault();
    const errors = validateTransactionForm(values);
    setClientErrors(errors);
    if (Object.keys(errors).length > 0) return;

    create.mutate(toCreateRequest(values), {
      onSuccess: () => {
        toast.success('Transaction added');
        onDone();
      },
    });
  }

  const isExpense = values.type === 'Expense';
  const tone = isExpense ? 'text-rose-600' : 'text-emerald-600';

  return (
    <form id={`${id}-form`} noValidate onSubmit={submit} className="flex flex-col gap-4">
      <div className="flex rounded-xl bg-base-200 p-1" role="group" aria-label="Type">
        {(['Expense', 'Income'] as const).map((type) => (
          <button
            key={type}
            type="button"
            aria-pressed={values.type === type}
            className={`btn btn-sm flex-1 rounded-lg border-0 shadow-none ${values.type === type ? `bg-base-100 shadow-sm ${type === 'Expense' ? 'text-rose-600' : 'text-emerald-600'}` : 'btn-ghost text-base-content/60'}`}
            // A category belongs to one type: switching the type clears it
            onClick={() => values.type !== type && update({ type, categoryId: '' })}
          >
            {type}
          </button>
        ))}
      </div>

      <div className="text-center">
        <div className={`flex items-baseline justify-center gap-1 text-4xl font-bold ${tone}`}>
          <span aria-hidden="true">{isExpense ? '−' : '+'}</span>
          <input
            id={`${id}-amount`}
            aria-label="Amount in euro"
            aria-invalid={errorOf('amount') ? true : undefined}
            aria-describedby={errorOf('amount') ? `${id}-amount-error` : undefined}
            inputMode="decimal"
            autoComplete="off"
            placeholder="0,00"
            // As wide as its content (field-sizing), so sign and € stay next to the number; size = fallback
            size={Math.max(values.amount.length, 4)}
            className="field-sizing-content w-auto min-w-[3ch] bg-transparent text-center tracking-tight outline-none placeholder:text-base-content/25"
            value={values.amount}
            onChange={(e) => update({ amount: e.target.value })}
            onBlur={() => {
              // Only tidy up valid input: formatting 42,505 would silently round it to 42,51
              const amount = parseAmount(values.amount);
              if (amount !== null && amount > 0 && decimalPlaces(amount) <= 2)
                setValues((v) => ({ ...v, amount: formatAmountInput(amount) }));
            }}
          />
          <span aria-hidden="true">€</span>
        </div>
        {errorOf('amount') ? (
          <p id={`${id}-amount-error`} className="text-xs text-error">
            {errorOf('amount')}
          </p>
        ) : (
          <p className="text-xs text-base-content/50">Amount</p>
        )}
      </div>

      <CategoryTiles
        categories={categories}
        value={values.categoryId}
        onChange={(categoryId) => update({ categoryId })}
        error={errorOf('categoryId')}
        errorId={`${id}-category-error`}
      />

      <div className="grid gap-3 sm:grid-cols-2">
        <Field label="Description" htmlFor={`${id}-description`} error={errorOf('description')}>
          <input
            id={`${id}-description`}
            className={`input w-full rounded-lg ${errorOf('description') ? 'input-error' : ''}`}
            maxLength={DESCRIPTION_MAX}
            aria-invalid={errorOf('description') ? true : undefined}
            aria-describedby={errorOf('description') ? `${id}-description-error` : undefined}
            value={values.description}
            onChange={(e) => update({ description: e.target.value })}
          />
        </Field>
        <Field label="Date" htmlFor={`${id}-date`} error={errorOf('date')}>
          <input
            id={`${id}-date`}
            type="date"
            className={`input w-full rounded-lg ${errorOf('date') ? 'input-error' : ''}`}
            max={toIsoDate(new Date())}
            aria-invalid={errorOf('date') ? true : undefined}
            aria-describedby={errorOf('date') ? `${id}-date-error` : undefined}
            value={values.date}
            onChange={(e) => update({ date: e.target.value })}
          />
        </Field>
      </div>

      {showsAlert(serverError) && <ErrorAlert title="Could not save" error={serverError} />}

      <div className="modal-action mt-0">
        <button type="button" className="btn btn-ghost" onClick={onDone}>
          Cancel
        </button>
        <button type="submit" className="btn btn-primary" disabled={create.isPending}>
          {create.isPending && <span className="loading loading-spinner loading-sm" />}
          Save
        </button>
      </div>
    </form>
  );
}

/**
 * Alert for 4xx errors that no field shows: a business rule (detail, e.g. wrong category type)
 * or a field the form does not have. Network/5xx errors are shown as toast globally.
 */
function showsAlert(error: ApiError | undefined): error is ApiError {
  if (!error || error.isUnexpected) return false;
  if (error.detail) return true;
  return Object.keys(error.fieldErrors).some((f) => !(FIELDS as readonly string[]).includes(f));
}

function Field({
  label,
  htmlFor,
  error,
  children,
}: {
  label: string;
  htmlFor: string;
  error?: string;
  children: ReactNode;
}) {
  return (
    <div>
      <label htmlFor={htmlFor} className="mb-1.5 block text-sm font-semibold">
        {label}
      </label>
      {children}
      {error && (
        <p id={`${htmlFor}-error`} className="mt-1 text-xs text-error">
          {error}
        </p>
      )}
    </div>
  );
}
