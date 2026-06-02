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
  Star,
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
const LEVEL_COLORS = [
  'from-slate-400 to-slate-500',
  'from-emerald-400 to-emerald-600',
  'from-blue-400 to-blue-600',
  'from-violet-400 to-violet-600',
  'from-amber-400 to-amber-600',
];

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

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <Zap className="h-5 w-5 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Aurora Home</h1>
            <p className="text-sm text-muted-foreground">Dashboard por modulo, conforme seu acesso.</p>
          </div>
        </div>

        <div className="flex items-center gap-1 rounded-lg border bg-card px-1 py-1">
          <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(-1)}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="min-w-[160px] px-3 text-center text-sm font-medium capitalize">{monthLabel}</span>
          <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(1)}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>

      <ModuleFilterBar filters={filters} selected={active} onSelect={setSelectedModule} />

      {overview && <XpCard data={data} />}

      {(show('today') || show('habits')) && (
        <DashboardSection title={overview ? 'Execucao' : null}>
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
            {show('today') && <TodayCard tasks={data} />}
            {show('habits') && <HabitsCard habits={data.todayHabits} />}
          </div>
        </DashboardSection>
      )}

      {show('finances') && <FinanceDashboard data={data} />}

      {(show('goals') || show('diary')) && (
        <DashboardSection title={overview ? 'Evolucao pessoal' : null}>
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
            {show('goals') && (
              <div className="lg:col-span-2">
                <GoalsCard goals={data.featuredGoals} />
              </div>
            )}
            {show('diary') && <MoodCard moodHistory={data.moodHistory} todayMood={data.todayMood} />}
          </div>
        </DashboardSection>
      )}

      {(show('weekly-planning') || show('evolution') || show('studies') || show('retrospectives')) && (
        <DashboardSection title={overview ? 'Ciclos e memoria' : null}>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
            {show('weekly-planning') && <ModuleShortcutCard to="/weekly" icon={CalendarDays} title="Minha Semana" />}
            {show('evolution') && <ModuleShortcutCard to="/evolution" icon={Camera} title="Evolucao" />}
            {show('studies') && <ModuleShortcutCard to="/studies" icon={BookOpen} title="Estudos" />}
            {show('retrospectives') && <ModuleShortcutCard to="/retrospectives" icon={TrendingUp} title="Retrospectiva" />}
          </div>
        </DashboardSection>
      )}

      {show('timeline') && <TimelineCard events={data.recentEvents} />}
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

function XpCard({ data }) {
  const { totalXp, level, levelName, xpToNextLevel } = data;
  const colorIdx = Math.min(Math.floor((level - 1) / 10), LEVEL_COLORS.length - 1);
  const pct = xpToNextLevel > 0 ? Math.round((totalXp / (totalXp + xpToNextLevel)) * 100) : 100;

  return (
    <Card className={cn('bg-gradient-to-r text-white', LEVEL_COLORS[colorIdx])}>
      <CardContent className="p-4">
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center gap-2">
              <Star className="h-4 w-4" />
              <span className="text-sm font-medium opacity-90">Nivel {level} - {levelName}</span>
            </div>
            <p className="mt-0.5 text-2xl font-bold">{totalXp.toLocaleString('pt-BR')} XP</p>
          </div>
          {xpToNextLevel > 0 && (
            <div className="text-right text-sm opacity-80">
              <p>+{xpToNextLevel} para</p>
              <p>Nivel {level + 1}</p>
            </div>
          )}
        </div>
        {xpToNextLevel > 0 && (
          <div className="mt-3">
            <Progress value={pct} className="h-1.5 bg-white/30" indicatorClassName="bg-white" />
          </div>
        )}
      </CardContent>
    </Card>
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
          <MetricNumber value={pendingTasksCount} label="pendentes" className="text-amber-600" />
          <MetricNumber value={completedTasksCount} label="concluidas" className="text-emerald-600" />
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
          <div className="flex items-center gap-1.5"><Flame className="h-4 w-4 text-orange-500" /> Rituais</div>
          <Link to="/habits" className="text-xs text-muted-foreground hover:text-primary">Ver tudo</Link>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="mb-3 flex items-center gap-3">
          <p className="text-2xl font-bold">{done}<span className="text-lg text-muted-foreground">/{total}</span></p>
          <div className="flex-1">
            <Progress value={pct} className="h-2" indicatorClassName={pct === 100 ? 'bg-emerald-500' : 'bg-orange-400'} />
            <p className="mt-0.5 text-[10px] text-muted-foreground">{pct}% concluidos hoje</p>
          </div>
        </div>
        <div className="space-y-1.5">
          {habits.slice(0, 4).map((habit) => (
            <div key={habit.id} className="flex items-center gap-2">
              <span className={cn('h-2 w-2 shrink-0 rounded-full', habit.checkedInToday ? 'bg-emerald-500' : 'bg-muted-foreground/30')} />
              <span className={cn('flex-1 truncate text-sm', habit.checkedInToday && 'text-muted-foreground line-through')}>{habit.name}</span>
              {habit.currentStreak > 0 && <span className="text-[10px] font-medium text-orange-500">{habit.currentStreak} dias</span>}
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
          <FinanceCard label="Saldo total" value={data.totalBalance} icon={Wallet} color={data.totalBalance >= 0 ? 'text-emerald-600' : 'text-red-600'} />
          <FinanceCard label="Receitas do mes" value={data.monthlyIncome} icon={ArrowUpRight} color="text-emerald-600" />
          <FinanceCard label="Despesas do mes" value={data.monthlyExpense} icon={ArrowDownRight} color="text-red-600" />
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
          <div className="flex items-center gap-1.5"><Target className="h-4 w-4 text-blue-500" /> Minha Jornada</div>
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
            <Progress value={goal.progress ?? 0} className="h-1.5" indicatorClassName={goal.progress >= 100 ? 'bg-violet-500' : 'bg-blue-500'} />
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
          <div className="flex items-center gap-1.5"><BookOpen className="h-4 w-4 text-pink-500" /> Humor</div>
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
              <div className="w-full rounded-t-sm bg-pink-400/60" style={{ height: `${(entry.mood / 5) * 100}%`, minHeight: 4 }} />
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
  1: 'text-orange-500',
  2: 'text-emerald-500',
  3: 'text-blue-500',
  4: 'text-violet-500',
  5: 'text-pink-500',
  10: 'text-slate-500',
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
              {event.isFavorite && <Heart className="h-3.5 w-3.5 shrink-0 text-red-400" fill="currentColor" />}
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
