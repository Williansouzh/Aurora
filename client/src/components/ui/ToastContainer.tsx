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
  success: 'border-income/40 bg-income-soft text-income dark:border-income/40 dark:bg-income dark:text-income',
  error: 'border-expense/40 bg-expense-soft text-expense dark:border-expense/40 dark:bg-expense dark:text-expense',
  warning: 'border-pending/40 bg-pending-soft text-pending dark:border-pending/40 dark:bg-pending dark:text-pending',
  info: 'border-primary/40 bg-accent text-primary dark:border-primary/40 dark:bg-primary dark:text-primary',
};

const iconClasses = {
  success: 'text-income',
  error: 'text-expense',
  warning: 'text-pending',
  info: 'text-primary',
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
