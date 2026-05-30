import { MoreHorizontal, Plus, Tag } from 'lucide-react';
import { useState } from 'react';
import { ColorPicker } from '../components/ui/ColorPicker';
import { ConfirmModal } from '../components/ui/ConfirmModal';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '../components/ui/dropdown-menu';
import { EmptyState } from '../components/ui/EmptyState';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Screen } from '../components/ui/Screen';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { categoryIcons, categoryTypes, colors } from '../constants/financeOptions';
import { useData } from '../hooks/useData';
import { useToast } from '../hooks/useToast';
import { cn } from '../lib/utils';
import { enumLabel, enumValue } from '../utils/enumHelpers';

const initialForm = { name: '', type: 'Expense', color: colors[1], icon: categoryIcons[0] };

export function CategoriesPage({ api }) {
  const categories = useData(() => api.get('/api/categories'), []);
  const toast = useToast();
  const [form, setForm] = useState(initialForm);
  const [editing, setEditing] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleting, setDeleting] = useState(null);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const set = (field) => (val) => setForm((f) => ({ ...f, [field]: val }));
  const setEv = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  const openNew = (type = 'Expense') => {
    setEditing(null);
    setForm({ ...initialForm, type });
    setError('');
    setDialogOpen(true);
  };

  const openEdit = (category) => {
    setEditing(category);
    setForm({
      name: category.name,
      type: categoryTypes[category.type]?.[0] || initialForm.type,
      color: category.color,
      icon: category.icon,
    });
    setError('');
    setDialogOpen(true);
  };

  const reset = () => { setDialogOpen(false); setEditing(null); setForm(initialForm); setError(''); };

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    const payload = { ...form, type: enumValue(categoryTypes, form.type), userId: '' };
    try {
      if (editing) {
        await api.put(`/api/categories/${editing.id}`, { ...payload, id: editing.id });
      } else {
        await api.post('/api/categories', payload);
      }
      reset();
      categories.reload();
      toast.success(editing ? 'Categoria atualizada' : 'Categoria criada');
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const deleteCategory = async () => {
    if (!deleting) return;
    try {
      await api.delete(`/api/categories/${deleting.id}`);
      setDeleting(null);
      await categories.reload();
      toast.success('Categoria removida');
    } catch (err) {
      toast.error(err.message);
    }
  };

  const income = (categories.data || []).filter((c) => c.type === 0);
  const expense = (categories.data || []).filter((c) => c.type === 1);

  return (
    <Screen title="Categorias" loading={categories.loading} error={categories.error}>
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          {income.length} receita{income.length !== 1 ? 's' : ''} · {expense.length} despesa{expense.length !== 1 ? 's' : ''}
        </p>
        <Button size="sm" onClick={() => openNew()}>
          <Plus className="h-4 w-4" />
          Nova categoria
        </Button>
      </div>

      <Tabs defaultValue="expense">
        <TabsList>
          <TabsTrigger value="expense">Despesas ({expense.length})</TabsTrigger>
          <TabsTrigger value="income">Receitas ({income.length})</TabsTrigger>
        </TabsList>

        <TabsContent value="expense" className="mt-4">
          <CategoryGrid
            categories={expense}
            onEdit={openEdit}
            onDelete={setDeleting}
            onNew={() => openNew('Expense')}
          />
        </TabsContent>

        <TabsContent value="income" className="mt-4">
          <CategoryGrid
            categories={income}
            onEdit={openEdit}
            onDelete={setDeleting}
            onNew={() => openNew('Income')}
          />
        </TabsContent>
      </Tabs>

      {/* Dialog */}
      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o) reset(); }}>
        <DialogContent className="sm:max-w-sm">
          <DialogHeader>
            <DialogTitle>{editing ? 'Editar categoria' : 'Nova categoria'}</DialogTitle>
          </DialogHeader>
          <form id="cat-form" onSubmit={submit} className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label htmlFor="cat-name">Nome</Label>
              <Input id="cat-name" value={form.name} onChange={setEv('name')} required />
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="cat-type">Tipo</Label>
              <select
                id="cat-type"
                value={form.type}
                onChange={(e) => setForm((f) => ({ ...f, type: e.target.value }))}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
              >
                {categoryTypes.map(([val, label]) => (
                  <option key={val} value={val}>{label}</option>
                ))}
              </select>
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="cat-icon">Ícone</Label>
              <select
                id="cat-icon"
                value={form.icon}
                onChange={(e) => setForm((f) => ({ ...f, icon: e.target.value }))}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
              >
                {categoryIcons.map((icon) => (
                  <option key={icon} value={icon}>{icon}</option>
                ))}
              </select>
            </div>

            <ColorPicker value={form.color} onChange={set('color')} />

            {error && <p className="text-sm text-destructive">{error}</p>}
          </form>
          <DialogFooter>
            <Button variant="outline" type="button" onClick={reset}>Cancelar</Button>
            <Button form="cat-form" type="submit" disabled={saving}>
              {saving ? 'Salvando...' : editing ? 'Salvar' : 'Criar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {deleting && (
        <ConfirmModal
          title="Excluir categoria?"
          description="Esta ação não pode ser desfeita."
          confirmLabel="Excluir"
          danger
          onCancel={() => setDeleting(null)}
          onConfirm={deleteCategory}
        />
      )}
    </Screen>
  );
}

function CategoryGrid({ categories, onEdit, onDelete, onNew }) {
  if (categories.length === 0) {
    return (
      <EmptyState
        icon={Tag}
        title="Nenhuma categoria"
        description="Crie uma nova categoria para organizar suas transações"
        actionLabel="Nova categoria"
        onAction={onNew}
      />
    );
  }

  return (
    <div className="grid grid-cols-1 gap-2 sm:grid-cols-2 lg:grid-cols-3">
      {categories.map((cat) => (
        <div
          key={cat.id}
          className="group flex items-center gap-3 rounded-lg border bg-card px-4 py-3 hover:shadow-card-hover transition-shadow"
        >
          <div
            className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-sm font-bold"
            style={{ background: `${cat.color}20`, color: cat.color }}
          >
            {cat.icon?.slice(0, 2) ?? '?'}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium truncate">{cat.name}</p>
            <p className="text-xs text-muted-foreground truncate">
              {cat.icon}
              {cat.isDefault && (
                <Badge variant="secondary" className="ml-1 text-[10px] py-0 px-1 h-4">padrão</Badge>
              )}
            </p>
          </div>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="h-7 w-7 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => onEdit(cat)}>Editar</DropdownMenuItem>
              {!cat.isDefault && (
                <DropdownMenuItem onClick={() => onDelete(cat)} className="text-destructive focus:text-destructive">
                  Excluir
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ))}
    </div>
  );
}
