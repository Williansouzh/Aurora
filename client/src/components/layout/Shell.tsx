import {
  ArrowLeftRight,
  BookOpen,
  Building2,
  CalendarCheck,
  CalendarDays,
  Camera,
  Flame,
  Inbox,
  LayoutDashboard,
  LogOut,
  Menu,
  Moon,
  Scroll,
  Settings,
  Shield,
  Sun,
  Tag,
  Target,
  TrendingUp,
  Wallet,
  X,
  Zap,
} from 'lucide-react';
import { useState, useEffect } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { cn, getInitials } from '@/lib/utils';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { Tooltip, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';

const navSections = [
  {
    title: 'Central',
    items: [
      { moduleKey: 'home', to: '/', end: true, icon: LayoutDashboard, label: 'Dashboard' },
      { moduleKey: 'today', to: '/today', icon: CalendarCheck, label: 'Meu Dia' },
      { moduleKey: 'tasks', to: '/backlog', icon: Inbox, label: 'Backlog' },
    ],
  },
  {
    title: 'Life OS',
    items: [
      { moduleKey: 'habits', to: '/habits', icon: Flame, label: 'Rituais' },
      { moduleKey: 'goals', to: '/goals', icon: Target, label: 'Minha Jornada' },
      { moduleKey: 'timeline', to: '/timeline', icon: Scroll, label: 'Linha da Vida' },
      { moduleKey: 'weekly-planning', to: '/weekly', icon: CalendarDays, label: 'Minha Semana' },
      { moduleKey: 'diary', to: '/diary', icon: BookOpen, label: 'Diario' },
      { moduleKey: 'evolution', to: '/evolution', icon: Camera, label: 'Evolucao' },
      { moduleKey: 'studies', to: '/studies', icon: BookOpen, label: 'Estudos' },
      { moduleKey: 'retrospectives', to: '/retrospectives', icon: TrendingUp, label: 'Retrospectiva' },
    ],
  },
  {
    title: 'Dinheiro',
    items: [
      { moduleKey: 'finances', to: '/transactions', icon: ArrowLeftRight, label: 'Transacoes' },
      { moduleKey: 'finances', to: '/accounts', icon: Wallet, label: 'Contas' },
      { moduleKey: 'finances', to: '/categories', icon: Tag, label: 'Categorias' },
      { moduleKey: 'finances', to: '/budgets', icon: Target, label: 'Orcamentos' },
      { moduleKey: 'finances', to: '/financings', icon: Building2, label: 'Financiamentos' },
    ],
  },
  {
    title: 'Operacao',
    items: [
      { moduleKey: 'admin', to: '/admin', icon: Shield, label: 'Super Admin' },
    ],
  },
];

function NavItem({ to, end, icon: Icon, label, collapsed = false, onClick }) {
  return (
    <NavLink
      to={to}
      end={end}
      onClick={onClick}
      className={({ isActive }) =>
        cn(
          'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-150',
          'hover:bg-primary/8 hover:text-primary',
          isActive
            ? 'bg-primary/10 text-primary border-l-2 border-primary pl-[10px]'
            : 'text-muted-foreground border-l-2 border-transparent',
          collapsed && 'justify-center px-2 pl-2'
        )
      }
    >
      <Icon className={cn('shrink-0', collapsed ? 'h-5 w-5' : 'h-4 w-4')} />
      {!collapsed && <span>{label}</span>}
    </NavLink>
  );
}

function useDarkMode() {
  const [dark, setDark] = useState(() => {
    const stored = localStorage.getItem('aurora-theme');
    if (stored) return stored === 'dark';
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  });

  useEffect(() => {
    document.documentElement.classList.toggle('dark', dark);
    localStorage.setItem('aurora-theme', dark ? 'dark' : 'light');
  }, [dark]);

  return [dark, setDark] as const;
}

export function Shell({ children, user, onSignOut, access }) {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [dark, setDark] = useDarkMode();
  const location = useLocation();

  useEffect(() => { setMobileOpen(false); }, [location.pathname]);

  const allowedModules = new Set(
    access?.modules
      ?.filter((module) => module.isAllowed && module.showInNavigation)
      .map((module) => module.key)
  );
  const visibleSections = navSections
    .map((section) => ({
      ...section,
      items: access ? section.items.filter((item) => allowedModules.has(item.moduleKey)) : section.items,
    }))
    .filter((section) => section.items.length > 0);

  const sidebarContent = (isMobile = false) => (
    <div className="flex h-full flex-col">
      <div className="flex items-center gap-2.5 px-4 py-5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary">
          <Zap className="h-4 w-4 text-primary-foreground" />
        </div>
        <div className="flex flex-col leading-none">
          <span className="text-base font-bold text-foreground">Aurora</span>
          <span className="text-[10px] text-muted-foreground font-medium tracking-wide uppercase">Life OS</span>
        </div>
        {isMobile && (
          <Button variant="ghost" size="icon" className="ml-auto h-7 w-7" onClick={() => setMobileOpen(false)}>
            <X className="h-4 w-4" />
          </Button>
        )}
      </div>

      <Separator />

      <nav className="flex-1 space-y-0.5 px-3 py-4">
        <TooltipProvider delayDuration={300}>
          {visibleSections.map((section) => (
            <div key={section.title} className="mb-4 last:mb-0">
              <div className="mb-1.5 px-3 text-[10px] font-semibold uppercase tracking-wide text-muted-foreground">
                {section.title}
              </div>
              <div className="space-y-0.5">
                {section.items.map((item) => (
                  <Tooltip key={item.to}>
                    <TooltipTrigger asChild>
                      <NavItem {...item} onClick={isMobile ? () => setMobileOpen(false) : undefined} />
                    </TooltipTrigger>
                  </Tooltip>
                ))}
              </div>
            </div>
          ))}
        </TooltipProvider>
      </nav>

      <Separator />

      <div className="p-3 space-y-1">
        <button
          onClick={() => setDark((d) => !d)}
          className={cn(
            'flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
            'text-muted-foreground hover:bg-accent hover:text-foreground border-0 bg-transparent min-h-0'
          )}
        >
          {dark ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
          <span>{dark ? 'Modo claro' : 'Modo escuro'}</span>
        </button>

        <NavLink
          to="/settings"
          className={({ isActive }) =>
            cn(
              'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
              'text-muted-foreground hover:bg-accent hover:text-foreground',
              isActive && 'bg-accent text-foreground'
            )
          }
        >
          <Settings className="h-4 w-4" />
          <span>Configuracoes</span>
        </NavLink>

        <Separator className="my-2" />

        <div className="flex items-center gap-3 px-3 py-2">
          <Avatar className="h-7 w-7">
            <AvatarFallback className="text-xs bg-primary/10 text-primary">
              {getInitials(user?.name)}
            </AvatarFallback>
          </Avatar>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-foreground truncate">{user?.name ?? 'Usuario'}</p>
            <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
          </div>
          <Button variant="ghost" size="icon" className="h-7 w-7 shrink-0 text-muted-foreground hover:text-foreground" onClick={onSignOut} title="Sair">
            <LogOut className="h-3.5 w-3.5" />
          </Button>
        </div>
      </div>
    </div>
  );

  return (
    <div className="flex min-h-screen bg-background">
      <aside className="hidden md:flex md:w-60 md:flex-col md:fixed md:inset-y-0 md:left-0 border-r border-border bg-card z-30">
        {sidebarContent()}
      </aside>

      {mobileOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 md:hidden"
          onClick={() => setMobileOpen(false)}
        />
      )}

      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-50 w-72 bg-card border-r border-border transform transition-transform duration-300 ease-in-out md:hidden',
          mobileOpen ? 'translate-x-0' : '-translate-x-full'
        )}
      >
        {sidebarContent(true)}
      </aside>

      <div className="flex flex-1 flex-col md:pl-60">
        <header className="sticky top-0 z-20 flex h-14 items-center gap-3 border-b border-border bg-card/80 backdrop-blur-sm px-4 md:hidden">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => setMobileOpen(true)}>
            <Menu className="h-4 w-4" />
          </Button>
          <div className="flex items-center gap-2">
            <div className="flex h-6 w-6 items-center justify-center rounded-md bg-primary">
              <Zap className="h-3 w-3 text-primary-foreground" />
            </div>
            <span className="font-bold text-foreground">Aurora</span>
          </div>
        </header>

        <main className="flex-1 p-4 md:p-6 lg:p-8">
          {children}
        </main>
      </div>
    </div>
  );
}
