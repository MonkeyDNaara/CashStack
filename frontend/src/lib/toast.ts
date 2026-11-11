// Tiny toast store outside React, so non-React code (the QueryClient's global error
// handler) can show messages too. <Toaster /> subscribes with useSyncExternalStore.

export type ToastKind = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

type Listener = () => void;

const DEFAULT_DURATION_MS = 5000;

let toasts: Toast[] = [];
let nextId = 1;
const listeners = new Set<Listener>();

function emit() {
  for (const listener of listeners) listener();
}

function dismiss(id: number) {
  toasts = toasts.filter((t) => t.id !== id);
  emit();
}

function show(kind: ToastKind, message: string, durationMs = DEFAULT_DURATION_MS): number {
  // The same message twice in a row (e.g. several failed requests) is shown only once
  const existing = toasts.find((t) => t.kind === kind && t.message === message);
  if (existing) return existing.id;

  const id = nextId++;
  toasts = [...toasts, { id, kind, message }];
  emit();
  if (durationMs > 0) {
    setTimeout(() => dismiss(id), durationMs);
  }
  return id;
}

export const toast = {
  success: (message: string, durationMs?: number) => show('success', message, durationMs),
  error: (message: string, durationMs?: number) => show('error', message, durationMs),
  info: (message: string, durationMs?: number) => show('info', message, durationMs),
  dismiss,
  /** Removes all toasts (used between tests) */
  clear: () => {
    toasts = [];
    emit();
  },
};

export const toastStore = {
  subscribe(listener: Listener) {
    listeners.add(listener);
    return () => listeners.delete(listener);
  },
  getSnapshot: () => toasts,
};
