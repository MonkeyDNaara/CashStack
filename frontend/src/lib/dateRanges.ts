import type { IsoDate } from '../api/types';
import { toIsoDate } from './format';

export interface Range {
  from: IsoDate;
  to: IsoDate;
}

export type PresetId = 'thisMonth' | 'lastMonth' | 'last3Months' | 'thisYear';

export interface Preset {
  id: PresetId;
  label: string;
  range: (today: Date) => Range;
}

const firstOfMonth = (year: number, month: number) => new Date(year, month, 1);
const lastOfMonth = (year: number, month: number) => new Date(year, month + 1, 0);

// Ranges end today (not at the end of the month): the API rejects nothing in the future,
// but a range "up to today" matches the API default and reads naturally.
export const presets: Preset[] = [
  {
    id: 'thisMonth',
    label: 'This month',
    range: (t) => ({
      from: toIsoDate(firstOfMonth(t.getFullYear(), t.getMonth())),
      to: toIsoDate(t),
    }),
  },
  {
    id: 'lastMonth',
    label: 'Last month',
    range: (t) => ({
      from: toIsoDate(firstOfMonth(t.getFullYear(), t.getMonth() - 1)),
      to: toIsoDate(lastOfMonth(t.getFullYear(), t.getMonth() - 1)),
    }),
  },
  {
    id: 'last3Months',
    label: 'Last 3 months',
    range: (t) => ({
      from: toIsoDate(firstOfMonth(t.getFullYear(), t.getMonth() - 2)),
      to: toIsoDate(t),
    }),
  },
  {
    id: 'thisYear',
    label: 'This year',
    range: (t) => ({ from: toIsoDate(new Date(t.getFullYear(), 0, 1)), to: toIsoDate(t) }),
  },
];

export function presetRange(id: PresetId, today: Date = new Date()): Range {
  const preset = presets.find((p) => p.id === id);
  if (!preset) throw new Error(`Unknown preset ${id}`);
  return preset.range(today);
}

/** The preset that produces exactly this range, if any (to highlight its button) */
export function matchPreset(range: Range, today: Date = new Date()): PresetId | undefined {
  return presets.find((p) => {
    const r = p.range(today);
    return r.from === range.from && r.to === range.to;
  })?.id;
}

const ISO_DATE = /^\d{4}-\d{2}-\d{2}$/;

export function isIsoDate(value: string | null | undefined): value is IsoDate {
  return !!value && ISO_DATE.test(value) && !Number.isNaN(new Date(value).getTime());
}
