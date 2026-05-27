import { AlertCircle, AlertTriangle, CheckCircle, Info, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useToast } from '../../hooks/useToast';

const icons = {
  success: CheckCircle,
  error: AlertCircle,
  warning: AlertTriangle,
  info: Info,
};

const typeClasses = {
  success: 'border-emerald-200 bg-emerald-50 text-emerald-900 dark:border-emerald-800 dark:bg-emerald-950 dark:text-emerald-100',
  error: 'border-red-200 bg-red-50 text-red-900 dark:border-red-800 dark:bg-red-950 dark:text-red-100',
  warning: 'border-amber-200 bg-amber-50 text-amber-900 dark:border-amber-800 dark:bg-amber-950 dark:text-amber-100',
  info: 'border-blue-200 bg-blue-50 text-blue-900 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-100',
};

const iconClasses = {
  success: 'text-emerald-500',
  error: 'text-red-500',
  warning: 'text-amber-500',
  info: 'text-blue-500',
};

export function ToastContainer() {
  const toast = useToast();

  if (!toast.toasts.length) return null;

  return (
    <div
      role="status"
      aria-live="polite"
      className="fixed bottom-4 right-4 z-[100] flex flex-col gap-2 w-80"
    >
      {toast.toasts.map((item) => {
        const Icon = icons[item.type] ?? Info;
        return (
          <div
            key={item.id}
            className={cn(
              'flex items-start gap-3 rounded-lg border p-4 shadow-lg animate-fade-in',
              typeClasses[item.type] ?? 'border-border bg-background text-foreground'
            )}
          >
            <Icon className={cn('h-4 w-4 mt-0.5 shrink-0', iconClasses[item.type])} />
            <p className="flex-1 text-sm font-medium leading-snug">{item.message}</p>
            <button
              type="button"
              aria-label="Fechar"
              onClick={() => toast.remove(item.id)}
              className="shrink-0 rounded-sm opacity-60 hover:opacity-100 transition-opacity border-0 bg-transparent p-0 min-h-0 cursor-pointer"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          </div>
        );
      })}
    </div>
  );
}
