import { ChevronLeft, ChevronRight, Plus } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../components/ui/button';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn, formatCurrency } from '../lib/utils';
import { months } from '../constants/financeOptions';

// Quiet design system — Finance · Dashboard ("Financial overview")
// Backed by /api/dashboard/* (monthly-summary, cash-flow, category-expenses).

const STATUS = {
  0: { label: 'Paga', cls: 'text-income bg-income-soft' },
  1: { label: 'Pendente', cls: 'text-pending bg-pending-soft' },
  2: { label: 'Atrasada', cls: 'text-expense bg-expense-soft' },
  3: { label: 'Cancelada', cls: 'text-faint bg-muted' },
};
const TYPE_DOT = { 0: 'bg-income', 1: 'bg-expense', 2: 'bg-transfer' };

const MONTH_ABBR = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

export function FinanceOverviewPage({ api }) {
  const now = new Date();
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());

  const summary = useData(() => api.get(`/api/dashboard/monthly-summary?month=${month}&year=${year}`), [month, year]);
  const cashFlow = useData(() => api.get(`/api/dashboard/cash-flow?year=${year}`), [year]);
  const categories = useData(() => api.get(`/api/dashboard/category-expenses?month=${month}&year=${year}`), [month, year]);

  const navigateMonth = (delta) => {
    let m = month + delta;
    let y = year;
    if (m < 1) { m = 12; y--; }
    if (m > 12) { m = 1; y++; }
    setMonth(m);
    setYear(y);
  };

  const data = summary.data;
  const recent = data?.recentTransactions ?? [];
  const upcoming = data?.upcomingDueTransactions ?? [];
  const cats = categories.data ?? [];
  const flow = cashFlow.data ?? [];
  const txCount = (data?.paidTransactionsCount ?? 0) + (data?.pendingTransactionsCount ?? 0);

  return (
    <div className="space-y-6">
      {/* Header — title + month stepper + primary action */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="font-display text-3xl text-foreground">Visão financeira</h1>
          <p className="mt-1 text-sm text-mut2">
            {months[month - 1]} de {year} · {txCount} {txCount === 1 ? 'transação' : 'transações'}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <div className="flex items-center gap-1 rounded-lg border border-chipline bg-card px-1 py-1">
            <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(-1)} aria-label="Mês anterior">
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <span className="min-w-[140px] px-2 text-center text-sm font-semibold capitalize">
              {months[month - 1]} {year}
            </span>
            <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(1)} aria-label="Próximo mês">
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
          <Button asChild>
            <Link to="/transactions"><Plus className="h-4 w-4" /> Nova transação</Link>
          </Button>
        </div>
      </div>

      {summary.error && <p className="text-sm text-destructive">{summary.error}</p>}

      {summary.loading ? (
        <KpiSkeleton />
      ) : data ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <Kpi label="Saldo total" value={formatCurrency(data.totalBalance)} />
          <Kpi
            label="Receitas pagas"
            value={formatCurrency(data.monthlyIncome)}
            valueClass="text-income"
            sub={data.pendingIncome > 0 ? `${formatCurrency(data.pendingIncome)} pendente` : 'tudo recebido'}
            trend={data.incomeVariation}
          />
          <Kpi
            label="Despesas pagas"
            value={formatCurrency(data.monthlyExpense)}
            valueClass="text-expense"
            sub={data.pendingExpense > 0 ? `${formatCurrency(data.pendingExpense)} pendente` : 'tudo pago'}
            trend={data.expenseVariation}
            trendInverted
          />
          <Kpi
            label="Resultado"
            value={`${data.monthlyResult >= 0 ? '+' : ''}${formatCurrency(data.monthlyResult)}`}
            valueClass="text-primary"
            sub={`taxa de poupança ${Math.round(data.savingsRate)}%`}
            accent
          />
        </div>
      ) : null}

      {/* Body — two columns */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-[1.6fr_1fr]">
        <div className="space-y-4">
          <CashFlowCard flow={flow} loading={cashFlow.loading} currentMonth={month} year={year} now={now} />
          <RecentCard recent={recent} />
        </div>
        <div className="space-y-4">
          <SpendingCard cats={cats} loading={categories.loading} />
          <UpcomingCard upcoming={upcoming} />
        </div>
      </div>
    </div>
  );
}

function Card({ className, children }) {
  return <div className={cn('rounded-[14px] border border-border bg-card', className)}>{children}</div>;
}

function CardTitle({ children, action }) {
  return (
    <div className="flex items-center justify-between px-5 pt-5">
      <h2 className="text-[15px] font-semibold text-ink2">{children}</h2>
      {action}
    </div>
  );
}

function Kpi({ label, value, valueClass, sub, trend, trendInverted, accent }) {
  return (
    <div className={cn('rounded-[14px] border border-border p-5', accent ? 'bg-muted' : 'bg-card')}>
      <p className="text-xs font-medium text-mut2">{label}</p>
      <p className={cn('mt-1 font-numeral text-[30px] leading-none', valueClass ?? 'text-foreground')}>{value}</p>
      <div className="mt-2 flex items-center gap-2 text-[11.5px]">
        {trend !== undefined && trend !== null && <TrendTag value={trend} inverted={trendInverted} />}
        {sub && <span className="text-faint">{sub}</span>}
      </div>
    </div>
  );
}

function TrendTag({ value, inverted }) {
  const up = value > 0;
  // For expenses, "up" is bad — invert the positive/negative coloring.
  const positive = inverted ? !up : up;
  if (value === 0) return <span className="text-faint">estável</span>;
  return (
    <span className={cn('font-medium', positive ? 'text-income' : 'text-expense')}>
      {up ? '▲' : '▼'} {Math.abs(value).toFixed(1)}% vs mês ant.
    </span>
  );
}

function CashFlowCard({ flow, loading, currentMonth, year, now }) {
  const max = Math.max(1, ...flow.map((f) => Math.max(f.income, f.expense)));
  const isFuture = (m) => year > now.getFullYear() || (year === now.getFullYear() && m > now.getMonth() + 1);

  return (
    <Card>
      <CardTitle>Fluxo de caixa · {year}</CardTitle>
      <div className="px-5 pb-5 pt-4">
        {loading ? (
          <Skeleton className="h-40 w-full rounded-lg" />
        ) : (
          <div className="flex h-44 items-end gap-2">
            {Array.from({ length: 12 }, (_, i) => {
              const m = i + 1;
              const item = flow.find((f) => f.month === m) ?? { income: 0, expense: 0 };
              const future = isFuture(m);
              const active = m === currentMonth;
              return (
                <div key={m} className="flex flex-1 flex-col items-center gap-1.5" title={`${MONTH_ABBR[i]} · +${formatCurrency(item.income)} / −${formatCurrency(item.expense)}`}>
                  <div className="flex h-36 w-full items-end justify-center gap-[3px]" style={{ opacity: future ? 0.32 : 1 }}>
                    <div className="w-1/2 rounded-t bg-income" style={{ height: `${(item.income / max) * 100}%`, minHeight: item.income > 0 ? 3 : 0 }} />
                    <div className="w-1/2 rounded-t bg-expense" style={{ height: `${(item.expense / max) * 100}%`, minHeight: item.expense > 0 ? 3 : 0 }} />
                  </div>
                  <span className={cn('text-[10px] capitalize', active ? 'font-bold text-primary' : 'text-faint')}>{MONTH_ABBR[i]}</span>
                </div>
              );
            })}
          </div>
        )}
        <div className="mt-4 flex items-center gap-4 text-[11px] text-mut2">
          <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-income" /> Receitas</span>
          <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-expense" /> Despesas</span>
        </div>
      </div>
    </Card>
  );
}

function RecentCard({ recent }) {
  return (
    <Card>
      <CardTitle action={<Link to="/transactions" className="text-[13px] font-semibold text-primary hover:underline">Ver todas →</Link>}>
        Transações recentes
      </CardTitle>
      <div className="px-5 pb-3 pt-3">
        {recent.length === 0 ? (
          <p className="py-6 text-center text-sm text-faint">Nenhuma transação ainda.</p>
        ) : (
          <div className="divide-y divide-line2">
            {recent.slice(0, 6).map((t) => {
              const status = STATUS[t.status] ?? STATUS[1];
              const sign = t.type === 1 ? '−' : '+';
              const amountCls = t.type === 0 ? 'text-income' : t.type === 2 ? 'text-mut2' : 'text-foreground';
              return (
                <div key={t.id} className="flex items-center gap-3 py-2.5">
                  <span className={cn('h-[7px] w-[7px] shrink-0 rounded-full', TYPE_DOT[t.type] ?? 'bg-mut2')} />
                  <span className="flex-1 truncate text-sm text-foreground">{t.description}</span>
                  <span className={cn('shrink-0 rounded-md px-2 py-0.5 text-[11px] font-medium', status.cls)}>{status.label}</span>
                  <span className={cn('w-24 shrink-0 text-right text-sm font-semibold tabular-nums', amountCls)}>
                    {sign}{formatCurrency(t.amount)}
                  </span>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </Card>
  );
}

function SpendingCard({ cats, loading }) {
  const max = Math.max(1, ...cats.map((c) => c.percentage));
  return (
    <Card>
      <CardTitle>Gastos por categoria</CardTitle>
      <div className="space-y-3.5 px-5 pb-5 pt-4">
        {loading ? (
          [0, 1, 2, 3].map((i) => <Skeleton key={i} className="h-5 w-full" />)
        ) : cats.length === 0 ? (
          <p className="py-4 text-center text-sm text-faint">Sem despesas neste mês.</p>
        ) : (
          cats.slice(0, 6).map((c) => (
            <div key={c.categoryId}>
              <div className="mb-1 flex items-center justify-between text-[13px]">
                <span className="truncate pr-2 text-ink2">{c.categoryName}</span>
                <span className="shrink-0 tabular-nums text-mut2">{formatCurrency(c.total)}</span>
              </div>
              <div className="h-1.5 overflow-hidden rounded-full bg-track">
                <div className="h-full rounded-full" style={{ width: `${(c.percentage / max) * 100}%`, background: c.categoryColor || 'hsl(var(--primary))' }} />
              </div>
            </div>
          ))
        )}
      </div>
    </Card>
  );
}

function UpcomingCard({ upcoming }) {
  const chip = (days) => {
    if (days < 0) return { label: `${Math.abs(days)}d atrasado`, cls: 'text-expense bg-expense-soft' };
    if (days === 0) return { label: 'hoje', cls: 'text-pending bg-pending-soft' };
    return { label: `em ${days}d`, cls: 'text-mut2 bg-muted' };
  };
  return (
    <Card>
      <CardTitle>A vencer & atrasadas</CardTitle>
      <div className="px-5 pb-3 pt-3">
        {upcoming.length === 0 ? (
          <p className="py-6 text-center text-sm text-faint">Nada a vencer nos próximos dias.</p>
        ) : (
          <div className="divide-y divide-line2">
            {upcoming.slice(0, 6).map((t) => {
              const c = chip(t.daysUntilDue);
              return (
                <div key={t.id} className="flex items-center gap-3 py-2.5">
                  <span className="flex-1 truncate text-sm text-foreground">{t.description}</span>
                  <span className={cn('shrink-0 rounded-full px-2.5 py-0.5 text-[11px] font-medium', c.cls)}>{c.label}</span>
                  <span className="w-20 shrink-0 text-right text-sm font-semibold tabular-nums text-foreground">{formatCurrency(t.amount)}</span>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </Card>
  );
}

function KpiSkeleton() {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {[0, 1, 2, 3].map((i) => <Skeleton key={i} className="h-28 rounded-[14px]" />)}
    </div>
  );
}
