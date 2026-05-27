import { X } from 'lucide-react';
import { periodPresets } from '../../hooks/useTransactionFilters';
import { transactionStatuses, transactionTypes } from '../../constants/financeOptions';
import { formatCurrency } from '../../lib/utils';

const presetLabel = (preset) => periodPresets.find(([v]) => v === preset)?.[1] || preset;

const buildChips = (filters, accounts, categories) => {
  const chips = [];
  if (filters.search?.trim()) chips.push({ key: 'search', label: `"${filters.search.trim()}"`, onRemove: { search: '' } });
  if (filters.periodPreset !== 'currentMonth') {
    const label = filters.periodPreset === 'custom'
      ? `${filters.customFrom || '—'} → ${filters.customTo || '—'}`
      : presetLabel(filters.periodPreset);
    chips.push({ key: 'period', label: `Período: ${label}`, onRemove: { periodPreset: 'currentMonth', customFrom: '', customTo: '' } });
  }
  if (filters.type !== '') {
    const opt = transactionTypes.find(([v]) => v === filters.type);
    if (opt) chips.push({ key: 'type', label: `Tipo: ${opt[1]}`, onRemove: { type: '' } });
  }
  if (filters.status !== '') {
    const opt = transactionStatuses.find(([v]) => v === filters.status);
    if (opt) chips.push({ key: 'status', label: `Status: ${opt[1]}`, onRemove: { status: '' } });
  }
  (filters.accountIds || []).forEach((id) => {
    const acc = accounts.find((a) => a.id === id);
    if (acc) chips.push({ key: `acc-${id}`, label: `Conta: ${acc.name}`, onRemove: { accountIds: filters.accountIds.filter((x) => x !== id) } });
  });
  (filters.categoryIds || []).forEach((id) => {
    const cat = categories.find((c) => c.id === id);
    if (cat) chips.push({ key: `cat-${id}`, label: `Cat: ${cat.name}`, onRemove: { categoryIds: filters.categoryIds.filter((x) => x !== id) } });
  });
  if (filters.minAmount !== '') chips.push({ key: 'min', label: `Min ${formatCurrency(filters.minAmount)}`, onRemove: { minAmount: '' } });
  if (filters.maxAmount !== '') chips.push({ key: 'max', label: `Até ${formatCurrency(filters.maxAmount)}`, onRemove: { maxAmount: '' } });
  return chips;
};

export function FilterChips({ filters, setFilters, resetFilters, accounts = [], categories = [] }) {
  const chips = buildChips(filters, accounts, categories);
  if (!chips.length) return null;

  return (
    <div className="flex flex-wrap items-center gap-1.5">
      {chips.map((chip) => (
        <button
          key={chip.key}
          type="button"
          onClick={() => setFilters(chip.onRemove)}
          className="inline-flex items-center gap-1 rounded-full bg-primary/10 text-primary px-2.5 py-1 text-xs font-medium hover:bg-primary/20 transition-colors border-0 min-h-0"
        >
          {chip.label}
          <X className="h-3 w-3" />
        </button>
      ))}
      <button
        type="button"
        onClick={resetFilters}
        className="text-xs text-muted-foreground hover:text-foreground underline border-0 bg-transparent p-0 min-h-0 cursor-pointer"
      >
        Limpar todos
      </button>
    </div>
  );
}
