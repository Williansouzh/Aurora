import * as React from 'react';
import { cn } from '@/lib/utils';

type OptionSelectProps = Omit<React.SelectHTMLAttributes<HTMLSelectElement>, 'onChange'> & {
  value: string;
  options: string[][];
  onChange: (value: string) => void;
};

/**
 * Lightweight value/options/onChange dropdown backed by a native <select>. Used where a simple
 * options list is more convenient than composing the Radix Select primitives.
 */
export function OptionSelect({ value, options, onChange, className, ...rest }: OptionSelectProps) {
  return (
    <select
      value={value}
      onChange={(event) => onChange(event.target.value)}
      className={cn(
        'flex h-9 w-full items-center rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50',
        className,
      )}
      {...rest}
    >
      {options.map(([optionValue, label]) => (
        <option key={optionValue} value={optionValue}>{label}</option>
      ))}
    </select>
  );
}
