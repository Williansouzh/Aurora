import { useEffect, useMemo, useState } from 'react';
import { RefreshCw, Search, UserPlus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/Select';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { cn, getInitials } from '@/lib/utils';

const ROLES = [
  ['User', 1],
  ['Support', 2],
  ['Admin', 3],
  ['SuperAdmin', 4],
];

const STATUSES = [
  ['Active', 1],
  ['Suspended', 2],
  ['Invited', 3],
  ['Deleted', 4],
];

const OVERRIDES = [
  ['Allow', 1],
  ['Deny', 2],
  ['Beta', 3],
  ['Readonly', 4],
];

export function AdminPage({ api }) {
  const [users, setUsers] = useState([]);
  const [plans, setPlans] = useState([]);
  const [modules, setModules] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [detail, setDetail] = useState(null);
  const [search, setSearch] = useState('');
  const [busy, setBusy] = useState(false);
  const [tab, setTab] = useState('users');

  const selectedUser = useMemo(
    () => users.find((user) => user.userId === selectedUserId),
    [users, selectedUserId]
  );

  const load = async () => {
    setBusy(true);
    try {
      const [nextUsers, nextPlans, nextModules] = await Promise.all([
        api.get(`/api/admin/users${search ? `?search=${encodeURIComponent(search)}` : ''}`),
        api.get('/api/admin/plans'),
        api.get('/api/admin/modules'),
      ]);
      setUsers(nextUsers);
      setPlans(nextPlans);
      setModules(nextModules);
      if (!selectedUserId && nextUsers[0]) setSelectedUserId(nextUsers[0].userId);
    } finally {
      setBusy(false);
    }
  };

  const loadDetail = async (userId) => {
    if (!userId) return;
    setDetail(await api.get(`/api/admin/users/${userId}`));
  };

  useEffect(() => { load().catch(() => {}); }, []);
  useEffect(() => { loadDetail(selectedUserId).catch(() => setDetail(null)); }, [selectedUserId]);

  const updateRole = async (role, status = selectedUser?.status ?? 1) => {
    await api.put(`/api/admin/users/${selectedUserId}/role`, { role, status, reason: 'admin-screen' });
    await load();
    await loadDetail(selectedUserId);
  };

  const updateStatus = async (status) => {
    await updateRole(selectedUser?.role ?? 1, status);
  };

  const updatePlan = async (planKey) => {
    await api.put(`/api/admin/users/${selectedUserId}/plan`, { planKey, status: 2, reason: 'admin-screen' });
    await load();
    await loadDetail(selectedUserId);
  };

  const setOverride = async (moduleKey, access) => {
    await api.put(`/api/admin/users/${selectedUserId}/modules/${moduleKey}`, {
      access,
      reason: 'admin-screen',
      expiresAt: null,
    });
    await load();
    await loadDetail(selectedUserId);
  };

  const removeOverride = async (moduleKey) => {
    await api.delete(`/api/admin/users/${selectedUserId}/modules/${moduleKey}`);
    await load();
    await loadDetail(selectedUserId);
  };

  const liveModules = modules.filter((m) => m.status === 1 || m.releaseStage === 4).length;
  const superAdmins = users.filter((u) => u.role === 4).length;

  const TABS = [
    ['users', 'Usuários'],
    ['plans', 'Planos'],
    ['modules', 'Módulos'],
    ['areas', 'Áreas de vida'],
    ['audit', 'Log de auditoria'],
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="font-display text-3xl text-foreground">Super Admin</h1>
          <p className="mt-1 text-sm text-mut2">Controle de acesso · planos · libere módulos um a um.</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={load} disabled={busy}>
            <RefreshCw className="h-4 w-4" /> Atualizar
          </Button>
          <Button><UserPlus className="h-4 w-4" /> Convidar usuário</Button>
        </div>
      </div>

      {/* KPI cards */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Kpi label="Total de usuários" value={users.length} />
        <Kpi label="Planos ativos" value={plans.length} />
        <Kpi label="Módulos no ar" value={`${liveModules}/${modules.length}`} />
        <Kpi label="Super admins" value={superAdmins} />
      </div>

      {/* Tab bar */}
      <div className="flex gap-1 border-b border-border">
        {TABS.map(([key, label]) => (
          <button
            key={key}
            onClick={() => setTab(key)}
            className={cn(
              'px-3 py-3 text-[13.5px] font-semibold transition-colors',
              tab === key ? 'border-b-2 border-primary text-primary' : 'text-mut2 hover:text-foreground'
            )}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === 'users' && (
        <div className="grid gap-4 lg:grid-cols-[1.7fr_1fr]">
          {/* Users table */}
          <div className="rounded-[14px] border border-border bg-card">
            <div className="flex items-center gap-2 border-b border-border p-3">
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-faint" />
                <Input value={search} onChange={(e) => setSearch(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && load()} placeholder="Buscar por nome ou email" className="pl-9" />
              </div>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border bg-secondary [&>th]:px-4 [&>th]:py-3 [&>th]:text-left [&>th]:text-[11px] [&>th]:font-semibold [&>th]:uppercase [&>th]:tracking-[0.1em] [&>th]:text-faint">
                    <th>Usuário</th>
                    <th className="hidden sm:table-cell">Plano</th>
                    <th>Função</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-line2">
                  {users.map((user) => (
                    <tr
                      key={user.userId}
                      onClick={() => setSelectedUserId(user.userId)}
                      className={cn('cursor-pointer transition-colors', selectedUserId === user.userId ? 'bg-accent' : 'hover:bg-secondary')}
                    >
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <Avatar className="h-8 w-8">
                            <AvatarFallback className="bg-accent text-[11px] text-primary">{getInitials(user.name)}</AvatarFallback>
                          </Avatar>
                          <div className="min-w-0">
                            <p className="truncate font-medium text-ink2">{user.name}</p>
                            <p className="truncate text-xs text-mut2">{user.email}</p>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3 hidden sm:table-cell text-mut2">{user.planName ?? '—'}</td>
                      <td className="px-4 py-3 text-mut2">{roleName(user.role)}</td>
                      <td className="px-4 py-3"><StatusBadge status={user.status} /></td>
                    </tr>
                  ))}
                  {users.length === 0 && (
                    <tr><td colSpan={4} className="px-4 py-8 text-center text-sm text-faint">{busy ? 'Carregando…' : 'Nenhum usuário.'}</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* User detail panel */}
          <div className="space-y-4">
            <div className="rounded-[14px] border border-border bg-card p-5">
              <p className="text-overline mb-3">Configuração</p>
              {selectedUser ? (
                <div className="space-y-3">
                  <Field label="Plano">
                    <Select value={selectedUser.planKey ?? ''} onValueChange={updatePlan}>
                      <SelectTrigger><SelectValue placeholder="Plano" /></SelectTrigger>
                      <SelectContent>
                        {plans.map((plan) => <SelectItem key={plan.key} value={plan.key}>{plan.name}</SelectItem>)}
                      </SelectContent>
                    </Select>
                  </Field>
                  <div className="grid grid-cols-2 gap-3">
                    <Field label="Função">
                      <Select value={String(selectedUser.role)} onValueChange={(value) => updateRole(Number(value))}>
                        <SelectTrigger><SelectValue /></SelectTrigger>
                        <SelectContent>
                          {ROLES.map(([label, value]) => <SelectItem key={value} value={String(value)}>{label}</SelectItem>)}
                        </SelectContent>
                      </Select>
                    </Field>
                    <Field label="Status">
                      <Select value={String(selectedUser.status)} onValueChange={(value) => updateStatus(Number(value))}>
                        <SelectTrigger><SelectValue /></SelectTrigger>
                        <SelectContent>
                          {STATUSES.map(([label, value]) => <SelectItem key={value} value={String(value)}>{label}</SelectItem>)}
                        </SelectContent>
                      </Select>
                    </Field>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-faint">Selecione um usuário.</p>
              )}
            </div>

            {/* Module access matrix */}
            <div className="rounded-[14px] border border-border bg-card p-5">
              <p className="text-overline mb-3">Acesso a módulos</p>
              <div className="space-y-1.5">
                {(detail?.modules ?? modules).map((module) => {
                  const override = detail?.overrides?.find((item) => item.moduleKey === module.key);
                  const final = module.isAllowed ? (override ? 'Beta ⚑' : module.isReadonly ? 'Leitura' : 'Liberado') : 'Bloqueado';
                  const finalCls = module.isAllowed
                    ? (override ? 'text-pending bg-pending-soft' : 'text-income bg-income-soft')
                    : 'text-faint bg-muted';
                  return (
                    <div key={module.key} className="flex items-center gap-2 rounded-lg px-2 py-2 hover:bg-secondary">
                      <span className="flex-1 truncate text-sm text-ink2">{module.productName ?? module.name}</span>
                      <span className={cn('shrink-0 rounded-md px-2 py-0.5 text-[11px] font-medium', finalCls)}>{final}</span>
                      <Select value={override ? String(override.access) : ''} onValueChange={(value) => setOverride(module.key, Number(value))}>
                        <SelectTrigger className="h-7 w-[104px] text-xs"><SelectValue placeholder="Override" /></SelectTrigger>
                        <SelectContent>
                          {OVERRIDES.map(([label, value]) => <SelectItem key={value} value={String(value)}>{label}</SelectItem>)}
                        </SelectContent>
                      </Select>
                      <Button variant="ghost" size="sm" className="h-7 px-2 text-xs" onClick={() => removeOverride(module.key)} disabled={!override}>
                        Limpar
                      </Button>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
        </div>
      )}

      {tab === 'plans' && (
        <div className="grid gap-4 sm:grid-cols-2">
          {plans.map((plan) => (
            <div key={plan.key} className="rounded-[14px] border border-border bg-card p-5">
              <div className="flex items-center justify-between gap-2">
                <div>
                  <p className="text-[15px] font-semibold text-ink2">{plan.name}</p>
                  <p className="text-xs text-faint">{plan.key}</p>
                </div>
                <span className="rounded-full border border-chipline px-2.5 py-0.5 text-xs text-mut2">{plan.moduleKeys?.length ?? 0} módulos</span>
              </div>
              <div className="mt-3 flex flex-wrap gap-1.5">
                {plan.moduleKeys?.map((key) => (
                  <span key={key} className="rounded-md bg-secondary px-2 py-0.5 text-[11px] text-mut2">{key}</span>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      {(tab === 'modules' || tab === 'areas' || tab === 'audit') && (
        <div className="rounded-[14px] border border-dashed border-chipline bg-card p-12 text-center">
          <p className="font-display text-xl text-foreground">Em breve</p>
          <p className="mx-auto mt-1 max-w-sm text-sm text-faint">
            Esta aba ainda será construída. O catálogo de módulos, áreas de vida e o log de auditoria entram nas próximas iterações.
          </p>
        </div>
      )}
    </div>
  );
}

function Kpi({ label, value }) {
  return (
    <div className="rounded-[14px] border border-border bg-card p-5">
      <p className="text-xs font-medium text-mut2">{label}</p>
      <p className="mt-1 font-numeral text-[30px] leading-none text-foreground">{value}</p>
    </div>
  );
}

const STATUS_BADGE = {
  1: { label: 'Ativo', cls: 'text-income border-income/40' },
  2: { label: 'Suspenso', cls: 'text-expense border-expense/40' },
  3: { label: 'Convidado', cls: 'text-pending border-pending/40' },
  4: { label: 'Removido', cls: 'text-faint border-chipline' },
};

function StatusBadge({ status }) {
  const s = STATUS_BADGE[status] ?? STATUS_BADGE[1];
  return <span className={cn('inline-flex items-center rounded-full border px-2.5 py-0.5 text-[11px] font-medium', s.cls)}>{s.label}</span>;
}

function Field({ label, children }) {
  return (
    <label className="block space-y-1">
      <span className="text-xs font-medium text-mut2">{label}</span>
      {children}
    </label>
  );
}

function roleName(value) {
  return ROLES.find(([, role]) => role === value)?.[0] ?? 'User';
}
