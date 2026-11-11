import { useLayoutEffect, useRef, useSyncExternalStore } from 'react';
import { toast, toastStore, type ToastKind } from '../../lib/toast';

const alertClass: Record<ToastKind, string> = {
  success: 'alert-success',
  error: 'alert-error',
  info: 'alert-info',
};

// jsdom (tests) has no popover API: without it the container stays a normal fixed element
const supportsPopover =
  typeof HTMLElement !== 'undefined' && 'showPopover' in HTMLElement.prototype;

export function Toaster() {
  const toasts = useSyncExternalStore(toastStore.subscribe, toastStore.getSnapshot);
  const ref = useRef<HTMLDivElement>(null);

  // An open <dialog> lives in the browser's top layer and would cover a normal fixed element.
  // As a popover the toast container joins the top layer too; re-showing it puts it above
  // a dialog opened later.
  useLayoutEffect(() => {
    const el = ref.current;
    if (!el || !supportsPopover) return;
    if (el.matches(':popover-open')) el.hidePopover();
    if (toasts.length > 0) el.showPopover();
  }, [toasts]);

  return (
    <div
      ref={ref}
      popover={supportsPopover ? 'manual' : undefined}
      // Reset the browser's popover defaults (centered box with border and background)
      className="toast toast-end toast-bottom z-50 m-0 overflow-visible border-0 bg-transparent p-0"
      style={{ inset: 'auto 1rem 1rem auto' }}
      aria-live="polite"
    >
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
