import { useState } from 'react';
import { financingInstallmentStatuses } from '../../constants/financeOptions';
import { enumLabel } from '../../utils/enumHelpers';
import { formatCurrency, formatDate } from '../../lib/utils';
import { Badge } from '../ui/badge';
import { Button } from '../ui/button';
import { PayInstallmentModal } from './PayInstallmentModal';

const INST_STATUS_VARIANTS = {
  0: 'neutral',
  1: 'success',
  2: 'danger',
};

export function InstallmentTable({ installments = [], financingId, onMarkPaid, showAll = false }) {
  const [payTarget, setPayTarget] = useState(null);
  const visible = showAll ? installments : installments.slice(0, 24);

  async function handlePay(paidAmount, paidAt) {
    await onMarkPaid(financingId, payTarget.number, { paidAmount, paidAt });
    setPayTarget(null);
  }

  return (
    <>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border bg-secondary [&>th]:px-4 [&>th]:py-3 [&>th]:text-[11px] [&>th]:font-semibold [&>th]:uppercase [&>th]:tracking-[0.1em] [&>th]:text-faint">
              <th className="text-left">#</th>
              <th className="text-left">Vencimento</th>
              <th className="text-right">Amortização</th>
              <th className="text-right">Juros</th>
              <th className="text-right hidden md:table-cell">Seg/Taxas</th>
              <th className="text-right">Parcela</th>
              <th className="text-right hidden lg:table-cell">Saldo</th>
              <th className="text-center">Status</th>
              {onMarkPaid && <th />}
            </tr>
          </thead>
          <tbody className="divide-y divide-line2">
            {visible.map((inst) => (
              <tr
                key={inst.number}
                className={inst.status === 1 ? 'bg-secondary/50 opacity-70' : 'transition-colors hover:bg-secondary'}
              >
                <td className="px-4 py-2.5 tabular-nums text-muted-foreground">{inst.number}</td>
                <td className="px-4 py-2.5 tabular-nums">{formatDate(inst.dueDate)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums">{formatCurrency(inst.amortization)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums text-destructive/80">{formatCurrency(inst.interest)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums hidden md:table-cell">{formatCurrency(inst.insurance + inst.fees)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums font-semibold">{formatCurrency(inst.totalPayment)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums hidden lg:table-cell text-muted-foreground">{formatCurrency(inst.closingBalance)}</td>
                <td className="px-4 py-2.5 text-center">
                  <Badge variant={INST_STATUS_VARIANTS[inst.status] ?? 'secondary'}>
                    {enumLabel(financingInstallmentStatuses, inst.status)}
                  </Badge>
                </td>
                {onMarkPaid && (
                  <td className="px-4 py-2.5">
                    {inst.status !== 1 ? (
                      <Button variant="ghost" size="sm" onClick={() => setPayTarget(inst)} className="h-7 text-xs">
                        Pagar
                      </Button>
                    ) : inst.paidAmount ? (
                      <span className="text-xs text-muted-foreground tabular-nums">{formatCurrency(inst.paidAmount)}</span>
                    ) : null}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {payTarget && (
        <PayInstallmentModal
          installment={payTarget}
          onPay={handlePay}
          onClose={() => setPayTarget(null)}
        />
      )}
    </>
  );
}
