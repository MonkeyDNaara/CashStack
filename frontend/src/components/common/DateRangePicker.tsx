import { isIsoDate, matchPreset, presets, presetRange, type Range } from '../../lib/dateRanges';

type DateRangePickerProps = {
  value: Range;
  onChange: (range: Range) => void;
  /** Field errors from the API (ValidateDateRange reports them on "from") */
  fromError?: string;
  toError?: string;
};

export function DateRangePicker({ value, onChange, fromError, toError }: DateRangePickerProps) {
  const active = matchPreset(value);
  const error = fromError ?? toError;

  return (
    <div className="flex flex-col gap-2">
      <div className="flex flex-wrap items-center gap-2">
        <div className="flex flex-wrap gap-1" role="group" aria-label="Period">
          {presets.map((p) => (
            <button
              key={p.id}
              type="button"
              aria-pressed={active === p.id}
              className={`btn btn-sm rounded-full ${active === p.id ? 'btn-neutral' : 'btn-ghost bg-base-100'}`}
              onClick={() => onChange(presetRange(p.id))}
            >
              {p.label}
            </button>
          ))}
        </div>
        <div className="flex items-center gap-1">
          <input
            type="date"
            aria-label="From"
            aria-invalid={fromError ? true : undefined}
            className={`input input-sm w-36 rounded-lg ${fromError ? 'input-error' : ''}`}
            value={value.from}
            max={value.to}
            onChange={(e) =>
              isIsoDate(e.target.value) && onChange({ ...value, from: e.target.value })
            }
          />
          <span className="text-base-content/50">–</span>
          <input
            type="date"
            aria-label="To"
            aria-invalid={toError ? true : undefined}
            className={`input input-sm w-36 rounded-lg ${toError ? 'input-error' : ''}`}
            value={value.to}
            min={value.from}
            onChange={(e) =>
              isIsoDate(e.target.value) && onChange({ ...value, to: e.target.value })
            }
          />
        </div>
      </div>
      {error && (
        <p role="alert" className="text-sm text-error">
          {error}
        </p>
      )}
    </div>
  );
}
