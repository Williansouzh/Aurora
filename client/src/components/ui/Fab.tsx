import { Plus } from 'lucide-react';
import { cn } from '@/lib/utils';

// Mobile floating action button — 52px accent circle with elevation (Quiet DS).
// Hidden on desktop, where the primary action lives in the screen header.
export function Fab({ onClick, label = 'Adicionar', icon: Icon = Plus, className }) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={label}
      className={cn(
        'fixed bottom-[76px] right-4 z-40 flex h-[52px] w-[52px] items-center justify-center rounded-full bg-primary text-primary-foreground shadow-fab transition-transform active:scale-95 md:hidden',
        className
      )}
    >
      <Icon className="h-6 w-6" />
    </button>
  );
}
