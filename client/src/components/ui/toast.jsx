import * as React from 'react';
import { X, CheckCircle, AlertCircle, Info, AlertTriangle } from 'lucide-react';
import { cn } from '@/lib/utils';

const ToastContext = React.createContext(null);

let toastId = 0;

export function ToastProvider({ children }) {
  const [toasts, setToasts] = React.useState([]);

  const add = React.useCallback((toast) => {
    const id = ++toastId;
    setToasts((prev) => [...prev, { id, ...toast }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, toast.duration ?? 4000);
    return id;
  }, []);

  const remove = React.useCallback((id) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const toast = React.useMemo(
    () => ({
      success: (msg, opts) => add({ type: 'success', message: msg, ...opts }),
      error: (msg, opts) => add({ type: 'error', message: msg, ...opts }),
      warning: (msg, opts) => add({ type: 'warning', message: msg, ...opts }),
      info: (msg, opts) => add({ type: 'info', message: msg, ...opts }),
    }),
    [add]
  );

  return (
    <ToastContext.Provider value={toast}>
      {children}
      <Toaster toasts={toasts} onRemove={remove} />
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = React.useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within ToastProvider');
  return ctx;
}

const icons = {
  success: <CheckCircle className="h-4 w-4 text-emerald-500" />,
  error: <AlertCircle className="h-4 w-4 text-red-500" />,
  warning: <AlertTriangle className="h-4 w-4 text-amber-500" />,
  info: <Info className="h-4 w-4 text-blue-500" />,
};

const typeClasses = {
  success: 'border-emerald-200 bg-emerald-50 text-emerald-900',
  error: 'border-red-200 bg-red-50 text-red-900',
  warning: 'border-amber-200 bg-amber-50 text-amber-900',
  info: 'border-blue-200 bg-blue-50 text-blue-900',
};

function Toaster({ toasts, onRemove }) {
  if (!toasts.length) return null;
  return (
    <div className="fixed bottom-4 right-4 z-[100] flex flex-col gap-2 w-80">
      {toasts.map((t) => (
        <div
          key={t.id}
          className={cn(
            'flex items-start gap-3 rounded-lg border p-4 shadow-lg animate-fade-in',
            typeClasses[t.type] ?? 'border-border bg-background text-foreground'
          )}
        >
          {icons[t.type]}
          <p className="flex-1 text-sm font-medium">{t.message}</p>
          <button
            onClick={() => onRemove(t.id)}
            className="shrink-0 rounded-sm opacity-70 hover:opacity-100 transition-opacity border-0 bg-transparent p-0 min-h-0"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      ))}
    </div>
  );
}
