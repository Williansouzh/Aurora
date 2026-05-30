import { ChevronLeft } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { TransactionTable } from '../components/transactions/TransactionTable';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Screen } from '../components/ui/Screen';
import { months } from '../constants/financeOptions';
import { useData } from '../hooks/useData';
import { useToast } from '../hooks/useToast';
import { cn, formatCurrency, formatDate } from '../lib/utils';

const invoiceStatuses = ['Aberta', 'Fechada', 'Paga'];
const statusVariants = ['warning', 'secondary', 'success'];

const currentPeriod = () => {
  const now = new Date();
  return { month: now.getMonth() + 1, year: now.getFullYear() };
};

export function InvoicePage({ api }) {
  const { accountId } = useParams();
  const toast = useToast();
  const [period, setPeriod] = useState(currentPeriod);
  const [payment, setPayment] = useState({ open: false, sourceAccountId: '', amount: '' });
  const [paying, setPaying] = useState(false);
  const [payError, setPayError] = useState('');

  const account = useData(() => api.get(`/api/accounts/${accountId}`), [accountId]);
  const accounts = useData(() => api.get('/api/accounts'), []);
  const categories = useData(() => api.get('/api/categories'), []);
  const invoice = useData(
    () => api.get(`/api/accounts/${accountId}/invoices/${period.month}/${period.year}`),
    [accountId, period.month, period.year]
  );

  const debitAccounts = useMemo(() => (accounts.data || []).filter((a) => a.type !== 4), [accounts.data]);

  useEffect(() => {
    setPayment((p) => ({
      ...p,
      sourceAccountId: p.sourceAccountId || debitAccounts[0]?.id || '',
      amount: p.amount || invoice.data?.totalAmount || '',
    }));
  }, [debitAccounts, invoice.data]);

  const openPayment = () => {
    setPayError('');
    setPayment({ open: true, sourceAccountId: debitAccounts[0]?.id || '', amount: invoice.data?.totalAmount || '' });
  };

  const payInvoice = async (e) => {
    e.preventDefault();
    setPayError('');
    setPaying(true);
    try {
      await api.post(`/api/invoices/${invoice.data.id}/pay`, {
        sourceAccountId: payment.sourceAccountId,
        amount: Number(payment.amount),
      });
      setPayment({ open: false, sourceAccountId: '', amount: '' });
      await invoice.reload();
      accounts.reload();
      account.reload();
      toast.success('Fatura paga');
    } catch (err) {
      setPayError(err.message);
    } finally {
      setPaying(false);
    }
  };

  const yearOptions = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - 2 + i);
  const status = invoice.data?.status ?? 0;

  return (
    <Screen
      title="Fatura do Cartão"
      loading={account.loading || accounts.loading || invoice.loading}
      error={account.error || accounts.error || invoice.error}
    >
      {/* Back */}
      <Button asChild variant="ghost" size="sm" className="-ml-2 text-muted-foreground">
        <Link to="/accounts">
          <ChevronLeft className="h-4 w-4" />
          Voltar para contas
        </Link>
      </Button>

      {/* Hero */}
      <Card>
        <CardContent className="p-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-xl font-bold">{account.data?.name}</h2>
              <p className="text-sm text-muted-foreground mt-0.5">
                Limite {formatCurrency(account.data?.creditLimit)} · Disponível {formatCurrency(account.data?.availableLimit)}
              </p>
            </div>
            <div className="text-right">
              <p className="text-xs text-muted-foreground">Fatura selecionada</p>
              <p className="text-3xl font-bold tabular-nums">{formatCurrency(invoice.data?.totalAmount)}</p>
              <Badge variant={statusVariants[status] ?? 'secondary'} className="mt-1">
                {invoiceStatuses[status] ?? '—'}
              </Badge>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Period selector */}
      <div className="flex flex-wrap items-center gap-3">
        <select
          value={period.month}
          onChange={(e) => setPeriod((p) => ({ ...p, month: Number(e.target.value) }))}
          className="flex h-9 rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
        >
          {months.map((name, i) => <option key={name} value={i + 1}>{name}</option>)}
        </select>
        <select
          value={period.year}
          onChange={(e) => setPeriod((p) => ({ ...p, year: Number(e.target.value) }))}
          className="flex h-9 rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
        >
          {yearOptions.map((y) => <option key={y} value={y}>{y}</option>)}
        </select>
        {invoice.data?.dueDate && (
          <span className="text-sm text-muted-foreground">Vencimento: {formatDate(invoice.data.dueDate)}</span>
        )}
        <Button
          className="ml-auto"
          disabled={!invoice.data?.id || status === 2 || !invoice.data?.totalAmount}
          onClick={openPayment}
        >
          Pagar fatura
        </Button>
      </div>

      {/* Transaction list */}
      <Card>
        <CardHeader className="flex-row items-center justify-between pb-2">
          <CardTitle className="text-sm font-semibold">Transações da fatura</CardTitle>
          <span className="text-xs text-muted-foreground">{invoice.data?.transactions?.length ?? 0} itens</span>
        </CardHeader>
        <CardContent className="p-0">
          <TransactionTable
            transactions={invoice.data?.transactions || []}
            accounts={accounts.data || []}
            categories={categories.data || []}
            compact
          />
          {(invoice.data?.transactions?.length ?? 0) === 0 && (
            <p className="py-10 text-center text-sm text-muted-foreground">Nenhuma transação nesta fatura.</p>
          )}
        </CardContent>
      </Card>

      {/* Payment dialog */}
      <Dialog open={payment.open} onOpenChange={(o) => { if (!o) setPayment((p) => ({ ...p, open: false })); }}>
        <DialogContent className="sm:max-w-sm">
          <DialogHeader>
            <DialogTitle>Pagar fatura</DialogTitle>
          </DialogHeader>
          <form id="pay-form" onSubmit={payInvoice} className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label htmlFor="pay-source">Conta de débito</Label>
              <select
                id="pay-source"
                value={payment.sourceAccountId}
                onChange={(e) => setPayment((p) => ({ ...p, sourceAccountId: e.target.value }))}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
              >
                {debitAccounts.map((a) => <option key={a.id} value={a.id}>{a.name}</option>)}
              </select>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="pay-amount">Valor</Label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
                <Input
                  id="pay-amount"
                  type="number"
                  step="0.01"
                  min="0.01"
                  value={payment.amount}
                  onChange={(e) => setPayment((p) => ({ ...p, amount: e.target.value }))}
                  className="pl-8"
                  required
                />
              </div>
            </div>
            {payError && <p className="text-sm text-destructive">{payError}</p>}
          </form>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPayment((p) => ({ ...p, open: false }))}>Cancelar</Button>
            <Button form="pay-form" type="submit" disabled={paying || !payment.sourceAccountId}>
              {paying ? 'Pagando...' : 'Confirmar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Screen>
  );
}
