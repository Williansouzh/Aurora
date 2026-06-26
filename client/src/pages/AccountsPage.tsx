import { Archive, CreditCard, MoreHorizontal, Plus, Wallet } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ColorPicker } from '../components/ui/ColorPicker';
import { ConfirmModal } from '../components/ui/ConfirmModal';
import { Button } from '../components/ui/button';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator, DropdownMenuTrigger } from '../components/ui/dropdown-menu';
import { EmptyState } from '../components/ui/EmptyState';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Screen } from '../components/ui/Screen';
import { Skeleton } from '../components/ui/Skeleton';
import { accountTypes, colors } from '../constants/financeOptions';
import { useData } from '../hooks/useData';
import { useToast } from '../hooks/useToast';
import { cn, formatCurrency } from '../lib/utils';
import { enumLabel, enumValue } from '../utils/enumHelpers';

const initialForm = {
  name: '',
  type: 'CheckingAccount',
  initialBalance: 0,
  color: colors[0],
  isArchived: false,
  creditLimit: 0,
  closingDay: 10,
  dueDay: 15,
};

const typeIcons = {
  0: Wallet,
  1: Wallet,
  2: Wallet,
  3: Wallet,
  4: CreditCard,
};

export function AccountsPage({ api }) {
  const accounts = useData(() => api.get('/api/accounts'), []);
  const toast = useToast();
  const [form, setForm] = useState(initialForm);
  const [editing, setEditing] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleting, setDeleting] = useState(null);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const set = (field) => (val) => setForm((f) => ({ ...f, [field]: val }));
  const setEv = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  const openNew = () => {
    setEditing(null);
    setForm(initialForm);
    setError('');
    setDialogOpen(true);
  };

  const openEdit = (account) => {
    setEditing(account);
    setForm({
      name: account.name,
      type: accountTypes[account.type]?.[0] || initialForm.type,
      initialBalance: account.initialBalance,
      color: account.color,
      isArchived: account.isArchived,
      creditLimit: account.creditLimit || 0,
      closingDay: account.closingDay || 10,
      dueDay: account.dueDay || 15,
    });
    setError('');
    setDialogOpen(true);
  };

  const reset = () => { setDialogOpen(false); setEditing(null); setForm(initialForm); setError(''); };

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    const payload = {
      ...form,
      type: enumValue(accountTypes, form.type),
      initialBalance: Number(form.initialBalance),
      creditLimit: Number(form.creditLimit || 0),
      closingDay: Number(form.closingDay || 10),
      dueDay: Number(form.dueDay || 15),
    };
    try {
      if (editing) {
        await api.put(`/api/accounts/${editing.id}`, { ...payload, id: editing.id, userId: '', isArchived: !!form.isArchived });
      } else {
        await api.post('/api/accounts', { ...payload, userId: '' });
      }
      reset();
      accounts.reload();
      toast.success(editing ? 'Conta atualizada' : 'Conta criada');
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const archiveAccount = async (account) => {
    try {
      await api.patch(`/api/accounts/${account.id}/archive`);
      await accounts.reload();
      toast.success(account.isArchived ? 'Conta reativada' : 'Conta arquivada');
    } catch (err) {
      toast.error(err.message);
    }
  };

  const deleteAccount = async () => {
    if (!deleting) return;
    try {
      await api.delete(`/api/accounts/${deleting.id}`);
      setDeleting(null);
      await accounts.reload();
      toast.success('Conta removida');
    } catch (err) {
      toast.error(err.message);
    }
  };

  const isCreditCard = form.type === 'CreditCard';
  const activeAccounts = (accounts.data || []).filter((a) => !a.isArchived);
  const archivedAccounts = (accounts.data || []).filter((a) => a.isArchived);
  const totalBalance = activeAccounts.reduce((s, a) => s + (a.type !== 4 ? (a.currentBalance ?? 0) : 0), 0);

  return (
    <Screen
      title="Contas"
      subtitle={`${activeAccounts.length} ${activeAccounts.length === 1 ? 'ativa' : 'ativas'} · ${archivedAccounts.length} ${archivedAccounts.length === 1 ? 'arquivada' : 'arquivadas'} · ${formatCurrency(totalBalance)} total`}
      actions={<Button onClick={openNew}><Plus className="h-4 w-4" /> Nova conta</Button>}
      loading={accounts.loading}
      error={accounts.error}
    >
      {/* Account cards */}
      {activeAccounts.length === 0 && !accounts.loading ? (
        <EmptyState
          icon={Wallet}
          title="Nenhuma conta cadastrada"
          description="Crie sua primeira conta para começar a registrar transações"
          actionLabel="Nova conta"
          onAction={openNew}
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {activeAccounts.map((account) => (
            <AccountCard
              key={account.id}
              account={account}
              onEdit={() => openEdit(account)}
              onArchive={() => archiveAccount(account)}
              onDelete={() => setDeleting(account)}
            />
          ))}
          {/* Dashed "add account" tile */}
          <button
            onClick={openNew}
            className="flex min-h-[140px] flex-col items-center justify-center gap-2 rounded-[14px] border-2 border-dashed border-chipline bg-transparent text-mut2 transition-colors hover:border-primary/40 hover:text-primary"
          >
            <Plus className="h-5 w-5" />
            <span className="text-sm font-medium">Adicionar conta</span>
          </button>
        </div>
      )}

      {/* Archived */}
      {archivedAccounts.length > 0 && (
        <div className="space-y-3">
          <p className="text-sm font-medium text-muted-foreground flex items-center gap-2">
            <Archive className="h-4 w-4" />
            Contas arquivadas ({archivedAccounts.length})
          </p>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3 opacity-60">
            {archivedAccounts.map((account) => (
              <AccountCard
                key={account.id}
                account={account}
                onEdit={() => openEdit(account)}
                onArchive={() => archiveAccount(account)}
                onDelete={() => setDeleting(account)}
              />
            ))}
          </div>
        </div>
      )}

      {/* Account Form Dialog */}
      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o) reset(); }}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{editing ? 'Editar conta' : 'Nova conta'}</DialogTitle>
          </DialogHeader>
          <form id="account-form" onSubmit={submit} className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label htmlFor="acc-name">Nome</Label>
              <Input id="acc-name" value={form.name} onChange={setEv('name')} required />
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="acc-type">Tipo</Label>
              <select
                id="acc-type"
                value={form.type}
                onChange={(e) => setForm((f) => ({ ...f, type: e.target.value }))}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
              >
                {accountTypes.map(([val, label]) => (
                  <option key={val} value={val}>{label}</option>
                ))}
              </select>
            </div>

            {!isCreditCard && (
              <div className="space-y-1.5">
                <Label htmlFor="acc-balance">Saldo inicial</Label>
                <Input id="acc-balance" type="number" step="0.01" value={form.initialBalance} onChange={setEv('initialBalance')} />
              </div>
            )}

            {isCreditCard && (
              <div className="grid grid-cols-3 gap-3">
                <div className="space-y-1.5">
                  <Label htmlFor="acc-limit">Limite</Label>
                  <Input id="acc-limit" type="number" step="0.01" min="0" value={form.creditLimit} onChange={setEv('creditLimit')} required />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="acc-closing">Fecha dia</Label>
                  <Input id="acc-closing" type="number" min="1" max="28" value={form.closingDay} onChange={setEv('closingDay')} required />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="acc-due">Vence dia</Label>
                  <Input id="acc-due" type="number" min="1" max="28" value={form.dueDay} onChange={setEv('dueDay')} required />
                </div>
              </div>
            )}

            <ColorPicker value={form.color} onChange={set('color')} />

            {editing && (
              <label className="flex items-center gap-2 text-sm cursor-pointer">
                <input
                  type="checkbox"
                  checked={!!form.isArchived}
                  onChange={(e) => setForm((f) => ({ ...f, isArchived: e.target.checked }))}
                  className="h-4 w-4 rounded border-input"
                />
                Conta arquivada
              </label>
            )}

            {error && (
              <p className="text-sm text-destructive">{error}</p>
            )}
          </form>
          <DialogFooter>
            <Button variant="outline" type="button" onClick={reset}>Cancelar</Button>
            <Button form="account-form" type="submit" disabled={saving}>
              {saving ? 'Salvando...' : editing ? 'Salvar' : 'Criar conta'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {deleting && (
        <ConfirmModal
          title="Excluir conta?"
          description="Todas as transações vinculadas serão afetadas. Esta ação não pode ser desfeita."
          confirmLabel="Excluir"
          danger
          onCancel={() => setDeleting(null)}
          onConfirm={deleteAccount}
        />
      )}
    </Screen>
  );
}

function AccountCard({ account, onEdit, onArchive, onDelete }) {
  const Icon = typeIcons[account.type] ?? Wallet;
  const isCredit = account.type === 4;
  const balance = isCredit ? account.availableLimit : account.currentBalance;
  const delta = (account.currentBalance ?? 0) - (account.initialBalance ?? 0);
  const showDelta = !isCredit && Math.abs(delta) > 0.005;

  return (
    <div className="relative overflow-hidden rounded-[14px] border border-border bg-card transition-shadow hover:shadow-card-hover">
      {/* 3px top border in the account's color */}
      <div className="h-[3px] w-full" style={{ background: account.color }} />
      <div className="p-5">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg" style={{ background: `${account.color}20`, color: account.color }}>
              <Icon className="h-4 w-4" />
            </div>
            <div>
              <p className="text-[15px] font-semibold text-ink2">{account.name}</p>
              <p className="text-xs text-mut2">{enumLabel(accountTypes, account.type)}</p>
            </div>
          </div>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="h-7 w-7 shrink-0">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {isCredit && (
                <DropdownMenuItem asChild>
                  <Link to={`/accounts/${account.id}/invoices`}>Ver fatura</Link>
                </DropdownMenuItem>
              )}
              <DropdownMenuItem onClick={onEdit}>Editar</DropdownMenuItem>
              <DropdownMenuItem onClick={onArchive}>
                {account.isArchived ? 'Reativar' : 'Arquivar'}
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={onDelete} className="text-destructive focus:text-destructive">
                Excluir
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        <p className={cn('mt-4 font-numeral text-[28px] leading-none', balance >= 0 ? 'text-foreground' : 'text-expense')}>
          {formatCurrency(balance)}
        </p>
        <div className="mt-1.5 flex items-center gap-2 text-xs">
          <span className="text-faint">{isCredit ? 'Limite disponível' : 'Saldo atual'}</span>
          {showDelta && (
            <span className={cn('font-medium', delta >= 0 ? 'text-income' : 'text-expense')}>
              {delta >= 0 ? '▲' : '▼'} {formatCurrency(Math.abs(delta))}
            </span>
          )}
        </div>

        {isCredit && (
          <div className="mt-3 space-y-1.5 border-t border-line2 pt-3 text-xs text-mut2">
            <div className="flex justify-between">
              <span>Fatura atual</span>
              <span className="font-medium text-foreground">{formatCurrency(account.currentInvoiceAmount)}</span>
            </div>
            <div className="flex justify-between">
              <span>Limite total</span>
              <span className="font-medium text-foreground">{formatCurrency(account.creditLimit)}</span>
            </div>
            <div className="text-faint">Fecha dia {account.closingDay} · Vence dia {account.dueDay}</div>
          </div>
        )}
      </div>
    </div>
  );
}
