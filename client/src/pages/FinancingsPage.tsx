import { Building2, ChevronRight, Plus, TrendingDown } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FinancingForm } from '../components/financings/FinancingForm';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { EmptyState } from '../components/ui/EmptyState';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Progress } from '../components/ui/progress';
import { Screen } from '../components/ui/Screen';
import { amortizationSystems, financingStatuses, financingTypes } from '../constants/financeOptions';
import { useData } from '../hooks/useData';
import { useToast } from '../hooks/useToast';
import { formatCurrency } from '../lib/utils';
import { enumLabel } from '../utils/enumHelpers';
import { todayInput } from '../utils/formatters';

const STATUS_VARIANTS = { 0: 'success', 1: 'secondary', 2: 'warning' };

const defaultSimParams = {
  assetValue: 500000,
  downPayment: 100000,
  annualInterestRate: 10.5,
  monthlyInsurance: 85,
  monthlyFees: 25,
  termMonths: 360,
  firstDueDate: todayInput(),
};

export function FinancingsPage({ api }) {
  const financings = useData(() => api.get('/api/financings'), []);
  const toast = useToast();
  const navigate = useNavigate();
  const [showForm, setShowForm] = useState(false);
  const [simParams, setSimParams] = useState(defaultSimParams);
  const [comparison, setComparison] = useState(null);
  const [simError, setSimError] = useState('');
  const [simLoading, setSimLoading] = useState(false);

  async function handleCreate(data) {
    const created = await api.post('/api/financings', data);
    financings.reload();
    setShowForm(false);
    toast.success('Financiamento salvo');
    navigate(`/financings/${created.id}`);
  }

  async function handleCompare() {
    setSimError('');
    setSimLoading(true);
    try {
      const result = await api.post('/api/financings/compare', { ...simParams, type: 0, userId: '' });
      setComparison(result);
    } catch (err) {
      setSimError(err.message);
    } finally {
      setSimLoading(false);
    }
  }

  const overview = buildOverview(financings.data || []);
  const setSim = (key) => (e) => setSimParams((p) => ({ ...p, [key]: Number(e.target.value) }));

  return (
    <Screen
      title="Financiamentos"
      loading={financings.loading}
      error={financings.error}
      actions={
        <Button onClick={() => setShowForm(true)}>
          <Plus className="h-4 w-4" />Novo financiamento
        </Button>
      }
    >
      {/* Overview metrics */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard label="Saldo devedor total" value={formatCurrency(overview.remainingBalance)} />
        <MetricCard label="Juros contratados" value={formatCurrency(overview.totalInterest)} tone="bad" />
        <MetricCard label="Juros já pagos" value={formatCurrency(overview.paidInterest)} />
        <MetricCard label="Progresso médio" value={`${overview.averageProgress.toFixed(1)}%`} tone="good" />
      </div>

      {/* Financing list */}
      <Card>
        <CardHeader className="flex-row items-center justify-between pb-2">
          <CardTitle className="text-sm font-semibold">Contratos</CardTitle>
          <span className="text-xs text-muted-foreground">{financings.data?.length || 0} itens</span>
        </CardHeader>
        <CardContent className="space-y-2 p-4">
          {(financings.data || []).length === 0 ? (
            <EmptyState
              icon={Building2}
              title="Nenhum financiamento"
              description="Adicione seu primeiro financiamento para acompanhar parcelas e juros."
              actionLabel="Novo financiamento"
              onAction={() => setShowForm(true)}
            />
          ) : (
            (financings.data || []).map((f) => (
              <button
                key={f.id}
                type="button"
                onClick={() => navigate(`/financings/${f.id}`)}
                className="w-full flex items-center gap-4 rounded-lg border p-4 text-left hover:bg-muted/40 transition-colors group"
              >
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-indigo-50 text-indigo-600">
                  <Building2 className="h-5 w-5" />
                </div>
                <div className="flex-1 min-w-0 space-y-1">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-semibold truncate">{f.name}</span>
                    <Badge variant={STATUS_VARIANTS[f.status] ?? 'secondary'}>
                      {enumLabel(financingStatuses, f.status)}
                    </Badge>
                  </div>
                  <p className="text-xs text-muted-foreground">
                    {f.institution || 'Sem instituição'} · {enumLabel(financingTypes, f.type)} · {enumLabel(amortizationSystems, f.amortizationSystem)}
                  </p>
                  <div className="flex items-center gap-4 text-xs text-muted-foreground">
                    <span>{formatCurrency(f.remainingBalance)} restantes</span>
                    <span>{f.paidInstallments}/{f.termMonths} parcelas</span>
                  </div>
                  <Progress value={f.progressPercentage} className="h-1.5 mt-1" indicatorClassName="bg-indigo-500" />
                </div>
                <ChevronRight className="h-4 w-4 text-muted-foreground shrink-0 group-hover:translate-x-0.5 transition-transform" />
              </button>
            ))
          )}
        </CardContent>
      </Card>

      {/* SAC vs Price comparison */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-semibold flex items-center gap-2">
            <TrendingDown className="h-4 w-4 text-indigo-500" />
            Comparar SAC vs Price
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div className="space-y-1.5">
              <Label htmlFor="sim-asset">Valor do bem</Label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
                <Input id="sim-asset" type="number" step="0.01" value={simParams.assetValue} onChange={setSim('assetValue')} className="pl-8" />
              </div>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="sim-down">Entrada</Label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
                <Input id="sim-down" type="number" step="0.01" value={simParams.downPayment} onChange={setSim('downPayment')} className="pl-8" />
              </div>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="sim-rate">Juros ao ano (%)</Label>
              <Input id="sim-rate" type="number" step="0.01" value={simParams.annualInterestRate} onChange={setSim('annualInterestRate')} />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="sim-term">Prazo (meses)</Label>
              <Input id="sim-term" type="number" value={simParams.termMonths} onChange={setSim('termMonths')} />
            </div>
          </div>

          {simError && <p className="text-sm text-destructive">{simError}</p>}

          <Button onClick={handleCompare} disabled={simLoading} variant="secondary">
            {simLoading ? 'Calculando...' : 'Calcular comparação'}
          </Button>

          {comparison && <ComparisonResult comparison={comparison} />}
        </CardContent>
      </Card>

      {/* Create dialog */}
      <Dialog open={showForm} onOpenChange={(o) => { if (!o) setShowForm(false); }}>
        <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Novo financiamento</DialogTitle>
          </DialogHeader>
          <FinancingForm api={api} onSubmit={handleCreate} onCancel={() => setShowForm(false)} />
        </DialogContent>
      </Dialog>
    </Screen>
  );
}

function MetricCard({ label, value, tone }) {
  return (
    <Card>
      <CardContent className="p-4">
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className={`text-lg font-bold tabular-nums mt-0.5 ${tone === 'bad' ? 'text-destructive' : tone === 'good' ? 'text-emerald-600' : ''}`}>
          {value}
        </p>
      </CardContent>
    </Card>
  );
}

function ComparisonResult({ comparison }) {
  return (
    <div className="grid gap-4 sm:grid-cols-3 mt-2">
      <ComparisonCard title="SAC" simulation={comparison.sac} />
      <ComparisonCard title="Price" simulation={comparison.price} />
      <Card className="bg-emerald-50 border-emerald-200 dark:bg-emerald-950/30 dark:border-emerald-800">
        <CardContent className="p-4 space-y-1">
          <p className="text-xs font-semibold text-emerald-700 dark:text-emerald-400">Economia no SAC</p>
          <p className="text-xl font-bold tabular-nums text-emerald-700 dark:text-emerald-400">
            {formatCurrency(comparison.sacInterestSavings)}
          </p>
          <p className="text-xs text-emerald-600 dark:text-emerald-500">
            {comparison.sacTotalSavingsPercentage}% do total pago no Price
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

function ComparisonCard({ title, simulation }) {
  return (
    <Card>
      <CardContent className="p-4 space-y-2">
        <p className="font-semibold text-sm">{title}</p>
        <div className="space-y-1 text-xs">
          <div className="flex justify-between">
            <span className="text-muted-foreground">1ª parcela</span>
            <span className="font-medium tabular-nums">{formatCurrency(simulation.firstPayment)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">Última parcela</span>
            <span className="font-medium tabular-nums">{formatCurrency(simulation.lastPayment)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">Juros totais</span>
            <span className="font-medium tabular-nums text-destructive">{formatCurrency(simulation.totalInterest)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">% em juros</span>
            <span className="font-medium tabular-nums">{simulation.interestSharePercentage}%</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

function buildOverview(financings) {
  if (!financings.length) return { remainingBalance: 0, totalInterest: 0, paidInterest: 0, averageProgress: 0 };
  return {
    remainingBalance: financings.reduce((s, f) => s + Number(f.remainingBalance || 0), 0),
    totalInterest: financings.reduce((s, f) => s + Number(f.totalInterest || 0), 0),
    paidInterest: financings.reduce((s, f) => s + Number(f.paidInterest || 0), 0),
    averageProgress: financings.reduce((s, f) => s + Number(f.progressPercentage || 0), 0) / financings.length,
  };
}
