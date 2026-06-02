import { useEffect, useMemo, useState } from 'react';
import { Shield, RefreshCw, Save, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/Select';

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

  return (
    <div className="space-y-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
            <Shield className="h-6 w-6" /> Super Admin
          </h1>
          <p className="text-sm text-muted-foreground">Controle usuários, planos e liberação gradual dos módulos.</p>
        </div>
        <Button onClick={load} disabled={busy} variant="outline">
          <RefreshCw className="mr-2 h-4 w-4" /> Atualizar
        </Button>
      </div>

      <div className="grid gap-4 lg:grid-cols-[340px_1fr]">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Usuários</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex gap-2">
              <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Buscar por nome ou email" />
              <Button size="icon" variant="outline" onClick={load}><Search className="h-4 w-4" /></Button>
            </div>

            <div className="space-y-2">
              {users.map((user) => (
                <button
                  key={user.userId}
                  onClick={() => setSelectedUserId(user.userId)}
                  className={`w-full rounded-md border p-3 text-left transition-colors ${
                    selectedUserId === user.userId ? 'border-primary bg-primary/5' : 'border-border hover:bg-muted'
                  }`}
                >
                  <div className="font-medium">{user.name}</div>
                  <div className="text-xs text-muted-foreground">{user.email}</div>
                  <div className="mt-2 flex gap-2">
                    <Badge variant="secondary">{user.planName ?? 'Sem plano'}</Badge>
                    <Badge variant="outline">{roleName(user.role)}</Badge>
                  </div>
                </button>
              ))}
            </div>
          </CardContent>
        </Card>

        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Configuração do usuário</CardTitle>
            </CardHeader>
            <CardContent>
              {selectedUser ? (
                <div className="grid gap-3 md:grid-cols-3">
                  <Field label="Plano">
                    <Select value={selectedUser.planKey ?? ''} onValueChange={updatePlan}>
                      <SelectTrigger><SelectValue placeholder="Plano" /></SelectTrigger>
                      <SelectContent>
                        {plans.map((plan) => <SelectItem key={plan.key} value={plan.key}>{plan.name}</SelectItem>)}
                      </SelectContent>
                    </Select>
                  </Field>
                  <Field label="Role">
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
              ) : (
                <p className="text-sm text-muted-foreground">Selecione um usuário.</p>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Módulos do usuário</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <div className="min-w-[760px] space-y-2">
                  {(detail?.modules ?? modules).map((module) => {
                    const override = detail?.overrides?.find((item) => item.moduleKey === module.key);
                    return (
                      <div key={module.key} className="grid grid-cols-[1.4fr_0.8fr_0.8fr_1fr_auto] items-center gap-3 rounded-md border p-3">
                        <div>
                          <div className="font-medium">{module.productName ?? module.name}</div>
                          <div className="text-xs text-muted-foreground">{module.key}</div>
                        </div>
                        <Badge variant={module.isAllowed ? 'default' : 'secondary'}>
                          {module.isAllowed ? (module.isReadonly ? 'Somente leitura' : 'Liberado') : 'Bloqueado'}
                        </Badge>
                        <span className="text-xs text-muted-foreground">{module.reason ?? module.releaseStage}</span>
                        <Select
                          value={override ? String(override.access) : ''}
                          onValueChange={(value) => setOverride(module.key, Number(value))}
                        >
                          <SelectTrigger><SelectValue placeholder="Override" /></SelectTrigger>
                          <SelectContent>
                            {OVERRIDES.map(([label, value]) => <SelectItem key={value} value={String(value)}>{label}</SelectItem>)}
                          </SelectContent>
                        </Select>
                        <Button variant="ghost" size="sm" onClick={() => removeOverride(module.key)} disabled={!override}>
                          Limpar
                        </Button>
                      </div>
                    );
                  })}
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Planos e módulos globais</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-3 md:grid-cols-2">
              {plans.map((plan) => (
                <div key={plan.key} className="rounded-md border p-3">
                  <div className="flex items-center justify-between gap-2">
                    <div>
                      <div className="font-medium">{plan.name}</div>
                      <div className="text-xs text-muted-foreground">{plan.key}</div>
                    </div>
                    <Badge variant="outline">{plan.moduleKeys?.length ?? 0} módulos</Badge>
                  </div>
                  <div className="mt-2 flex flex-wrap gap-1">
                    {plan.moduleKeys?.map((key) => <Badge key={key} variant="secondary">{key}</Badge>)}
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

function Field({ label, children }) {
  return (
    <label className="space-y-1">
      <span className="text-xs font-medium text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}

function roleName(value) {
  return ROLES.find(([, role]) => role === value)?.[0] ?? 'User';
}
