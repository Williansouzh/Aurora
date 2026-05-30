import { useEffect, useState } from 'react';

const ONBOARDING_FLAG = 'onboarding_completed';

export function useOnboarding(api, token) {
  const [state, setState] = useState({
    shouldShow: false,
    loading: true,
    accounts: [],
    categories: [],
    error: '',
  });

  const completeOnboarding = () => {
    localStorage.setItem(ONBOARDING_FLAG, 'true');
    setState((current) => ({ ...current, shouldShow: false }));
  };

  const loadOnboardingState = async () => {
    if (!token || localStorage.getItem(ONBOARDING_FLAG) === 'true') {
      setState((current) => ({ ...current, shouldShow: false, loading: false }));
      return;
    }

    setState((current) => ({ ...current, loading: true, error: '' }));

    try {
      const accounts = await api.get('/api/accounts');

      if (accounts.length > 0) {
        setState({ shouldShow: false, loading: false, accounts, categories: [], error: '' });
        return;
      }

      const categories = await api.get('/api/categories');
      setState({ shouldShow: true, loading: false, accounts, categories, error: '' });
    } catch (err) {
      setState((current) => ({ ...current, loading: false, error: err.message }));
    }
  };

  const createFirstAccount = async ({ name, type, initialBalance, creditLimit, closingDay, dueDay }) => {
    const account = await api.post('/api/accounts', {
      userId: '',
      name,
      type,
      initialBalance: Number(initialBalance),
      color: '#0f766e',
      creditLimit: Number(creditLimit || 0),
      closingDay: Number(closingDay || 10),
      dueDay: Number(dueDay || 15),
    });

    setState((current) => ({ ...current, accounts: [account] }));
    return account;
  };

  const updateCategory = async (category, patch) => {
    const nextCategory = { ...category, ...patch };
    const updated = await api.put(`/api/categories/${category.id}`, {
      userId: '',
      id: category.id,
      name: nextCategory.name,
      type: nextCategory.type,
      color: nextCategory.color,
      icon: nextCategory.icon,
    });

    setState((current) => ({
      ...current,
      categories: current.categories.map((item) => (item.id === category.id ? updated : item)),
    }));
  };

  const deleteCategory = async (categoryId) => {
    await api.delete(`/api/categories/${categoryId}`);
    setState((current) => ({
      ...current,
      categories: current.categories.filter((category) => category.id !== categoryId),
    }));
  };

  const createFirstTransaction = async (transaction) => {
    return api.post('/api/transactions', {
      userId: '',
      accountId: transaction.accountId,
      categoryId: transaction.categoryId,
      description: transaction.description,
      amount: Number(transaction.amount),
      type: transaction.type,
      status: 0,
      date: transaction.date,
      dueDate: null,
      notes: 'Primeira transacao criada pelo onboarding',
    });
  };

  useEffect(() => {
    loadOnboardingState();
  }, [token]);

  return {
    ...state,
    completeOnboarding,
    createFirstAccount,
    updateCategory,
    deleteCategory,
    createFirstTransaction,
  };
}
