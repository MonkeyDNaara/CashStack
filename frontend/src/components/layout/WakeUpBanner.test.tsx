import { act, render, screen, waitFor } from '@testing-library/react';
import { delay, http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { HEALTH_URL } from '../../config';
import { createWrapper } from '../../test/createWrapper';
import { server } from '../../test/server';
import type { WakeUpOptions } from '../../hooks/useServerWakeUp';
import { WakeUpBanner } from './WakeUpBanner';

// Short times instead of the real 2 s / 3 s, so the tests run with real timers and MSW delays
const fast = { slowAfterMs: 50, awakeVisibleMs: 100, retryDelayMs: 20, recheckAfterHiddenMs: 0 };

function renderBanner(options: WakeUpOptions = fast) {
  const { wrapper: Wrapper } = createWrapper();
  return render(
    <Wrapper>
      <WakeUpBanner {...options} />
    </Wrapper>,
  );
}

function countHealthRequests(respond: () => Promise<Response> | Response) {
  const calls = { count: 0 };
  server.use(
    http.get(HEALTH_URL, () => {
      calls.count++;
      return respond();
    }),
  );
  return calls;
}

function setVisibility(state: DocumentVisibilityState) {
  Object.defineProperty(document, 'visibilityState', { value: state, configurable: true });
  document.dispatchEvent(new Event('visibilitychange'));
}

describe('WakeUpBanner', () => {
  it('shows nothing when the server answers quickly', async () => {
    const calls = countHealthRequests(() => HttpResponse.text('Healthy'));

    renderBanner();

    await waitFor(() => expect(calls.count).toBe(1));
    await act(() => delay(120));
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
  });

  it('shows the waking-up bar for a slow answer, then "awake", then nothing', async () => {
    countHealthRequests(async () => {
      await delay(250);
      return HttpResponse.text('Healthy');
    });

    renderBanner();

    expect(await screen.findByText(/waking up the free demo server/i)).toBeInTheDocument();
    expect(screen.getByRole('progressbar', { name: /wake-up progress/i })).toBeInTheDocument();
    expect(await screen.findByText(/server is awake/i)).toBeInTheDocument();
    await waitFor(() => expect(screen.queryByRole('status')).not.toBeInTheDocument());
  });

  it('keeps waiting while the platform answers 503 during the cold start', async () => {
    let attempt = 0;
    countHealthRequests(() =>
      ++attempt <= 4 ? new HttpResponse(null, { status: 503 }) : HttpResponse.text('Healthy'),
    );

    renderBanner();

    expect(await screen.findByText(/waking up/i)).toBeInTheDocument();
    expect(await screen.findByText(/server is awake/i)).toBeInTheDocument();
    expect(attempt).toBe(5);
  });

  it('hides the bar when the server never answers (the pages show their errors)', async () => {
    countHealthRequests(() => new HttpResponse(null, { status: 503 }));

    renderBanner({ ...fast, retryDelayMs: 40, maxRetries: 3 });

    expect(await screen.findByText(/waking up/i)).toBeInTheDocument();
    await waitFor(() => expect(screen.queryByRole('status')).not.toBeInTheDocument());
    expect(screen.queryByText(/server is awake/i)).not.toBeInTheDocument();
  });

  it('checks again when the tab comes back after a long break', async () => {
    const calls = countHealthRequests(() => HttpResponse.text('Healthy'));
    renderBanner();
    await waitFor(() => expect(calls.count).toBe(1));

    act(() => setVisibility('hidden'));
    act(() => setVisibility('visible'));

    await waitFor(() => expect(calls.count).toBe(2));
  });

  it('does not check again after a short break', async () => {
    const calls = countHealthRequests(() => HttpResponse.text('Healthy'));
    renderBanner({ ...fast, recheckAfterHiddenMs: 60_000 });
    await waitFor(() => expect(calls.count).toBe(1));

    act(() => setVisibility('hidden'));
    act(() => setVisibility('visible'));

    await act(() => delay(80));
    expect(calls.count).toBe(1);
  });
});
