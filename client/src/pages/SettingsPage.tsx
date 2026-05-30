import { Bell, Download, KeyRound, LogOut, Settings, Shield, SlidersHorizontal, Trash2, User } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Avatar, AvatarFallback } from '../components/ui/avatar';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Screen } from '../components/ui/Screen';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { useProfile } from '../hooks/useProfile';
import { useToast } from '../hooks/useToast';
import { cn, getInitials } from '../lib/utils';
import { getPreferences, savePreferences } from '../services/preferencesStorage';

export function SettingsPage({ api, user, onProfileUpdated, onSignOut }) {
  const toast = useToast();
  const { profile, loading, updateProfile, updatePassword } = useProfile({ api, initialUser: user });

  return (
    <Screen title="Configurações">
      <Tabs defaultValue="profile" className="space-y-4">
        <TabsList>
          <TabsTrigger value="profile" className="gap-2">
            <User className="h-3.5 w-3.5" />Perfil
          </TabsTrigger>
          <TabsTrigger value="security" className="gap-2">
            <Shield className="h-3.5 w-3.5" />Segurança
          </TabsTrigger>
          <TabsTrigger value="preferences" className="gap-2">
            <SlidersHorizontal className="h-3.5 w-3.5" />Preferências
          </TabsTrigger>
          <TabsTrigger value="notifications" className="gap-2">
            <Bell className="h-3.5 w-3.5" />Notificações
          </TabsTrigger>
        </TabsList>

        <TabsContent value="profile">
          <ProfileTab
            profile={profile}
            loading={loading}
            updateProfile={updateProfile}
            onSuccess={(updated) => { toast.success('Perfil atualizado'); onProfileUpdated?.(updated); }}
            onError={(msg) => toast.error(msg)}
          />
        </TabsContent>

        <TabsContent value="security">
          <SecurityTab api={api} updatePassword={updatePassword} toast={toast} onSignOut={onSignOut} />
        </TabsContent>

        <TabsContent value="preferences">
          <PreferencesTab toast={toast} />
        </TabsContent>

        <TabsContent value="notifications">
          <NotificationsTab api={api} toast={toast} />
        </TabsContent>
      </Tabs>
    </Screen>
  );
}

function NotificationsTab({ api, toast }) {
  const [form, setForm] = useState({
    habitReminderEnabled: false,
    habitReminderHour: 20,
    weeklyPlanningReminderEnabled: false,
    weeklyPlanningReminderHour: 8,
  });
  const [saving, setSaving] = useState(false);

  const toggle = (k) => setForm((f) => ({ ...f, [k]: !f[k] }));
  const setHour = (k, v) => setForm((f) => ({ ...f, [k]: parseInt(v) || 0 }));

  const save = async () => {
    setSaving(true);
    try {
      await api.put('/api/auth/notifications', form);
      toast.success('Preferências de notificação salvas.');
    } catch (err) {
      toast.error(err.message);
    } finally { setSaving(false); }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base flex items-center gap-2">
          <Bell className="h-4 w-4" /> Notificações por email
        </CardTitle>
        <CardDescription>Configure lembretes automáticos. Requer SMTP ativo.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium">Lembrete de hábitos</p>
              <p className="text-xs text-muted-foreground">Receba um email quando houver hábitos pendentes no dia.</p>
            </div>
            <button
              onClick={() => toggle('habitReminderEnabled')}
              className={`relative inline-flex h-5 w-9 items-center rounded-full transition-colors ${form.habitReminderEnabled ? 'bg-primary' : 'bg-muted-foreground/30'}`}
            >
              <span className={`inline-block h-3.5 w-3.5 transform rounded-full bg-white shadow transition-transform ${form.habitReminderEnabled ? 'translate-x-4.5' : 'translate-x-0.5'}`} />
            </button>
          </div>
          {form.habitReminderEnabled && (
            <div className="flex items-center gap-2 pl-1">
              <label className="text-xs text-muted-foreground">Horário (UTC):</label>
              <input
                type="number"
                min="0" max="23"
                value={form.habitReminderHour}
                onChange={(e) => setHour('habitReminderHour', e.target.value)}
                className="w-16 h-7 rounded-md border border-input bg-background px-2 text-sm"
              />
              <span className="text-xs text-muted-foreground">h</span>
            </div>
          )}
        </div>

        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium">Lembrete de planejamento semanal</p>
              <p className="text-xs text-muted-foreground">Receba um email toda segunda sem plano da semana.</p>
            </div>
            <button
              onClick={() => toggle('weeklyPlanningReminderEnabled')}
              className={`relative inline-flex h-5 w-9 items-center rounded-full transition-colors ${form.weeklyPlanningReminderEnabled ? 'bg-primary' : 'bg-muted-foreground/30'}`}
            >
              <span className={`inline-block h-3.5 w-3.5 transform rounded-full bg-white shadow transition-transform ${form.weeklyPlanningReminderEnabled ? 'translate-x-4.5' : 'translate-x-0.5'}`} />
            </button>
          </div>
          {form.weeklyPlanningReminderEnabled && (
            <div className="flex items-center gap-2 pl-1">
              <label className="text-xs text-muted-foreground">Horário (UTC):</label>
              <input
                type="number"
                min="0" max="23"
                value={form.weeklyPlanningReminderHour}
                onChange={(e) => setHour('weeklyPlanningReminderHour', e.target.value)}
                className="w-16 h-7 rounded-md border border-input bg-background px-2 text-sm"
              />
              <span className="text-xs text-muted-foreground">h</span>
            </div>
          )}
        </div>

        <Button onClick={save} disabled={saving} className="w-full sm:w-auto">
          {saving ? 'Salvando...' : 'Salvar preferências'}
        </Button>
      </CardContent>
    </Card>
  );
}

function ProfileTab({ profile, loading, updateProfile, onSuccess, onError }) {
  const [name, setName] = useState(profile?.name || '');
  const [email, setEmail] = useState(profile?.email || '');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setName(profile?.name || '');
    setEmail(profile?.email || '');
  }, [profile?.name, profile?.email]);

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const updated = await updateProfile({ name: name.trim(), email: email.trim() });
      onSuccess?.(updated);
    } catch (err) {
      onError?.(err.message || 'Falha ao atualizar perfil.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card className="max-w-lg">
      <CardHeader>
        <div className="flex items-center gap-4">
          <Avatar className="h-16 w-16">
            <AvatarFallback className="text-xl font-bold bg-primary/10 text-primary">
              {getInitials(name || profile?.name)}
            </AvatarFallback>
          </Avatar>
          <div>
            <CardTitle>{name || 'Sem nome'}</CardTitle>
            <CardDescription>{email}</CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <form onSubmit={submit} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="profile-name">Nome</Label>
            <Input id="profile-name" value={name} onChange={(e) => setName(e.target.value)} required maxLength={120} />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="profile-email">E-mail</Label>
            <Input id="profile-email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </div>
          <Button type="submit" disabled={saving || loading}>
            {saving ? 'Salvando...' : 'Salvar alterações'}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}

function SecurityTab({ api, updatePassword, toast, onSignOut }) {
  const [form, setForm] = useState({ current: '', next: '', confirm: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (f) => (e) => setForm((p) => ({ ...p, [f]: e.target.value }));

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    if (form.next.length < 10) { setError('Nova senha deve ter ao menos 10 caracteres.'); return; }
    if (form.next !== form.confirm) { setError('As senhas não conferem.'); return; }
    setSaving(true);
    try {
      await updatePassword({ currentPassword: form.current, newPassword: form.next, confirmPassword: form.confirm });
      toast.success('Senha alterada. Faça login novamente.');
      setForm({ current: '', next: '', confirm: '' });
      window.setTimeout(() => onSignOut?.(), 2000);
    } catch (err) {
      setError(err.message || 'Falha ao alterar senha.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-6 max-w-lg">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <KeyRound className="h-4 w-4" /> Alterar senha
          </CardTitle>
          <CardDescription>Use ao menos 8 caracteres. Após alterar será necessário entrar novamente.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={submit} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="pw-current">Senha atual</Label>
              <Input id="pw-current" type="password" value={form.current} onChange={set('current')} required autoComplete="current-password" />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="pw-new">Nova senha</Label>
              <Input id="pw-new" type="password" value={form.next} onChange={set('next')} required minLength={10} autoComplete="new-password" />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="pw-confirm">Confirmar nova senha</Label>
              <Input id="pw-confirm" type="password" value={form.confirm} onChange={set('confirm')} required minLength={10} autoComplete="new-password" />
            </div>
            {error && <p className="text-sm text-destructive">{error}</p>}
            <Button type="submit" disabled={saving}>{saving ? 'Alterando...' : 'Alterar senha'}</Button>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Download className="h-4 w-4" /> Exportar dados
          </CardTitle>
          <CardDescription>Baixe uma copia dos dados pessoais e eventos de auditoria.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button
            variant="outline"
            onClick={async () => {
              try {
                const data = await api.get('/api/auth/me/export');
                const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
                const url = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = 'aurora-meus-dados.json';
                link.click();
                URL.revokeObjectURL(url);
              } catch (err) {
                toast.error(err.message || 'Falha ao exportar dados.');
              }
            }}
          >
            Exportar JSON
          </Button>
        </CardContent>
      </Card>

      <Card className="border-destructive/30">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base text-destructive">
            <LogOut className="h-4 w-4" /> Sair da conta
          </CardTitle>
          <CardDescription>Encerra sua sessão neste dispositivo.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button variant="destructive" onClick={onSignOut}>Sair agora</Button>
        </CardContent>
      </Card>

      <Card className="border-destructive/30">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base text-destructive">
            <Trash2 className="h-4 w-4" /> Excluir meus dados
          </CardTitle>
          <CardDescription>Anonimiza seus dados pessoais e revoga suas sessoes.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button
            variant="destructive"
            onClick={async () => {
              if (!window.confirm('Anonimizar sua conta? Esta acao nao pode ser desfeita.')) return;
              try {
                await api.delete('/api/auth/me', { reason: 'user-request' });
                await onSignOut?.();
              } catch (err) {
                toast.error(err.message || 'Falha ao excluir dados.');
              }
            }}
          >
            Anonimizar conta
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}

function PreferencesTab({ toast }) {
  const [prefs, setPrefs] = useState(getPreferences);

  const update = (patch) => {
    const next = { ...prefs, ...patch };
    setPrefs(next);
    savePreferences(next);
    toast.success('Preferências salvas');
  };

  return (
    <Card className="max-w-lg">
      <CardHeader>
        <CardTitle className="text-base">Preferências do app</CardTitle>
        <CardDescription>Armazenadas localmente neste dispositivo.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="pref-month">Mês inicial do dashboard</Label>
          <select
            id="pref-month"
            value={prefs.dashboardMonth}
            onChange={(e) => update({ dashboardMonth: e.target.value })}
            className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
          >
            <option value="current">Mês atual</option>
            <option value="last">Último mês visualizado</option>
          </select>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="pref-pagesize">Transações por página</Label>
          <select
            id="pref-pagesize"
            value={String(prefs.pageSize)}
            onChange={(e) => update({ pageSize: Number(e.target.value) })}
            className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
          >
            <option value="10">10 por página</option>
            <option value="20">20 por página</option>
            <option value="50">50 por página</option>
          </select>
        </div>
      </CardContent>
    </Card>
  );
}
