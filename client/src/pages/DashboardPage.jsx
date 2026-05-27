import { ArrowDownRight, ArrowUpRight, ChevronLeft, ChevronRight, LayoutDashboard, Receipt, TrendingUp, Wallet } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { BudgetBar } from '../components/budgets/BudgetBar';
import { UpcomingDuesList } from '../components/dashboard/UpcomingDuesList';
import { VariationBadge } from '../components/dashboard/VariationBadge';
import { FinancingSummaryCard } from '../components/financings/FinancingSummaryCard';
import { UpcomingInstallmentsList } from '../components/financings/UpcomingInstallmentsList';
import { TransactionTable } from '../components/transactions/TransactionTable';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Progress } from '../components/ui/progress';
import { Screen } from '../components/ui/Screen';
import { Skeleton } from '../components/ui/Skeleton';
import { months } from '../constants/financeOptions';
import { useData } from '../hooks/useData';
import { cn, formatCurrency } from '../lib/utils';

export function DashboardPage({ api }) {
  const now = new Date();
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());
  const query = `month=${month}&year=${year}`;

  const summary = useData(() => api.get(`/api/dashboard/monthly-summary?${query}`), [month, year]);
  const expenses = useData(() => api.get(`/api/dashboard/category-expenses?${query}`), [month, year]);
  const budgets = useData(() => api.get(`/api/budgets?${query}`), [month, year]);
  const cashFlow = useData(() => api.get(`/api/dashboard/cash-flow?year=${year}`), [year]);
  const previousCashFlow = useData(() => api.get(`/api/dashboard/cash-flow?year=${year - 1}`), [year]);
  const financingSummary = useData(() => api.get('/api/dashboard/financing-summary'), []);

  if (summary.loading) return <DashboardSkeleton />;
  if (summary.error) return <Screen title="Dashboard" error={summary.error} />;

  const data = summary.data;
  const maxFlow = Math.max(
    ...(cashFlow.data || []).flatMap((item) => [item.income, item.expense]),
    ...(previousCashFlow.data || []).map((item) => item.expense),
    1,
  );
  const budgetHighlights = (budgets.data || [])
    .filter((item) => item.hasBudget)
    .sort((a, b) => b.usagePercentage - a.usagePercentage)
    .slice(0, 4);
  const orderedExpenses = [...(expenses.data || [])].sort((a, b) => b.total - a.total).slice(0, 6);
  const totalExpense = orderedExpenses.reduce((s, e) => s + e.total, 0) || 1;

  const monthLabel = `${months[month - 1]} ${year}`;

  const navigateMonth = (delta) => {
    let m = month + delta;
    let y = year;
    if (m < 1) { m = 12; y--; }
    if (m > 12) { m = 1; y++; }
    setMonth(m);
    setYear(y);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <LayoutDashboard className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">Dashboard</h1>
        </div>
        <div className="flex items-center gap-1 rounded-lg border bg-card px-1 py-1">
          <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(-1)}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="px-3 text-sm font-medium min-w-[130px] text-center">{monthLabel}</span>
          <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(1)}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Stat Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Saldo total"
          value={formatCurrency(data.totalBalance)}
          icon={Wallet}
          iconBg="bg-indigo-50"
          iconColor="text-indigo-600"
          valueColor={data.totalBalance >= 0 ? 'text-foreground' : 'text-red-700'}
        />
        <StatCard
          title="Receitas do mês"
          value={formatCurrency(data.monthlyIncome)}
          icon={ArrowUpRight}
          iconBg="bg-emerald-50"
          iconColor="text-emerald-600"
          valueColor="text-emerald-700"
          badge={<VariationBadge value={data.incomeVariation} />}
        />
        <StatCard
          title="Despesas do mês"
          value={formatCurrency(data.monthlyExpense)}
          icon={ArrowDownRight}
          iconBg="bg-red-50"
          iconColor="text-red-600"
          valueColor="text-red-700"
          badge={<VariationBadge value={data.expenseVariation} invert />}
        />
        <StatCard
          title="Resultado"
          value={formatCurrency(data.monthlyResult)}
          icon={TrendingUp}
          iconBg={data.monthlyResult >= 0 ? 'bg-emerald-50' : 'bg-red-50'}
          iconColor={data.monthlyResult >= 0 ? 'text-emerald-600' : 'text-red-600'}
          valueColor={data.monthlyResult >= 0 ? 'text-emerald-700' : 'text-red-700'}
        />
      </div>

      {/* Savings rate + Upcoming */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <SavingsCard rate={Number(data.savingsRate || 0)} />
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-semibold">Próximos vencimentos</CardTitle>
          </CardHeader>
          <CardContent>
            <UpcomingDuesList transactions={data.upcomingDueTransactions || []} />
          </CardContent>
        </Card>
      </div>

      {/* Budgets + Expenses by category */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader className="flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-semibold">Orçamentos do mês</CardTitle>
            <Button asChild variant="ghost" size="sm" className="h-7 text-xs text-muted-foreground">
              <Link to="/budgets">Ver todos</Link>
            </Button>
          </CardHeader>
          <CardContent className="space-y-3">
            {budgetHighlights.length === 0 && (
              <p className="text-sm text-muted-foreground py-4 text-center">Nenhum limite definido para este mês.</p>
            )}
            {budgetHighlights.map((item) => {
              const pct = Math.round(item.usagePercentage);
              return (
                <div key={item.categoryId} className="space-y-1.5">
                  <div className="flex items-center justify-between text-sm">
                    <div className="flex items-center gap-2">
                      <span className="h-2.5 w-2.5 rounded-full shrink-0" style={{ background: item.categoryColor }} />
                      <span className="font-medium">{item.categoryName}</span>
                    </div>
                    <div className="flex items-center gap-2 text-xs text-muted-foreground">
                      <span>{formatCurrency(item.spentAmount)}</span>
                      <span>de</span>
                      <span>{formatCurrency(item.limitAmount)}</span>
                      <span className={cn('font-semibold tabular-nums', pct >= 100 ? 'text-red-600' : pct >= 70 ? 'text-amber-600' : 'text-emerald-600')}>
                        {pct}%
                      </span>
                    </div>
                  </div>
                  <BudgetBar spentAmount={item.spentAmount} limitAmount={item.limitAmount || 0} />
                </div>
              );
            })}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-semibold">Gastos por categoria</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2.5">
            {orderedExpenses.length === 0 && (
              <p className="text-sm text-muted-foreground py-4 text-center">Sem despesas no período.</p>
            )}
            {orderedExpenses.map((item) => (
              <div key={item.categoryId} className="space-y-1">
                <div className="flex items-center justify-between text-sm">
                  <div className="flex items-center gap-2">
                    <span className="h-2.5 w-2.5 rounded-full shrink-0" style={{ background: item.categoryColor }} />
                    <span className="font-medium">{item.categoryName}</span>
                  </div>
                  <div className="flex items-center gap-2 text-xs">
                    <span className="text-muted-foreground">{item.percentage}%</span>
                    <span className="font-semibold tabular-nums text-red-700">{formatCurrency(item.total)}</span>
                  </div>
                </div>
                <Progress
                  value={(item.total / totalExpense) * 100}
                  indicatorClassName="bg-red-400"
                  className="h-1"
                />
              </div>
            ))}
          </CardContent>
        </Card>
      </div>

      {/* Cash Flow Chart */}
      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-semibold">Fluxo anual — {year}</CardTitle>
        </CardHeader>
        <CardContent>
          <CashFlowChart
            data={cashFlow.data || []}
            previousData={previousCashFlow.data || []}
            maxFlow={maxFlow}
          />
        </CardContent>
      </Card>

      {/* Financing Summary */}
      {financingSummary.data?.activeCount > 0 && (
        <>
          <FinancingSummaryCard summary={financingSummary.data} />
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-semibold">Próximas parcelas de financiamento</CardTitle>
            </CardHeader>
            <CardContent>
              <UpcomingInstallmentsList installments={financingSummary.data.upcomingInstallments || []} />
            </CardContent>
          </Card>
        </>
      )}

      {/* Recent Transactions */}
      <Card>
        <CardHeader className="flex-row items-center justify-between pb-2">
          <CardTitle className="text-sm font-semibold">Transações recentes</CardTitle>
          <div className="flex items-center gap-3 text-xs text-muted-foreground">
            <span className="flex items-center gap-1">
              <span className="inline-block h-1.5 w-1.5 rounded-full bg-emerald-500" />
              {data.paidTransactionsCount} pagas
            </span>
            <span className="flex items-center gap-1">
              <span className="inline-block h-1.5 w-1.5 rounded-full bg-amber-500" />
              {data.pendingTransactionsCount} pendentes
            </span>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          {(data.recentTransactions || []).length === 0 ? (
            <div className="flex flex-col items-center py-12 text-center">
              <div className="rounded-full bg-muted p-4 mb-3">
                <Receipt className="h-6 w-6 text-muted-foreground/40" />
              </div>
              <p className="text-sm font-medium text-muted-foreground">Nenhuma transação neste mês</p>
            </div>
          ) : (
            <TransactionTable transactions={data.recentTransactions || []} compact />
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function StatCard({ title, value, icon: Icon, iconBg, iconColor, valueColor, badge }) {
  return (
    <Card className="hover:shadow-card-hover transition-shadow">
      <CardContent className="p-6">
        <div className="flex items-start justify-between">
          <div className="space-y-1 flex-1 min-w-0">
            <p className="text-sm font-medium text-muted-foreground">{title}</p>
            <p className={cn('text-2xl font-bold tracking-tight truncate', valueColor)}>{value}</p>
            {badge && <div className="mt-1">{badge}</div>}
          </div>
          <div className={cn('rounded-lg p-2.5 shrink-0 ml-3', iconBg)}>
            <Icon className={cn('h-5 w-5', iconColor)} />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

function SavingsCard({ rate }) {
  const tone = rate >= 20 ? 'emerald' : rate >= 5 ? 'amber' : 'red';
  const bg = { emerald: 'bg-emerald-50 border-emerald-100', amber: 'bg-amber-50 border-amber-100', red: 'bg-red-50 border-red-100' }[tone];
  const text = { emerald: 'text-emerald-700', amber: 'text-amber-700', red: 'text-red-700' }[tone];
  const sub = { emerald: 'text-emerald-600', amber: 'text-amber-600', red: 'text-red-600' }[tone];
  const msg = rate >= 20 ? 'Ótima taxa de poupança!' : rate >= 5 ? 'Você está poupando algo' : 'Atenção com os gastos';

  return (
    <Card className={cn('border', bg)}>
      <CardContent className="p-6">
        <p className={cn('text-sm font-medium mb-2', text)}>Taxa de economia</p>
        <p className={cn('text-3xl font-bold mb-1', text)}>{rate.toFixed(1)}%</p>
        <p className={cn('text-sm', sub)}>{msg} da sua renda este mês</p>
        <div className="mt-3">
          <Progress
            value={Math.min(rate, 100)}
            indicatorClassName={cn(
              tone === 'emerald' && 'bg-emerald-500',
              tone === 'amber' && 'bg-amber-500',
              tone === 'red' && 'bg-red-500'
            )}
            className={cn(
              tone === 'emerald' && 'bg-emerald-200',
              tone === 'amber' && 'bg-amber-200',
              tone === 'red' && 'bg-red-200'
            )}
          />
        </div>
      </CardContent>
    </Card>
  );
}

function CashFlowChart({ data, previousData, maxFlow }) {
  const monthAbbr = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
  return (
    <div className="flex items-end gap-1 h-40 w-full">
      {data.map((item, i) => {
        const prevExpense = previousData.find((p) => p.month === item.month)?.expense || 0;
        const incomeH = Math.max((item.income / maxFlow) * 100, 2);
        const expenseH = Math.max((item.expense / maxFlow) * 100, 2);
        const prevH = Math.max((prevExpense / maxFlow) * 100, 0);
        return (
          <div key={item.month} className="flex-1 flex flex-col items-center gap-0.5">
            <div className="flex items-end gap-[2px] h-full w-full">
              {prevH > 0 && (
                <div
                  title={`Despesa ano anterior: ${formatCurrency(prevExpense)}`}
                  className="flex-1 rounded-t-sm opacity-25 bg-red-400 transition-all"
                  style={{ height: `${prevH}%` }}
                />
              )}
              <div
                title={`Receita: ${formatCurrency(item.income)}`}
                className="flex-1 rounded-t-sm bg-emerald-500 transition-all"
                style={{ height: `${incomeH}%` }}
              />
              <div
                title={`Despesa: ${formatCurrency(item.expense)}`}
                className="flex-1 rounded-t-sm bg-red-500 transition-all"
                style={{ height: `${expenseH}%` }}
              />
            </div>
            <span className="text-[10px] text-muted-foreground mt-1">{monthAbbr[i]}</span>
          </div>
        );
      })}
      {data.length === 0 && (
        <p className="w-full text-center text-sm text-muted-foreground py-8">Sem dados de fluxo de caixa</p>
      )}
    </div>
  );
}

function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between">
        <Skeleton className="h-8 w-40" />
        <Skeleton className="h-9 w-44" />
      </div>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="rounded-xl border bg-card p-6 space-y-3">
            <Skeleton className="h-4 w-2/3" />
            <Skeleton className="h-8 w-1/2" />
          </div>
        ))}
      </div>
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Skeleton className="h-40 rounded-xl" />
        <Skeleton className="h-40 rounded-xl" />
      </div>
    </div>
  );
}
