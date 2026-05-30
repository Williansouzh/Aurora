import { useState } from 'react';
import { financingInstallmentStatuses } from '../../constants/financeOptions';
import { enumLabel } from '../../utils/enumHelpers';
import { formatCurrency, formatDate } from '../../lib/utils';
import { Badge } from '../ui/badge';
import { Button } from '../ui/button';
import { PayInstallmentModal } from './PayInstallmentModal';

const INST_STATUS_VARIANTS = {
  0: 'secondary',
  1: 'success',
  2: 'destructive',
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
            <tr className="border-b bg-muted/40 text-xs text-muted-foreground">
              <th className="px-4 py-2 text-left font-medium">#</th>
              <th className="px-4 py-2 text-left font-medium">Vencimento</th>
              <th className="px-4 py-2 text-right font-medium">Amortização</th>
              <th className="px-4 py-2 text-right font-medium">Juros</th>
              <th className="px-4 py-2 text-right font-medium hidden md:table-cell">Seg/Taxas</th>
              <th className="px-4 py-2 text-right font-medium">Parcela</th>
              <th className="px-4 py-2 text-right font-medium hidden lg:table-cell">Saldo</th>
              <th className="px-4 py-2 text-center font-medium">Status</th>
              {onMarkPaid && <th className="px-4 py-2" />}
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {visible.map((inst) => (
              <tr
                key={inst.number}
                className={inst.status === 1 ? 'opacity-60 bg-muted/20' : 'hover:bg-muted/30 transition-colors'}
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
