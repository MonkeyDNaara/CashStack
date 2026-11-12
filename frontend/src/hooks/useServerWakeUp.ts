import { useQuery } from '@tanstack/react-query';
import { useEffect, useRef, useState } from 'react';
import { checkHealth } from '../api/healthApi';

/** idle: nothing to show · waking: /health is slow (server asleep) · awake: just answered after waking */
export type WakeUpPhase = 'idle' | 'waking' | 'awake';

export interface WakeUpOptions {
  /** No answer after this time → the server is probably asleep */
  slowAfterMs?: number;
  /** How long "Server is awake" stays visible */
  awakeVisibleMs?: number;
  /** Render puts a free server to sleep after 15 idle minutes; check again a bit earlier */
  recheckAfterHiddenMs?: number;
  /** While waking up Render may answer with 502/503: retry until it is up (≈ 90 s in total) */
  retryDelayMs?: number;
  maxRetries?: number;
}

export const healthQueryKey = ['health'] as const;

/**
 * Pings GET /health when the app starts and when the tab becomes visible again after a long
 * break. Only a slow answer (cold start of the free server) shows the wake-up banner.
 */
export function useServerWakeUp({
  slowAfterMs = 2000,
  awakeVisibleMs = 2000,
  recheckAfterHiddenMs = 14 * 60_000,
  retryDelayMs = 3000,
  maxRetries = 30,
}: WakeUpOptions = {}) {
  const { isFetching, isError, refetch } = useQuery({
    queryKey: healthQueryKey,
    queryFn: ({ signal }) => checkHealth(signal).then(() => true),
    retry: maxRetries,
    retryDelay: retryDelayMs,
    // Fetched on purpose only: at start and via refetch() after a long hidden tab
    staleTime: Infinity,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  });

  const [phase, setPhase] = useState<WakeUpPhase>('idle');
  const [startedAt, setStartedAt] = useState<number | null>(null);

  // The check finished: adjust the phase during render (React's pattern for state that
  // follows a changed value, no extra effect run). Gave up → idle, the pages show their errors.
  const [wasFetching, setWasFetching] = useState(isFetching);
  if (wasFetching !== isFetching) {
    setWasFetching(isFetching);
    if (!isFetching && phase === 'waking') {
      setPhase(isError ? 'idle' : 'awake');
    }
  }

  // No answer after slowAfterMs → the server is probably asleep
  useEffect(() => {
    if (!isFetching) return;
    const started = Date.now();
    const timer = setTimeout(() => {
      setStartedAt(started);
      setPhase('waking');
    }, slowAfterMs);
    return () => clearTimeout(timer);
  }, [isFetching, slowAfterMs]);

  // "Server is awake" disappears after a moment
  useEffect(() => {
    if (phase !== 'awake') return;
    const timer = setTimeout(() => setPhase('idle'), awakeVisibleMs);
    return () => clearTimeout(timer);
  }, [phase, awakeVisibleMs]);

  // Tab back after a long break: the server may have fallen asleep in the meantime
  const hiddenAt = useRef<number | null>(null);
  useEffect(() => {
    function onVisibilityChange() {
      if (document.visibilityState === 'hidden') {
        hiddenAt.current = Date.now();
      } else if (
        hiddenAt.current !== null &&
        Date.now() - hiddenAt.current >= recheckAfterHiddenMs
      ) {
        hiddenAt.current = null;
        void refetch();
      }
    }
    document.addEventListener('visibilitychange', onVisibilityChange);
    return () => document.removeEventListener('visibilitychange', onVisibilityChange);
  }, [recheckAfterHiddenMs, refetch]);

  return { phase, startedAt };
}
