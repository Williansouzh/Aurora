import { ArrowRight, Inbox, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/Select';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const PRIORITY_LABEL = { 1: 'Baixa', 2: 'Média', 3: 'Alta' };
const PRIORITY_COLOR = { 1: 'text-slate-500', 2: 'text-amber-500', 3: 'text-red-500' };

export function BacklogPage({ api }) {
  const [title, setTitle] = useState('');
  const [priority, setPriority] = useState('2');
  const [adding, setAdding] = useState(false);

  const backlog = useData(() => api.get('/api/today/backlog'), []);
  const reload = () => backlog.reload();

  const add = async () => {
    if (!title.trim()) return;
    setAdding(true);
    try {
      await api.post('/api/today/backlog', { title: title.trim(), priority: parseInt(priority) });
      setTitle('');
      reload();
    } finally { setAdding(false); }
  };

  const moveToToday = async (id) => {
    await api.patch(`/api/today/${id}/move-to-today`);
    reload();
  };

  const remove = async (id) => {
    await api.delete(`/api/today/${id}`);
    reload();
  };

  if (backlog.loading) return <BacklogSkeleton />;

  const items = backlog.data ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
          <Inbox className="h-5 w-5 text-primary" />
        </div>
        <h1 className="text-2xl font-bold tracking-tight">Backlog</h1>
      </div>

      {/* Quick capture */}
      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-semibold">Captura rápida</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input
              placeholder="O que precisa ser feito algum dia..."
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && add()}
              className="flex-1"
            />
            <Select value={priority} onValueChange={setPriority}>
              <SelectTrigger className="w-28">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="1">Baixa</SelectItem>
                <SelectItem value="2">Média</SelectItem>
                <SelectItem value="3">Alta</SelectItem>
              </SelectContent>
            </Select>
            <Button onClick={add} disabled={adding || !title.trim()}>
              <Plus className="h-4 w-4 mr-1" /> Adicionar
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-semibold flex items-center justify-between">
            <span>Tarefas no backlog</span>
            <span className="text-xs text-muted-foreground font-normal">{items.length} itens</span>
          </CardTitle>
        </CardHeader>
        <CardContent>
          {items.length === 0 ? (
            <div className="flex flex-col items-center py-10 text-center">
              <div className="rounded-full bg-muted p-4 mb-3">
                <Inbox className="h-6 w-6 text-muted-foreground/40" />
              </div>
              <p className="text-sm font-medium text-muted-foreground">Backlog vazio</p>
              <p className="text-xs text-muted-foreground mt-1">Capture tarefas sem data definida aqui.</p>
            </div>
          ) : (
            <div className="space-y-1.5">
              {items.map((t) => (
                <div key={t.id} className="flex items-center gap-3 rounded-lg px-3 py-2.5 group hover:bg-accent/50 transition-colors">
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium">{t.title}</p>
                    {t.notes && <p className="text-xs text-muted-foreground">{t.notes}</p>}
                  </div>
                  <span className={cn('text-[11px] font-medium', PRIORITY_COLOR[t.priority])}>
                    {PRIORITY_LABEL[t.priority]}
                  </span>
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-7 text-xs opacity-0 group-hover:opacity-100 transition-opacity"
                    onClick={() => moveToToday(t.id)}
                  >
                    <ArrowRight className="h-3 w-3 mr-1" /> Hoje
                  </Button>
                  <button
                    className="opacity-0 group-hover:opacity-100 transition-opacity text-muted-foreground hover:text-destructive"
                    onClick={() => remove(t.id)}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function BacklogSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-32" />
      <Skeleton className="h-16 rounded-xl" />
      <Skeleton className="h-64 rounded-xl" />
    </div>
  );
}
