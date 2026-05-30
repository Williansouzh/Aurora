import { ChevronLeft, ChevronRight, Target } from 'lucide-react';
import { useState } from 'react';
import { BudgetBar } from '../components/budgets/BudgetBar';
import { ConfirmModal } from '../components/ui/ConfirmModal';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { EmptyState } from '../components/ui/EmptyState';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Screen } from '../components/ui/Screen';
import { months } from '../constants/financeOptions';
import { useData } from '../hooks/useData';
import { useToast } from '../hooks/useToast';
import { cn, formatCurrency } from '../lib/utils';

const currentPeriod = () => {
  const now = new Date();
  return { month: now.getMonth() + 1, year: now.getFullYear() };
};

export function BudgetsPage({ api }) {
  const toast = useToast();
  const [period, setPeriod] = useState(currentPeriod);
  const [modal, setModal] = useState({ open: false, budget: null, limitAmount: '' });
  const [deleting, setDeleting] = useState(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const query = `month=${period.month}&year=${period.year}`;
  const budgets = useData(() => api.get(`/api/budgets?${query}`), [period.month, period.year]);

  const items = budgets.data || [];
  const limited = items.filter((b) => b.hasBudget);
  const insideCount = limited.filter((b) => Number(b.spentAmount) <= Number(b.limitAmount || 0)).length;

  const navigateMonth = (delta) => {
    let m = period.month + delta;
    let y = period.year;
    if (m < 1) { m = 12; y--; }
    if (m > 12) { m = 1; y++; }
    setPeriod({ month: m, year: y });
  };

  const openModal = (budget) => {
    setError('');
    setModal({ open: true, budget, limitAmount: budget.limitAmount || '' });
  };
  const closeModal = () => { setModal({ open: false, budget: null, limitAmount: '' }); setError(''); };

  const saveBudget = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      await api.post('/api/budgets', {
        userId: '',
        categoryId: modal.budget.categoryId,
        month: period.month,
        year: period.year,
        limitAmount: Number(modal.limitAmount),
      });
      closeModal();
      budgets.reload();
      toast.success('Limite salvo');
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const deleteBudget = async () => {
    if (!deleting?.id) return;
    try {
      await api.delete(`/api/budgets/${deleting.id}`);
      setDeleting(null);
      await budgets.reload();
      toast.success('Limite removido');
    } catch (err) {
      toast.error(err.message);
    }
  };

  const monthLabel = `${months[period.month - 1]} ${period.year}`;

  return (
    <Screen title="Orçamentos" loading={budgets.loading} error={budgets.error}>
      {/* Header */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-2 rounded-lg border bg-card px-1 py-1 w-fit">
          <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(-1)}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="px-3 text-sm font-medium min-w-[130px] text-center">{monthLabel}</span>
          <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(1)}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>

        {/* Summary badge */}
        {limited.length > 0 && (
          <div className="flex items-center gap-2 text-sm">
            <Badge variant={insideCount === limited.length ? 'success' : insideCount > 0 ? 'warning' : 'danger'}>
              {insideCount}/{limited.length} dentro do limite
            </Badge>
          </div>
        )}
      </div>

      {/* Empty state */}
      {items.length === 0 && !budgets.loading && (
        <EmptyState
          icon={Target}
          title="Nenhuma categoria de despesa"
          description="Crie categorias de despesa para definir limites mensais"
        />
      )}

      {/* Budget list */}
      {items.length > 0 && (
        <div className="space-y-2">
          {items.map((budget) => {
            const pct = budget.hasBudget ? Math.round(budget.usagePercentage) : null;
            const over = pct !== null && pct >= 100;
            const warn = pct !== null && pct >= 70 && pct < 100;
            return (
              <div
                key={budget.categoryId}
                className="group flex items-center gap-4 rounded-xl border bg-card px-4 py-3.5 hover:shadow-card-hover transition-shadow"
              >
                {/* Color dot + name */}
                <div className="flex items-center gap-3 flex-1 min-w-0">
                  <div className="h-8 w-8 rounded-lg shrink-0 flex items-center justify-center text-sm font-bold"
                    style={{ background: `${budget.categoryColor}20`, color: budget.categoryColor }}>
                    {budget.categoryIcon?.slice(0, 2) ?? '?'}
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <p className="text-sm font-semibold truncate">{budget.categoryName}</p>
                      {over && <Badge variant="danger" className="text-[10px] py-0 px-1.5 h-4 shrink-0">Excedido</Badge>}
                      {warn && <Badge variant="warning" className="text-[10px] py-0 px-1.5 h-4 shrink-0">Atenção</Badge>}
                    </div>
                    <div className="mt-1.5 flex items-center gap-2">
                      <div className="flex-1 max-w-[180px]">
                        <BudgetBar spentAmount={budget.spentAmount} limitAmount={budget.limitAmount || 0} />
                      </div>
                      <p className="text-xs text-muted-foreground whitespace-nowrap">
                        {formatCurrency(budget.spentAmount)}
                        {budget.hasBudget && ` de ${formatCurrency(budget.limitAmount)}`}
                      </p>
                      {pct !== null && (
                        <p className={cn('text-xs font-semibold tabular-nums shrink-0', over ? 'text-red-600' : warn ? 'text-amber-600' : 'text-emerald-600')}>
                          {pct}%
                        </p>
                      )}
                    </div>
                  </div>
                </div>

                {/* Actions */}
                <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
                  <Button variant="outline" size="sm" className="h-7 text-xs" onClick={() => openModal(budget)}>
                    {budget.hasBudget ? 'Editar' : 'Definir limite'}
                  </Button>
                  {budget.hasBudget && (
                    <Button variant="ghost" size="sm" className="h-7 text-xs text-muted-foreground hover:text-destructive" onClick={() => setDeleting(budget)}>
                      Remover
                    </Button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Budget Modal */}
      <Dialog open={modal.open} onOpenChange={(o) => { if (!o) closeModal(); }}>
        <DialogContent className="sm:max-w-sm">
          <DialogHeader>
            <DialogTitle>Definir limite — {modal.budget?.categoryName}</DialogTitle>
          </DialogHeader>
          <form id="budget-form" onSubmit={saveBudget} className="space-y-4 py-2">
            <div className="rounded-lg bg-muted px-3 py-2 text-sm text-muted-foreground">
              {monthLabel}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="budget-limit">Valor limite mensal</Label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
                <Input
                  id="budget-limit"
                  type="number"
                  step="0.01"
                  min="0.01"
                  value={modal.limitAmount}
                  onChange={(e) => setModal((m) => ({ ...m, limitAmount: e.target.value }))}
                  className="pl-8"
                  required
                />
              </div>
            </div>
            {error && <p className="text-sm text-destructive">{error}</p>}
          </form>
          <DialogFooter>
            <Button variant="outline" onClick={closeModal}>Cancelar</Button>
            <Button form="budget-form" type="submit" disabled={saving}>
              {saving ? 'Salvando...' : 'Salvar limite'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {deleting && (
        <ConfirmModal
          title="Remover orçamento?"
          description={`Remove o limite de ${formatCurrency(deleting.limitAmount)} para ${deleting.categoryName} em ${monthLabel}.`}
          confirmLabel="Remover"
          danger
          onCancel={() => setDeleting(null)}
          onConfirm={deleteBudget}
        />
      )}
    </Screen>
  );
}
