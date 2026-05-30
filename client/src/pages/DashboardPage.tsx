import {
  ArrowDownRight, ArrowUpRight, BookOpen, CalendarCheck, CheckCircle2,
  ChevronLeft, ChevronRight, Flame, Heart, LayoutDashboard,
  Receipt, Scroll, Star, Target, TrendingUp, Wallet, Zap,
} from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Progress } from '../components/ui/progress';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn, formatCurrency } from '../lib/utils';

const MOOD_EMOJI = { 1: '😞', 2: '😕', 3: '😐', 4: '🙂', 5: '😄' };
const AREA_COLORS = {
  1:'bg-emerald-100 text-emerald-700', 2:'bg-blue-100 text-blue-700',
  3:'bg-violet-100 text-violet-700',  4:'bg-amber-100 text-amber-700',
  5:'bg-pink-100 text-pink-700',      6:'bg-orange-100 text-orange-700',
  7:'bg-cyan-100 text-cyan-700',      8:'bg-indigo-100 text-indigo-700',
  9:'bg-slate-100 text-slate-700',
};
const LEVEL_COLORS = [
  'from-slate-400 to-slate-500',
  'from-emerald-400 to-emerald-600',
  'from-blue-400 to-blue-600',
  'from-violet-400 to-violet-600',
  'from-amber-400 to-amber-600',
];

export function DashboardPage({ api }) {
  const now = new Date();
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());

  const home = useData(() => api.get(`/api/home?month=${month}&year=${year}`), [month, year]);

  if (home.loading) return <HomeSkeleton />;
  if (home.error) return <p className="text-destructive text-sm">{home.error}</p>;

  const d = home.data;
  const navigateMonth = (delta) => {
    let m = month + delta, y = year;
    if (m < 1) { m = 12; y--; } if (m > 12) { m = 1; y++; }
    setMonth(m); setYear(y);
  };
  const monthLabel = new Date(year, month - 1).toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <Zap className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight">Aurora</h1>
        </div>
        <div className="flex items-center gap-1 rounded-lg border bg-card px-1 py-1">
          <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(-1)}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="px-3 text-sm font-medium min-w-[160px] text-center capitalize">{monthLabel}</span>
          <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateMonth(1)}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* XP / Level bar */}
      <XpCard data={d} />

      {/* Top row: Today + Habits */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <TodayCard tasks={d} />
        <HabitsCard habits={d.todayHabits} />
      </div>

      {/* Middle row: Finances */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <FinanceCard label="Saldo total" value={d.totalBalance} icon={Wallet}
          color={d.totalBalance >= 0 ? 'text-emerald-600' : 'text-red-600'} />
        <FinanceCard label="Receitas do mês" value={d.monthlyIncome} icon={ArrowUpRight} color="text-emerald-600" />
        <FinanceCard label="Despesas do mês" value={d.monthlyExpense} icon={ArrowDownRight} color="text-red-600" />
      </div>

      {/* Goals + Mood + Timeline */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <GoalsCard goals={d.featuredGoals} />
        </div>
        <MoodCard moodHistory={d.moodHistory} todayMood={d.todayMood} />
      </div>

      {/* Recent timeline */}
      <TimelineCard events={d.recentEvents} />
    </div>
  );
}

/* ── XP / Level ── */
function XpCard({ data }) {
  const { totalXp, level, levelName, xpToNextLevel } = data;
  const colorIdx = Math.min(Math.floor((level - 1) / 10), LEVEL_COLORS.length - 1);
  const pct = xpToNextLevel > 0 ? Math.round(((totalXp % (totalXp + xpToNextLevel)) / (totalXp + xpToNextLevel)) * 100) : 100;
  const xpForNext = totalXp + xpToNextLevel;
  const xpInLevel = totalXp - (xpForNext - xpToNextLevel - totalXp < 0 ? 0 : 0); // simplified

  return (
    <Card className={cn('bg-gradient-to-r text-white', LEVEL_COLORS[colorIdx])}>
      <CardContent className="p-4">
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center gap-2">
              <Star className="h-4 w-4" />
              <span className="text-sm font-medium opacity-90">Nível {level} — {levelName}</span>
            </div>
            <p className="text-2xl font-bold mt-0.5">{totalXp.toLocaleString('pt-BR')} XP</p>
          </div>
          {xpToNextLevel > 0 && (
            <div className="text-right text-sm opacity-80">
              <p>+{xpToNextLevel} para</p>
              <p>Nível {level + 1}</p>
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

/* ── Meu Dia ── */
function TodayCard({ tasks }) {
  const { pendingTasksCount, completedTasksCount, topPendingTasks } = tasks;
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-semibold flex items-center justify-between">
          <div className="flex items-center gap-1.5"><CalendarCheck className="h-4 w-4 text-primary" /> Meu Dia</div>
          <Link to="/today" className="text-xs text-muted-foreground hover:text-primary">Ver tudo →</Link>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex items-center gap-4 mb-3">
          <div className="text-center">
            <p className="text-2xl font-bold text-amber-600">{pendingTasksCount}</p>
            <p className="text-[10px] text-muted-foreground">pendentes</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-emerald-600">{completedTasksCount}</p>
            <p className="text-[10px] text-muted-foreground">concluídas</p>
          </div>
        </div>
        <div className="space-y-1.5">
          {(topPendingTasks ?? []).slice(0, 3).map((t) => (
            <div key={t.id} className="flex items-center gap-2 text-sm">
              <CheckCircle2 className="h-3.5 w-3.5 text-muted-foreground/50 shrink-0" />
              <span className="truncate">{t.title}</span>
            </div>
          ))}
          {pendingTasksCount === 0 && (
            <p className="text-xs text-muted-foreground text-center py-2">Nenhuma tarefa pendente 🎉</p>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

/* ── Rituais ── */
function HabitsCard({ habits = [] }) {
  const done = habits.filter((h) => h.checkedInToday).length;
  const total = habits.length;
  const pct = total > 0 ? Math.round((done / total) * 100) : 0;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-semibold flex items-center justify-between">
          <div className="flex items-center gap-1.5"><Flame className="h-4 w-4 text-orange-500" /> Rituais</div>
          <Link to="/habits" className="text-xs text-muted-foreground hover:text-primary">Ver tudo →</Link>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex items-center gap-3 mb-3">
          <p className="text-2xl font-bold">{done}<span className="text-muted-foreground text-lg">/{total}</span></p>
          <div className="flex-1">
            <Progress value={pct} className="h-2"
              indicatorClassName={pct === 100 ? 'bg-emerald-500' : 'bg-orange-400'} />
            <p className="text-[10px] text-muted-foreground mt-0.5">{pct}% concluídos hoje</p>
          </div>
        </div>
        <div className="space-y-1.5">
          {habits.slice(0, 4).map((h) => (
            <div key={h.id} className="flex items-center gap-2">
              <span className={cn('w-2 h-2 rounded-full shrink-0',
                h.checkedInToday ? 'bg-emerald-500' : 'bg-muted-foreground/30')} />
              <span className={cn('text-sm flex-1 truncate', h.checkedInToday && 'text-muted-foreground line-through')}>
                {h.name}
              </span>
              {h.currentStreak > 0 && (
                <span className="text-[10px] text-orange-500 font-medium">{h.currentStreak}🔥</span>
              )}
            </div>
          ))}
          {habits.length === 0 && (
            <p className="text-xs text-muted-foreground text-center py-2">
              <Link to="/habits" className="underline">Crie seus primeiros rituais</Link>
            </p>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

/* ── Finances ── */
function FinanceCard({ label, value, icon: Icon, color }) {
  return (
    <Card>
      <CardContent className="p-4">
        <div className="flex items-center justify-between mb-2">
          <p className="text-xs font-medium text-muted-foreground">{label}</p>
          <Icon className={cn('h-4 w-4', color)} />
        </div>
        <p className={cn('text-xl font-bold tabular-nums', color)}>{formatCurrency(value)}</p>
      </CardContent>
    </Card>
  );
}

/* ── Goals ── */
function GoalsCard({ goals = [] }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-semibold flex items-center justify-between">
          <div className="flex items-center gap-1.5"><Target className="h-4 w-4 text-blue-500" /> Minha Jornada</div>
          <Link to="/goals" className="text-xs text-muted-foreground hover:text-primary">Ver tudo →</Link>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {goals.map((g) => (
          <div key={g.id}>
            <div className="flex items-center justify-between text-sm mb-1">
              <span className="font-medium truncate pr-2">{g.title}</span>
              <span className="text-xs font-semibold tabular-nums text-muted-foreground shrink-0">
                {g.progress?.toFixed(0)}%
              </span>
            </div>
            <Progress value={g.progress ?? 0} className="h-1.5"
              indicatorClassName={g.progress >= 100 ? 'bg-violet-500' : 'bg-blue-500'} />
          </div>
        ))}
        {goals.length === 0 && (
          <p className="text-xs text-muted-foreground text-center py-3">
            <Link to="/goals" className="underline">Defina seus objetivos de vida</Link>
          </p>
        )}
      </CardContent>
    </Card>
  );
}

/* ── Mood ── */
function MoodCard({ moodHistory = [], todayMood }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-semibold flex items-center justify-between">
          <div className="flex items-center gap-1.5"><BookOpen className="h-4 w-4 text-pink-500" /> Humor</div>
          <Link to="/diary" className="text-xs text-muted-foreground hover:text-primary">Diário →</Link>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {todayMood ? (
          <div className="text-center mb-3">
            <span className="text-3xl">{MOOD_EMOJI[todayMood]}</span>
            <p className="text-xs text-muted-foreground mt-1">hoje</p>
          </div>
        ) : (
          <p className="text-xs text-muted-foreground text-center mb-3 py-1">Sem registro hoje</p>
        )}
        <div className="flex items-end gap-1 h-12">
          {moodHistory.slice(-7).map((m, i) => (
            <div key={i} className="flex-1 flex flex-col items-center gap-0.5">
              <div className="w-full rounded-t-sm bg-pink-400/60"
                style={{ height: `${(m.mood / 5) * 100}%`, minHeight: 4 }}
                title={MOOD_EMOJI[m.mood]} />
              <span className="text-[8px] text-muted-foreground">
                {new Date(m.date).toLocaleDateString('pt-BR', { day: '2-digit' })}
              </span>
            </div>
          ))}
          {moodHistory.length === 0 && (
            <p className="w-full text-[10px] text-muted-foreground text-center">Sem dados</p>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

/* ── Timeline ── */
const EVENT_ICONS = { 1:Flame, 2:CheckCircle2, 3:Target, 4:Target, 5:BookOpen, 10:Heart };
const EVENT_COLORS = {
  1:'text-orange-500', 2:'text-emerald-500', 3:'text-blue-500',
  4:'text-violet-500', 5:'text-pink-500', 10:'text-slate-500',
};

function TimelineCard({ events = [] }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-semibold flex items-center justify-between">
          <div className="flex items-center gap-1.5"><Scroll className="h-4 w-4 text-muted-foreground" /> Linha da Vida</div>
          <Link to="/timeline" className="text-xs text-muted-foreground hover:text-primary">Ver tudo →</Link>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {events.map((e) => {
          const Icon = EVENT_ICONS[e.type] ?? Zap;
          return (
            <div key={e.id} className="flex items-start gap-3">
              <Icon className={cn('h-4 w-4 mt-0.5 shrink-0', EVENT_COLORS[e.type] ?? 'text-muted-foreground')} />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">{e.title}</p>
                <p className="text-[11px] text-muted-foreground">
                  {new Date(e.occurredAt).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })}
                </p>
              </div>
              {e.isFavorite && <Heart className="h-3.5 w-3.5 text-red-400 shrink-0" fill="currentColor" />}
            </div>
          );
        })}
        {events.length === 0 && (
          <p className="text-xs text-muted-foreground text-center py-4">
            Seus hábitos, tarefas e registros aparecem aqui.
          </p>
        )}
      </CardContent>
    </Card>
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
        {[0,1,2].map(i => <Skeleton key={i} className="h-24 rounded-xl" />)}
      </div>
    </div>
  );
}
