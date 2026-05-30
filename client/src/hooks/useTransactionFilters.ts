import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { enumValue } from '../utils/enumHelpers';
import { transactionStatuses, transactionTypes } from '../constants/financeOptions';

const DEFAULT_PAGE_SIZE = 20;
const SEARCH_DEBOUNCE_MS = 400;

const monthRange = (offsetMonths = 0) => {
  const reference = new Date();
  reference.setMonth(reference.getMonth() + offsetMonths);
  const from = new Date(reference.getFullYear(), reference.getMonth(), 1);
  const to = new Date(reference.getFullYear(), reference.getMonth() + 1, 0);
  return { from, to };
};

const lastMonthsRange = (count) => {
  const today = new Date();
  const from = new Date(today.getFullYear(), today.getMonth() - (count - 1), 1);
  const to = new Date(today.getFullYear(), today.getMonth() + 1, 0);
  return { from, to };
};

const currentYearRange = () => {
  const today = new Date();
  return {
    from: new Date(today.getFullYear(), 0, 1),
    to: new Date(today.getFullYear(), 11, 31),
  };
};

export const periodPresets = [
  ['currentMonth', 'Mês atual'],
  ['previousMonth', 'Mês anterior'],
  ['lastThreeMonths', 'Últimos 3 meses'],
  ['currentYear', 'Este ano'],
  ['custom', 'Personalizado'],
];

export const resolvePeriodRange = (preset, customFrom, customTo) => {
  switch (preset) {
    case 'previousMonth':
      return monthRange(-1);
    case 'lastThreeMonths':
      return lastMonthsRange(3);
    case 'currentYear':
      return currentYearRange();
    case 'custom':
      return {
        from: customFrom ? new Date(`${customFrom}T00:00:00`) : null,
        to: customTo ? new Date(`${customTo}T00:00:00`) : null,
      };
    case 'currentMonth':
    default:
      return monthRange(0);
  }
};

const toDateInput = (date) => (date ? date.toISOString().slice(0, 10) : '');

export const buildInitialFilters = () => ({
  search: '',
  periodPreset: 'currentMonth',
  customFrom: '',
  customTo: '',
  accountIds: [],
  categoryIds: [],
  type: '',
  status: '',
  minAmount: '',
  maxAmount: '',
  page: 1,
  pageSize: DEFAULT_PAGE_SIZE,
});

const buildQueryString = (filters) => {
  const params = new URLSearchParams();
  const range = resolvePeriodRange(filters.periodPreset, filters.customFrom, filters.customTo);

  if (range.from) params.set('dateFrom', toDateInput(range.from));
  if (range.to) params.set('dateTo', toDateInput(range.to));

  if (filters.search.trim()) params.set('search', filters.search.trim());
  if (filters.accountIds.length) params.set('accountIds', filters.accountIds.join(','));
  if (filters.categoryIds.length) params.set('categoryIds', filters.categoryIds.join(','));

  if (filters.type !== '') {
    const typeIndex = enumValue(transactionTypes, filters.type);
    if (typeIndex >= 0) params.set('type', String(typeIndex));
  }

  if (filters.status !== '') {
    const statusIndex = enumValue(transactionStatuses, filters.status);
    if (statusIndex >= 0) params.set('status', String(statusIndex));
  }

  if (filters.minAmount !== '' && !Number.isNaN(Number(filters.minAmount))) {
    params.set('minAmount', String(Number(filters.minAmount)));
  }
  if (filters.maxAmount !== '' && !Number.isNaN(Number(filters.maxAmount))) {
    params.set('maxAmount', String(Number(filters.maxAmount)));
  }

  params.set('page', String(filters.page));
  params.set('pageSize', String(filters.pageSize));

  return params.toString();
};

export const countActiveFilters = (filters) => {
  let count = 0;
  if (filters.search.trim()) count += 1;
  if (filters.periodPreset !== 'currentMonth') count += 1;
  if (filters.accountIds.length) count += 1;
  if (filters.categoryIds.length) count += 1;
  if (filters.type !== '') count += 1;
  if (filters.status !== '') count += 1;
  if (filters.minAmount !== '') count += 1;
  if (filters.maxAmount !== '') count += 1;
  return count;
};

export function useTransactionFilters({ api, initialFilters } = {}) {
  const [filters, setFiltersState] = useState(() => ({ ...buildInitialFilters(), ...(initialFilters || {}) }));
  const [debouncedSearch, setDebouncedSearch] = useState(filters.search);
  const [data, setData] = useState({ items: [], totalCount: 0, page: 1, pageSize: filters.pageSize, totalPages: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const requestId = useRef(0);

  useEffect(() => {
    const handler = window.setTimeout(() => setDebouncedSearch(filters.search), SEARCH_DEBOUNCE_MS);
    return () => window.clearTimeout(handler);
  }, [filters.search]);

  const effectiveFilters = useMemo(
    () => ({ ...filters, search: debouncedSearch }),
    [filters, debouncedSearch],
  );

  const queryString = useMemo(() => buildQueryString(effectiveFilters), [effectiveFilters]);

  const reload = useCallback(async () => {
    if (!api) return;
    const currentRequest = ++requestId.current;
    setLoading(true);
    setError('');

    try {
      const response = await api.get(`/api/transactions?${queryString}`);
      if (requestId.current !== currentRequest) return;
      setData({
        items: response?.items || [],
        totalCount: response?.totalCount || 0,
        page: response?.page || 1,
        pageSize: response?.pageSize || filters.pageSize,
        totalPages: response?.totalPages || 0,
      });
    } catch (err) {
      if (requestId.current !== currentRequest) return;
      setError(err.message);
      setData({ items: [], totalCount: 0, page: 1, pageSize: filters.pageSize, totalPages: 0 });
    } finally {
      if (requestId.current === currentRequest) setLoading(false);
    }
  }, [api, queryString, filters.pageSize]);

  useEffect(() => {
    reload();
  }, [reload]);

  const setFilters = useCallback((updater) => {
    setFiltersState((current) => {
      const next = typeof updater === 'function' ? updater(current) : { ...current, ...updater };
      const resetPage = !Object.prototype.hasOwnProperty.call(updater || {}, 'page');
      return resetPage ? { ...next, page: 1 } : next;
    });
  }, []);

  const setPage = useCallback((page) => {
    setFiltersState((current) => ({ ...current, page }));
  }, []);

  const resetFilters = useCallback(() => {
    setFiltersState(buildInitialFilters());
  }, []);

  const activeCount = useMemo(() => countActiveFilters(filters), [filters]);

  return {
    filters,
    setFilters,
    setPage,
    resetFilters,
    activeCount,
    data,
    loading,
    error,
    reload,
  };
}
