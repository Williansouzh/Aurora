import { Award, CheckCircle2, Flame, Pause, Play, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/Select';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const AREA_LABELS = {
  1: 'Saúde', 2: 'Trabalho', 3: 'Estudos', 4: 'Dinheiro',
  5: 'Relacionamentos', 6: 'Casa', 7: 'Lazer', 8: 'Espiritualidade', 9: 'Projetos',
};
const DIFFICULTY_LABELS = { 1: 'Fácil', 2: 'Médio', 3: 'Difícil' };
const FREQ_LABELS = { 1: 'Diário', 2: 'Semanal' };
const AREA_COLORS = {
  1: 'bg-emerald-100 text-emerald-700',
  2: 'bg-blue-100 text-blue-700',
  3: 'bg-violet-100 text-violet-700',
  4: 'bg-amber-100 text-amber-700',
  5: 'bg-pink-100 text-pink-700',
  6: 'bg-orange-100 text-orange-700',
  7: 'bg-cyan-100 text-cyan-700',
  8: 'bg-indigo-100 text-indigo-700',
  9: 'bg-slate-100 text-slate-700',
};

export function HabitsPage({ api }) {
  const [showAll, setShowAll] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [checkInHabit, setCheckInHabit] = useState(null);

  const habits = useData(() => api.get(`/api/habits?activeOnly=${!showAll}`), [showAll]);
  const reload = () => habits.reload();

  const pause = async (id) => { await api.patch(`/api/habits/${id}/pause`); reload(); };
  const resume = async (id) => { await api.patch(`/api/habits/${id}/resume`); reload(); };
  const remove = async (id) => { await api.delete(`/api/habits/${id}`); reload(); };

  if (habits.loading) return <HabitsSkeleton />;
  if (habits.error) return <p className="text-red-600 text-sm">{habits.error}</p>;

  const list = habits.data ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <Flame className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">Rituais</h1>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => setShowAll((v) => !v)}>
            {showAll ? 'Só ativos' : 'Ver todos'}
          </Button>
          <Button size="sm" onClick={() => setShowForm(true)}>
            <Plus className="h-4 w-4 mr-1" /> Novo hábito
          </Button>
        </div>
      </div>

      {list.length === 0 && (
        <EmptyHabitsState onAdd={() => setShowForm(true)} />
      )}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {list.map((h) => (
          <HabitCard
            key={h.id}
            habit={h}
            api={api}
            onCheckIn={() => setCheckInHabit(h)}
            onPause={() => pause(h.id)}
            onResume={() => resume(h.id)}
            onDelete={() => remove(h.id)}
          />
        ))}
      </div>

      {showForm && (
        <HabitFormModal api={api} onClose={() => setShowForm(false)} onCreated={reload} />
      )}
      {checkInHabit && (
        <CheckInModal
          api={api}
          habit={checkInHabit}
          onClose={() => setCheckInHabit(null)}
          onDone={reload}
        />
      )}
    </div>
  );
}

function HabitCard({ habit, onCheckIn, onPause, onResume, onDelete }) {
  return (
    <Card className={cn('transition-shadow hover:shadow-md', !habit.isActive && 'opacity-60')}>
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between">
          <div className="flex-1 min-w-0">
            <CardTitle className="text-base font-semibold truncate">{habit.name}</CardTitle>
            <div className="flex items-center gap-1.5 mt-1 flex-wrap">
              <span className={cn('text-[11px] font-medium px-1.5 py-0.5 rounded-full', AREA_COLORS[habit.area])}>
                {AREA_LABELS[habit.area]}
              </span>
              <span className="text-[11px] text-muted-foreground">{FREQ_LABELS[habit.frequencyType]}</span>
              <span className="text-[11px] text-muted-foreground">{DIFFICULTY_LABELS[habit.difficulty]}</span>
            </div>
          </div>
          <div className="flex items-center gap-1 ml-2">
            <button
              className="text-muted-foreground hover:text-foreground transition-colors p-1"
              onClick={() => (habit.isActive ? onPause() : onResume())}
              title={habit.isActive ? 'Pausar' : 'Retomar'}
            >
              {habit.isActive ? <Pause className="h-3.5 w-3.5" /> : <Play className="h-3.5 w-3.5" />}
            </button>
            <button
              className="text-muted-foreground hover:text-destructive transition-colors p-1"
              onClick={onDelete}
              title="Excluir"
            >
              <Trash2 className="h-3.5 w-3.5" />
            </button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-3">
            <StreakBadge label="Atual" value={habit.currentStreak} icon={Flame} color="text-orange-500" />
            <StreakBadge label="Recorde" value={habit.bestStreak} icon={Award} color="text-violet-500" />
          </div>
          <Badge variant="outline" className="text-[11px]">
            +{habit.xpReward} XP
          </Badge>
        </div>
        {habit.isActive && (
          <Button className="w-full" size="sm" onClick={onCheckIn}>
            <CheckCircle2 className="h-3.5 w-3.5 mr-1.5" />
            Check-in
          </Button>
        )}
      </CardContent>
    </Card>
  );
}

function StreakBadge({ label, value, icon: Icon, color }) {
  return (
    <div className="flex flex-col items-center">
      <div className="flex items-center gap-1">
        <Icon className={cn('h-3.5 w-3.5', color)} />
        <span className="text-lg font-bold tabular-nums">{value}</span>
      </div>
      <span className="text-[10px] text-muted-foreground">{label}</span>
    </div>
  );
}

function EmptyHabitsState({ onAdd }) {
  return (
    <div className="flex flex-col items-center py-16 text-center">
      <div className="rounded-full bg-muted p-5 mb-4">
        <Flame className="h-7 w-7 text-muted-foreground/40" />
      </div>
      <p className="text-base font-semibold text-foreground mb-1">Nenhum hábito criado</p>
      <p className="text-sm text-muted-foreground mb-4 max-w-xs">
        Crie rituais consistentes e acompanhe sua evolução com streaks e XP.
      </p>
      <Button onClick={onAdd}>
        <Plus className="h-4 w-4 mr-1" /> Criar primeiro hábito
      </Button>
    </div>
  );
}

function HabitFormModal({ api, onClose, onCreated }) {
  const [form, setForm] = useState({
    name: '', description: '', area: '1',
    frequencyType: '1', timesPerWeek: '1', difficulty: '2',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  const submit = async (e) => {
    e.preventDefault();
    if (!form.name.trim()) return;
    setSaving(true);
    setError('');
    try {
      await api.post('/api/habits', {
        name: form.name,
        description: form.description || null,
        area: parseInt(form.area),
        frequencyType: parseInt(form.frequencyType),
        timesPerWeek: parseInt(form.timesPerWeek),
        difficulty: parseInt(form.difficulty),
      });
      onCreated();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Novo hábito</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label>Nome</Label>
            <Input autoFocus placeholder="Ex: Meditar 10 minutos" value={form.name} onChange={(e) => set('name', e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>Descrição (opcional)</Label>
            <Input placeholder="Detalhes..." value={form.description} onChange={(e) => set('description', e.target.value)} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Área da vida</Label>
              <Select value={form.area} onValueChange={(v) => set('area', v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(AREA_LABELS).map(([k, v]) => (
                    <SelectItem key={k} value={k}>{v}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Dificuldade</Label>
              <Select value={form.difficulty} onValueChange={(v) => set('difficulty', v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="1">Fácil (+5 XP)</SelectItem>
                  <SelectItem value="2">Médio (+10 XP)</SelectItem>
                  <SelectItem value="3">Difícil (+20 XP)</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Frequência</Label>
              <Select value={form.frequencyType} onValueChange={(v) => set('frequencyType', v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="1">Diário</SelectItem>
                  <SelectItem value="2">Semanal</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Vezes por semana</Label>
              <Select value={form.timesPerWeek} onValueChange={(v) => set('timesPerWeek', v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {[1,2,3,4,5,6,7].map((n) => (
                    <SelectItem key={n} value={String(n)}>{n}x</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving || !form.name.trim()}>
              {saving ? 'Salvando...' : 'Criar hábito'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function CheckInModal({ api, habit, onClose, onDone }) {
  const [note, setNote] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const submit = async (status) => {
    setSaving(true);
    setError('');
    try {
      await api.post(`/api/habits/${habit.id}/check-in`, {
        date: new Date().toISOString(),
        status,
        note: note || null,
      });
      onDone();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Check-in: {habit.name}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label>Nota (opcional)</Label>
            <Input
              placeholder="Como foi?"
              value={note}
              onChange={(e) => setNote(e.target.value)}
              autoFocus
            />
          </div>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="flex gap-2 pt-1">
            <Button
              className="flex-1 bg-emerald-600 hover:bg-emerald-700"
              onClick={() => submit(1)}
              disabled={saving}
            >
              <CheckCircle2 className="h-4 w-4 mr-1.5" />
              Concluído (+{habit.xpReward} XP)
            </Button>
            <Button variant="outline" onClick={() => submit(2)} disabled={saving}>
              Pular
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}

function HabitsSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between">
        <Skeleton className="h-9 w-32" />
        <Skeleton className="h-8 w-32" />
      </div>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-40 rounded-xl" />
        ))}
      </div>
    </div>
  );
}
