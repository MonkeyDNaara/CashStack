import { useEffect, useId, useRef, type ReactNode } from 'react';

type ModalProps = {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  /** Buttons at the bottom (DaisyUI modal-action) */
  actions?: ReactNode;
};

/**
 * DaisyUI modal on the native <dialog>: showModal() gives focus trapping, Esc to close
 * and the top layer for free. React state stays the single source of truth (`open`).
 */
export function Modal({ open, onClose, title, children, actions }: ModalProps) {
  const ref = useRef<HTMLDialogElement>(null);
  const titleId = useId();

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open]);

  return (
    <dialog
      ref={ref}
      className="modal"
      aria-labelledby={titleId}
      // Esc closes the native dialog → report it so the React state follows
      onClose={onClose}
    >
      <div className="modal-box rounded-2xl">
        <h3 id={titleId} className="text-lg font-bold">
          {title}
        </h3>
        <div className="py-3">{children}</div>
        {actions && <div className="modal-action mt-2">{actions}</div>}
      </div>
      {/* Click on the backdrop closes the dialog */}
      <form method="dialog" className="modal-backdrop">
        <button type="submit" aria-label="Close">
          close
        </button>
      </form>
    </dialog>
  );
}
