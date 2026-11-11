type CategoryChipProps = {
  name: string;
  color: string;
  size?: 'sm' | 'md';
};

/** Rounded square with the first letter in the category color (decorative, hidden for screen readers) */
export function CategoryChip({ name, color, size = 'md' }: CategoryChipProps) {
  const sizeClass = size === 'sm' ? 'h-7 w-7 text-xs' : 'h-9 w-9 text-sm';
  return (
    <span
      aria-hidden="true"
      className={`grid flex-none place-items-center rounded-xl font-bold ${sizeClass}`}
      style={{ color, backgroundColor: `color-mix(in srgb, ${color} 14%, white)` }}
    >
      {name.charAt(0).toUpperCase()}
    </span>
  );
}
