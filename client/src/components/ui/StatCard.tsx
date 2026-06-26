import { TrendingDown, TrendingUp } from 'lucide-react';
import { cn, formatCurrency } from '@/lib/utils';
import { Card, CardContent } from './card';

const variantConfig = {
  income: { bg: 'bg-income-soft', icon: 'text-income', value: 'text-income' },
  expense: { bg: 'bg-expense-soft', icon: 'text-expense', value: 'text-expense' },
  warning: { bg: 'bg-pending-soft', icon: 'text-pending', value: 'text-pending' },
  neutral: { bg: 'bg-accent', icon: 'text-primary', value: 'text-foreground' },
  transfer: { bg: 'bg-accent', icon: 'text-primary', value: 'text-primary' },
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
            <p className={cn('font-numeral text-3xl', cfg.value)}>
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
              <TrendingUp className="h-3.5 w-3.5 text-income" />
            ) : (
              <TrendingDown className="h-3.5 w-3.5 text-expense" />
            )}
            <span className={cn('text-xs font-medium', isPositiveTrend ? 'text-income' : 'text-expense')}>
              {isPositiveTrend ? '+' : ''}{trendValue?.toFixed(1)}%
            </span>
            <span className="text-xs text-muted-foreground">{trend}</span>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
