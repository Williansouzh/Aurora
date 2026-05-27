import { Check } from 'lucide-react';
import { colors } from '../../constants/financeOptions';
import { cn } from '../../lib/utils';

export function ColorPicker({ value, onChange }) {
  return (
    <div className="space-y-1.5">
      <p className="text-sm font-medium leading-none">Cor</p>
      <div className="flex flex-wrap gap-2">
        {colors.map((color) => (
          <button
            key={color}
            type="button"
            aria-label={color}
            onClick={() => onChange(color)}
            className={cn(
              'h-7 w-7 rounded-full border-2 transition-transform hover:scale-110 flex items-center justify-center border-0 p-0 min-h-0 cursor-pointer',
              value === color ? 'ring-2 ring-offset-2 ring-foreground/30 scale-110' : ''
            )}
            style={{ background: color }}
          >
            {value === color && <Check className="h-3.5 w-3.5 text-white drop-shadow-sm" />}
          </button>
        ))}
      </div>
    </div>
  );
}
