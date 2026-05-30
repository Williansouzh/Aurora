import { CalendarCheck, CheckCircle2, Circle, Clock, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/Select';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn, formatDate } from '../lib/utils';

const PRIORITY_LABEL = { 1: 'Baixa', 2: 'Média', 3: 'Alta' };
const PRIORITY_COLOR = {
  1: 'text-slate-500',
  2: 'text-amber-500',
  3: 'text-red-500',
};

export function TodayPage({ api }) {
  const today = new Date();
  const todayStr = today.toISOString().split('T')[0];
  const [showForm, setShowForm] = useState(false);
  const tasks = useData(() => api.get(`/api/today?date=${todayStr}`), [todayStr]);

  const reload = () => tasks.reload();

  const complete = async (id) => {
    await api.patch(`/api/today/${id}/complete`);
    reload();
  };

  const reopen = async (id) => {
    await api.patch(`/api/today/${id}/reopen`);
    reload();
  };

  const remove = async (id) => {
    await api.delete(`/api/today/${id}`);
    reload();
  };

  if (tasks.loading) return <TodaySkeleton />;
  if (tasks.error) return <p className="text-red-600 text-sm">{tasks.error}</p>;

  const { pending = [], completed = [], overdue = [] } = tasks.data ?? {};

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <CalendarCheck className="h-5 w-5 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight text-foreground">Meu Dia</h1>
            <p className="text-sm text-muted-foreground">{formatDate(today)}</p>
          </div>
        </div>
        <Button onClick={() => setShowForm(true)} size="sm">
          <Plus className="h-4 w-4 mr-1" /> Nova tarefa
        </Button>
      </div>

      {overdue.length > 0 && (
        <Card className="border-amber-200 bg-amber-50 dark:bg-amber-950/20">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-semibold text-amber-700 flex items-center gap-2">
              <Clock className="h-4 w-4" />
              Pendentes de dias anteriores ({overdue.length})
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {overdue.map((t) => (
              <TaskItem key={t.id} task={t} onComplete={complete} onReopen={reopen} onDelete={remove} muted />
            ))}
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-semibold">Tarefas de hoje</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          {pending.length === 0 && completed.length === 0 && (
            <EmptyState onAdd={() => setShowForm(true)} />
          )}
          {pending.map((t) => (
            <TaskItem key={t.id} task={t} onComplete={complete} onReopen={reopen} onDelete={remove} />
          ))}
          {completed.length > 0 && (
            <>
              <div className="border-t pt-2 mt-2">
                <p className="text-xs text-muted-foreground mb-2 font-medium">Concluídas</p>
                {completed.map((t) => (
                  <TaskItem key={t.id} task={t} onComplete={complete} onReopen={reopen} onDelete={remove} />
                ))}
              </div>
            </>
          )}
        </CardContent>
      </Card>

      {showForm && (
        <TaskFormModal api={api} onClose={() => setShowForm(false)} onCreated={reload} />
      )}
    </div>
  );
}

function TaskItem({ task, onComplete, onReopen, onDelete, muted }) {
  const done = task.status === 2;
  return (
    <div className={cn(
      'flex items-start gap-3 rounded-lg px-3 py-2.5 group hover:bg-accent/50 transition-colors',
      muted && 'opacity-70'
    )}>
      <button
        className="mt-0.5 shrink-0"
        onClick={() => done ? onReopen(task.id) : onComplete(task.id)}
      >
        {done
          ? <CheckCircle2 className="h-4 w-4 text-emerald-500" />
          : <Circle className="h-4 w-4 text-muted-foreground hover:text-primary transition-colors" />}
      </button>
      <div className="flex-1 min-w-0">
        <p className={cn('text-sm font-medium', done && 'line-through text-muted-foreground')}>
          {task.title}
        </p>
        {task.notes && (
          <p className="text-xs text-muted-foreground mt-0.5">{task.notes}</p>
        )}
      </div>
      <span className={cn('text-[11px] font-medium shrink-0', PRIORITY_COLOR[task.priority])}>
        {PRIORITY_LABEL[task.priority]}
      </span>
      <button
        className="opacity-0 group-hover:opacity-100 transition-opacity text-muted-foreground hover:text-destructive"
        onClick={() => onDelete(task.id)}
      >
        <Trash2 className="h-3.5 w-3.5" />
      </button>
    </div>
  );
}

function EmptyState({ onAdd }) {
  return (
    <div className="flex flex-col items-center py-10 text-center">
      <div className="rounded-full bg-muted p-4 mb-3">
        <CheckCircle2 className="h-6 w-6 text-muted-foreground/40" />
      </div>
      <p className="text-sm font-medium text-muted-foreground mb-1">Nenhuma tarefa para hoje</p>
      <p className="text-xs text-muted-foreground mb-3">Adicione o que precisa fazer hoje.</p>
      <Button size="sm" variant="outline" onClick={onAdd}>
        <Plus className="h-3.5 w-3.5 mr-1" /> Adicionar tarefa
      </Button>
    </div>
  );
}

function TaskFormModal({ api, onClose, onCreated }) {
  const [form, setForm] = useState({ title: '', notes: '', priority: '2' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const submit = async (e) => {
    e.preventDefault();
    if (!form.title.trim()) return;
    setSaving(true);
    setError('');
    try {
      await api.post('/api/today', {
        title: form.title,
        notes: form.notes || null,
        priority: parseInt(form.priority),
        date: new Date().toISOString(),
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
        <DialogHeader>
          <DialogTitle>Nova tarefa</DialogTitle>
        </DialogHeader>
        <form onSubmit={submit} className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label htmlFor="title">Título</Label>
            <Input
              id="title"
              autoFocus
              placeholder="O que precisa ser feito?"
              value={form.title}
              onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="notes">Notas (opcional)</Label>
            <Input
              id="notes"
              placeholder="Detalhes..."
              value={form.notes}
              onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
            />
          </div>
          <div className="space-y-1.5">
            <Label>Prioridade</Label>
            <Select value={form.priority} onValueChange={(v) => setForm((f) => ({ ...f, priority: v }))}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="1">Baixa</SelectItem>
                <SelectItem value="2">Média</SelectItem>
                <SelectItem value="3">Alta</SelectItem>
              </SelectContent>
            </Select>
          </div>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving || !form.title.trim()}>
              {saving ? 'Salvando...' : 'Salvar'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function TodaySkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between">
        <Skeleton className="h-9 w-40" />
        <Skeleton className="h-8 w-28" />
      </div>
      <div className="space-y-3">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-12 rounded-lg" />
        ))}
      </div>
    </div>
  );
}
