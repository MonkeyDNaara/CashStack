import { Cell, Pie, PieChart, Tooltip } from 'recharts';
import type { CategoryTotal } from '../../api/types';
import { useCategoryColors } from '../../hooks/useCategoryColors';
import { formatMoney } from '../../lib/format';

type ExpensesDonutProps = {
  totals: CategoryTotal[];
  activeId?: string;
  onActiveChange: (categoryId: string | undefined) => void;
};

const SIZE = 220;

export function ExpensesDonut({ totals, activeId, onActiveChange }: ExpensesDonutProps) {
  const colorOf = useCategoryColors();
  const sum = totals.reduce((s, t) => s + t.total, 0);

  return (
    // Fixed size instead of ResponsiveContainer: the donut never needs to grow, and it renders in tests
    <div className="relative mx-auto" style={{ width: SIZE, height: SIZE }}>
      <PieChart width={SIZE} height={SIZE}>
        <Pie
          data={totals}
          dataKey="total"
          nameKey="name"
          innerRadius={72}
          outerRadius={102}
          paddingAngle={totals.length > 1 ? 3 : 0}
          cornerRadius={4}
          stroke="none"
          isAnimationActive={false}
          onMouseLeave={() => onActiveChange(undefined)}
        >
          {totals.map((t) => (
            <Cell
              key={t.categoryId}
              fill={colorOf(t.categoryId)}
              opacity={activeId === undefined || activeId === t.categoryId ? 1 : 0.3}
              onMouseEnter={() => onActiveChange(t.categoryId)}
            />
          ))}
        </Pie>
        <Tooltip formatter={(value) => formatMoney(Number(value))} />
      </PieChart>
      {/* Total in the hole; pointer-events-none keeps the segments hoverable */}
      <div className="pointer-events-none absolute inset-0 grid place-content-center text-center">
        <span className="text-xs text-base-content/60">Expenses</span>
        <span className="text-xl font-bold tracking-tight tabular-nums">{formatMoney(sum)}</span>
      </div>
    </div>
  );
}
