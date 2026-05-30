import { Link } from 'react-router-dom';
import { cn, formatCurrency, formatDate } from '../../lib/utils';

function daysUntil(value) {
  if (!value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const due = new Date(value);
  due.setHours(0, 0, 0, 0);
  return Math.round((due - today) / 86400000);
}

export function UpcomingDuesList({ transactions = [] }) {
  const items = transactions.slice(0, 5);

  if (items.length === 0) {
    return <p className="py-6 text-center text-sm text-muted-foreground">Sem vencimentos nos próximos dias.</p>;
  }

  return (
    <div className="space-y-2">
      {items.map((t) => {
        const days = daysUntil(t.dueDate);
        const overdue = days !== null && days < 0;
        const today = days === 0;
        const soon = days !== null && days <= 2 && days > 0;

        return (
          <div key={t.id} className="flex items-center gap-3 rounded-lg px-3 py-2 hover:bg-muted/50 transition-colors">
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium truncate">{t.description}</p>
              <p className="text-xs text-muted-foreground">{formatDate(t.dueDate)}</p>
            </div>
            <span className={cn(
              'shrink-0 rounded-full px-2 py-0.5 text-[11px] font-semibold border',
              overdue ? 'bg-red-50 text-red-700 border-red-200' :
              today ? 'bg-amber-50 text-amber-700 border-amber-200' :
              soon ? 'bg-yellow-50 text-yellow-700 border-yellow-200' :
              'bg-muted text-muted-foreground border-border'
            )}>
              {overdue ? 'Atrasada' : today ? 'Hoje' : days === 1 ? 'Amanhã' : `${days}d`}
            </span>
            <span className="text-sm font-semibold tabular-nums text-red-700 shrink-0">
              {formatCurrency(t.amount)}
            </span>
          </div>
        );
      })}
      <Link to="/transactions" className="block mt-2 text-center text-xs text-primary hover:underline font-medium">
        Ver todas as pendências
      </Link>
    </div>
  );
}
