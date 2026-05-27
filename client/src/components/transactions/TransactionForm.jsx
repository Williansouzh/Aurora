import { useState } from 'react';
import { Select } from '../ui/Select';

const transactionTypes = [
  ['Expense', 'Despesa'],
  ['Income', 'Receita'],
];

const transactionStatuses = [
  ['Paid', 'Pago'],
  ['Pending', 'Pendente'],
];

const recurrenceTypes = [
  ['Monthly', 'Mensal'],
  ['Weekly', 'Semanal'],
  ['Yearly', 'Anual'],
];

const buildEmpty = (accounts = [], categories = []) => ({
  accountId: accounts[0]?.id || '',
  categoryId: categories[0]?.id || '',
  description: '',
  amount: '',
  type: 'Expense',
  status: 'Paid',
  date: new Date().toISOString().slice(0, 10),
  dueDate: '',
  notes: '',
  isRecurring: false,
  recurrenceType: 'Monthly',
  recurrenceInterval: 1,
  recurrenceEndDate: '',
  totalInstallments: '',
});

export function TransactionForm({ accounts = [], categories = [], onSubmit, editing = false, initialValues }) {
  const [form, setForm] = useState(() => ({ ...buildEmpty(accounts, categories), ...(initialValues || {}) }));
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const selectedAccount = accounts.find((a) => a.id === form.accountId);
  const isCreditCard = selectedAccount?.type === 4; // AccountType.CreditCard = 4

  const submit = async (event) => {
    event.preventDefault();
    setError('');

    const amount = Number(form.amount);
    if (!amount || amount <= 0) {
      setError('O valor deve ser maior que zero.');
      return;
    }

    setSaving(true);
    try {
      await onSubmit?.(form);
    } catch (err) {
      setError(err.message || 'Erro ao salvar transação.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <form className="panel form-grid" onSubmit={submit} aria-label="Formulário de transação">
      <label>
        Conta
        <Select
          value={form.accountId}
          options={accounts.map((a) => [a.id, a.name])}
          onChange={(accountId) => setForm({ ...form, accountId })}
        />
      </label>

      {isCreditCard && (
        <p role="status" className="info-alert">Esta despesa será lançada na fatura do cartão.</p>
      )}

      <label>
        Categoria
        <Select
          value={form.categoryId}
          options={categories.map((c) => [c.id, c.name])}
          onChange={(categoryId) => setForm({ ...form, categoryId })}
        />
      </label>

      <label>
        Descrição
        <input
          name="description"
          aria-label="Descrição"
          value={form.description}
          onChange={(event) => setForm({ ...form, description: event.target.value })}
          required
        />
      </label>

      <label>
        Valor
        <input
          type="number"
          aria-label="Valor"
          step="0.01"
          min="0"
          value={form.amount}
          onChange={(event) => setForm({ ...form, amount: event.target.value })}
          required
        />
      </label>

      <label>
        Tipo
        <Select value={form.type} options={transactionTypes} onChange={(type) => setForm({ ...form, type })} />
      </label>

      <label>
        Status
        <Select value={form.status} options={transactionStatuses} onChange={(status) => setForm({ ...form, status })} />
      </label>

      <label>
        Data
        <input
          type="date"
          aria-label="Data"
          value={form.date}
          onChange={(event) => setForm({ ...form, date: event.target.value })}
          required
        />
      </label>

      {!editing && (
        <fieldset>
          <legend>Repetir</legend>
          <label className="check-row">
            <input
              type="checkbox"
              aria-label="Repetir transação"
              checked={form.isRecurring}
              onChange={(event) => setForm({ ...form, isRecurring: event.target.checked })}
            />
            Esta transação se repete
          </label>

          {form.isRecurring && (
            <div className="repeat-grid">
              <label>
                Tipo de recorrência
                <Select
                  value={form.recurrenceType}
                  options={recurrenceTypes}
                  onChange={(recurrenceType) => setForm({ ...form, recurrenceType })}
                />
              </label>
            </div>
          )}
        </fieldset>
      )}

      {error && <p role="alert" className="alert">{error}</p>}

      <div className="button-row">
        <button type="submit" className="primary" disabled={saving}>
          {saving ? 'Salvando...' : editing ? 'Salvar' : 'Criar'}
        </button>
      </div>
    </form>
  );
}
