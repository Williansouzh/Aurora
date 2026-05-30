import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { BudgetBar } from '../components/budgets/BudgetBar';

describe('BudgetBar', () => {
  it('deve exibir cor verde quando uso for menor que 70%', () => {
    const { container } = render(<BudgetBar spentAmount={60} limitAmount={100} />);
    expect(container.firstChild).toHaveClass('good');
  });

  it('deve exibir cor amarela entre 70% e 100%', () => {
    const { container } = render(<BudgetBar spentAmount={85} limitAmount={100} />);
    expect(container.firstChild).toHaveClass('warning');
  });

  it('deve exibir cor vermelha quando uso ultrapassar 100%', () => {
    const { container } = render(<BudgetBar spentAmount={120} limitAmount={100} />);
    expect(container.firstChild).toHaveClass('danger');
  });

  it('deve exibir percentual correto no atributo aria-label', () => {
    render(<BudgetBar spentAmount={75} limitAmount={100} />);
    expect(screen.getByRole('generic', { name: /75%/i })).toBeInTheDocument();
  });

  it('deve exibir 0% quando limitAmount for zero', () => {
    render(<BudgetBar spentAmount={50} limitAmount={0} />);
    expect(screen.getByRole('generic', { name: /0%/i })).toBeInTheDocument();
  });
});
