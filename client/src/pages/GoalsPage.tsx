import {
  Award, Flag, Loader2, Plus, Target, Trash2, X,
} from 'lucide-react';
import { useState } from 'react';
import { Button } from '../components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/Select';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const AREA_LABELS = {
  1:'Saúde',2:'Trabalho',3:'Estudos',4:'Dinheiro',
  5:'Relacionamentos',6:'Casa',7:'Lazer',8:'Espiritualidade',9:'Projetos',
};
// Muted, Quiet-harmonious per-area palette (goal progress bars + dots)
const AREA_COLORS = {
  1:'#6f8f6a', 2:'#3d4eac', 3:'#7b5cd6', 4:'#c1976a', 5:'#c1796a',
  6:'#5b8a8a', 7:'#b08968', 8:'#8a7fb0', 9:'#6b7280',
};
const STATUS_LABELS = { 1:'Ativa',2:'Pausada',3:'Concluída',4:'Cancelada' };
const STATUS_COLORS = {
  1:'bg-income-soft text-income',
  2:'bg-pending-soft text-pending',
  3:'bg-accent text-primary',
  4:'bg-secondary text-mut2',
};
const METRIC_LABELS = { 0:'Sem métrica',1:'Numérico',2:'Percentual' };

export function GoalsPage({ api }) {
  const [statusFilter, setStatusFilter] = useState('1');
  const [areaFilter, setAreaFilter] = useState('_all');
  const [showForm, setShowForm] = useState(false);
  const [selectedId, setSelectedId] = useState(null);

  const goals = useData(
    () => api.get(`/api/goals${statusFilter !== '_all' ? `?status=${statusFilter}` : ''}`),
    [statusFilter],
  );
  // Options for the "Vinculados" editor — degrade gracefully if a module isn't accessible.
  const habitsData = useData(() => api.get('/api/habits').catch(() => []), []);
  const skillsData = useData(() => api.get('/api/studies/skills').catch(() => []), []);
  const habitOptions = (habitsData.data ?? []).map((h) => ({ id: h.id, name: h.name }));
  const skillOptions = (skillsData.data ?? []).map((s) => ({ id: s.id, name: s.title }));
  const reload = () => goals.reload();

  const remove = async (id) => {
    await api.delete(`/api/goals/${id}`);
    if (selectedId === id) setSelectedId(null);
    reload();
  };

  if (goals.loading) return <GoalsSkeleton />;

  const all = goals.data ?? [];
  const areas = [...new Set(all.map((g) => g.area))];
  const list = areaFilter === '_all' ? all : all.filter((g) => String(g.area) === areaFilter);
  const selected = list.find((g) => g.id === selectedId) ?? list[0] ?? null;

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <h1 className="font-display text-3xl text-foreground">Minha Jornada</h1>
          <p className="mt-1 text-sm text-mut2">Seus objetivos de vida, por área — e o quanto você já caminhou.</p>
        </div>
        <div className="flex items-center gap-2">
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-36 h-9 text-sm"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="_all">Todas</SelectItem>
              <SelectItem value="1">Ativas</SelectItem>
              <SelectItem value="2">Pausadas</SelectItem>
              <SelectItem value="3">Concluídas</SelectItem>
            </SelectContent>
          </Select>
          <Button onClick={() => setShowForm(true)}>
            <Plus className="h-4 w-4" /> Nova meta
          </Button>
        </div>
      </div>

      {/* Area filter chips */}
      {areas.length > 1 && (
        <div className="flex flex-wrap gap-2">
          <AreaChip label="Todas" active={areaFilter === '_all'} onClick={() => setAreaFilter('_all')} />
          {areas.map((a) => (
            <AreaChip
              key={a}
              label={AREA_LABELS[a]}
              color={AREA_COLORS[a]}
              active={areaFilter === String(a)}
              onClick={() => setAreaFilter(String(a))}
            />
          ))}
        </div>
      )}

      {list.length === 0 ? (
        <EmptyGoals onAdd={() => setShowForm(true)} />
      ) : (
        <div className="flex flex-col gap-5 lg:flex-row lg:items-start">
          {/* goal grid */}
          <div className="grid min-w-0 grid-cols-1 gap-4 sm:grid-cols-2 lg:flex-[1.25]">
            {list.map((g) => (
              <GoalCard
                key={g.id}
                goal={g}
                selected={selected?.id === g.id}
                onOpen={() => setSelectedId(g.id)}
                onDelete={() => remove(g.id)}
              />
            ))}
          </div>
          {/* inline detail panel */}
          {selected && (
            <div className="lg:flex-1 lg:sticky lg:top-4">
              <GoalDetailPanel
                api={api}
                goal={selected}
                habitOptions={habitOptions}
                skillOptions={skillOptions}
                onUpdated={(updated) => { setSelectedId(updated.id); reload(); }}
              />
            </div>
          )}
        </div>
      )}

      {showForm && <GoalFormModal api={api} onClose={() => setShowForm(false)} onCreated={reload} />}
    </div>
  );
}

function AreaChip({ label, color, active, onClick }) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-[12px] font-medium transition-colors',
        active ? 'border-transparent bg-foreground text-background' : 'border-chipline text-mut2 hover:text-foreground'
      )}
    >
      {color && <span className="h-2 w-2 rounded-full" style={{ background: color }} />}
      {label}
    </button>
  );
}

function GoalCard({ goal, selected, onOpen, onDelete }) {
  const progress = Math.min(100, goal.progress ?? 0);
  const color = AREA_COLORS[goal.area] ?? '#3d4eac';
  const doneMs = goal.milestones.filter((m) => m.isCompleted).length;

  return (
    <div
      onClick={onOpen}
      className={cn(
        'group cursor-pointer rounded-[14px] border bg-card p-5 transition-shadow hover:shadow-card-hover',
        selected ? 'border-primary' : 'border-border'
      )}
    >
      <div className="flex items-start justify-between">
        <div className="min-w-0 pr-2">
          <div className="flex items-center gap-2">
            <span className="h-2 w-2 shrink-0 rounded-full" style={{ background: color }} />
            <span className="text-overline" style={{ color }}>{AREA_LABELS[goal.area]}</span>
            {goal.targetDate && (
              <span className="text-[11px] text-faint">· até {new Date(goal.targetDate).toLocaleDateString('pt-BR')}</span>
            )}
          </div>
          <p className="mt-2 truncate text-[17px] font-semibold text-ink2">{goal.title}</p>
        </div>
        <button
          className="p-1 text-faint opacity-0 transition-opacity hover:text-destructive group-hover:opacity-100"
          onClick={(e) => { e.stopPropagation(); onDelete(); }}
        >
          <Trash2 className="h-3.5 w-3.5" />
        </button>
      </div>

      <div className="mt-4 space-y-1.5">
        <div className="flex items-center justify-between text-xs">
          <span className="text-mut2">Progresso</span>
          <span className="font-numeral text-base text-foreground">{progress.toFixed(0)}%</span>
        </div>
        <div className="h-2 overflow-hidden rounded-full bg-track">
          <div className="h-full rounded-full transition-all" style={{ width: `${progress}%`, background: color }} />
        </div>
      </div>

      <div className="mt-3 flex items-center gap-3 text-xs text-faint">
        <span className={cn('rounded-full px-2 py-0.5 font-medium', STATUS_COLORS[goal.status])}>{STATUS_LABELS[goal.status]}</span>
        {goal.milestones.length > 0 && (
          <span className="flex items-center gap-1.5"><Flag className="h-3 w-3" /> {doneMs}/{goal.milestones.length} marcos</span>
        )}
      </div>
    </div>
  );
}

function LinkChip({ label, onRemove }) {
  return (
    <span className="inline-flex items-center gap-1.5 rounded-full border border-chipline px-3 py-1 text-[12.5px] text-mut">
      {label}
      <button onClick={onRemove} className="text-faint hover:text-destructive">
        <X className="h-3 w-3" />
      </button>
    </span>
  );
}

function GoalDetailPanel({ api, goal, habitOptions = [], skillOptions = [], onUpdated }) {
  const [newMilestone, setNewMilestone] = useState('');
  const [progressVal, setProgressVal] = useState(String(goal.currentValue ?? 0));
  const [saving, setSaving] = useState(false);

  // Reset the editable progress field whenever a different goal is selected.
  const [lastId, setLastId] = useState(goal.id);
  if (lastId !== goal.id) {
    setLastId(goal.id);
    setProgressVal(String(goal.currentValue ?? 0));
    setNewMilestone('');
  }

  const color = AREA_COLORS[goal.area] ?? '#3d4eac';
  const progress = Math.min(100, goal.progress ?? 0);
  const doneMs = goal.milestones.filter((m) => m.isCompleted).length;

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

  const linkedHabits = goal.linkedHabits ?? [];
  const linkedSkills = goal.linkedSkills ?? [];

  const saveLinks = async (habitIds, skillIds) => {
    const updated = await api.put(`/api/goals/${goal.id}/links`, { habitIds, skillIds });
    onUpdated(updated);
  };
  const addHabit = (id) => {
    if (!id || linkedHabits.some((h) => h.id === id)) return;
    saveLinks([...linkedHabits.map((h) => h.id), id], linkedSkills.map((s) => s.id));
  };
  const addSkill = (id) => {
    if (!id || linkedSkills.some((s) => s.id === id)) return;
    saveLinks(linkedHabits.map((h) => h.id), [...linkedSkills.map((s) => s.id), id]);
  };
  const removeHabit = (id) => saveLinks(linkedHabits.filter((h) => h.id !== id).map((h) => h.id), linkedSkills.map((s) => s.id));
  const removeSkill = (id) => saveLinks(linkedHabits.map((h) => h.id), linkedSkills.filter((s) => s.id !== id).map((s) => s.id));

  const availableHabits = habitOptions.filter((o) => !linkedHabits.some((h) => h.id === o.id));
  const availableSkills = skillOptions.filter((o) => !linkedSkills.some((s) => s.id === o.id));

  return (
    <div className="rounded-[14px] border bg-card p-6">
      <div className="text-overline" style={{ color }}>
        {AREA_LABELS[goal.area]}
        {goal.targetDate
          ? ` · alvo ${new Date(goal.targetDate).toLocaleDateString('pt-BR', { month: 'short', year: 'numeric' })}`
          : ' · em andamento'}
      </div>
      <h2 className="mt-1.5 font-display text-2xl leading-tight text-foreground">{goal.title}</h2>
      {goal.description && (
        <p className="mt-2 text-sm leading-relaxed text-mut">{goal.description}</p>
      )}

      <div className="mt-5 flex items-center gap-3">
        <div className="h-2 flex-1 overflow-hidden rounded-full bg-track">
          <div className="h-full rounded-full" style={{ width: `${progress}%`, background: color }} />
        </div>
        <span className="font-numeral text-[22px]" style={{ color }}>{progress.toFixed(0)}%</span>
      </div>

      {goal.metricType !== 0 && (
        <div className="mt-4 flex items-center gap-2">
          <Input
            type="number"
            value={progressVal}
            onChange={(e) => setProgressVal(e.target.value)}
            className="h-8 w-28"
          />
          <span className="text-sm text-mut2">{goal.metricType === 2 ? '%' : `/ ${goal.targetValue}`}</span>
          <Button size="sm" onClick={updateProgress} disabled={saving}>
            {saving ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : 'Salvar'}
          </Button>
        </div>
      )}

      <div className="mt-6 flex items-center justify-between">
        <span className="text-overline">Marcos</span>
        {goal.milestones.length > 0 && (
          <span className="text-xs text-faint">{doneMs} / {goal.milestones.length}</span>
        )}
      </div>
      <div className="mt-3 flex flex-col">
        {goal.milestones.map((m, i) => (
          <div
            key={m.id}
            className={cn('flex items-center gap-3 py-2.5', i < goal.milestones.length - 1 && 'border-b border-line2')}
          >
            <button onClick={() => completeMilestone(m.id, m.isCompleted)} className="shrink-0">
              <span
                className={cn(
                  'flex h-[18px] w-[18px] items-center justify-center rounded-full border-[1.5px] transition-colors',
                  m.isCompleted ? 'border-transparent bg-income text-white' : 'border-track text-transparent'
                )}
              >
                <span className="text-[10px] leading-none">✓</span>
              </span>
            </button>
            <span className={cn('flex-1 text-[14.5px]', m.isCompleted && 'text-faint line-through')}>{m.title}</span>
            {m.isRequired && !m.isCompleted && (
              <span className="rounded-[5px] bg-track px-1.5 py-0.5 text-[11px] text-mut2">obrigatório</span>
            )}
          </div>
        ))}
        {goal.milestones.length === 0 && (
          <p className="py-2 text-xs text-muted-foreground">Sem marcos ainda — adicione o primeiro abaixo.</p>
        )}
      </div>
      <div className="mt-3 flex gap-2">
        <Input
          placeholder="Novo marco..."
          value={newMilestone}
          onChange={(e) => setNewMilestone(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && addMilestone()}
          className="h-8 text-sm"
        />
        <Button size="sm" variant="outline" onClick={addMilestone} disabled={!newMilestone.trim()}>
          <Plus className="h-3.5 w-3.5" />
        </Button>
      </div>

      <div className="mt-6 text-overline">Vinculados</div>
      <div className="mt-3 flex flex-wrap gap-2">
        {linkedHabits.map((h) => (
          <LinkChip key={h.id} label={`Ritual · ${h.name}`} onRemove={() => removeHabit(h.id)} />
        ))}
        {linkedSkills.map((s) => (
          <LinkChip key={s.id} label={`Habilidade · ${s.name}`} onRemove={() => removeSkill(s.id)} />
        ))}
        {linkedHabits.length === 0 && linkedSkills.length === 0 && (
          <span className="text-xs text-faint">Vincule rituais e habilidades que sustentam esta meta.</span>
        )}
      </div>
      {(availableHabits.length > 0 || availableSkills.length > 0) && (
        <div className="mt-3 grid grid-cols-2 gap-2">
          {availableHabits.length > 0 && (
            <Select value="" onValueChange={addHabit}>
              <SelectTrigger className="h-8 text-xs"><SelectValue placeholder="+ Ritual" /></SelectTrigger>
              <SelectContent>
                {availableHabits.map((o) => <SelectItem key={o.id} value={o.id}>{o.name}</SelectItem>)}
              </SelectContent>
            </Select>
          )}
          {availableSkills.length > 0 && (
            <Select value="" onValueChange={addSkill}>
              <SelectTrigger className="h-8 text-xs"><SelectValue placeholder="+ Habilidade" /></SelectTrigger>
              <SelectContent>
                {availableSkills.map((o) => <SelectItem key={o.id} value={o.id}>{o.name}</SelectItem>)}
              </SelectContent>
            </Select>
          )}
        </div>
      )}

      <div className="mt-6 flex flex-wrap items-center gap-2 border-t pt-4">
        <span className={cn('rounded-full px-2.5 py-1 text-xs font-medium', STATUS_COLORS[goal.status])}>
          {STATUS_LABELS[goal.status]}
        </span>
        <div className="flex-1" />
        {goal.status === 1 && <Button size="sm" variant="outline" onClick={() => changeStatus('pause')}>Pausar</Button>}
        {goal.status === 2 && <Button size="sm" variant="outline" onClick={() => changeStatus('resume')}>Retomar</Button>}
        {goal.status !== 3 && (
          <Button size="sm" onClick={() => changeStatus('complete')}>
            <Award className="mr-1 h-3.5 w-3.5" /> Concluir
          </Button>
        )}
      </div>
    </div>
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
