import { act, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { ApiError } from '../../api/ApiError';
import { toast } from '../../lib/toast';
import { ErrorAlert } from './ErrorAlert';
import { Toaster } from './Toaster';

describe('Toaster', () => {
  it('shows toasts and removes them on dismiss', async () => {
    const user = userEvent.setup();
    render(<Toaster />);

    act(() => {
      toast.error('The CashStack API is not reachable.');
    });
    expect(screen.getByRole('alert')).toHaveTextContent('not reachable');

    await user.click(screen.getByRole('button', { name: 'Dismiss' }));
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });

  it('removes a toast after its duration', () => {
    vi.useFakeTimers();
    try {
      render(<Toaster />);
      act(() => {
        toast.success('Transaction added', 1000);
      });
      expect(screen.getByRole('status')).toHaveTextContent('Transaction added');

      act(() => vi.advanceTimersByTime(1000));
      expect(screen.queryByRole('status')).not.toBeInTheDocument();
    } finally {
      vi.useRealTimers();
    }
  });

  it('shows the same message only once', () => {
    render(<Toaster />);
    act(() => {
      toast.error('Server error');
      toast.error('Server error');
    });
    expect(screen.getAllByRole('alert')).toHaveLength(1);
  });
});

describe('ErrorAlert', () => {
  it('shows the API message and a retry button', async () => {
    const user = userEvent.setup();
    const onRetry = vi.fn();
    render(
      <ErrorAlert error={new ApiError(409, { detail: 'Category is in use.' })} onRetry={onRetry} />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Category is in use.');
    await user.click(screen.getByRole('button', { name: 'Try again' }));
    expect(onRetry).toHaveBeenCalledOnce();
  });
});
