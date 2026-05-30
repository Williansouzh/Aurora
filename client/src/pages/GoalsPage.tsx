import {
  Award, ChevronRight, Flag, Loader2, Plus, Target, Trash2, TrendingUp,
} from 'lucide-react';
import { useState } from 'react';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Progress } from '../components/ui/progress';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/Select';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const AREA_LABELS = {
  1:'Saúde',2:'Trabalho',3:'Estudos',4:'Dinheiro',
  5:'Relacionamentos',6:'Casa',7:'Lazer',8:'Espiritualidade',9:'Projetos',
};
const STATUS_LABELS = { 1:'Ativa',2:'Pausada',3:'Concluída',4:'Cancelada' };
const STATUS_COLORS = {
  1:'bg-emerald-100 text-emerald-700',
  2:'bg-amber-100 text-amber-700',
  3:'bg-violet-100 text-violet-700',
  4:'bg-slate-100 text-slate-500',
};
const METRIC_LABELS = { 0:'Sem métrica',1:'Numérico',2:'Percentual' };

export function GoalsPage({ api }) {
  const [statusFilter, setStatusFilter] = useState('1');
  const [showForm, setShowForm] = useState(false);
  const [detail, setDetail] = useState(null);

  const goals = useData(
    () => api.get(`/api/goals${statusFilter !== '_all' ? `?status=${statusFilter}` : ''}`),
    [statusFilter],
  );
  const reload = () => goals.reload();

  const remove = async (id) => { await api.delete(`/api/goals/${id}`); reload(); };

  if (goals.loading) return <GoalsSkeleton />;

  const list = goals.data ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <Target className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight">Minha Jornada</h1>
        </div>
        <div className="flex items-center gap-2">
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-36 h-8 text-sm"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="_all">Todas</SelectItem>
              <SelectItem value="1">Ativas</SelectItem>
              <SelectItem value="2">Pausadas</SelectItem>
              <SelectItem value="3">Concluídas</SelectItem>
            </SelectContent>
          </Select>
          <Button size="sm" onClick={() => setShowForm(true)}>
            <Plus className="h-4 w-4 mr-1" /> Nova meta
          </Button>
        </div>
      </div>

      {list.length === 0 && <EmptyGoals onAdd={() => setShowForm(true)} />}

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        {list.map((g) => (
          <GoalCard
            key={g.id}
            goal={g}
            onOpen={() => setDetail(g)}
            onDelete={() => remove(g.id)}
          />
        ))}
      </div>

      {showForm && <GoalFormModal api={api} onClose={() => setShowForm(false)} onCreated={reload} />}
      {detail && (
        <GoalDetailModal
          api={api}
          goal={detail}
          onClose={() => setDetail(null)}
          onUpdated={(updated) => { setDetail(updated); reload(); }}
        />
      )}
    </div>
  );
}

function GoalCard({ goal, onOpen, onDelete }) {
  const progress = goal.progress ?? 0;
  const progressColor = progress >= 100 ? 'bg-violet-500' : progress >= 60 ? 'bg-emerald-500' : 'bg-blue-500';

  return (
    <Card className="hover:shadow-md transition-shadow cursor-pointer" onClick={onOpen}>
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between">
          <div className="flex-1 min-w-0 pr-2">
            <CardTitle className="text-base font-semibold truncate">{goal.title}</CardTitle>
            <div className="flex items-center gap-1.5 mt-1 flex-wrap">
              <span className={cn('text-[11px] font-medium px-1.5 py-0.5 rounded-full', STATUS_COLORS[goal.status])}>
                {STATUS_LABELS[goal.status]}
              </span>
              <span className="text-[11px] text-muted-foreground">{AREA_LABELS[goal.area]}</span>
              {goal.targetDate && (
                <span className="text-[11px] text-muted-foreground">
                  até {new Date(goal.targetDate).toLocaleDateString('pt-BR')}
                </span>
              )}
            </div>
          </div>
          <button
            className="text-muted-foreground hover:text-destructive p-1 shrink-0"
            onClick={(e) => { e.stopPropagation(); onDelete(); }}
          >
            <Trash2 className="h-3.5 w-3.5" />
          </button>
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-1.5">
          <div className="flex justify-between text-xs text-muted-foreground">
            <span>Progresso</span>
            <span className="font-semibold tabular-nums">{progress.toFixed(0)}%</span>
          </div>
          <Progress value={progress} indicatorClassName={progressColor} className="h-2" />
        </div>
        {goal.milestones.length > 0 && (
          <div className="mt-3 flex items-center gap-1.5 text-xs text-muted-foreground">
            <Flag className="h-3 w-3" />
            <span>
              {goal.milestones.filter((m) => m.isCompleted).length}/{goal.milestones.length} milestones
            </span>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function GoalDetailModal({ api, goal, onClose, onUpdated }) {
  const [newMilestone, setNewMilestone] = useState('');
  const [progressVal, setProgressVal] = useState(String(goal.currentValue ?? 0));
  const [saving, setSaving] = useState(false);

  const completeMilestone = async (milestoneId, isCompleted) => {
    const action = isCompleted ? 'reopen' : 'complete';
    const updated = await api.patch(`/api/goals/${goal.id}/milestones/${milestoneId}/${action}`);
    onUpdated(updated);
  };

  const addMilestone = async () => {
    if (!newMilestone.trim()) return;
    const updated = await api.post(`/api/goals/${goal.id}/milestones`, { title: newMilestone, isRequired: true });
    setNewMilestone('');
    onUpdated(updated);
  };

  const updateProgress = async () => {
    setSaving(true);
    try {
      const updated = await api.patch(`/api/goals/${goal.id}/progress`, { currentValue: parseFloat(progressVal) });
      onUpdated(updated);
    } finally { setSaving(false); }
  };

  const changeStatus = async (action) => {
    const updated = await api.patch(`/api/goals/${goal.id}/status`, { action, reason: 'Ação do usuário' });
    onUpdated(updated);
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{goal.title}</DialogTitle>
        </DialogHeader>
        <div className="space-y-5 mt-2">
          <div className="flex items-center gap-2 flex-wrap">
            <span className={cn('text-xs font-medium px-2 py-1 rounded-full', STATUS_COLORS[goal.status])}>
              {STATUS_LABELS[goal.status]}
            </span>
            <span className="text-xs text-muted-foreground">{AREA_LABELS[goal.area]}</span>
          </div>

          {goal.metricType !== 0 && (
            <div className="space-y-2">
              <p className="text-sm font-medium">Progresso</p>
              <Progress value={goal.progress} className="h-2" />
              <div className="flex items-center gap-2">
                <Input
                  type="number"
                  value={progressVal}
                  onChange={(e) => setProgressVal(e.target.value)}
                  className="h-8 w-28"
                />
                <span className="text-sm text-muted-foreground">
                  {goal.metricType === 2 ? '%' : `/ ${goal.targetValue}`}
                </span>
                <Button size="sm" onClick={updateProgress} disabled={saving}>
                  {saving ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : 'Salvar'}
                </Button>
              </div>
            </div>
          )}

          <div className="space-y-2">
            <p className="text-sm font-medium flex items-center gap-1.5">
              <Flag className="h-3.5 w-3.5" /> Milestones
            </p>
            <div className="space-y-1.5">
              {goal.milestones.map((m) => (
                <div key={m.id} className="flex items-center gap-2">
                  <button onClick={() => completeMilestone(m.id, m.isCompleted)}>
                    <div className={cn(
                      'w-4 h-4 rounded border-2 flex items-center justify-center transition-colors',
                      m.isCompleted ? 'bg-emerald-500 border-emerald-500' : 'border-muted-foreground'
                    )}>
                      {m.isCompleted && <span className="text-white text-[10px]">✓</span>}
                    </div>
                  </button>
                  <span className={cn('text-sm flex-1', m.isCompleted && 'line-through text-muted-foreground')}>
                    {m.title}
                    {m.isRequired && <span className="text-[10px] text-muted-foreground ml-1">(obrigatório)</span>}
                  </span>
                </div>
              ))}
            </div>
            <div className="flex gap-2 mt-2">
              <Input
                placeholder="Novo milestone..."
                value={newMilestone}
                onChange={(e) => setNewMilestone(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && addMilestone()}
                className="h-8 text-sm"
              />
              <Button size="sm" variant="outline" onClick={addMilestone} disabled={!newMilestone.trim()}>
                <Plus className="h-3.5 w-3.5" />
              </Button>
            </div>
          </div>

          <div className="flex gap-2 flex-wrap pt-1 border-t">
            {goal.status === 1 && <Button size="sm" variant="outline" onClick={() => changeStatus('pause')}>Pausar</Button>}
            {goal.status === 2 && <Button size="sm" variant="outline" onClick={() => changeStatus('resume')}>Retomar</Button>}
            {goal.status !== 3 && (
              <Button size="sm" className="bg-violet-600 hover:bg-violet-700" onClick={() => changeStatus('complete')}>
                <Award className="h-3.5 w-3.5 mr-1" /> Concluir
              </Button>
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}

function GoalFormModal({ api, onClose, onCreated }) {
  const [form, setForm] = useState({
    title: '', description: '', area: '1', metricType: '0', targetValue: '0',
    startDate: '', targetDate: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      await api.post('/api/goals', {
        title: form.title,
        description: form.description || null,
        area: parseInt(form.area),
        metricType: parseInt(form.metricType),
        targetValue: parseFloat(form.targetValue) || 0,
        startDate: form.startDate || null,
        targetDate: form.targetDate || null,
      });
      onCreated(); onClose();
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Nova meta</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label>Título</Label>
            <Input autoFocus placeholder="Ex: Ler 12 livros em 2026" value={form.title} onChange={(e) => set('title', e.target.value)} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Área</Label>
              <Select value={form.area} onValueChange={(v) => set('area', v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(AREA_LABELS).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Tipo de métrica</Label>
              <Select value={form.metricType} onValueChange={(v) => set('metricType', v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(METRIC_LABELS).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          </div>
          {form.metricType !== '0' && (
            <div className="space-y-1.5">
              <Label>Valor alvo</Label>
              <Input type="number" value={form.targetValue} onChange={(e) => set('targetValue', e.target.value)} />
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Início</Label>
              <Input type="date" value={form.startDate} onChange={(e) => set('startDate', e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Prazo</Label>
              <Input type="date" value={form.targetDate} onChange={(e) => set('targetDate', e.target.value)} />
            </div>
          </div>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving || !form.title.trim()}>{saving ? 'Salvando...' : 'Criar meta'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function EmptyGoals({ onAdd }) {
  return (
    <div className="flex flex-col items-center py-16 text-center">
      <div className="rounded-full bg-muted p-5 mb-4"><Target className="h-7 w-7 text-muted-foreground/40" /></div>
      <p className="text-base font-semibold mb-1">Nenhuma meta definida</p>
      <p className="text-sm text-muted-foreground mb-4 max-w-xs">Defina objetivos de vida com milestones e acompanhe seu progresso.</p>
      <Button onClick={onAdd}><Plus className="h-4 w-4 mr-1" /> Criar primeira meta</Button>
    </div>
  );
}

function GoalsSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between"><Skeleton className="h-9 w-40" /><Skeleton className="h-8 w-28" /></div>
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-36 rounded-xl" />)}
      </div>
    </div>
  );
}
