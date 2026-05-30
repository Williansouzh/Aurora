import { Navigate, Route, Routes } from 'react-router-dom';
import { Shell } from './components/layout/Shell';
import { Onboarding } from './components/Onboarding';
import { useAuth } from './hooks/useAuth';
import { useOnboarding } from './hooks/useOnboarding';
import { useToast } from './hooks/useToast';
import { AccountsPage } from './pages/AccountsPage';
import { AuthPage } from './pages/AuthPage';
import { BudgetsPage } from './pages/BudgetsPage';
import { CategoriesPage } from './pages/CategoriesPage';
import { DashboardPage } from './pages/DashboardPage';
import { ConfirmEmailPage, ForgotPasswordPage, ResetPasswordPage } from './pages/ForgotPasswordPage';
import { FinancingDetailPage } from './pages/FinancingDetailPage';
import { FinancingsPage } from './pages/FinancingsPage';
import { BacklogPage } from './pages/BacklogPage';
import { DiaryPage } from './pages/DiaryPage';
import { EvolutionPage } from './pages/EvolutionPage';
import { GoalsPage } from './pages/GoalsPage';
import { HabitsPage } from './pages/HabitsPage';
import { InvoicePage } from './pages/InvoicePage';
import { SettingsPage } from './pages/SettingsPage';
import { TimelinePage } from './pages/TimelinePage';
import { TodayPage } from './pages/TodayPage';
import { RetrospectivesPage } from './pages/RetrospectivesPage';
import { TransactionsPage } from './pages/TransactionsPage';
import { WeeklyPlanningPage } from './pages/WeeklyPlanningPage';

function App() {
  const { api, ready, user, signIn, signOut, updateUser } = useAuth();
  const onboarding = useOnboarding(api, user);
  const toast = useToast();

  const completeOnboarding = () => {
    toast.success('Tudo pronto! Seu Aurora está configurado.');
  };

  if (!ready) {
    return <div className="app-loading">Carregando...</div>;
  }

  return (
    <>
      <Routes>
        <Route path="/login" element={<AuthPage mode="login" api={api} onAuth={signIn} />} />
        <Route path="/register" element={<AuthPage mode="register" api={api} onAuth={signIn} />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage api={api} />} />
        <Route path="/reset-password" element={<ResetPasswordPage api={api} />} />
        <Route path="/confirm-email" element={<ConfirmEmailPage api={api} />} />
        <Route
          path="/*"
          element={
            user ? (
              <Shell user={user} onSignOut={signOut} api={api}>
                <Routes>
                  <Route path="/" element={<DashboardPage api={api} />} />
                  <Route path="/today" element={<TodayPage api={api} />} />
                  <Route path="/backlog" element={<BacklogPage api={api} />} />
                  <Route path="/habits" element={<HabitsPage api={api} />} />
                  <Route path="/goals" element={<GoalsPage api={api} />} />
                  <Route path="/timeline" element={<TimelinePage api={api} />} />
                  <Route path="/weekly" element={<WeeklyPlanningPage api={api} />} />
                  <Route path="/diary" element={<DiaryPage api={api} />} />
                  <Route path="/evolution" element={<EvolutionPage api={api} />} />
                  <Route path="/retrospectives" element={<RetrospectivesPage api={api} />} />
                  <Route path="/accounts" element={<AccountsPage api={api} />} />
                  <Route path="/accounts/:accountId/invoices" element={<InvoicePage api={api} />} />
                  <Route path="/categories" element={<CategoriesPage api={api} />} />
                  <Route path="/budgets" element={<BudgetsPage api={api} />} />
                  <Route path="/transactions" element={<TransactionsPage api={api} />} />
                  <Route path="/financings" element={<FinancingsPage api={api} />} />
                  <Route path="/financings/:id" element={<FinancingDetailPage api={api} />} />
                  <Route
                    path="/settings"
                    element={(
                      <SettingsPage
                        api={api}
                        user={user}
                        onProfileUpdated={updateUser}
                        onSignOut={signOut}
                      />
                    )}
                  />
                </Routes>
              </Shell>
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />
      </Routes>

      {user && <Onboarding onboarding={onboarding} onCompleted={completeOnboarding} />}
    </>
  );
}

export default App;
