import {
  Award, BookOpen, CalendarDays, ChevronLeft, ChevronRight,
  Flame, Star, TrendingUp, Wallet,
} from 'lucide-react';
import { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn, formatCurrency } from '../lib/utils';

const MOOD_EMOJI = { 1: '😞', 2: '😕', 3: '😐', 4: '🙂', 5: '😄' };

function startOfWeek(date = new Date()) {
  const d = new Date(date);
  const day = d.getDay();
  const diff = d.getDate() - day + (day === 0 ? -6 : 1);
  return new Date(d.setDate(diff)).toISOString().split('T')[0];
}

export function RetrospectivesPage({ api }) {
  const now = new Date();
  const [tab, setTab] = useState('weekly');
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());
  const [weekStart, setWeekStart] = useState(startOfWeek());

  const weekly = useData(() => api.get(`/api/retrospectives/weekly?weekStart=${weekStart}`), [weekStart]);
  const monthly = useData(() => api.get(`/api/retrospectives/monthly?month=${month}&year=${year}`), [month, year]);

  const navigateWeek = (delta) => {
    const d = new Date(weekStart);
    d.setDate(d.getDate() + delta * 7);
    setWeekStart(d.toISOString().split('T')[0]);
  };

  const navigateMonth = (delta) => {
    let m = month + delta, y = year;
    if (m < 1) { m = 12; y--; } if (m > 12) { m = 1; y++; }
    setMonth(m); setYear(y);
  };

  const monthLabel = new Date(year, month - 1).toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
  const weekLabel = `${new Date(weekStart + 'T12:00:00').toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' })} – ${new Date(new Date(weekStart).getTime() + 6 * 86400000).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' })}`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <TrendingUp className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight">Retrospectiva</h1>
        </div>
        <div className="flex rounded-lg border overflow-hidden">
          {['weekly', 'monthly'].map((t) => (
            <button
              key={t}
              onClick={() => setTab(t)}
              className={cn(
                'px-4 py-1.5 text-sm font-medium transition-colors',
                tab === t ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:bg-accent'
              )}
            >
              {t === 'weekly' ? 'Semanal' : 'Mensal'}
            </button>
          ))}
        </div>
      </div>

      {tab === 'weekly' ? (
        <>
          <NavBar label={weekLabel} onPrev={() => navigateWeek(-1)} onNext={() => navigateWeek(1)} />
          {weekly.loading ? <RetroSkeleton /> : weekly.data && <WeeklyView data={weekly.data} />}
        </>
      ) : (
        <>
          <NavBar label={monthLabel} onPrev={() => navigateMonth(-1)} onNext={() => navigateMonth(1)} capitalize />
          {monthly.loading ? <RetroSkeleton /> : monthly.data && <MonthlyView data={monthly.data} />}
        </>
      )}
    </div>
  );
}

function NavBar({ label, onPrev, onNext, capitalize }) {
  return (
    <div className="flex items-center gap-1 rounded-lg border bg-card px-1 py-1 w-fit">
      <Button variant="ghost" size="icon" className="h-7 w-7" onClick={onPrev}><ChevronLeft className="h-4 w-4" /></Button>
      <span className={cn('px-3 text-sm font-medium min-w-[180px] text-center', capitalize && 'capitalize')}>{label}</span>
      <Button variant="ghost" size="icon" className="h-7 w-7" onClick={onNext}><ChevronRight className="h-4 w-4" /></Button>
    </div>
  );
}

function WeeklyView({ data }) {
  const avgMoodInt = Math.round(data.averageMood);
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <StatCard icon={Star} label="XP ganho" value={`+${data.xpEarned}`} color="text-amber-500" />
        <StatCard icon={Flame} label="Check-ins" value={data.habitCheckIns} color="text-orange-500" />
        <StatCard icon={CalendarDays} label="Tarefas" value={data.tasksCompleted} color="text-blue-500" />
        <StatCard
          icon={BookOpen}
          label="Humor médio"
          value={data.averageMood > 0 ? `${data.averageMood.toFixed(1)} ${MOOD_EMOJI[avgMoodInt] ?? ''}` : '—'}
          color="text-pink-500"
        />
      </div>

      {data.topHabits?.length > 0 && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-semibold flex items-center gap-1.5">
              <Flame className="h-4 w-4 text-orange-500" /> Hábitos da semana
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {data.topHabits.map((h, i) => (
              <div key={i} className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-2">
                  <span className="w-5 h-5 rounded-full bg-orange-100 text-orange-700 text-[10px] flex items-center justify-center font-bold">
                    {i + 1}
                  </span>
                  {h.habitName}
                </span>
                <span className="font-semibold text-muted-foreground">{h.checkIns}x</span>
              </div>
            ))}
          </CardContent>
        </Card>
      )}

      <StudyRetroCard
        minutes={data.studyMinutes}
        sessions={data.studySessionsCompleted}
        reviews={data.studyReviewsCompleted}
        practices={data.studyPracticesCompleted}
      />

      {data.weeklyReview && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-semibold">Reflexão da semana</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground leading-relaxed">{data.weeklyReview}</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function MonthlyView({ data }) {
  const savings = data.monthlySavings;
  const savingsPct = data.monthlyIncome > 0 ? Math.round((savings / data.monthlyIncome) * 100) : 0;
  const avgMoodInt = Math.round(data.averageMood);

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <StatCard icon={Star} label="XP ganho" value={`+${data.xpEarned}`} color="text-amber-500" />
        <StatCard icon={Flame} label="Check-ins" value={data.habitCheckIns} color="text-orange-500" />
        <StatCard icon={BookOpen} label="Diário" value={data.diaryEntriesCount} color="text-pink-500" />
        <StatCard icon={Award} label="Metas concluídas" value={data.goalsCompleted} color="text-violet-500" />
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard icon={Wallet} label="Receitas" value={formatCurrency(data.monthlyIncome)} color="text-emerald-600" large />
        <StatCard icon={Wallet} label="Despesas" value={formatCurrency(data.monthlyExpense)} color="text-red-600" large />
        <StatCard
          icon={TrendingUp}
          label={`Economia (${savingsPct}%)`}
          value={formatCurrency(savings)}
          color={savings >= 0 ? 'text-emerald-600' : 'text-red-600'}
          large
        />
      </div>

      {data.averageMood > 0 && (
        <Card>
          <CardContent className="p-4 flex items-center gap-4">
            <span className="text-4xl">{MOOD_EMOJI[avgMoodInt]}</span>
            <div>
              <p className="text-sm font-medium">Humor médio do mês</p>
              <p className="text-muted-foreground text-sm">{data.averageMood.toFixed(1)} / 5 — {data.moodEntriesCount} registros</p>
            </div>
          </CardContent>
        </Card>
      )}

      <StudyRetroCard
        minutes={data.studyMinutes}
        sessions={data.studySessionsCompleted}
        reviews={data.studyReviewsCompleted}
        practices={data.studyPracticesCompleted}
      />
    </div>
  );
}

function StudyRetroCard({ minutes = 0, sessions = 0, reviews = 0, practices = 0 }) {
  const hours = (minutes / 60).toFixed(1);

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center gap-1.5 text-sm font-semibold">
          <BookOpen className="h-4 w-4 text-blue-500" /> Estudos
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
          <StudyMetric label="Horas" value={`${hours}h`} />
          <StudyMetric label="Sessoes" value={sessions} />
          <StudyMetric label="Revisoes" value={reviews} />
          <StudyMetric label="Praticas" value={practices} />
        </div>
      </CardContent>
    </Card>
  );
}

function StudyMetric({ label, value }) {
  return (
    <div className="rounded-lg border bg-muted/30 p-3">
      <p className="text-xs font-medium text-muted-foreground">{label}</p>
      <p className="mt-1 text-lg font-bold tabular-nums">{value}</p>
    </div>
  );
}

function StatCard({ icon: Icon, label, value, color, large }) {
  return (
    <Card>
      <CardContent className="p-4">
        <div className="flex items-center gap-2 mb-1">
          <Icon className={cn('h-4 w-4', color)} />
          <p className="text-xs text-muted-foreground font-medium">{label}</p>
        </div>
        <p className={cn('font-bold tabular-nums', large ? 'text-lg' : 'text-2xl', color)}>{value}</p>
      </CardContent>
    </Card>
  );
}

function RetroSkeleton() {
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        {[0,1,2,3].map(i => <Skeleton key={i} className="h-20 rounded-xl" />)}
      </div>
      <Skeleton className="h-32 rounded-xl" />
    </div>
  );
}
