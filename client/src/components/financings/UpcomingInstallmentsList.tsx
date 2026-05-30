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

export function UpcomingInstallmentsList({ installments = [] }) {
  if (!installments.length) {
    return <p className="py-4 text-center text-sm text-muted-foreground">Nenhuma parcela próxima.</p>;
  }

  return (
    <div className="space-y-2">
      {installments.map((inst) => {
        const days = daysUntil(inst.dueDate);
        const overdue = days !== null && days < 0;
        const today = days === 0;
        const soon = days !== null && days <= 3 && days > 0;

        return (
          <div key={`${inst.financingId}-${inst.number}`} className="flex items-center gap-3 rounded-lg px-3 py-2.5 hover:bg-muted/50 transition-colors">
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium truncate">{inst.financingName}</p>
              <p className="text-xs text-muted-foreground">Parcela {inst.number} · {formatDate(inst.dueDate)}</p>
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
              {formatCurrency(inst.totalPayment)}
            </span>
          </div>
        );
      })}
      <Link to="/financings" className="block mt-2 text-center text-xs text-primary hover:underline font-medium">
        Ver financiamentos
      </Link>
    </div>
  );
}
