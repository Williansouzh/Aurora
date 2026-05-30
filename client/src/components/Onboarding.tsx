import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { accountTypes, categoryTypes, transactionTypes } from '../constants/financeOptions';
import { enumLabel, enumValue } from '../utils/enumHelpers';
import { todayInput } from '../utils/formatters';
import { Select } from './ui/Select';

const steps = ['Conta', 'Categorias', 'Transacao'];

export function Onboarding({ onboarding, onCompleted }) {
  const navigate = useNavigate();
  const [step, setStep] = useState(0);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const [accountForm, setAccountForm] = useState({
    name: 'Conta principal',
    type: 'CheckingAccount',
    initialBalance: 0,
    creditLimit: 0,
    closingDay: 10,
    dueDay: 15,
  });
  const [transactionForm, setTransactionForm] = useState({
    description: '',
    amount: '',
    type: 'Expense',
    categoryId: '',
    accountId: '',
    date: todayInput(),
  });
  const [categoryDrafts, setCategoryDrafts] = useState({});

  useEffect(() => {
    setCategoryDrafts((current) => {
      const nextDrafts = {};
      onboarding.categories.forEach((category) => {
        nextDrafts[category.id] = current[category.id] ?? category.name;
      });
      return nextDrafts;
    });
  }, [onboarding.categories]);

  const categoriesForType = useMemo(() => {
    const type = enumValue(categoryTypes, transactionForm.type);
    return onboarding.categories.filter((category) => category.type === type);
  }, [onboarding.categories, transactionForm.type]);

  const finish = () => {
    onboarding.completeOnboarding();
    onCompleted?.();
    navigate('/', { replace: true });
  };

  const skip = () => finish();

  const submitAccount = async (event) => {
    event.preventDefault();
    setError('');

    if (!accountForm.name.trim()) {
      setError('Informe o nome da conta.');
      return;
    }

    if (accountForm.type !== 'CreditCard' && Number(accountForm.initialBalance) < 0) {
      setError('O saldo inicial deve ser maior ou igual a zero.');
      return;
    }

    if (accountForm.type === 'CreditCard' && Number(accountForm.creditLimit) <= 0) {
      setError('Informe um limite positivo para o cartao.');
      return;
    }

    setSaving(true);
    try {
      const account = await onboarding.createFirstAccount({
        name: accountForm.name.trim(),
        type: enumValue(accountTypes, accountForm.type),
        initialBalance: accountForm.initialBalance,
        creditLimit: accountForm.creditLimit,
        closingDay: accountForm.closingDay,
        dueDay: accountForm.dueDay,
      });
      setTransactionForm((current) => ({ ...current, accountId: account.id }));
      setStep(1);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const confirmCategories = async () => {
    setError('');
    setSaving(true);

    try {
      const changes = onboarding.categories
        .filter((category) => (categoryDrafts[category.id] || '').trim() && categoryDrafts[category.id] !== category.name)
        .map((category) => onboarding.updateCategory(category, { name: categoryDrafts[category.id].trim() }));

      await Promise.all(changes);

      const firstCategory = categoriesForType[0] || onboarding.categories[0];
      const firstAccount = onboarding.accounts[0];
      setTransactionForm((current) => ({
        ...current,
        categoryId: current.categoryId || firstCategory?.id || '',
        accountId: current.accountId || firstAccount?.id || '',
      }));
      setStep(2);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const deleteCategory = async (categoryId) => {
    setError('');
    setSaving(true);

    try {
      await onboarding.deleteCategory(categoryId);
      setCategoryDrafts((current) => {
        const nextDrafts = { ...current };
        delete nextDrafts[categoryId];
        return nextDrafts;
      });
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const submitTransaction = async (event) => {
    event.preventDefault();
    setError('');

    if (!transactionForm.description.trim()) {
      setError('Informe uma descricao.');
      return;
    }

    if (Number(transactionForm.amount) <= 0) {
      setError('O valor deve ser maior que zero.');
      return;
    }

    if (!transactionForm.categoryId || !transactionForm.accountId) {
      setError('Selecione conta e categoria.');
      return;
    }

    setSaving(true);
    try {
      await onboarding.createFirstTransaction({
        ...transactionForm,
        description: transactionForm.description.trim(),
        type: enumValue(transactionTypes, transactionForm.type),
      });
      finish();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  if (!onboarding.shouldShow) return null;

  return (
    <div className="onboarding-overlay" role="dialog" aria-modal="true" aria-labelledby="onboarding-title">
      <section className="onboarding-modal">
        <button className="onboarding-skip" onClick={skip}>Pular onboarding</button>

        <header className="onboarding-head">
          <p className="eyebrow">Primeiro acesso</p>
          <h1 id="onboarding-title">Vamos configurar seu Aurora</h1>
          <span>Passo {step + 1} de 3 - {steps[step]}</span>
          <div className="onboarding-progress">
            {steps.map((label, index) => (
              <span key={label} className={index <= step ? 'active' : ''} />
            ))}
          </div>
        </header>

        {step === 0 && (
          <form className="onboarding-body form-grid" onSubmit={submitAccount}>
            <label>
              Nome da conta
              <input value={accountForm.name} onChange={(event) => setAccountForm({ ...accountForm, name: event.target.value })} required />
            </label>
            <label>
              Tipo
              <Select value={accountForm.type} options={accountTypes} onChange={(type) => setAccountForm({ ...accountForm, type })} />
            </label>
            {accountForm.type === 'CreditCard' ? (
              <>
                <label>
                  Limite total
                  <input
                    type="number"
                    step="0.01"
                    min="0.01"
                    value={accountForm.creditLimit}
                    onChange={(event) => setAccountForm({ ...accountForm, creditLimit: event.target.value })}
                    required
                  />
                </label>
                <label>
                  Dia de fechamento
                  <input
                    type="number"
                    min="1"
                    max="28"
                    value={accountForm.closingDay}
                    onChange={(event) => setAccountForm({ ...accountForm, closingDay: event.target.value })}
                    required
                  />
                </label>
                <label>
                  Dia de vencimento
                  <input
                    type="number"
                    min="1"
                    max="28"
                    value={accountForm.dueDay}
                    onChange={(event) => setAccountForm({ ...accountForm, dueDay: event.target.value })}
                    required
                  />
                </label>
              </>
            ) : (
              <label>
                Saldo inicial
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  value={accountForm.initialBalance}
                  onChange={(event) => setAccountForm({ ...accountForm, initialBalance: event.target.value })}
                  required
                />
              </label>
            )}
            {error && <p className="alert">{error}</p>}
            <div className="button-row">
              <button className="primary" disabled={saving}>{saving ? 'Salvando...' : 'Criar conta e continuar'}</button>
            </div>
          </form>
        )}

        {step === 1 && (
          <div className="onboarding-body">
            <div className="category-editor-list">
              {onboarding.categories.map((category) => (
                <div className="category-editor-row" key={category.id}>
                  <span className="dot" style={{ background: category.color }} />
                  <input
                    value={categoryDrafts[category.id] ?? category.name}
                    onChange={(event) => setCategoryDrafts({ ...categoryDrafts, [category.id]: event.target.value })}
                  />
                  <small>{enumLabel(categoryTypes, category.type)}</small>
                  <button className="danger" disabled={saving} onClick={() => deleteCategory(category.id)}>Excluir</button>
                </div>
              ))}
            </div>
            {error && <p className="alert">{error}</p>}
            <div className="button-row">
              <button disabled={saving} onClick={() => setStep(0)}>Voltar</button>
              <button className="primary" disabled={saving} onClick={confirmCategories}>{saving ? 'Salvando...' : 'Confirmar e continuar'}</button>
            </div>
          </div>
        )}

        {step === 2 && (
          <form className="onboarding-body form-grid" onSubmit={submitTransaction}>
            <label>
              Descricao
              <input value={transactionForm.description} onChange={(event) => setTransactionForm({ ...transactionForm, description: event.target.value })} required />
            </label>
            <label>
              Valor
              <input
                type="number"
                step="0.01"
                min="0.01"
                value={transactionForm.amount}
                onChange={(event) => setTransactionForm({ ...transactionForm, amount: event.target.value })}
                required
              />
            </label>
            <label>
              Tipo
              <Select
                value={transactionForm.type}
                options={transactionTypes.filter(([type]) => type !== 'Transfer')}
                onChange={(type) => {
                  const nextCategory = onboarding.categories.find((category) => category.type === enumValue(categoryTypes, type));
                  setTransactionForm({ ...transactionForm, type, categoryId: nextCategory?.id || '' });
                }}
              />
            </label>
            <label>
              Categoria
              <Select
                value={transactionForm.categoryId}
                options={categoriesForType.map((category) => [category.id, category.name])}
                onChange={(categoryId) => setTransactionForm({ ...transactionForm, categoryId })}
              />
            </label>
            <label>
              Conta
              <Select
                value={transactionForm.accountId}
                options={onboarding.accounts.map((account) => [account.id, account.name])}
                onChange={(accountId) => setTransactionForm({ ...transactionForm, accountId })}
              />
            </label>
            <label>
              Data
              <input type="date" value={transactionForm.date} onChange={(event) => setTransactionForm({ ...transactionForm, date: event.target.value })} />
            </label>
            {error && <p className="alert">{error}</p>}
            <div className="button-row">
              <button type="button" onClick={() => setStep(1)}>Voltar</button>
              <button type="button" onClick={finish}>Pular esta etapa</button>
              <button className="primary" disabled={saving}>{saving ? 'Salvando...' : 'Salvar e concluir'}</button>
            </div>
          </form>
        )}
      </section>
    </div>
  );
}
