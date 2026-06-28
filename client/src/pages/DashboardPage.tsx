import {
  ArrowDownRight,
  ArrowUpRight,
  BookOpen,
  CalendarCheck,
  CalendarDays,
  Camera,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  Flame,
  Heart,
  LayoutDashboard,
  Receipt,
  Scroll,
  Target,
  TrendingUp,
  Wallet,
  Zap,
} from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Progress } from '../components/ui/progress';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn, formatCurrency } from '../lib/utils';

const MOOD_EMOJI = { 1: ':(', 2: ':/', 3: ':|', 4: ':)', 5: ':D' };

const AREA_LABELS = {
  1: 'Saúde', 2: 'Trabalho', 3: 'Estudos', 4: 'Dinheiro',
  5: 'Relacionamentos', 6: 'Casa', 7: 'Lazer', 8: 'Espiritualidade', 9: 'Projetos',
};
// Muted, Quiet-harmonious per-area palette (shared with Goals)
const AREA_COLORS = {
  1: '#6f8f6a', 2: '#3d4eac', 3: '#7b5cd6', 4: '#c1976a', 5: '#c1796a',
  6: '#5b8a8a', 7: '#b08968', 8: '#8a7fb0', 9: '#6b7280',
};

function greeting() {
  const h = new Date().getHours();
  if (h < 12) return 'Bom dia';
  if (h < 18) return 'Boa tarde';
  return 'Boa noite';
}

function todaySubtitle() {
  return new Date().toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long' });
}

function relTime(value) {
  const d = new Date(value);
  const diffMin = Math.round((Date.now() - d.getTime()) / 60000);
  if (diffMin < 1) return 'agora';
  if (diffMin < 60) return `há ${diffMin} min`;
  const diffH = Math.round(diffMin / 60);
  if (diffH < 24) return `há ${diffH} h`;
  const diffD = Math.round(diffH / 24);
  if (diffD === 1) return 'ontem';
  if (diffD < 7) return `há ${diffD} dias`;
  return d.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });
}

const MODULE_FILTERS = [
  { key: 'overview', label: 'Geral', moduleKey: 'home', icon: LayoutDashboard },
  { key: 'today', label: 'Meu Dia', moduleKey: 'today', icon: CalendarCheck },
  { key: 'habits', label: 'Rituais', moduleKey: 'habits', icon: Flame },
  { key: 'goals', label: 'Jornada', moduleKey: 'goals', icon: Target },
  { key: 'weekly-planning', label: 'Minha Semana', moduleKey: 'weekly-planning', icon: CalendarDays },
  { key: 'timeline', label: 'Linha da Vida', moduleKey: 'timeline', icon: Scroll },
  { key: 'diary', label: 'Diario', moduleKey: 'diary', icon: BookOpen },
  { key: 'evolution', label: 'Evolucao', moduleKey: 'evolution', icon: Camera },
  { key: 'studies', label: 'Estudos', moduleKey: 'studies', icon: BookOpen },
  { key: 'retrospectives', label: 'Retrospectiva', moduleKey: 'retrospectives', icon: TrendingUp },
  { key: 'finances', label: 'Dinheiro', moduleKey: 'finances', icon: Wallet },
];

export function DashboardPage({ api, access }) {
  const now = new Date();
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());
  const [selectedModule, setSelectedModule] = useState('overview');

  const home = useData(() => api.get(`/api/home?month=${month}&year=${year}`), [month, year]);

  if (home.loading) return <HomeSkeleton />;
  if (home.error) return <p className="text-destructive text-sm">{home.error}</p>;

  const data = home.data;
  const navigateMonth = (delta) => {
    let nextMonth = month + delta;
    let nextYear = year;
    if (nextMonth < 1) { nextMonth = 12; nextYear--; }
    if (nextMonth > 12) { nextMonth = 1; nextYear++; }
    setMonth(nextMonth);
    setYear(nextYear);
  };

  const can = (moduleKey) => !access || access.modules?.some((m) => m.key === moduleKey && m.isAllowed);
  const filters = MODULE_FILTERS.filter((filter) => can(filter.moduleKey));
  const active = filters.some((filter) => filter.key === selectedModule) ? selectedModule : 'overview';
  const overview = active === 'overview';
  const show = (moduleKey) => overview ? can(moduleKey) : active === moduleKey;
  const monthLabel = new Date(year, month - 1).toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
  const levelPct = data?.xpToNextLevel > 0 ? Math.round((data.totalXp / (data.totalXp + data.xpToNextLevel)) * 100) : 100;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="font-display text-3xl text-foreground">{greeting()}</h1>
          <p className="mt-1 text-sm capitalize text-mut2">{todaySubtitle()} · sua vida em um olhar</p>
        </div>

        <div className="flex items-center gap-2">
          {overview && data?.level != null && (
            <span className="flex items-center gap-2.5 rounded-full border border-chipline bg-card px-3 py-1.5 text-sm">
              <span className="font-semibold text-ink2">Nível {data.level}</span>
              <span className="relative inline-block h-[5px] w-[54px] overflow-hidden rounded-full bg-track">
                <span
                  className="absolute inset-y-0 left-0 rounded-full bg-primary"
                  style={{ width: `${levelPct}%` }}
                />
              </span>
            </span>
          )}
          <div className="flex items-center gap-1 rounded-lg border border-chipline bg-card px-1 py-1">
            <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(-1)}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <span className="min-w-[150px] px-2 text-center text-sm font-semibold capitalize">{monthLabel}</span>
            <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(1)}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      <ModuleFilterBar filters={filters} selected={active} onSelect={setSelectedModule} />

      {overview ? (
        <OverviewDashboard data={data} can={can} />
      ) : (
        <>
          {(show('today') || show('habits')) && (
            <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
              {show('today') && <TodayCard tasks={data} />}
              {show('habits') && <HabitsCard habits={data.todayHabits} />}
            </div>
          )}

          {show('finances') && <FinanceDashboard data={data} />}

          {(show('goals') || show('diary')) && (
            <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
              {show('goals') && (
                <div className="lg:col-span-2">
                  <GoalsCard goals={data.featuredGoals} />
                </div>
              )}
              {show('diary') && <MoodCard moodHistory={data.moodHistory} todayMood={data.todayMood} />}
            </div>
          )}

          {(show('weekly-planning') || show('evolution') || show('studies') || show('retrospectives')) && (
            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              {show('weekly-planning') && <ModuleShortcutCard to="/weekly" icon={CalendarDays} title="Minha Semana" />}
              {show('evolution') && <ModuleShortcutCard to="/evolution" icon={Camera} title="Evolucao" />}
              {show('studies') && <ModuleShortcutCard to="/studies" icon={BookOpen} title="Estudos" />}
              {show('retrospectives') && <ModuleShortcutCard to="/retrospectives" icon={TrendingUp} title="Retrospectiva" />}
            </div>
          )}

          {show('timeline') && <TimelineCard events={data.recentEvents} />}
        </>
      )}
    </div>
  );
}

// ===================== Overview (Quiet dashboard) =====================

function OverviewDashboard({ data, can }) {
  return (
    <div className="space-y-[18px]">
      <KpiRow data={data} can={can} />
      <div className="flex flex-col gap-[18px] lg:flex-row lg:items-start">
        <div className="flex min-w-0 flex-1 flex-col gap-[18px] lg:flex-[1.55]">
          {(can('today') || can('habits')) && <WeeklyActivityChart data={data.weeklyActivity} />}
          {can('goals') && <GoalsFocusCard goals={data.featuredGoals} />}
          {can('timeline') && <RecentActivityCard events={data.recentEvents} />}
        </div>
        <div className="flex min-w-0 flex-1 flex-col gap-[18px]">
          {can('today') && <Top3Card data={data} />}
          {can('finances') && <MoneySnapshotCard data={data} />}
          {can('habits') && <RitualStreaksCard habits={data.todayHabits} />}
        </div>
      </div>
    </div>
  );
}

function Kpi({ label, value, bar, foot, soft }) {
  return (
    <div className={cn('rounded-xl border p-[18px]', soft ? 'bg-accent' : 'bg-card')}>
      <p className="text-xs text-faint">{label}</p>
      <p className="mt-1 font-numeral text-[30px] leading-none">{value}</p>
      {bar != null && (
        <div className="mt-2.5 h-[5px] overflow-hidden rounded-full bg-track">
          <div className="h-full rounded-full bg-primary" style={{ width: `${Math.min(bar, 100)}%` }} />
        </div>
      )}
      {foot && <p className="mt-2.5 text-[11.5px]">{foot}</p>}
    </div>
  );
}

function KpiRow({ data, can }) {
  const items = [];

  if (can('today')) {
    const total = data.pendingTasksCount + data.completedTasksCount;
    const pct = total > 0 ? (data.completedTasksCount / total) * 100 : 0;
    items.push(
      <Kpi
        key="tasks"
        label="Tarefas de hoje"
        value={<>{data.completedTasksCount}<span className="text-[15px] text-faint"> / {total}</span></>}
        bar={pct}
      />
    );
  }

  if (can('habits')) {
    const done = data.todayHabits.filter((h) => h.checkedInToday).length;
    const total = data.todayHabits.length;
    const best = data.todayHabits.reduce((m, h) => Math.max(m, h.currentStreak || 0), 0);
    items.push(
      <Kpi
        key="rituals"
        label="Rituais de hoje"
        value={<span className="text-primary">{done}<span className="text-[15px] text-faint"> / {total}</span></span>}
        foot={best > 0 ? <span className="text-income">▲ sequência de {best} dias</span> : <span className="text-mut2">comece um ritual</span>}
      />
    );
  }

  if (can('finances')) {
    const net = data.monthlyIncome - data.monthlyExpense;
    items.push(
      <Kpi
        key="net"
        label="Resultado do mês"
        value={<span className={net >= 0 ? 'text-income' : 'text-expense'}>{net >= 0 ? '+' : ''}{formatCurrency(net)}</span>}
        foot={<span className="text-mut2">{formatCurrency(data.totalBalance)} de saldo</span>}
      />
    );
  }

  items.push(
    <Kpi
      key="xp"
      soft
      label="Experiência"
      value={`${(data.totalXp ?? 0).toLocaleString('pt-BR')} XP`}
      foot={<span className="text-mut2">Nível {data.level} · {data.levelName}</span>}
    />
  );

  return <div className="grid grid-cols-2 gap-[14px] lg:grid-cols-4">{items}</div>;
}

const DOW = ['D', 'S', 'T', 'Q', 'Q', 'S', 'S'];

function WeeklyActivityChart({ data = [] }) {
  const days = data.slice(-7);
  const max = Math.max(1, ...days.map((d) => Math.max(d.tasksDone ?? 0, d.ritualsDone ?? 0)));
  const todayStr = new Date().toDateString();
  return (
    <div className="rounded-xl border bg-card p-[22px]">
      <div className="mb-5 flex items-baseline justify-between">
        <span className="text-overline">Atividade da semana</span>
        <span className="flex gap-3.5 text-xs text-mut2">
          <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-[2px] bg-primary" />Tarefas</span>
          <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-[2px] bg-income" />Rituais</span>
        </span>
      </div>
      {days.length === 0 ? (
        <p className="py-10 text-center text-xs text-muted-foreground">Sem atividade registrada ainda.</p>
      ) : (
        <div className="flex h-[150px] items-end justify-between gap-2.5">
          {days.map((day, index) => {
            const d = new Date(day.date);
            const isToday = d.toDateString() === todayStr;
            const th = Math.max((day.tasksDone / max) * 118, day.tasksDone > 0 ? 4 : 0);
            const rh = Math.max((day.ritualsDone / max) * 118, day.ritualsDone > 0 ? 4 : 0);
            return (
              <div key={index} className="flex flex-1 flex-col items-center gap-2">
                <div className="flex h-[118px] items-end gap-1">
                  <span className="w-[9px] rounded-[3px] bg-primary" style={{ height: `${th}px` }} />
                  <span className="w-[9px] rounded-[3px] bg-income" style={{ height: `${rh}px` }} />
                </div>
                <span className={cn('text-[11px]', isToday ? 'font-bold text-primary' : 'text-faint')}>
                  {DOW[d.getDay()]}
                </span>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

function GoalsFocusCard({ goals = [] }) {
  return (
    <div className="rounded-xl border bg-card p-[22px]">
      <div className="mb-[18px] flex items-baseline justify-between">
        <span className="text-overline">Em foco · metas</span>
        <Link to="/goals" className="text-[13px] font-semibold text-primary">Ver todas →</Link>
      </div>
      {goals.length === 0 ? (
        <p className="py-3 text-center text-xs text-muted-foreground">
          <Link to="/goals" className="underline">Defina seus objetivos de vida</Link>
        </p>
      ) : (
        <div className="flex flex-col gap-4">
          {goals.map((goal) => {
            const color = AREA_COLORS[goal.area] ?? '#3d4eac';
            return (
              <div key={goal.id}>
                <div className="mb-[7px] flex items-center justify-between text-sm">
                  <span className="truncate pr-2">
                    {goal.title} <span className="text-xs text-faint">· {AREA_LABELS[goal.area]}</span>
                  </span>
                  <span className="shrink-0 tabular-nums text-mut2">{Math.round(goal.progress)}%</span>
                </div>
                <div className="h-1.5 overflow-hidden rounded-full bg-track">
                  <div
                    className="h-full rounded-full"
                    style={{ width: `${Math.min(goal.progress, 100)}%`, background: color }}
                  />
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

function RecentActivityCard({ events = [] }) {
  return (
    <div className="rounded-xl border bg-card p-[22px]">
      <div className="mb-3.5 text-overline">Atividade recente</div>
      {events.length === 0 ? (
        <p className="py-4 text-center text-xs text-muted-foreground">Seus registros aparecem aqui.</p>
      ) : (
        <div className="flex flex-col">
          {events.map((event, index) => (
            <div
              key={event.id}
              className={cn('flex gap-3.5 py-[9px]', index < events.length - 1 && 'border-b border-line2')}
            >
              <span
                className="mt-1.5 h-2 w-2 shrink-0 rounded-full"
                style={{ background: event.area ? AREA_COLORS[event.area] ?? '#cfccc4' : '#cfccc4' }}
              />
              <div className="min-w-0">
                <p className="text-sm">{event.title}</p>
                <p className="text-xs text-faint">
                  {relTime(event.occurredAt)}{event.area ? ` · ${AREA_LABELS[event.area]}` : ''}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function Top3Card({ data }) {
  const tasks = (data.topPendingTasks ?? []).slice(0, 3);
  return (
    <div className="rounded-xl border bg-card p-[22px]">
      <div className="mb-3.5 flex items-baseline justify-between">
        <span className="text-overline">Hoje · Top 3</span>
        <span className="text-[13px] text-mut2">{data.completedTasksCount} concluídas</span>
      </div>
      {tasks.length === 0 ? (
        <p className="py-3 text-center text-xs text-muted-foreground">Nenhuma tarefa pendente. 🎉</p>
      ) : (
        <div className="flex flex-col">
          {tasks.map((task, index) => (
            <Link
              key={task.id}
              to="/today"
              className={cn('flex items-center gap-3.5 py-2.5', index < tasks.length - 1 && 'border-b border-line2')}
            >
              <span className="h-[18px] w-[18px] shrink-0 rounded-[5px] border-[1.5px] border-track" />
              <span className="flex-1 truncate text-[14.5px]">{task.title}</span>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}

function MoneySnapshotCard({ data }) {
  const net = data.monthlyIncome - data.monthlyExpense;
  return (
    <div className="rounded-xl border bg-card p-[22px]">
      <div className="mb-3 text-overline">Este mês · dinheiro</div>
      <div className="font-numeral text-[32px] leading-none">{formatCurrency(data.totalBalance)}</div>
      <div className="mb-4 mt-1.5 text-xs text-mut2">saldo total</div>
      <div className="flex gap-2.5">
        <SnapshotStat label="Entrou" value={formatCurrency(data.monthlyIncome)} className="border-income" />
        <SnapshotStat label="Saiu" value={formatCurrency(data.monthlyExpense)} className="border-expense" />
        <SnapshotStat label="Resultado" value={`${net >= 0 ? '+' : ''}${formatCurrency(net)}`} className="border-primary" />
      </div>
    </div>
  );
}

function SnapshotStat({ label, value, className }) {
  return (
    <div className={cn('min-w-0 flex-1 border-t-2 pt-2', className)}>
      <div className="text-[11px] text-faint">{label}</div>
      <div className="truncate text-sm font-semibold tabular-nums">{value}</div>
    </div>
  );
}

function RitualStreaksCard({ habits = [] }) {
  const sorted = [...habits].sort((a, b) => (b.currentStreak || 0) - (a.currentStreak || 0)).slice(0, 4);
  return (
    <div className="rounded-xl border bg-card p-[22px]">
      <div className="mb-4 text-overline">Sequências de rituais</div>
      {sorted.length === 0 ? (
        <p className="py-3 text-center text-xs text-muted-foreground">
          <Link to="/habits" className="underline">Crie seus primeiros rituais</Link>
        </p>
      ) : (
        <div className="grid grid-cols-2 gap-3">
          {sorted.map((habit) => {
            const cold = (habit.currentStreak || 0) === 0;
            return (
              <Link
                key={habit.id}
                to="/habits"
                className={cn('rounded-[10px] border p-3.5 text-center', cold && 'bg-secondary')}
              >
                <div className="truncate text-[13px] font-semibold">{habit.name}</div>
                <div className={cn('mt-1 font-numeral text-2xl', cold ? 'text-track' : 'text-primary')}>
                  {habit.currentStreak || 0}
                </div>
                <div className="text-[11px] text-faint">{cold ? 'comece →' : 'dias'}</div>
              </Link>
            );
          })}
        </div>
      )}
    </div>
  );
}

function ModuleFilterBar({ filters, selected, onSelect }) {
  return (
    <div className="overflow-x-auto">
      <div className="flex min-w-max gap-2 rounded-lg border bg-card p-1">
        {filters.map((filter) => {
          const Icon = filter.icon;
          const active = selected === filter.key;
          return (
            <button
              key={filter.key}
              onClick={() => onSelect(filter.key)}
              className={cn(
                'inline-flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                active ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:bg-muted hover:text-foreground'
              )}
            >
              <Icon className="h-4 w-4" />
              {filter.label}
            </button>
          );
        })}
      </div>
    </div>
  );
}

function DashboardSection({ title, children }) {
  if (!title) return children;
  return (
    <section className="space-y-3">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">{title}</h2>
      {children}
    </section>
  );
}

function TodayCard({ tasks }) {
  const { pendingTasksCount, completedTasksCount, topPendingTasks } = tasks;
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center justify-between text-sm font-semibold">
          <div className="flex items-center gap-1.5"><CalendarCheck className="h-4 w-4 text-primary" /> Meu Dia</div>
          <Link to="/today" className="text-xs text-muted-foreground hover:text-primary">Ver tudo</Link>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="mb-3 flex items-center gap-4">
          <MetricNumber value={pendingTasksCount} label="pendentes" className="text-pending" />
          <MetricNumber value={completedTasksCount} label="concluidas" className="text-income" />
        </div>
        <div className="space-y-1.5">
          {(topPendingTasks ?? []).slice(0, 3).map((task) => (
            <div key={task.id} className="flex items-center gap-2 text-sm">
              <CheckCircle2 className="h-3.5 w-3.5 shrink-0 text-muted-foreground/50" />
              <span className="truncate">{task.title}</span>
            </div>
          ))}
          {pendingTasksCount === 0 && <p className="py-2 text-center text-xs text-muted-foreground">Nenhuma tarefa pendente.</p>}
        </div>
      </CardContent>
    </Card>
  );
}

function HabitsCard({ habits = [] }) {
  const done = habits.filter((habit) => habit.checkedInToday).length;
  const total = habits.length;
  const pct = total > 0 ? Math.round((done / total) * 100) : 0;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center justify-between text-sm font-semibold">
          <div className="flex items-center gap-1.5"><Flame className="h-4 w-4 text-pending" /> Rituais</div>
          <Link to="/habits" className="text-xs text-muted-foreground hover:text-primary">Ver tudo</Link>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="mb-3 flex items-center gap-3">
          <p className="text-2xl font-bold">{done}<span className="text-lg text-muted-foreground">/{total}</span></p>
          <div className="flex-1">
            <Progress value={pct} className="h-2" indicatorClassName={pct === 100 ? 'bg-income' : 'bg-pending'} />
            <p className="mt-0.5 text-[10px] text-muted-foreground">{pct}% concluidos hoje</p>
          </div>
        </div>
        <div className="space-y-1.5">
          {habits.slice(0, 4).map((habit) => (
            <div key={habit.id} className="flex items-center gap-2">
              <span className={cn('h-2 w-2 shrink-0 rounded-full', habit.checkedInToday ? 'bg-income' : 'bg-muted-foreground/30')} />
              <span className={cn('flex-1 truncate text-sm', habit.checkedInToday && 'text-muted-foreground line-through')}>{habit.name}</span>
              {habit.currentStreak > 0 && <span className="text-[10px] font-medium text-pending">{habit.currentStreak} dias</span>}
            </div>
          ))}
          {habits.length === 0 && (
            <p className="py-2 text-center text-xs text-muted-foreground">
              <Link to="/habits" className="underline">Crie seus primeiros rituais</Link>
            </p>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

function FinanceDashboard({ data }) {
  return (
    <DashboardSection title="Dinheiro">
      <div className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <FinanceCard label="Saldo total" value={data.totalBalance} icon={Wallet} color={data.totalBalance >= 0 ? 'text-income' : 'text-expense'} />
          <FinanceCard label="Receitas do mes" value={data.monthlyIncome} icon={ArrowUpRight} color="text-income" />
          <FinanceCard label="Despesas do mes" value={data.monthlyExpense} icon={ArrowDownRight} color="text-expense" />
        </div>
        <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
          <FinanceShortcut to="/transactions" icon={Receipt} label="Transacoes" />
          <FinanceShortcut to="/accounts" icon={Wallet} label="Contas" />
          <FinanceShortcut to="/budgets" icon={Target} label="Orcamentos" />
          <FinanceShortcut to="/financings" icon={TrendingUp} label="Financiamentos" />
        </div>
      </div>
    </DashboardSection>
  );
}

function FinanceCard({ label, value, icon: Icon, color }) {
  return (
    <Card>
      <CardContent className="p-4">
        <div className="mb-2 flex items-center justify-between">
          <p className="text-xs font-medium text-muted-foreground">{label}</p>
          <Icon className={cn('h-4 w-4', color)} />
        </div>
        <p className={cn('text-xl font-bold tabular-nums', color)}>{formatCurrency(value)}</p>
      </CardContent>
    </Card>
  );
}

function FinanceShortcut({ to, icon: Icon, label }) {
  return (
    <Link to={to} className="rounded-lg border bg-card p-4 text-sm font-medium transition-colors hover:bg-muted">
      <Icon className="mb-2 h-4 w-4 text-primary" />
      {label}
    </Link>
  );
}

function ModuleShortcutCard({ to, icon: Icon, title }) {
  return (
    <Card>
      <CardContent className="flex items-center justify-between gap-4 p-4">
        <div className="flex min-w-0 items-center gap-3">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary/10">
            <Icon className="h-4 w-4 text-primary" />
          </div>
          <p className="truncate text-sm font-semibold">{title}</p>
        </div>
        <Link to={to} className="shrink-0 text-xs font-medium text-muted-foreground hover:text-primary">Abrir</Link>
      </CardContent>
    </Card>
  );
}

function GoalsCard({ goals = [] }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center justify-between text-sm font-semibold">
          <div className="flex items-center gap-1.5"><Target className="h-4 w-4 text-primary" /> Minha Jornada</div>
          <Link to="/goals" className="text-xs text-muted-foreground hover:text-primary">Ver tudo</Link>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {goals.map((goal) => (
          <div key={goal.id}>
            <div className="mb-1 flex items-center justify-between text-sm">
              <span className="truncate pr-2 font-medium">{goal.title}</span>
              <span className="shrink-0 text-xs font-semibold tabular-nums text-muted-foreground">{goal.progress?.toFixed(0)}%</span>
            </div>
            <Progress value={goal.progress ?? 0} className="h-1.5" indicatorClassName={goal.progress >= 100 ? 'bg-primary' : 'bg-primary'} />
          </div>
        ))}
        {goals.length === 0 && (
          <p className="py-3 text-center text-xs text-muted-foreground">
            <Link to="/goals" className="underline">Defina seus objetivos de vida</Link>
          </p>
        )}
      </CardContent>
    </Card>
  );
}

function MoodCard({ moodHistory = [], todayMood }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center justify-between text-sm font-semibold">
          <div className="flex items-center gap-1.5"><BookOpen className="h-4 w-4 text-primary" /> Humor</div>
          <Link to="/diary" className="text-xs text-muted-foreground hover:text-primary">Diario</Link>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {todayMood ? (
          <div className="mb-3 text-center">
            <span className="text-3xl">{MOOD_EMOJI[todayMood]}</span>
            <p className="mt-1 text-xs text-muted-foreground">hoje</p>
          </div>
        ) : (
          <p className="mb-3 py-1 text-center text-xs text-muted-foreground">Sem registro hoje</p>
        )}
        <div className="flex h-12 items-end gap-1">
          {moodHistory.slice(-7).map((entry, index) => (
            <div key={index} className="flex flex-1 flex-col items-center gap-0.5">
              <div className="w-full rounded-t-sm bg-primary/50" style={{ height: `${(entry.mood / 5) * 100}%`, minHeight: 4 }} />
              <span className="text-[8px] text-muted-foreground">
                {new Date(entry.date).toLocaleDateString('pt-BR', { day: '2-digit' })}
              </span>
            </div>
          ))}
          {moodHistory.length === 0 && <p className="w-full text-center text-[10px] text-muted-foreground">Sem dados</p>}
        </div>
      </CardContent>
    </Card>
  );
}

const EVENT_ICONS = { 1: Flame, 2: CheckCircle2, 3: Target, 4: Target, 5: BookOpen, 10: Heart };
const EVENT_COLORS = {
  1: 'text-pending',
  2: 'text-income',
  3: 'text-primary',
  4: 'text-primary',
  5: 'text-primary',
  10: 'text-mut2',
};

function TimelineCard({ events = [] }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center justify-between text-sm font-semibold">
          <div className="flex items-center gap-1.5"><Scroll className="h-4 w-4 text-muted-foreground" /> Linha da Vida</div>
          <Link to="/timeline" className="text-xs text-muted-foreground hover:text-primary">Ver tudo</Link>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {events.map((event) => {
          const Icon = EVENT_ICONS[event.type] ?? Zap;
          return (
            <div key={event.id} className="flex items-start gap-3">
              <Icon className={cn('mt-0.5 h-4 w-4 shrink-0', EVENT_COLORS[event.type] ?? 'text-muted-foreground')} />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{event.title}</p>
                <p className="text-[11px] text-muted-foreground">
                  {new Date(event.occurredAt).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })}
                </p>
              </div>
              {event.isFavorite && <Heart className="h-3.5 w-3.5 shrink-0 text-expense" fill="currentColor" />}
            </div>
          );
        })}
        {events.length === 0 && <p className="py-4 text-center text-xs text-muted-foreground">Seus registros aparecem aqui.</p>}
      </CardContent>
    </Card>
  );
}

function MetricNumber({ value, label, className }) {
  return (
    <div className="text-center">
      <p className={cn('text-2xl font-bold', className)}>{value}</p>
      <p className="text-[10px] text-muted-foreground">{label}</p>
    </div>
  );
}

function HomeSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between"><Skeleton className="h-9 w-24" /><Skeleton className="h-9 w-44" /></div>
      <Skeleton className="h-20 rounded-xl" />
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Skeleton className="h-44 rounded-xl" /><Skeleton className="h-44 rounded-xl" />
      </div>
      <div className="grid grid-cols-3 gap-4">
        {[0, 1, 2].map((i) => <Skeleton key={i} className="h-24 rounded-xl" />)}
      </div>
    </div>
  );
}
