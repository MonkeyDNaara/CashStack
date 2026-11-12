import { useEffect, useState } from 'react';
import { useServerWakeUp, type WakeUpOptions } from '../../hooks/useServerWakeUp';

// Render's free plan needs about a minute for a cold start; the bar fills up to this point
const EXPECTED_WAKE_UP_SECONDS = 60;

/** Bar under the navigation while the free demo server wakes up (no answer from /health yet) */
export function WakeUpBanner(options: WakeUpOptions) {
  const { phase, startedAt } = useServerWakeUp(options);
  const seconds = useSecondsSince(phase === 'waking' ? startedAt : null);

  if (phase === 'waking') {
    return (
      <div role="status" className="border-b border-info/30 bg-info/10 text-sm">
        <div className="mx-auto flex w-full max-w-6xl items-center gap-3 px-4 py-2">
          <span className="loading loading-spinner loading-sm text-info" aria-hidden="true" />
          <span>Waking up the free demo server – this can take up to a minute.</span>
          <span className="ml-auto tabular-nums text-base-content/60">{seconds} s</span>
        </div>
        <progress
          className="progress progress-info block h-1 w-full rounded-none"
          value={Math.min(seconds, EXPECTED_WAKE_UP_SECONDS)}
          max={EXPECTED_WAKE_UP_SECONDS}
          aria-label="Wake-up progress"
        />
      </div>
    );
  }

  if (phase === 'awake') {
    return (
      <div role="status" className="border-b border-success/30 bg-success/10 text-sm">
        <div className="mx-auto w-full max-w-6xl px-4 py-2">✓ Server is awake.</div>
      </div>
    );
  }

  return null;
}

/** Whole seconds since a timestamp, updated every second (0 when there is none) */
function useSecondsSince(start: number | null): number {
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    if (start === null) return;
    const timer = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(timer);
  }, [start]);

  return start === null ? 0 : Math.max(0, Math.floor((now - start) / 1000));
}
