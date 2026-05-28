import { render, screen, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { MemoryRouter } from 'react-router-dom';
import { afterAll, afterEach, beforeAll, describe, expect, it, vi } from 'vitest';
import { DashboardPage } from '../pages/DashboardPage';
import { ToastProvider } from '../hooks/useToast';

const BASE = 'http://localhost:8080';

const emptySummary = {
  totalBalance: 0, monthlyIncome: 0, monthlyExpense: 0, monthlyResult: 0,
  pendingIncome: 0, pendingExpense: 0, paidTransactionsCount: 0,
  pendingTransactionsCount: 0, recentTransactions: [],
  previousMonthIncome: 0, previousMonthExpense: 0,
  incomeVariation: 0, expenseVariation: 0, savingsRate: 0, upcomingDueTransactions: [],
};

const filledSummary = {
  ...emptySummary,
  totalBalance: 5000,
  monthlyIncome: 3000,
  monthlyExpense: 1200,
  monthlyResult: 1800,
};

function apiResponse(data) {
  return HttpResponse.json({ success: true, data });
}

const server = setupServer(
  http.get(`${BASE}/api/dashboard/monthly-summary`, () => apiResponse(emptySummary)),
  http.get(`${BASE}/api/dashboard/category-expenses`, () => apiResponse([])),
  http.get(`${BASE}/api/budgets`, () => apiResponse([])),
  http.get(`${BASE}/api/dashboard/cash-flow`, () => apiResponse([])),
);

beforeAll(() => server.listen({ onUnhandledRequest: 'bypass' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const mockApi = {
  get: (path) => fetch(`${BASE}${path}`).then((r) => r.json()).then((b) => b.data),
};

function renderDashboard() {
  return render(
    <MemoryRouter>
      <ToastProvider>
        <DashboardPage api={mockApi} />
      </ToastProvider>
    </MemoryRouter>,
  );
}

describe('DashboardPage', () => {
  it('deve renderizar skeletons enquanto carrega', () => {
    renderDashboard();
    const skeletons = document.querySelectorAll('.skeleton');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('deve exibir empty state quando não há transações', async () => {
    renderDashboard();
    await waitFor(() => {
      expect(screen.queryAllByRole('generic', { name: /skeleton/i })).toHaveLength(0);
    });
    expect(screen.getAllByText(/sem dados/i).length).toBeGreaterThan(0);
  });

  it('deve exibir os valores corretos após carregar dados', async () => {
    server.use(
      http.get(`${BASE}/api/dashboard/monthly-summary`, () => apiResponse(filledSummary)),
    );

    renderDashboard();

    await waitFor(() => {
      expect(screen.getByText(/3\.000/)).toBeInTheDocument();
    });
    expect(screen.getByText(/1\.200/)).toBeInTheDocument();
  });
});
