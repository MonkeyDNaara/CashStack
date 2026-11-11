import { useState } from 'react';
import type { TransactionType } from '../../api/types';
import { CreateTransactionDialog } from './CreateTransactionDialog';

/** "+ New transaction" button with its dialog, used on the dashboard and the list */
export function NewTransactionButton({ defaultType }: { defaultType?: TransactionType }) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <button type="button" className="btn btn-primary rounded-xl" onClick={() => setOpen(true)}>
        <span aria-hidden="true">+</span> New transaction
      </button>
      <CreateTransactionDialog
        open={open}
        onClose={() => setOpen(false)}
        defaultType={defaultType}
      />
    </>
  );
}
