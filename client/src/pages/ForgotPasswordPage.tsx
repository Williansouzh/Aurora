import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';

export function ForgotPasswordPage({ api }) {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await api.post('/api/auth/forgot-password', { email });
      setMessage('Se o e-mail existir, enviaremos um token de redefinicao.');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout title="Esqueci minha senha" description="Solicite um token de redefinicao por e-mail.">
      <form className="space-y-4" onSubmit={submit}>
        <div className="space-y-1.5">
          <Label htmlFor="email">E-mail</Label>
          <Input id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        {message && <div className="rounded-md border border-emerald-500/30 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-700">{message}</div>}
        {error && <div className="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</div>}
        <Button className="w-full" disabled={loading}>{loading ? 'Aguarde...' : 'Enviar token'}</Button>
        <button type="button" onClick={() => navigate('/reset-password')} className="w-full text-sm font-medium text-primary hover:underline border-0 bg-transparent p-0 min-h-0">Ja tenho um token</button>
      </form>
    </AuthLayout>
  );
}

export function ResetPasswordPage({ api }) {
  const navigate = useNavigate();
  const [form, setForm] = useState({ token: '', newPassword: '', confirmPassword: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const set = (field) => (e) => setForm((current) => ({ ...current, [field]: e.target.value }));

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await api.post('/api/auth/reset-password', form);
      navigate('/login', { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout title="Redefinir senha" description="Informe o token recebido e escolha uma nova senha.">
      <form className="space-y-4" onSubmit={submit}>
        <Field id="token" label="Token" value={form.token} onChange={set('token')} />
        <Field id="newPassword" label="Nova senha" type="password" minLength={10} value={form.newPassword} onChange={set('newPassword')} />
        <Field id="confirmPassword" label="Confirmar senha" type="password" minLength={10} value={form.confirmPassword} onChange={set('confirmPassword')} />
        {error && <div className="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</div>}
        <Button className="w-full" disabled={loading}>{loading ? 'Aguarde...' : 'Redefinir senha'}</Button>
      </form>
    </AuthLayout>
  );
}

export function ConfirmEmailPage({ api }) {
  const navigate = useNavigate();
  const [token, setToken] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await api.post('/api/auth/confirm-email', { token });
      navigate('/login', { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout title="Confirmar e-mail" description="Cole o token recebido no cadastro.">
      <form className="space-y-4" onSubmit={submit}>
        <Field id="token" label="Token" value={token} onChange={(e) => setToken(e.target.value)} />
        {error && <div className="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</div>}
        <Button className="w-full" disabled={loading}>{loading ? 'Aguarde...' : 'Confirmar e-mail'}</Button>
      </form>
    </AuthLayout>
  );
}

function Field({ id, label, type = 'text', ...props }) {
  return (
    <div className="space-y-1.5">
      <Label htmlFor={id}>{label}</Label>
      <Input id={id} type={type} required {...props} />
    </div>
  );
}

function AuthLayout({ title, description, children }) {
  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-6">
      <div className="w-full max-w-sm space-y-8">
        <div className="space-y-2">
          <h1 className="text-2xl font-bold text-foreground">{title}</h1>
          <p className="text-sm text-muted-foreground">{description}</p>
        </div>
        {children}
      </div>
    </div>
  );
}
