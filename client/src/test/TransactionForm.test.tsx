import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { TransactionForm } from '../components/transactions/TransactionForm';

const accounts = [
  { id: 'acc1', name: 'Corrente', type: 0 },
  { id: 'acc2', name: 'Cartão Visa', type: 4 },
];
const categories = [{ id: 'cat1', name: 'Alimentação', type: 1 }];

function renderForm(props = {}) {
  return render(
    <TransactionForm accounts={accounts} categories={categories} {...props} />,
  );
}

describe('TransactionForm', () => {
  it('deve renderizar campos obrigatórios', () => {
    renderForm();
    expect(screen.getByRole('textbox', { name: /descrição/i })).toBeInTheDocument();
    expect(screen.getByRole('spinbutton', { name: /valor/i })).toBeInTheDocument();
    expect(screen.getByRole('combobox', { name: /conta/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /criar/i })).toBeInTheDocument();
  });

  it('deve exibir erro de validação se valor for zero ou negativo', async () => {
    const user = userEvent.setup();
    renderForm();

    await user.type(screen.getByRole('textbox', { name: /descrição/i }), 'Teste');
    await user.clear(screen.getByRole('spinbutton', { name: /valor/i }));
    await user.type(screen.getByRole('spinbutton', { name: /valor/i }), '0');
    await user.click(screen.getByRole('button', { name: /criar/i }));

    expect(await screen.findByRole('alert')).toHaveTextContent(/valor deve ser maior/i);
  });

  it('deve chamar onSubmit com os dados corretos ao submeter', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    renderForm({ onSubmit });

    await user.type(screen.getByRole('textbox', { name: /descrição/i }), 'Almoço');
    await user.clear(screen.getByRole('spinbutton', { name: /valor/i }));
    await user.type(screen.getByRole('spinbutton', { name: /valor/i }), '35.50');
    await user.click(screen.getByRole('button', { name: /criar/i }));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledOnce();
      const arg = onSubmit.mock.calls[0][0];
      expect(arg.description).toBe('Almoço');
      expect(Number(arg.amount)).toBe(35.5);
    });
  });

  it('deve exibir seção de recorrência ao ativar o toggle', async () => {
    const user = userEvent.setup();
    renderForm();

    expect(screen.queryByText(/tipo de recorrência/i)).not.toBeInTheDocument();
    await user.click(screen.getByRole('checkbox', { name: /repetir/i }));
    expect(screen.getByText(/tipo de recorrência/i)).toBeInTheDocument();
  });

  it('deve exibir aviso de fatura quando conta selecionada for cartão de crédito', async () => {
    const user = userEvent.setup();
    renderForm({ initialValues: { accountId: 'acc2' } });

    expect(screen.getByRole('status')).toHaveTextContent(/fatura do cartão/i);
  });
});
