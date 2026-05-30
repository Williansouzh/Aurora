import { ArrowRight, CheckCircle, Clock, MoreHorizontal, RefreshCw } from 'lucide-react';
import { Badge } from '../ui/badge';
import { Button } from '../ui/button';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator, DropdownMenuTrigger } from '../ui/dropdown-menu';
import { cn, formatCurrency, formatDate } from '../../lib/utils';
import { recurrenceTypes, transactionStatuses, transactionTypes } from '../../constants/financeOptions';
import { enumLabel } from '../../utils/enumHelpers';

const TYPE_STYLES = {
  0: { label: 'Receita', cls: 'text-emerald-700 bg-emerald-50 border-emerald-200' },
  1: { label: 'Despesa', cls: 'text-red-700 bg-red-50 border-red-200' },
  2: { label: 'Transfer.', cls: 'text-indigo-700 bg-indigo-50 border-indigo-200' },
};

const STATUS_STYLES = {
  0: { label: 'Pago', cls: 'text-emerald-700 bg-emerald-50 border-emerald-200' },
  1: { label: 'Pendente', cls: 'text-amber-700 bg-amber-50 border-amber-200' },
  2: { label: 'Atrasado', cls: 'text-red-700 bg-red-50 border-red-200' },
};

export function TransactionTable({ transactions, accounts = [], categories = [], compact = false, onEdit, onDelete, onPaid, onPending }) {
  const accountById = Object.fromEntries(accounts.map((a) => [a.id, a.name]));
  const categoryById = Object.fromEntries(categories.map((c) => [c.id, { name: c.name, color: c.color }]));

  if (!transactions.length) return null;

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-border bg-muted/40">
            <th className="px-4 py-2.5 text-left text-xs font-medium text-muted-foreground">Descrição</th>
            {!compact && <th className="px-4 py-2.5 text-left text-xs font-medium text-muted-foreground hidden md:table-cell">Conta</th>}
            {!compact && <th className="px-4 py-2.5 text-left text-xs font-medium text-muted-foreground hidden lg:table-cell">Categoria</th>}
            <th className="px-4 py-2.5 text-left text-xs font-medium text-muted-foreground hidden sm:table-cell">Status</th>
            <th className="px-4 py-2.5 text-left text-xs font-medium text-muted-foreground hidden sm:table-cell">Data</th>
            <th className="px-4 py-2.5 text-right text-xs font-medium text-muted-foreground">Valor</th>
            {!compact && <th className="w-10" />}
          </tr>
        </thead>
        <tbody className="divide-y divide-border">
          {transactions.map((t) => {
            const typeStyle = TYPE_STYLES[t.type] ?? TYPE_STYLES[1];
            const statusStyle = STATUS_STYLES[t.status] ?? STATUS_STYLES[1];
            const cat = categoryById[t.categoryId];
            const isTransfer = t.kind === 'transfer';

            return (
              <tr key={t.id} className="group hover:bg-muted/30 transition-colors">
                <td className="px-4 py-3">
                  <div className="flex items-center gap-2">
                    {cat?.color && !isTransfer && (
                      <span className="h-2 w-2 rounded-full shrink-0" style={{ background: cat.color }} />
                    )}
                    {isTransfer && <ArrowRight className="h-3.5 w-3.5 text-indigo-500 shrink-0" />}
                    <span className="font-medium text-foreground truncate max-w-[160px]">{t.description}</span>
                    {t.isRecurring && (
                      <RefreshCw className="h-3 w-3 text-muted-foreground shrink-0" title={`Recorrente — ${enumLabel(recurrenceTypes, t.recurrenceType)}`} />
                    )}
                  </div>
                </td>
                {!compact && (
                  <td className="px-4 py-3 text-muted-foreground hidden md:table-cell">
                    {isTransfer
                      ? `${accountById[t.fromAccountId] ?? accountById[t.accountId] ?? '—'} → ${accountById[t.toAccountId] ?? '—'}`
                      : accountById[t.accountId] ?? '—'}
                  </td>
                )}
                {!compact && (
                  <td className="px-4 py-3 hidden lg:table-cell">
                    {isTransfer ? '—' : cat?.name ?? '—'}
                  </td>
                )}
                <td className="px-4 py-3 hidden sm:table-cell">
                  <span className={cn('inline-flex items-center rounded-full border px-2 py-0.5 text-[11px] font-medium', statusStyle.cls)}>
                    {statusStyle.label}
                  </span>
                </td>
                <td className="px-4 py-3 text-muted-foreground text-xs hidden sm:table-cell">
                  {formatDate(t.date)}
                </td>
                <td className="px-4 py-3 text-right">
                  <span className={cn('font-semibold tabular-nums', t.type === 0 ? 'text-emerald-700' : t.type === 2 ? 'text-indigo-700' : 'text-red-700')}>
                    {t.type === 1 ? '-' : '+'}{formatCurrency(t.amount)}
                  </span>
                </td>
                {!compact && (
                  <td className="px-2 py-3">
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon" className="h-7 w-7 opacity-0 group-hover:opacity-100 transition-opacity">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        {!isTransfer && <DropdownMenuItem onClick={() => onEdit?.(t)}>Editar</DropdownMenuItem>}
                        {!isTransfer && t.status === 0 && (
                          <DropdownMenuItem onClick={() => onPending?.(t.id)} className="gap-2">
                            <Clock className="h-4 w-4" /> Marcar pendente
                          </DropdownMenuItem>
                        )}
                        {!isTransfer && t.status !== 0 && (
                          <DropdownMenuItem onClick={() => onPaid?.(t.id)} className="gap-2">
                            <CheckCircle className="h-4 w-4 text-emerald-500" /> Marcar pago
                          </DropdownMenuItem>
                        )}
                        <DropdownMenuSeparator />
                        <DropdownMenuItem onClick={() => onDelete?.(t)} className="text-destructive focus:text-destructive">
                          Excluir
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
