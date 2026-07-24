import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { DashboardPage } from '../pages/DashboardPage';
import { ToastProvider } from '../hooks/useToast';

// Shape of the /api/home HomeDto the dashboard consumes. All collections empty and counters at
// zero by default so the overview renders its empty states without crashing.
const emptyHome = {
  pendingTasksCount: 0,
  completedTasksCount: 0,
  topPendingTasks: [],
  todayHabits: [],
  featuredGoals: [],
  recentEvents: [],
  todayMood: null,
  moodHistory: [],
  totalBalance: 0,
  monthlyIncome: 0,
  monthlyExpense: 0,
  totalXp: 0,
  level: 1,
  levelName: 'Iniciante',
  xpToNextLevel: 100,
  achievements: [],
  weeklyActivity: [],
};

const filledHome = {
  ...emptyHome,
  totalBalance: 5000,
  monthlyIncome: 3000,
  monthlyExpense: 1200,
};

// DashboardPage consumes an injected `api` client, so we stub it directly instead of routing
// through global fetch — this keeps the test deterministic and free of the jsdom/undici fetch
// interception issues that MSW hits under Vitest.
function makeApi(home) {
  return {
    get: vi.fn((path) => {
      if (path.startsWith('/api/home')) return Promise.resolve(home);
      return Promise.resolve(null);
    }),
  };
}

function renderDashboard(home = emptyHome) {
  return render(
    <MemoryRouter>
      <ToastProvider>
        <DashboardPage api={makeApi(home)} />
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

  it('deve exibir empty state quando não há dados', async () => {
    renderDashboard();
    await waitFor(() => {
      expect(document.querySelectorAll('.skeleton')).toHaveLength(0);
    });
    expect(screen.getByText(/sem atividade registrada ainda/i)).toBeInTheDocument();
  });

  it('deve exibir os valores financeiros após carregar dados', async () => {
    renderDashboard(filledHome);

    await waitFor(() => {
      expect(screen.getAllByText(/3\.000/).length).toBeGreaterThan(0);
    });
    expect(screen.getAllByText(/1\.200/).length).toBeGreaterThan(0);
  });
});
