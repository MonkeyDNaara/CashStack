// Small inline icons (stroke = currentColor), decorative: the buttons carry the label
const base = {
  viewBox: '0 0 24 24',
  className: 'h-4 w-4',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 2,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const,
  'aria-hidden': true,
};

export function TrashIcon() {
  return (
    <svg {...base}>
      <path d="M3 6h18M8 6V4h8v2m-9 0 1 14h8l1-14" />
    </svg>
  );
}

export function PencilIcon() {
  return (
    <svg {...base}>
      <path d="M4 20h4L19 9l-4-4L4 16v4zM13.5 6.5l4 4" />
    </svg>
  );
}

export function LockIcon() {
  return (
    <svg {...base} className="h-3.5 w-3.5">
      <rect x="5" y="11" width="14" height="9" rx="2" />
      <path d="M8 11V8a4 4 0 0 1 8 0v3" />
    </svg>
  );
}
