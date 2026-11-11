import { useSyncExternalStore } from 'react';
import { toast, toastStore, type ToastKind } from '../../lib/toast';

const alertClass: Record<ToastKind, string> = {
  success: 'alert-success',
  error: 'alert-error',
  info: 'alert-info',
};

export function Toaster() {
  const toasts = useSyncExternalStore(toastStore.subscribe, toastStore.getSnapshot);

  if (toasts.length === 0) return null;

  return (
    // aria-live: screen readers announce new messages without moving the focus
    <div className="toast toast-end toast-bottom z-50" aria-live="polite">
      {toasts.map((t) => (
        <div
          key={t.id}
          role={t.kind === 'error' ? 'alert' : 'status'}
          className={`alert ${alertClass[t.kind]} max-w-sm shadow-lg`}
        >
          <span className="whitespace-normal">{t.message}</span>
          <button
            type="button"
            className="btn btn-ghost btn-xs"
            aria-label="Dismiss"
            onClick={() => toast.dismiss(t.id)}
          >
            ✕
          </button>
        </div>
      ))}
    </div>
  );
}
