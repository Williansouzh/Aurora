import { ArrowLeftRight } from 'lucide-react';
import { cn, formatCurrency } from '../../lib/utils';

// Mobile transactions view — grouped by day, each row a tinted category icon chip.
// Mirrors the Finance Mobile design; the desktop table stays on sm+.

const STATUS = {
  0: { label: 'Paga', cls: 'text-income bg-income-soft' },
  1: { label: 'Pendente', cls: 'text-pending bg-pending-soft' },
  2: { label: 'Atrasada', cls: 'text-expense bg-expense-soft' },
  3: { label: 'Cancelada', cls: 'text-faint bg-muted' },
};
const AMOUNT_CLS = { 0: 'text-income', 1: 'text-foreground', 2: 'text-mut2' };

function dayLabel(dateStr) {
  const d = new Date(dateStr);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const dd = new Date(d);
  dd.setHours(0, 0, 0, 0);
  const diff = Math.round((today - dd) / 86400000);
  if (diff === 0) return 'Hoje';
  if (diff === 1) return 'Ontem';
  return d.toLocaleDateString('pt-BR', { day: '2-digit', month: 'long' });
}

export function TransactionDayList({ transactions, categories = [], onSelect }) {
  const categoryById = Object.fromEntries(categories.map((c) => [c.id, c]));

  // Group by calendar day, preserving the incoming (already-sorted) order.
  const groups = [];
  const index = {};
  for (const t of transactions) {
    const key = (t.date ?? '').slice(0, 10);
    if (!(key in index)) {
      index[key] = groups.length;
      groups.push({ key, items: [] });
    }
    groups[index[key]].items.push(t);
  }

  return (
    <div className="space-y-5 p-4">
      {groups.map((group) => (
        <div key={group.key}>
          <p className="mb-2 text-overline">{dayLabel(group.key)}</p>
          <div className="space-y-1.5">
            {group.items.map((t) => {
              const isTransfer = t.kind === 'transfer' || t.type === 2;
              const cat = categoryById[t.categoryId];
              const color = isTransfer ? '#3d4eac' : cat?.color || '#a8a299';
              const status = STATUS[t.status] ?? STATUS[1];
              return (
                <button
                  key={t.id}
                  onClick={() => !isTransfer && onSelect?.(t)}
                  className="flex w-full items-center gap-3 rounded-xl border border-border bg-card p-3 text-left transition-colors active:bg-secondary"
                >
                  <span
                    className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-xs font-bold"
                    style={{ background: `${color}1f`, color }}
                  >
                    {isTransfer ? <ArrowLeftRight className="h-4 w-4" /> : cat?.icon?.slice(0, 2) ?? '?'}
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm text-foreground">{t.description}</p>
                    <span className={cn('mt-0.5 inline-flex rounded px-1.5 py-0.5 text-[10px] font-medium', status.cls)}>
                      {status.label}
                    </span>
                  </div>
                  <span className={cn('shrink-0 text-sm font-semibold tabular-nums', AMOUNT_CLS[t.type] ?? 'text-foreground')}>
                    {t.type === 1 ? '−' : '+'}{formatCurrency(t.amount)}
                  </span>
                </button>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}
