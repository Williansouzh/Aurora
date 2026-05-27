import { cn } from '@/lib/utils';

const toneMap = {
  good: 'text-emerald-700',
  bad: 'text-red-700',
  warning: 'text-amber-700',
};

export function Metric({ label, value, tone = '', children }) {
  return (
    <div className="rounded-xl border bg-card p-6 shadow-card hover:shadow-card-hover transition-shadow">
      <p className="text-sm font-medium text-muted-foreground mb-1">{label}</p>
      <p className={cn('text-2xl font-bold tracking-tight', toneMap[tone] ?? 'text-foreground')}>{value}</p>
      {children && <div className="mt-2">{children}</div>}
    </div>
  );
}
