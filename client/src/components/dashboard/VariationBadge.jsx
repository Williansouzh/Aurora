import { TrendingDown, TrendingUp } from 'lucide-react';
import { cn } from '../../lib/utils';

export function VariationBadge({ value = 0, invert = false }) {
  const numeric = Number(value || 0);
  const isUp = numeric > 0;
  const good = invert ? numeric < 0 : numeric > 0;
  const bad = invert ? numeric > 0 : numeric < 0;

  if (Math.abs(numeric) < 0.01) return null;

  return (
    <span className={cn(
      'inline-flex items-center gap-1 text-xs font-medium',
      good ? 'text-emerald-600' : bad ? 'text-red-600' : 'text-muted-foreground'
    )}>
      {isUp ? <TrendingUp className="h-3 w-3" /> : <TrendingDown className="h-3 w-3" />}
      {Math.abs(numeric).toFixed(1)}% vs mês ant.
    </span>
  );
}
