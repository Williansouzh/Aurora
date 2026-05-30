import { TrendingDown, TrendingUp } from 'lucide-react';
import { cn, formatCurrency } from '@/lib/utils';
import { Card, CardContent } from './card';

const variantConfig = {
  income: { bg: 'bg-emerald-50', icon: 'text-emerald-600', value: 'text-emerald-700' },
  expense: { bg: 'bg-red-50', icon: 'text-red-600', value: 'text-red-700' },
  warning: { bg: 'bg-amber-50', icon: 'text-amber-600', value: 'text-amber-700' },
  neutral: { bg: 'bg-indigo-50', icon: 'text-indigo-600', value: 'text-foreground' },
  transfer: { bg: 'bg-indigo-50', icon: 'text-indigo-600', value: 'text-indigo-700' },
};

export function StatCard({ title, value, subtitle, icon: Icon, trend, trendValue, variant = 'neutral', currency = true }) {
  const cfg = variantConfig[variant] ?? variantConfig.neutral;
  const isPositiveTrend = trendValue > 0;

  return (
    <Card className="relative overflow-hidden transition-shadow hover:shadow-card-hover">
      <CardContent className="p-6">
        <div className="flex items-start justify-between">
          <div className="space-y-1 flex-1">
            <p className="text-sm font-medium text-muted-foreground">{title}</p>
            <p className={cn('text-2xl font-bold tracking-tight', cfg.value)}>
              {currency ? formatCurrency(value) : value}
            </p>
            {subtitle && <p className="text-xs text-muted-foreground">{subtitle}</p>}
          </div>
          {Icon && (
            <div className={cn('rounded-lg p-2.5 shrink-0', cfg.bg)}>
              <Icon className={cn('h-5 w-5', cfg.icon)} />
            </div>
          )}
        </div>

        {trend !== undefined && trendValue !== undefined && (
          <div className="mt-3 flex items-center gap-1.5">
            {isPositiveTrend ? (
              <TrendingUp className="h-3.5 w-3.5 text-emerald-500" />
            ) : (
              <TrendingDown className="h-3.5 w-3.5 text-red-500" />
            )}
            <span className={cn('text-xs font-medium', isPositiveTrend ? 'text-emerald-600' : 'text-red-600')}>
              {isPositiveTrend ? '+' : ''}{trendValue?.toFixed(1)}%
            </span>
            <span className="text-xs text-muted-foreground">{trend}</span>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
