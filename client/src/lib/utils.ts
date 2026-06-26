import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs) {
  return twMerge(clsx(inputs));
}

export function formatCurrency(value) {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value ?? 0);
}

export function formatDate(date) {
  if (!date) return '—';
  return new Intl.DateTimeFormat('pt-BR').format(new Date(date));
}

export function formatDateShort(date) {
  if (!date) return '—';
  return new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: 'short' }).format(new Date(date));
}

export function transactionTypeClasses(type) {
  const map = {
    0: 'text-income bg-income-soft border-income/30',
    1: 'text-expense bg-expense-soft border-expense/30',
    2: 'text-transfer bg-accent border-transfer/30',
    income: 'text-income bg-income-soft border-income/30',
    expense: 'text-expense bg-expense-soft border-expense/30',
    transfer: 'text-transfer bg-accent border-transfer/30',
  };
  return map[type] ?? 'text-faint bg-muted border-border';
}

export function statusClasses(status) {
  const map = {
    0: 'text-income bg-income-soft border-income/30',
    1: 'text-pending bg-pending-soft border-pending/30',
    2: 'text-expense bg-expense-soft border-expense/30',
    paid: 'text-income bg-income-soft border-income/30',
    pending: 'text-pending bg-pending-soft border-pending/30',
    overdue: 'text-expense bg-expense-soft border-expense/30',
    cancelled: 'text-faint bg-muted border-border',
  };
  return map[status] ?? 'text-faint bg-muted border-border';
}

export function getInitials(name) {
  if (!name) return '?';
  return name
    .split(' ')
    .slice(0, 2)
    .map((w) => w[0])
    .join('')
    .toUpperCase();
}
