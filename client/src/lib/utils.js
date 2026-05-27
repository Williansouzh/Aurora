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
    0: 'text-emerald-700 bg-emerald-50 border-emerald-200',
    1: 'text-red-700 bg-red-50 border-red-200',
    2: 'text-indigo-700 bg-indigo-50 border-indigo-200',
    income: 'text-emerald-700 bg-emerald-50 border-emerald-200',
    expense: 'text-red-700 bg-red-50 border-red-200',
    transfer: 'text-indigo-700 bg-indigo-50 border-indigo-200',
  };
  return map[type] ?? 'text-gray-600 bg-gray-50 border-gray-200';
}

export function statusClasses(status) {
  const map = {
    0: 'text-emerald-700 bg-emerald-50 border-emerald-200',
    1: 'text-amber-700 bg-amber-50 border-amber-200',
    2: 'text-red-700 bg-red-50 border-red-200',
    paid: 'text-emerald-700 bg-emerald-50 border-emerald-200',
    pending: 'text-amber-700 bg-amber-50 border-amber-200',
    overdue: 'text-red-700 bg-red-50 border-red-200',
    cancelled: 'text-gray-500 bg-gray-50 border-gray-200',
  };
  return map[status] ?? 'text-gray-500 bg-gray-50 border-gray-200';
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
