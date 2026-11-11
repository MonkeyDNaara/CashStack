import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router';
import { isIsoDate, presetRange, type Range } from '../lib/dateRanges';

/**
 * Date range in the URL (?from=yyyy-MM-dd&to=yyyy-MM-dd), so a view can be bookmarked
 * and the back button works. Missing or invalid values fall back to "this month".
 */
export function useDateRangeParams(): [Range, (range: Range) => void] {
  const [params, setParams] = useSearchParams();
  const fromParam = params.get('from');
  const toParam = params.get('to');

  const range = useMemo<Range>(() => {
    const fallback = presetRange('thisMonth');
    return {
      from: isIsoDate(fromParam) ? fromParam : fallback.from,
      to: isIsoDate(toParam) ? toParam : fallback.to,
    };
  }, [fromParam, toParam]);

  const setRange = useCallback(
    (next: Range) =>
      setParams(
        (current) => {
          const updated = new URLSearchParams(current);
          updated.set('from', next.from);
          updated.set('to', next.to);
          return updated;
        },
        // Typing in a date field should not create one history entry per keystroke
        { replace: true },
      ),
    [setParams],
  );

  return [range, setRange];
}
