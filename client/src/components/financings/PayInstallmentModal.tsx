import { useState } from 'react';
import { Button } from '../ui/button';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '../ui/dialog';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { formatCurrency, formatDate } from '../../lib/utils';
import { todayInput } from '../../utils/formatters';

export function PayInstallmentModal({ installment, onPay, onClose }) {
  const [paidAmount, setPaidAmount] = useState(installment.totalPayment);
  const [paidAt, setPaidAt] = useState(todayInput());
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setLoading(true);
    try {
      await onPay(Number(paidAmount), new Date(paidAt + 'T12:00:00').toISOString());
    } finally {
      setLoading(false);
    }
  }

  return (
    <Dialog open onOpenChange={(o) => { if (!o) onClose(); }}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Pagar parcela #{installment.number}</DialogTitle>
        </DialogHeader>
        <div className="space-y-1 text-sm text-muted-foreground">
          <p>Vencimento: <span className="font-medium text-foreground">{formatDate(installment.dueDate)}</span></p>
          <p>Valor previsto: <span className="font-medium text-foreground">{formatCurrency(installment.totalPayment)}</span></p>
        </div>
        <form id="pay-inst-form" onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="inst-amount">Valor pago</Label>
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
              <Input
                id="inst-amount"
                type="number"
                step="0.01"
                min="0"
                value={paidAmount}
                onChange={(e) => setPaidAmount(e.target.value)}
                className="pl-8"
                required
              />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="inst-date">Data do pagamento</Label>
            <Input
              id="inst-date"
              type="date"
              value={paidAt}
              onChange={(e) => setPaidAt(e.target.value)}
              required
            />
          </div>
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancelar</Button>
          <Button form="pay-inst-form" type="submit" disabled={loading}>
            {loading ? 'Salvando...' : 'Confirmar pagamento'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
