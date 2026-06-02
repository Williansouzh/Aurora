import { BookOpen, CalendarDays, CheckCircle2, CheckSquare, Clock, Loader2, Plus, RotateCcw, X } from 'lucide-react';
import { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const STATUS_LABELS = { 1: 'Não iniciado', 2: 'Em andamento', 3: 'Encerrado' };
const STATUS_COLORS = {
  1: 'text-slate-600 bg-slate-100',
  2: 'text-blue-700 bg-blue-100',
  3: 'text-emerald-700 bg-emerald-100',
};

function weekStart() {
  const d = new Date();
  const day = d.getDay();
  const diff = d.getDate() - day + (day === 0 ? -6 : 1);
  const mon = new Date(d.setDate(diff));
  return mon.toISOString().split('T')[0];
}

export function WeeklyPlanningPage({ api }) {
  const [showCreate, setShowCreate] = useState(false);
  const [showClose, setShowClose] = useState(null);

  const current = useData(() => api.get('/api/weekly-planning/current'), []);
  const history = useData(() => api.get('/api/weekly-planning?limit=8'), []);
  const reload = () => { current.reload(); history.reload(); };

  if (current.loading) return <WeekSkeleton />;

  const plan = current.data;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <CalendarDays className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight">Minha Semana</h1>
        </div>
        {!plan && (
          <Button size="sm" onClick={() => setShowCreate(true)}>
            <Plus className="h-4 w-4 mr-1" /> Planejar semana
          </Button>
        )}
      </div>

      {plan ? (
        <CurrentPlanCard plan={plan} api={api} onUpdated={reload} onClose={() => setShowClose(plan)} />
      ) : (
        <EmptyWeek onStart={() => setShowCreate(true)} />
      )}

      {(history.data ?? []).filter(p => p.id !== plan?.id).length > 0 && (
        <div className="space-y-3">
          <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">Semanas anteriores</h2>
          {(history.data ?? []).filter(p => p.id !== plan?.id).map((p) => (
            <PastPlanCard key={p.id} plan={p} />
          ))}
        </div>
      )}

      {showCreate && (
        <CreatePlanModal api={api} onClose={() => setShowCreate(false)} onCreated={reload} />
      )}
      {showClose && (
        <ClosePlanModal api={api} plan={showClose} onClose={() => setShowClose(null)} onClosed={reload} />
      )}
    </div>
  );
}

function CurrentPlanCard({ plan, api, onUpdated, onClose }) {
  const [newPriority, setNewPriority] = useState('');
  const [saving, setSaving] = useState(false);

  const addPriority = async () => {
    if (!newPriority.trim()) return;
    setSaving(true);
    try {
      await api.put(`/api/weekly-planning/${plan.id}`, {
        mainFocus: plan.mainFocus,
        linkedGoalIds: plan.linkedGoalIds,
        priorities: [...plan.priorities, newPriority.trim()],
        notes: plan.notes,
      });
      setNewPriority('');
      onUpdated();
    } finally { setSaving(false); }
  };

  const removePriority = async (idx) => {
    const updated = plan.priorities.filter((_, i) => i !== idx);
    await api.put(`/api/weekly-planning/${plan.id}`, {
      mainFocus: plan.mainFocus,
      linkedGoalIds: plan.linkedGoalIds,
      priorities: updated,
      notes: plan.notes,
    });
    onUpdated();
  };

  const wStart = new Date(plan.weekStart).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });
  const wEnd = new Date(plan.weekEnd).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });

  return (
    <Card>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="text-base">{wStart} – {wEnd}</CardTitle>
            {plan.mainFocus && <p className="text-sm text-muted-foreground mt-0.5">Foco: {plan.mainFocus}</p>}
          </div>
          <div className="flex items-center gap-2">
            <span className={cn('text-[11px] font-medium px-2 py-0.5 rounded-full', STATUS_COLORS[plan.status])}>
              {STATUS_LABELS[plan.status]}
            </span>
            {plan.status !== 3 && (
              <Button size="sm" variant="outline" onClick={onClose}>
                <CheckSquare className="h-3.5 w-3.5 mr-1" /> Encerrar semana
              </Button>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <p className="text-sm font-medium mb-2">Prioridades da semana</p>
          <div className="space-y-1.5">
            {plan.priorities.map((p, i) => (
              <div key={i} className="flex items-center gap-2 group">
                <span className="w-5 h-5 rounded-full bg-primary/10 text-primary text-[11px] flex items-center justify-center font-semibold shrink-0">
                  {i + 1}
                </span>
                <span className="text-sm flex-1">{p}</span>
                <button
                  className="opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-destructive"
                  onClick={() => removePriority(i)}
                >
                  <X className="h-3.5 w-3.5" />
                </button>
              </div>
            ))}
          </div>
          <div className="flex gap-2 mt-2">
            <Input
              placeholder="Adicionar prioridade..."
              value={newPriority}
              onChange={(e) => setNewPriority(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && addPriority()}
              className="h-8 text-sm"
            />
            <Button size="sm" variant="outline" onClick={addPriority} disabled={saving || !newPriority.trim()}>
              <Plus className="h-3.5 w-3.5" />
            </Button>
          </div>
        </div>
        {plan.studies && <StudyWeekSummary summary={plan.studies} />}
        {plan.review && (
          <div className="border-t pt-3">
            <p className="text-xs font-medium text-muted-foreground mb-1">Revisão</p>
            <p className="text-sm">{plan.review}</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function StudyWeekSummary({ summary }) {
  const plannedHours = ((summary.plannedStudyMinutes ?? 0) / 60).toFixed(1);
  const completedHours = ((summary.completedStudyMinutes ?? 0) / 60).toFixed(1);

  return (
    <div className="border-t pt-3">
      <p className="mb-2 text-sm font-medium">Estudos na semana</p>
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        <MiniStat icon={BookOpen} label="Prioridades" value={summary.activePriorities ?? 0} />
        <MiniStat icon={Clock} label="Horas" value={`${completedHours}/${plannedHours}`} />
        <MiniStat icon={RotateCcw} label="Revisoes" value={summary.completedReviews ?? 0} />
        <MiniStat icon={CheckCircle2} label="Praticas" value={summary.completedPracticeTasks ?? 0} />
      </div>
      {(summary.dueReviews > 0 || summary.openPracticeTasks > 0) && (
        <p className="mt-2 text-xs text-muted-foreground">
          Pendencias: {summary.dueReviews ?? 0} revisoes e {summary.openPracticeTasks ?? 0} praticas abertas.
        </p>
      )}
    </div>
  );
}

function MiniStat({ icon: Icon, label, value }) {
  return (
    <div className="rounded-lg border bg-muted/30 p-3">
      <div className="mb-1 flex items-center gap-1.5 text-xs text-muted-foreground">
        <Icon className="h-3.5 w-3.5" />
        <span>{label}</span>
      </div>
      <p className="text-base font-semibold tabular-nums">{value}</p>
    </div>
  );
}

function PastPlanCard({ plan }) {
  const wStart = new Date(plan.weekStart).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });
  const wEnd = new Date(plan.weekEnd).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });
  return (
    <Card className="opacity-70">
      <CardContent className="p-4">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm font-medium">{wStart} – {wEnd}</p>
            {plan.mainFocus && <p className="text-xs text-muted-foreground">{plan.mainFocus}</p>}
          </div>
          <span className={cn('text-[11px] font-medium px-2 py-0.5 rounded-full', STATUS_COLORS[plan.status])}>
            {STATUS_LABELS[plan.status]}
          </span>
        </div>
        {plan.priorities.length > 0 && (
          <p className="text-xs text-muted-foreground mt-2">{plan.priorities.length} prioridades</p>
        )}
      </CardContent>
    </Card>
  );
}

function EmptyWeek({ onStart }) {
  return (
    <div className="flex flex-col items-center py-16 text-center">
      <div className="rounded-full bg-muted p-5 mb-4"><CalendarDays className="h-7 w-7 text-muted-foreground/40" /></div>
      <p className="text-base font-semibold mb-1">Nenhum plano para esta semana</p>
      <p className="text-sm text-muted-foreground mb-4 max-w-xs">Defina seu foco e prioridades para a semana.</p>
      <Button onClick={onStart}><Plus className="h-4 w-4 mr-1" /> Planejar semana</Button>
    </div>
  );
}

function CreatePlanModal({ api, onClose, onCreated }) {
  const [mainFocus, setMainFocus] = useState('');
  const [saving, setSaving] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.post('/api/weekly-planning', { weekStart: weekStart(), mainFocus: mainFocus || null });
      onCreated(); onClose();
    } finally { setSaving(false); }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Planejar semana</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label>Foco principal (opcional)</Label>
            <Input autoFocus placeholder="Ex: Finalizar projeto X" value={mainFocus} onChange={(e) => setMainFocus(e.target.value)} />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Criando...' : 'Criar plano'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function ClosePlanModal({ api, plan, onClose, onClosed }) {
  const [review, setReview] = useState('');
  const [saving, setSaving] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.patch(`/api/weekly-planning/${plan.id}/close`, { review: review || null });
      onClosed(); onClose();
    } finally { setSaving(false); }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Encerrar semana</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label>Reflexão da semana (opcional)</Label>
            <textarea
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm min-h-[100px] resize-none focus:outline-none focus:ring-1 focus:ring-ring"
              placeholder="Como foi a semana? O que aprendeu?"
              value={review}
              onChange={(e) => setReview(e.target.value)}
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Encerrando...' : 'Encerrar e ganhar +30 XP'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function WeekSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between"><Skeleton className="h-9 w-40" /><Skeleton className="h-8 w-32" /></div>
      <Skeleton className="h-48 rounded-xl" />
    </div>
  );
}
