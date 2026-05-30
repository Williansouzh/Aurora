import {
  BookOpen, CheckCircle2, ChevronDown, Flame, Heart, HeartOff,
  Eye, EyeOff, Plus, Scroll, Star, Target, Zap,
} from 'lucide-react';
import { useState } from 'react';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent } from '../components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/Select';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const EVENT_TYPE_LABELS = {
  1: 'Check-in de hábito', 2: 'Tarefa concluída', 3: 'Meta avançou',
  4: 'Meta concluída', 5: 'Diário escrito', 6: 'Foto adicionada',
  7: 'Semana encerrada', 8: 'Mês fechado', 9: 'Conquista', 10: 'Post',
};

const EVENT_TYPE_ICONS = {
  1: Flame, 2: CheckCircle2, 3: Target, 4: Target,
  5: BookOpen, 6: Star, 7: Scroll, 8: Zap, 9: Zap, 10: Heart,
};

const EVENT_TYPE_COLORS = {
  1: 'bg-orange-100 text-orange-700',
  2: 'bg-emerald-100 text-emerald-700',
  3: 'bg-blue-100 text-blue-700',
  4: 'bg-violet-100 text-violet-700',
  5: 'bg-pink-100 text-pink-700',
  6: 'bg-cyan-100 text-cyan-700',
  7: 'bg-amber-100 text-amber-700',
  8: 'bg-indigo-100 text-indigo-700',
  9: 'bg-yellow-100 text-yellow-700',
  10: 'bg-slate-100 text-slate-700',
};

const AREA_LABELS = {
  1: 'Saúde', 2: 'Trabalho', 3: 'Estudos', 4: 'Dinheiro',
  5: 'Relacionamentos', 6: 'Casa', 7: 'Lazer', 8: 'Espiritualidade', 9: 'Projetos',
};

export function TimelinePage({ api }) {
  const [page, setPage] = useState(1);
  const [typeFilter, setTypeFilter] = useState('');
  const [favoritesOnly, setFavoritesOnly] = useState(false);
  const [showPost, setShowPost] = useState(false);
  const [events, setEvents] = useState([]);
  const [totalPages, setTotalPages] = useState(1);

  const params = new URLSearchParams({ page, pageSize: 20 });
  if (typeFilter) params.set('type', typeFilter);
  if (favoritesOnly) params.set('favoritesOnly', 'true');

  const data = useData(async () => {
    const result = await api.get(`/api/timeline?${params}`);
    setEvents((prev) => page === 1 ? result.items : [...prev, ...result.items]);
    setTotalPages(Math.ceil((result.totalCount ?? 0) / 20));
    return result;
  }, [page, typeFilter, favoritesOnly]);

  const reload = () => {
    setPage(1);
    setEvents([]);
    data.reload();
  };

  const toggle = async (id, action) => {
    await api.patch(`/api/timeline/${id}/${action}`);
    reload();
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <Scroll className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">Linha da Vida</h1>
        </div>
        <Button size="sm" onClick={() => setShowPost(true)}>
          <Plus className="h-4 w-4 mr-1" /> Post
        </Button>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-2 flex-wrap">
        <Select value={typeFilter} onValueChange={(v) => { setTypeFilter(v === '_all' ? '' : v); setPage(1); setEvents([]); }}>
          <SelectTrigger className="w-44 h-8 text-sm">
            <SelectValue placeholder="Todos os tipos" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="_all">Todos os tipos</SelectItem>
            {Object.entries(EVENT_TYPE_LABELS).map(([k, v]) => (
              <SelectItem key={k} value={k}>{v}</SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Button
          variant={favoritesOnly ? 'default' : 'outline'}
          size="sm"
          className="h-8"
          onClick={() => { setFavoritesOnly((v) => !v); setPage(1); setEvents([]); }}
        >
          <Heart className="h-3.5 w-3.5 mr-1" />
          Favoritos
        </Button>
      </div>

      {/* Feed */}
      {data.loading && events.length === 0 ? (
        <TimelineSkeleton />
      ) : events.length === 0 ? (
        <EmptyTimeline onPost={() => setShowPost(true)} />
      ) : (
        <div className="space-y-3">
          {events.map((evt) => (
            <TimelineEventCard
              key={evt.id}
              event={evt}
              onHide={() => toggle(evt.id, evt.isHidden ? 'unhide' : 'hide')}
              onFavorite={() => toggle(evt.id, evt.isFavorite ? 'unfavorite' : 'favorite')}
            />
          ))}
          {page < totalPages && (
            <div className="flex justify-center pt-2">
              <Button variant="outline" size="sm" onClick={() => setPage((p) => p + 1)} disabled={data.loading}>
                <ChevronDown className="h-4 w-4 mr-1" />
                {data.loading ? 'Carregando...' : 'Carregar mais'}
              </Button>
            </div>
          )}
        </div>
      )}

      {showPost && (
        <NewPostModal api={api} onClose={() => setShowPost(false)} onCreated={reload} />
      )}
    </div>
  );
}

function TimelineEventCard({ event, onHide, onFavorite }) {
  const Icon = EVENT_TYPE_ICONS[event.type] ?? Zap;
  const colorClass = EVENT_TYPE_COLORS[event.type] ?? 'bg-slate-100 text-slate-700';

  return (
    <Card className="hover:shadow-sm transition-shadow">
      <CardContent className="p-4">
        <div className="flex items-start gap-3">
          <div className={cn('rounded-full p-2 shrink-0 mt-0.5', colorClass)}>
            <Icon className="h-3.5 w-3.5" />
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-start justify-between gap-2">
              <p className="text-sm font-medium text-foreground">{event.title}</p>
              <div className="flex items-center gap-1 shrink-0">
                <button
                  className={cn('p-1 rounded transition-colors', event.isFavorite ? 'text-red-500' : 'text-muted-foreground hover:text-red-400')}
                  onClick={onFavorite}
                  title={event.isFavorite ? 'Remover dos favoritos' : 'Favoritar'}
                >
                  <Heart className="h-3.5 w-3.5" fill={event.isFavorite ? 'currentColor' : 'none'} />
                </button>
                <button
                  className="p-1 rounded text-muted-foreground hover:text-foreground transition-colors"
                  onClick={onHide}
                  title="Ocultar"
                >
                  <EyeOff className="h-3.5 w-3.5" />
                </button>
              </div>
            </div>
            {event.description && (
              <p className="text-xs text-muted-foreground mt-0.5">{event.description}</p>
            )}
            <div className="flex items-center gap-2 mt-1.5">
              <span className="text-[11px] text-muted-foreground">
                {new Date(event.occurredAt).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })}
              </span>
              {event.area && (
                <Badge variant="outline" className="text-[10px] h-4 px-1.5">
                  {AREA_LABELS[event.area]}
                </Badge>
              )}
              <Badge variant="outline" className="text-[10px] h-4 px-1.5">
                {EVENT_TYPE_LABELS[event.type]}
              </Badge>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

function EmptyTimeline({ onPost }) {
  return (
    <div className="flex flex-col items-center py-16 text-center">
      <div className="rounded-full bg-muted p-5 mb-4">
        <Scroll className="h-7 w-7 text-muted-foreground/40" />
      </div>
      <p className="text-base font-semibold text-foreground mb-1">Linha da vida vazia</p>
      <p className="text-sm text-muted-foreground mb-4 max-w-sm">
        Seus hábitos, tarefas e marcos aparecerão aqui automaticamente. Você também pode criar posts manuais.
      </p>
      <Button onClick={onPost}>
        <Plus className="h-4 w-4 mr-1" /> Criar primeiro post
      </Button>
    </div>
  );
}

function NewPostModal({ api, onClose, onCreated }) {
  const [form, setForm] = useState({ title: '', description: '', area: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  const submit = async (e) => {
    e.preventDefault();
    if (!form.title.trim()) return;
    setSaving(true);
    setError('');
    try {
      await api.post('/api/timeline', {
        title: form.title,
        description: form.description || null,
        area: form.area ? parseInt(form.area) : null,
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
        <DialogHeader><DialogTitle>Novo post</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label>Título</Label>
            <Input autoFocus placeholder="O que aconteceu?" value={form.title} onChange={(e) => set('title', e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>Descrição (opcional)</Label>
            <Input placeholder="Detalhes..." value={form.description} onChange={(e) => set('description', e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>Área da vida</Label>
            <Select value={form.area} onValueChange={(v) => set('area', v === '_none' ? '' : v)}>
              <SelectTrigger><SelectValue placeholder="Sem área" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="_none">Sem área</SelectItem>
                {Object.entries(AREA_LABELS).map(([k, v]) => (
                  <SelectItem key={k} value={k}>{v}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving || !form.title.trim()}>
              {saving ? 'Publicando...' : 'Publicar'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function TimelineSkeleton() {
  return (
    <div className="space-y-3">
      {Array.from({ length: 5 }).map((_, i) => (
        <Skeleton key={i} className="h-20 rounded-xl" />
      ))}
    </div>
  );
}
