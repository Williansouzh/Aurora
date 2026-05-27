import { cn } from '@/lib/utils';

export function BudgetBar({ spentAmount = 0, limitAmount = 0 }) {
  const usage = limitAmount > 0 ? (Number(spentAmount || 0) / Number(limitAmount)) * 100 : 0;
  const cappedUsage = Math.min(Math.max(usage, 0), 100);

  return (
    <div className="relative h-1.5 w-full overflow-hidden rounded-full bg-secondary" aria-label={`${Math.round(usage)}% do limite`}>
      <div
        className={cn(
          'h-full rounded-full transition-all',
          usage > 100 ? 'bg-red-500' : usage >= 70 ? 'bg-amber-500' : 'bg-emerald-500'
        )}
        style={{ width: `${cappedUsage}%` }}
      />
    </div>
  );
}
