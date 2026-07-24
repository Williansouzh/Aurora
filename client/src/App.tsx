import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { Shell } from './components/layout/Shell';
import { Onboarding } from './components/Onboarding';
import { useAuth } from './hooks/useAuth';
import { useAccess } from './hooks/useAccess';
import { useOnboarding } from './hooks/useOnboarding';
import { useToast } from './hooks/useToast';

// Route components are code-split so each page ships in its own chunk and the initial bundle
// (login screen) stays small. Named exports are mapped to the default React.lazy expects.
const named = (loader, name) => lazy(() => loader().then((m) => ({ default: m[name] })));

const AccountsPage = named(() => import('./pages/AccountsPage'), 'AccountsPage');
const AdminPage = named(() => import('./pages/AdminPage'), 'AdminPage');
const AuthPage = named(() => import('./pages/AuthPage'), 'AuthPage');
const BudgetsPage = named(() => import('./pages/BudgetsPage'), 'BudgetsPage');
const CategoriesPage = named(() => import('./pages/CategoriesPage'), 'CategoriesPage');
const DashboardPage = named(() => import('./pages/DashboardPage'), 'DashboardPage');
const ForgotPasswordPage = named(() => import('./pages/ForgotPasswordPage'), 'ForgotPasswordPage');
const ResetPasswordPage = named(() => import('./pages/ForgotPasswordPage'), 'ResetPasswordPage');
const ConfirmEmailPage = named(() => import('./pages/ForgotPasswordPage'), 'ConfirmEmailPage');
const FinanceOverviewPage = named(() => import('./pages/FinanceOverviewPage'), 'FinanceOverviewPage');
const FinancingDetailPage = named(() => import('./pages/FinancingDetailPage'), 'FinancingDetailPage');
const FinancingsPage = named(() => import('./pages/FinancingsPage'), 'FinancingsPage');
const BacklogPage = named(() => import('./pages/BacklogPage'), 'BacklogPage');
const DiaryPage = named(() => import('./pages/DiaryPage'), 'DiaryPage');
const EvolutionPage = named(() => import('./pages/EvolutionPage'), 'EvolutionPage');
const GoalsPage = named(() => import('./pages/GoalsPage'), 'GoalsPage');
const HabitsPage = named(() => import('./pages/HabitsPage'), 'HabitsPage');
const InvoicePage = named(() => import('./pages/InvoicePage'), 'InvoicePage');
const SettingsPage = named(() => import('./pages/SettingsPage'), 'SettingsPage');
const StudiesPage = named(() => import('./pages/StudiesPage'), 'StudiesPage');
const TimelinePage = named(() => import('./pages/TimelinePage'), 'TimelinePage');
const TodayPage = named(() => import('./pages/TodayPage'), 'TodayPage');
const RetrospectivesPage = named(() => import('./pages/RetrospectivesPage'), 'RetrospectivesPage');
const TransactionsPage = named(() => import('./pages/TransactionsPage'), 'TransactionsPage');
const WeeklyPlanningPage = named(() => import('./pages/WeeklyPlanningPage'), 'WeeklyPlanningPage');

function App() {
  const { api, ready, user, signIn, signOut, updateUser } = useAuth();
  const access = useAccess(api, user);
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
      <Suspense fallback={<div className="app-loading">Carregando...</div>}>
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
              <Shell user={user} onSignOut={signOut} access={access.access}>
                <Routes>
                  <Route path="/" element={<Guard moduleKey="home" access={access}><DashboardPage api={api} access={access.access} /></Guard>} />
                  <Route path="/today" element={<Guard moduleKey="today" access={access}><TodayPage api={api} /></Guard>} />
                  <Route path="/backlog" element={<Guard moduleKey="tasks" access={access}><BacklogPage api={api} /></Guard>} />
                  <Route path="/habits" element={<Guard moduleKey="habits" access={access}><HabitsPage api={api} /></Guard>} />
                  <Route path="/goals" element={<Guard moduleKey="goals" access={access}><GoalsPage api={api} /></Guard>} />
                  <Route path="/timeline" element={<Guard moduleKey="timeline" access={access}><TimelinePage api={api} /></Guard>} />
                  <Route path="/weekly" element={<Guard moduleKey="weekly-planning" access={access}><WeeklyPlanningPage api={api} /></Guard>} />
                  <Route path="/diary" element={<Guard moduleKey="diary" access={access}><DiaryPage api={api} /></Guard>} />
                  <Route path="/evolution" element={<Guard moduleKey="evolution" access={access}><EvolutionPage api={api} /></Guard>} />
                  <Route path="/studies" element={<Guard moduleKey="studies" access={access}><StudiesPage api={api} /></Guard>} />
                  <Route path="/retrospectives" element={<Guard moduleKey="retrospectives" access={access}><RetrospectivesPage api={api} /></Guard>} />
                  <Route path="/finance" element={<Guard moduleKey="finances" access={access}><FinanceOverviewPage api={api} /></Guard>} />
                  <Route path="/accounts" element={<Guard moduleKey="finances" access={access}><AccountsPage api={api} /></Guard>} />
                  <Route path="/accounts/:accountId/invoices" element={<Guard moduleKey="finances" access={access}><InvoicePage api={api} /></Guard>} />
                  <Route path="/categories" element={<Guard moduleKey="finances" access={access}><CategoriesPage api={api} /></Guard>} />
                  <Route path="/budgets" element={<Guard moduleKey="finances" access={access}><BudgetsPage api={api} /></Guard>} />
                  <Route path="/transactions" element={<Guard moduleKey="finances" access={access}><TransactionsPage api={api} /></Guard>} />
                  <Route path="/financings" element={<Guard moduleKey="finances" access={access}><FinancingsPage api={api} /></Guard>} />
                  <Route path="/financings/:id" element={<Guard moduleKey="finances" access={access}><FinancingDetailPage api={api} /></Guard>} />
                  <Route path="/admin" element={<Guard moduleKey="admin" access={access}><AdminPage api={api} /></Guard>} />
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
      </Suspense>

      {user && <Onboarding onboarding={onboarding} onCompleted={completeOnboarding} />}
    </>
  );
}

export default App;

function Guard({ moduleKey, access, children }) {
  if (access.loading) return <div className="app-loading">Carregando...</div>;
  if (!access.canAccess(moduleKey)) return <BlockedModule moduleKey={moduleKey} />;
  return children;
}

function BlockedModule({ moduleKey }) {
  return (
    <div className="flex min-h-[50vh] items-center justify-center">
      <div className="max-w-md rounded-lg border bg-card p-6 text-center shadow-sm">
        <h1 className="text-lg font-semibold">Módulo indisponível</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          O módulo <span className="font-medium text-foreground">{moduleKey}</span> ainda não está liberado para o seu acesso.
        </p>
      </div>
    </div>
  );
}
