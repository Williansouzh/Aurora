import { CheckCircle, Clock, MoreHorizontal, RefreshCw } from 'lucide-react';
import { Button } from '../ui/button';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator, DropdownMenuTrigger } from '../ui/dropdown-menu';
import { cn, formatCurrency, formatDate } from '../../lib/utils';
import { recurrenceTypes } from '../../constants/financeOptions';
import { enumLabel } from '../../utils/enumHelpers';

// Leading 7px type dot — income green, transfer indigo, expense red (Quiet DS)
const TYPE_DOT = { 0: 'bg-income', 1: 'bg-expense', 2: 'bg-transfer' };
// Amount color by type — income green, transfer muted, expense ink
const AMOUNT_CLS = { 0: 'text-income', 1: 'text-foreground', 2: 'text-mut2' };

const STATUS_STYLES = {
  0: { label: 'Paga', cls: 'text-income bg-income-soft' },
  1: { label: 'Pendente', cls: 'text-pending bg-pending-soft' },
  2: { label: 'Atrasada', cls: 'text-expense bg-expense-soft' },
  3: { label: 'Cancelada', cls: 'text-faint bg-muted' },
};

export function TransactionTable({ transactions, accounts = [], categories = [], compact = false, onEdit, onDelete, onPaid, onPending }) {
  const accountById = Object.fromEntries(accounts.map((a) => [a.id, a.name]));
  const categoryById = Object.fromEntries(categories.map((c) => [c.id, { name: c.name, color: c.color }]));

  if (!transactions.length) return null;

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-border bg-secondary [&>th]:px-4 [&>th]:py-3 [&>th]:text-left [&>th]:text-[11px] [&>th]:font-semibold [&>th]:uppercase [&>th]:tracking-[0.1em] [&>th]:text-faint">
            <th>Descrição</th>
            {!compact && <th className="hidden md:table-cell">Conta</th>}
            {!compact && <th className="hidden lg:table-cell">Categoria</th>}
            <th className="hidden sm:table-cell">Status</th>
            <th className="hidden sm:table-cell">Data</th>
            <th className="!text-right">Valor</th>
            {!compact && <th className="w-10" />}
          </tr>
        </thead>
        <tbody className="divide-y divide-line2">
          {transactions.map((t) => {
            const statusStyle = STATUS_STYLES[t.status] ?? STATUS_STYLES[1];
            const cat = categoryById[t.categoryId];
            const isTransfer = t.kind === 'transfer' || t.type === 2;

            return (
              <tr key={t.id} className="group transition-colors hover:bg-secondary">
                <td className="px-4 py-3">
                  <div className="flex items-center gap-2.5">
                    <span className={cn('h-[7px] w-[7px] shrink-0 rounded-full', TYPE_DOT[t.type] ?? 'bg-mut2')} />
                    <span className="truncate max-w-[180px] text-foreground">{t.description}</span>
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
                  <span className={cn('inline-flex items-center rounded-md px-2 py-0.5 text-[11px] font-medium', statusStyle.cls)}>
                    {statusStyle.label}
                  </span>
                </td>
                <td className="px-4 py-3 text-mut2 text-xs hidden sm:table-cell">
                  {formatDate(t.date)}
                </td>
                <td className="px-4 py-3 text-right">
                  <span className={cn('font-semibold tabular-nums', AMOUNT_CLS[t.type] ?? 'text-foreground')}>
                    {t.type === 1 ? '−' : '+'}{formatCurrency(t.amount)}
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
                            <CheckCircle className="h-4 w-4 text-income" /> Marcar pago
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
