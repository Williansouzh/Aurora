import { Eye, EyeOff, ShieldCheck, Zap } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';

const initialForm = { name: '', email: '', password: '' };

export function AuthPage({ mode, api, onAuth }) {
  const navigate = useNavigate();
  const isRegister = mode === 'register';
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [mfa, setMfa] = useState(null);
  const [mfaCode, setMfaCode] = useState('');

  const set = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const payload = isRegister ? form : { email: form.email, password: form.password };
      const auth = await api.post(isRegister ? '/api/auth/register' : '/api/auth/login', payload);

      if (auth?.mfaRequired) {
        setMfa(auth);
        setMfaCode('');
        return;
      }

      onAuth(auth);
      navigate('/', { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const verifyMfa = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const auth = await api.post('/api/auth/mfa/verify', {
        challengeId: mfa.challengeId,
        code: mfaCode,
      });
      onAuth(auth);
      navigate('/', { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const resendMfa = async () => {
    setError('');
    setLoading(true);
    try {
      const result = await api.post('/api/auth/mfa/resend', { challengeId: mfa.challengeId });
      setMfa((current) => ({ ...current, challengeId: result.challengeId }));
      setMfaCode('');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-background flex">
      <div className="hidden lg:flex lg:w-1/2 bg-primary flex-col items-center justify-center p-12 text-primary-foreground">
        <div className="max-w-sm space-y-6 text-center">
          <div className="flex items-center justify-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-white/20">
              <Zap className="h-6 w-6 text-white" />
            </div>
            <span className="text-3xl font-bold">Aurora</span>
          </div>
          <p className="text-xl font-semibold leading-snug text-white/90">
            Controle financeiro simples, visual e inteligente
          </p>
          <p className="text-sm text-white/70 leading-relaxed">
            Gerencie contas, orcamentos, transacoes e financiamentos em um so lugar.
          </p>
          <div className="grid grid-cols-2 gap-3 pt-4">
            {['Contas e saldos', 'Orcamentos', 'Transacoes', 'Financiamentos'].map((f) => (
              <div key={f} className="rounded-lg bg-white/10 px-3 py-2 text-sm font-medium text-white/90">
                {f}
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="flex flex-1 items-center justify-center p-6">
        <div className="w-full max-w-sm space-y-8">
          <div className="flex items-center gap-2 lg:hidden">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary">
              <Zap className="h-4 w-4 text-primary-foreground" />
            </div>
            <span className="text-xl font-bold text-foreground">Aurora</span>
          </div>

          <div className="space-y-2">
            <h1 className="text-2xl font-bold text-foreground">
              {mfa ? 'Verificar acesso' : isRegister ? 'Criar conta' : 'Entrar na conta'}
            </h1>
            <p className="text-sm text-muted-foreground">
              {mfa
                ? 'Insira o codigo enviado para seu e-mail'
                : isRegister
                  ? 'Preencha os dados para comecar a usar o Aurora'
                  : 'Insira suas credenciais para acessar'}
            </p>
          </div>

          {mfa ? (
            <form className="space-y-4" onSubmit={verifyMfa}>
              <div className="flex items-center gap-3 rounded-md border border-border bg-muted/40 px-3 py-3">
                <ShieldCheck className="h-5 w-5 text-primary" />
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground">Verificacao em duas etapas</p>
                  <p className="text-xs text-muted-foreground">Codigo de 6 digitos enviado por e-mail.</p>
                </div>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="mfa-code">Codigo</Label>
                <Input
                  id="mfa-code"
                  inputMode="numeric"
                  pattern="[0-9]{6}"
                  maxLength={6}
                  placeholder="000000"
                  value={mfaCode}
                  onChange={(e) => setMfaCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                  required
                />
              </div>

              {error && (
                <div className="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {error}
                </div>
              )}

              <Button type="submit" className="w-full" disabled={loading || mfaCode.length !== 6}>
                {loading ? 'Aguarde...' : 'Verificar codigo'}
              </Button>
              <button
                type="button"
                onClick={resendMfa}
                disabled={loading}
                className="w-full text-center text-sm font-medium text-primary hover:underline border-0 bg-transparent p-0 min-h-0 cursor-pointer"
              >
                Reenviar codigo
              </button>
              <button
                type="button"
                onClick={() => { setMfa(null); setMfaCode(''); setError(''); }}
                className="w-full text-center text-sm text-muted-foreground hover:text-foreground border-0 bg-transparent p-0 min-h-0 cursor-pointer"
              >
                Voltar
              </button>
            </form>
          ) : (
            <form className="space-y-4" onSubmit={submit}>
              {isRegister && (
                <div className="space-y-1.5">
                  <Label htmlFor="name">Nome completo</Label>
                  <Input
                    id="name"
                    placeholder="Seu nome"
                    value={form.name}
                    onChange={set('name')}
                    required
                  />
                </div>
              )}

              <div className="space-y-1.5">
                <Label htmlFor="email">E-mail</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="voce@email.com"
                  value={form.email}
                  onChange={set('email')}
                  required
                />
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="password">Senha</Label>
                <div className="relative">
                  <Input
                    id="password"
                    type={showPassword ? 'text' : 'password'}
                    placeholder="**********"
                    value={form.password}
                    onChange={set('password')}
                    minLength={10}
                    required
                    className="pr-10"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((s) => !s)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors border-0 bg-transparent p-0 min-h-0"
                  >
                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
              </div>

              {error && (
                <div className="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {error}
                </div>
              )}

              <Button type="submit" className="w-full" disabled={loading}>
                {loading ? 'Aguarde...' : isRegister ? 'Criar conta' : 'Entrar'}
              </Button>
            </form>
          )}

          {!mfa && (
            <div className="space-y-3 text-center text-sm text-muted-foreground">
              {!isRegister && (
                <button
                  type="button"
                  onClick={() => navigate('/forgot-password')}
                  className="font-medium text-primary hover:underline border-0 bg-transparent p-0 min-h-0 cursor-pointer"
                >
                  Esqueci minha senha
                </button>
              )}
              <p>
                {isRegister ? 'Ja tem conta?' : 'Nao tem conta?'}{' '}
                <button
                  type="button"
                  onClick={() => navigate(isRegister ? '/login' : '/register')}
                  className="font-medium text-primary hover:underline border-0 bg-transparent p-0 min-h-0 cursor-pointer"
                >
                  {isRegister ? 'Entrar' : 'Criar conta'}
                </button>
              </p>
              <button
                type="button"
                onClick={() => navigate('/confirm-email')}
                className="font-medium text-primary hover:underline border-0 bg-transparent p-0 min-h-0 cursor-pointer"
              >
                Confirmar e-mail
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
