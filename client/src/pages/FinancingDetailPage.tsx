import { Building2, ChevronLeft, Edit2, PiggyBank, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { FinancingForm } from '../components/financings/FinancingForm';
import { InstallmentTable } from '../components/financings/InstallmentTable';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Progress } from '../components/ui/progress';
import { Screen } from '../components/ui/Screen';
import { amortizationSystems, financingStatuses, financingTypes } from '../constants/financeOptions';
import { useData } from '../hooks/useData';
import { useToast } from '../hooks/useToast';
import { formatCurrency } from '../lib/utils';
import { enumLabel } from '../utils/enumHelpers';

const STATUS_VARIANTS = { 0: 'success', 1: 'secondary', 2: 'warning' };

export function FinancingDetailPage({ api }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const financing = useData(() => api.get(`/api/financings/${id}`), [id]);
  const toast = useToast();
  const [editing, setEditing] = useState(false);
  const [extraAmount, setExtraAmount] = useState(10000);
  const [extraSim, setExtraSim] = useState(null);
  const [confirmDelete, setConfirmDelete] = useState(false);

  async function handleMarkPaid(financingId, number, body) {
    try {
      await api.patch(`/api/financings/${financingId}/installments/${number}/mark-as-paid`, body);
      financing.reload();
      toast.success('Parcela marcada como paga');
    } catch (err) {
      toast.error(err.message);
    }
  }

  async function handleUpdate(data) {
    try {
      await api.put(`/api/financings/${id}`, data);
      financing.reload();
      setEditing(false);
      toast.success('Financiamento atualizado');
    } catch (err) {
      toast.error(err.message);
      throw err;
    }
  }

  async function handleDelete() {
    try {
      await api.delete(`/api/financings/${id}`);
      navigate('/financings');
    } catch (err) {
      toast.error(err.message);
    }
  }

  async function simulateExtra() {
    try {
      const result = await api.post(`/api/financings/${id}/simulate-extra-amortization`, {
        userId: '',
        financingId: id,
        extraAmount: Number(extraAmount),
      });
      setExtraSim(result);
    } catch (err) {
      toast.error(err.message);
    }
  }

  if (financing.loading) return <Screen title="Financiamento" loading />;
  if (financing.error) return <Screen title="Financiamento" error={financing.error} />;

  const f = financing.data;
  if (!f) return null;

  const nextInstallment = f.installments.find((i) => i.status !== 1);

  const metrics = [
    { label: 'Valor do bem', value: formatCurrency(f.assetValue) },
    { label: 'Financiado', value: formatCurrency(f.financedAmount) },
    { label: 'Saldo devedor', value: formatCurrency(f.remainingBalance), tone: 'bad' },
    { label: 'Total de juros', value: formatCurrency(f.totalInterest), tone: 'bad' },
    { label: 'Amortizado', value: `${f.progressPercentage}%`, tone: 'good' },
    { label: '% juros/total', value: `${f.interestSharePercentage}%`, tone: 'bad' },
    { label: 'Juros pagos', value: formatCurrency(f.paidInterest) },
    { label: 'Próxima parcela', value: nextInstallment ? formatCurrency(nextInstallment.totalPayment) : 'Quitado' },
  ];

  return (
    <Screen title={f.name}>
      {/* Navigation + actions */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <Button asChild variant="ghost" size="sm" className="-ml-2 text-muted-foreground">
          <Link to="/financings">
            <ChevronLeft className="h-4 w-4" />
            Financiamentos
          </Link>
        </Button>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => setEditing(true)}>
            <Edit2 className="h-3.5 w-3.5" />Editar
          </Button>
          <Button variant="destructive" size="sm" onClick={() => setConfirmDelete(true)}>
            <Trash2 className="h-3.5 w-3.5" />Excluir
          </Button>
        </div>
      </div>

      {/* Hero card */}
      <Card>
        <CardContent className="p-6 space-y-5">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div className="flex items-center gap-3">
              <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
                <Building2 className="h-5 w-5" />
              </div>
              <div>
                <h2 className="text-lg font-bold">{f.name}</h2>
                <p className="text-sm text-muted-foreground">
                  {f.institution || 'Sem instituição'} · {enumLabel(financingTypes, f.type)} · {enumLabel(amortizationSystems, f.amortizationSystem)}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2 flex-wrap">
              <Badge variant={STATUS_VARIANTS[f.status] ?? 'secondary'}>
                {enumLabel(financingStatuses, f.status)}
              </Badge>
              <span className="text-sm text-muted-foreground">{f.paidInstallments}/{f.termMonths} parcelas pagas</span>
            </div>
          </div>

          {/* Metrics grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-x-6 gap-y-4">
            {metrics.map(({ label, value, tone }) => (
              <div key={label}>
                <p className="text-xs text-muted-foreground">{label}</p>
                <p className={`font-semibold tabular-nums mt-0.5 ${tone === 'bad' ? 'text-destructive' : tone === 'good' ? 'text-emerald-600' : ''}`}>
                  {value}
                </p>
              </div>
            ))}
          </div>

          {/* Amortization progress */}
          <div className="space-y-1.5">
            <div className="flex justify-between text-xs text-muted-foreground">
              <span>Progresso de amortização</span>
              <span>{formatCurrency(f.paidPrincipal)} amortizados</span>
            </div>
            <Progress value={f.progressPercentage} className="h-2" indicatorClassName="bg-indigo-500" />
          </div>

          {f.notes && (
            <p className="text-sm text-muted-foreground border-t pt-4">{f.notes}</p>
          )}
        </CardContent>
      </Card>

      {/* Extra amortization simulator */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-semibold flex items-center gap-2">
            <PiggyBank className="h-4 w-4 text-indigo-500" />
            Simular amortização extra
          </CardTitle>
          <p className="text-xs text-muted-foreground mt-0.5">
            Veja quanto de juros e meses você economizaria abatendo parte do saldo hoje.
          </p>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-end gap-3">
            <div className="space-y-1.5 flex-1 max-w-xs">
              <Label htmlFor="extra-amount">Valor extra</Label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
                <Input
                  id="extra-amount"
                  type="number"
                  step="0.01"
                  value={extraAmount}
                  onChange={(e) => setExtraAmount(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <Button variant="secondary" onClick={simulateExtra}>Calcular</Button>
          </div>

          {extraSim && (
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 rounded-lg border bg-muted/40 p-4">
              {[
                { label: 'Economia de juros', value: formatCurrency(extraSim.interestSavings), tone: 'good' },
                { label: '% economizado', value: `${extraSim.interestSavingsPercentage}%`, tone: 'good' },
                { label: 'Meses reduzidos', value: extraSim.monthsSaved },
                { label: 'Juros restantes', value: formatCurrency(extraSim.newRemainingInterest), tone: 'bad' },
              ].map(({ label, value, tone }) => (
                <div key={label}>
                  <p className="text-xs text-muted-foreground">{label}</p>
                  <p className={`font-semibold tabular-nums mt-0.5 ${tone === 'good' ? 'text-emerald-600' : tone === 'bad' ? 'text-destructive' : ''}`}>
                    {value}
                  </p>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Installment table */}
      <Card>
        <CardHeader className="flex-row items-center justify-between pb-2">
          <CardTitle className="text-sm font-semibold">Parcelas</CardTitle>
          <span className="text-xs text-muted-foreground">
            {f.installments.length} total · {f.paidInstallments} pagas
          </span>
        </CardHeader>
        <CardContent className="p-0">
          <InstallmentTable
            installments={f.installments}
            financingId={f.id}
            onMarkPaid={handleMarkPaid}
            showAll
          />
        </CardContent>
      </Card>

      {/* Edit dialog */}
      <Dialog open={editing} onOpenChange={(o) => { if (!o) setEditing(false); }}>
        <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Editar financiamento</DialogTitle>
          </DialogHeader>
          <FinancingForm api={api} initialValues={f} onSubmit={handleUpdate} onCancel={() => setEditing(false)} />
        </DialogContent>
      </Dialog>

      {/* Delete confirmation */}
      <ConfirmDialog
        open={confirmDelete}
        onOpenChange={setConfirmDelete}
        title="Excluir financiamento"
        description="Esta ação não pode ser desfeita. Todas as parcelas serão removidas permanentemente."
        onConfirm={handleDelete}
        danger
      />
    </Screen>
  );
}
