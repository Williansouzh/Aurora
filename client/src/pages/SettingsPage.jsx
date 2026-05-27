import { KeyRound, LogOut, Settings, Shield, SlidersHorizontal, User } from 'lucide-react';
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
          <SecurityTab updatePassword={updatePassword} toast={toast} onSignOut={onSignOut} />
        </TabsContent>

        <TabsContent value="preferences">
          <PreferencesTab toast={toast} />
        </TabsContent>
      </Tabs>
    </Screen>
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

function SecurityTab({ updatePassword, toast, onSignOut }) {
  const [form, setForm] = useState({ current: '', next: '', confirm: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (f) => (e) => setForm((p) => ({ ...p, [f]: e.target.value }));

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    if (form.next.length < 8) { setError('Nova senha deve ter ao menos 8 caracteres.'); return; }
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
              <Input id="pw-new" type="password" value={form.next} onChange={set('next')} required minLength={8} autoComplete="new-password" />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="pw-confirm">Confirmar nova senha</Label>
              <Input id="pw-confirm" type="password" value={form.confirm} onChange={set('confirm')} required minLength={8} autoComplete="new-password" />
            </div>
            {error && <p className="text-sm text-destructive">{error}</p>}
            <Button type="submit" disabled={saving}>{saving ? 'Alterando...' : 'Alterar senha'}</Button>
          </form>
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
