import { BookOpen, ChevronLeft, ChevronRight, Plus, Save, Tag, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const MOOD_EMOJI = { 1: '😞', 2: '😕', 3: '😐', 4: '🙂', 5: '😄' };
const MOOD_LABEL = { 1: 'Muito ruim', 2: 'Ruim', 3: 'Neutro', 4: 'Bom', 5: 'Ótimo' };
const MOOD_COLOR = {
  1: 'text-red-500', 2: 'text-orange-500', 3: 'text-slate-500',
  4: 'text-emerald-500', 5: 'text-emerald-600',
};

export function DiaryPage({ api }) {
  const today = new Date().toISOString().split('T')[0];
  const [selectedDate, setSelectedDate] = useState(today);
  const [mode, setMode] = useState('list');

  const entries = useData(() => api.get('/api/diary?pageSize=30'), []);
  const dayEntry = useData(() => api.get(`/api/diary/date/${selectedDate}`), [selectedDate]);

  const reload = () => { entries.reload(); dayEntry.reload(); };

  const navigateDay = (delta) => {
    const d = new Date(selectedDate);
    d.setDate(d.getDate() + delta);
    setSelectedDate(d.toISOString().split('T')[0]);
  };

  if (entries.loading) return <DiarySkeleton />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <BookOpen className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight">Diário</h1>
        </div>
        <div className="flex items-center gap-2">
          <div className="flex items-center gap-1 rounded-lg border bg-card px-1 py-1">
            <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateDay(-1)}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <span className="text-sm font-medium min-w-[110px] text-center px-1">
              {new Date(selectedDate + 'T12:00:00').toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' })}
            </span>
            <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigateDay(1)}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          {dayEntry.loading ? (
            <Skeleton className="h-64 rounded-xl" />
          ) : (
            <DayEditor
              api={api}
              date={selectedDate}
              entry={dayEntry.data}
              onSaved={reload}
            />
          )}
        </div>
        <div className="space-y-3">
          <p className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">Registros recentes</p>
          {(entries.data?.items ?? []).slice(0, 10).map((e) => (
            <RecentEntryCard
              key={e.id}
              entry={e}
              isSelected={e.date.split('T')[0] === selectedDate}
              onClick={() => setSelectedDate(e.date.split('T')[0])}
            />
          ))}
          {(entries.data?.items ?? []).length === 0 && (
            <p className="text-sm text-muted-foreground text-center py-4">Nenhum registro ainda.</p>
          )}
        </div>
      </div>
    </div>
  );
}

function DayEditor({ api, date, entry, onSaved }) {
  const [content, setContent] = useState(entry?.content ?? '');
  const [mood, setMood] = useState(entry?.mood ?? 3);
  const [tagInput, setTagInput] = useState('');
  const [tags, setTags] = useState(entry?.tags ?? []);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const addTag = () => {
    const t = tagInput.trim().toLowerCase();
    if (t && !tags.includes(t)) { setTags([...tags, t]); setTagInput(''); }
  };

  const save = async () => {
    if (!content.trim()) return;
    setSaving(true); setError('');
    try {
      if (entry) {
        await api.put(`/api/diary/${entry.id}`, { content, mood, tags });
      } else {
        await api.post('/api/diary', { date, content, mood, tags });
      }
      onSaved();
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  };

  const remove = async () => {
    if (!entry) return;
    await api.delete(`/api/diary/${entry.id}`);
    setContent(''); setMood(3); setTags([]);
    onSaved();
  };

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-semibold flex items-center justify-between">
          <span>
            {new Date(date + 'T12:00:00').toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long' })}
          </span>
          {entry && (
            <button className="text-muted-foreground hover:text-destructive" onClick={remove}>
              <Trash2 className="h-3.5 w-3.5" />
            </button>
          )}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <p className="text-xs font-medium text-muted-foreground mb-2">Como você está?</p>
          <div className="flex gap-2">
            {[1, 2, 3, 4, 5].map((m) => (
              <button
                key={m}
                onClick={() => setMood(m)}
                className={cn(
                  'flex flex-col items-center gap-0.5 p-2 rounded-lg border-2 transition-all text-xl',
                  mood === m ? 'border-primary bg-primary/5' : 'border-transparent hover:border-muted'
                )}
              >
                <span>{MOOD_EMOJI[m]}</span>
                <span className="text-[9px] text-muted-foreground">{MOOD_LABEL[m]}</span>
              </button>
            ))}
          </div>
        </div>

        <textarea
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm min-h-[160px] resize-none focus:outline-none focus:ring-1 focus:ring-ring"
          placeholder="O que aconteceu hoje? Como você se sentiu? O que aprendeu?"
          value={content}
          onChange={(e) => setContent(e.target.value)}
        />

        <div>
          <p className="text-xs font-medium text-muted-foreground mb-2 flex items-center gap-1">
            <Tag className="h-3 w-3" /> Tags
          </p>
          <div className="flex flex-wrap gap-1.5 mb-2">
            {tags.map((t) => (
              <span
                key={t}
                className="text-xs bg-secondary text-secondary-foreground px-2 py-0.5 rounded-full cursor-pointer hover:bg-destructive/20"
                onClick={() => setTags(tags.filter((x) => x !== t))}
              >
                #{t}
              </span>
            ))}
          </div>
          <div className="flex gap-2">
            <input
              className="flex-1 h-7 rounded-md border border-input bg-background px-2 text-xs focus:outline-none focus:ring-1 focus:ring-ring"
              placeholder="Adicionar tag..."
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && addTag()}
            />
            <Button size="sm" variant="outline" className="h-7 text-xs" onClick={addTag}>+</Button>
          </div>
        </div>

        {error && <p className="text-sm text-destructive">{error}</p>}

        <Button className="w-full" onClick={save} disabled={saving || !content.trim()}>
          <Save className="h-4 w-4 mr-1.5" />
          {saving ? 'Salvando...' : entry ? 'Atualizar' : 'Salvar registro'}
        </Button>
      </CardContent>
    </Card>
  );
}

function RecentEntryCard({ entry, isSelected, onClick }) {
  const d = new Date(entry.date + 'T12:00:00');
  return (
    <button
      className={cn(
        'w-full text-left rounded-lg border p-3 transition-colors hover:bg-accent',
        isSelected && 'border-primary bg-primary/5'
      )}
      onClick={onClick}
    >
      <div className="flex items-center justify-between">
        <span className="text-xs font-medium">
          {d.toLocaleDateString('pt-BR', { weekday: 'short', day: '2-digit', month: 'short' })}
        </span>
        <span className="text-base">{MOOD_EMOJI[entry.mood]}</span>
      </div>
      <p className="text-xs text-muted-foreground mt-1 line-clamp-2">{entry.content}</p>
      {entry.tags.length > 0 && (
        <div className="flex gap-1 mt-1.5 flex-wrap">
          {entry.tags.slice(0, 3).map((t) => (
            <span key={t} className="text-[10px] bg-secondary px-1.5 py-0.5 rounded-full">#{t}</span>
          ))}
        </div>
      )}
    </button>
  );
}

function DiarySkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between"><Skeleton className="h-9 w-32" /><Skeleton className="h-9 w-48" /></div>
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2"><Skeleton className="h-80 rounded-xl" /></div>
        <div className="space-y-3">{Array.from({length: 4}).map((_,i) => <Skeleton key={i} className="h-20 rounded-lg" />)}</div>
      </div>
    </div>
  );
}
