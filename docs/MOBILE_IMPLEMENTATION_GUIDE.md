# Aurora Mobile — Guia de Implementação

> Referência completa para construir o app React Native Expo Go com paridade de UX/UI em relação ao sistema web. Siga as etapas em ordem: cada fase entrega um bloco funcional e coerente.

---

## Índice

1. [Design System](#1-design-system)
2. [Arquitetura do App](#2-arquitetura-do-app)
3. [Contrato da API](#3-contrato-da-api)
4. [Navegação](#4-navegação)
5. [Etapa 1 — Fundação (Design System + Auth)](#etapa-1--fundação)
6. [Etapa 2 — Meu Dia + Backlog](#etapa-2--meu-dia--backlog)
7. [Etapa 3 — Rituais (Hábitos)](#etapa-3--rituais-hábitos)
8. [Etapa 4 — Minha Jornada (Goals)](#etapa-4--minha-jornada-goals)
9. [Etapa 5 — Diário](#etapa-5--diário)
10. [Etapa 6 — Minha Semana (Weekly Planning)](#etapa-6--minha-semana)
11. [Etapa 7 — Dashboard Home](#etapa-7--dashboard-home)
12. [Etapa 8 — Finanças (núcleo)](#etapa-8--finanças-núcleo)
13. [Etapa 9 — Finanças (avançado)](#etapa-9--finanças-avançado)
14. [Etapa 10 — Retrospectiva + Evolução + Estudos](#etapa-10--retrospectiva--evolução--estudos)
15. [Padrões de UX Mobile](#padrões-de-ux-mobile)
16. [Checklist de Paridade](#checklist-de-paridade)

---

## 1. Design System

O app mobile deve usar exatamente os mesmos tokens visuais do web. Mapeie-os para um arquivo `theme.ts` central.

### Cores

```ts
// theme/colors.ts
export const colors = {
  // Backgrounds
  background:     '#F9F9FA',   // light: hsl(0 0% 98%)
  backgroundDark: '#080D1A',   // dark:  hsl(222 47% 6%)

  card:           '#FFFFFF',
  cardDark:       '#0E1525',   // hsl(222 47% 9%)

  // Text
  foreground:     '#0F172A',   // hsl(222 47% 11%)
  foregroundDark: '#E2E8F0',   // hsl(210 40% 96%)
  muted:          '#64748B',   // hsl(215 16% 47%)
  mutedDark:      '#94A3B8',   // hsl(215 20% 65%)

  // Brand
  primary:        '#6366F1',   // hsl(239 84% 67%) — indigo
  primaryFg:      '#FFFFFF',

  // Semantic
  secondary:      '#F1F3F7',   // hsl(220 14% 96%)
  border:         '#E4E7EE',   // hsl(220 13% 91%)
  borderDark:     '#1E2A3B',   // hsl(217 33% 17%)
  destructive:    '#EF4444',   // hsl(0 84% 60%)
  destructiveDark:'#7F1D1D',   // hsl(0 63% 31%)

  // Domain
  income:         '#10B981',   // verde
  expense:        '#EF4444',   // vermelho
  pending:        '#F59E0B',   // amarelo
  transfer:       '#6366F1',   // indigo

  // Status badges
  success:        '#10B981',
  warning:        '#F59E0B',
  danger:         '#EF4444',
  info:           '#6366F1',
};
```

### Tipografia

```ts
// theme/typography.ts
// Fonte: Inter (expo-google-fonts/inter)
export const typography = {
  fontFamily: {
    sans:  'Inter_400Regular',
    medium:'Inter_500Medium',
    semi:  'Inter_600SemiBold',
    bold:  'Inter_700Bold',
  },
  size: {
    xs:   11,
    sm:   13,
    base: 15,
    lg:   17,
    xl:   20,
    '2xl':24,
    '3xl':30,
  },
  lineHeight: {
    tight:  1.25,
    normal: 1.5,
    relaxed:1.75,
  },
};
```

### Espaçamento e Bordas

```ts
// theme/spacing.ts
export const spacing = {
  1: 4, 2: 8, 3: 12, 4: 16, 5: 20, 6: 24, 8: 32, 10: 40, 12: 48,
};

export const radius = {
  sm:  6,   // calc(10px - 4px)
  md:  8,   // calc(10px - 2px)
  lg:  10,  // var(--radius)
  xl:  14,
  full:9999,
};

export const shadow = {
  card: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.07,
    shadowRadius: 3,
    elevation: 2,
  },
  cardHover: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.10,
    shadowRadius: 12,
    elevation: 6,
  },
};
```

### Componentes de Design System para criar primeiro

| Componente        | Equivalente Web          | Notas Mobile                                     |
|-------------------|--------------------------|--------------------------------------------------|
| `<AppText>`       | className typography     | Wrap `<Text>` com fontFamily + size props        |
| `<AppCard>`       | `<Card>`                 | `View` com border, bg, shadow.card, borderRadius |
| `<AppButton>`     | `<Button>`               | Variantes: default, destructive, outline, ghost  |
| `<AppBadge>`      | `<Badge>`                | Pill com variantes: success, warning, danger, info |
| `<AppInput>`      | `<Input>`                | `TextInput` com border, focus ring (borderColor primary) |
| `<AppDivider>`    | `<Separator>`            | `View` height:1, bg:border                       |
| `<SkeletonBox>`   | `<Skeleton>`             | Animated.View com pulse (opacity 0.4–0.8)        |
| `<EmptyState>`    | `<EmptyState>`           | Ícone + title + description + action button      |
| `<ConfirmModal>`  | `<ConfirmDialog>`        | Modal com botões Cancelar/Confirmar (danger prop)|
| `<ToastProvider>` | `<ToastContainer>`       | Toast fixed bottom, auto-dismiss 4s              |
| `<PageHeader>`    | `<PageHeader>`           | SafeAreaView + title + subtitle + actions slot   |

---

## 2. Arquitetura do App

### Stack recomendada

```
expo (SDK 52+)
expo-router v3          — navegação file-based
expo-secure-store       — armazenar tokens
expo-google-fonts/inter — tipografia
@tanstack/react-query   — fetching + cache + refresh
zustand                 — estado global de auth
nativewind v4           — Tailwind no React Native (opcional)
react-hook-form + zod   — formulários
@expo/vector-icons      — Lucide equivalente (MaterialCommunityIcons)
react-native-reanimated — animações suaves
```

### Estrutura de arquivos sugerida

```
app/
  (auth)/
    login.tsx
    register.tsx
  (tabs)/
    index.tsx          ← Today
    habits.tsx
    goals.tsx
    diary.tsx
    _layout.tsx        ← TabBar (5 tabs)
  backlog.tsx
  weekly.tsx
  retrospectives.tsx
  evolution.tsx
  studies.tsx
  timeline.tsx
  transactions.tsx
  accounts.tsx
  categories.tsx
  budgets.tsx
  financings/
    index.tsx
    [id].tsx
  settings.tsx
  _layout.tsx          ← Root layout (auth guard)

components/
  ui/
    AppText.tsx
    AppCard.tsx
    AppButton.tsx
    AppBadge.tsx
    AppInput.tsx
    AppDivider.tsx
    SkeletonBox.tsx
    EmptyState.tsx
    ConfirmModal.tsx
    ToastProvider.tsx
    PageHeader.tsx
  habits/
  goals/
  today/
  diary/
  finances/

hooks/
  useAuth.ts
  useToast.ts
  useQuery.ts           ← wrapper react-query

lib/
  api.ts                ← httpClient
  auth.ts               ← token storage
  formatters.ts         ← formatCurrency, formatDate

theme/
  colors.ts
  typography.ts
  spacing.ts
```

---

## 3. Contrato da API

### HTTP Client

```ts
// lib/api.ts
const BASE_URL = process.env.EXPO_PUBLIC_API_URL; // http://192.168.x.x:8080

// Todos os responses: { success: boolean, data: T }
// Erros: { success: false, message: string } ou { errors: { field: string[] } }
```

### Auth

| Método | Endpoint                     | Body / Params                         | Response data              |
|--------|------------------------------|---------------------------------------|----------------------------|
| POST   | `/api/auth/register`         | `{ name, email, password }`           | `AuthClientResponse`       |
| POST   | `/api/auth/login`            | `{ email, password }`                 | `AuthClientResponse`       |
| POST   | `/api/auth/refresh`          | Cookie `aurora.refresh` (ver nota)    | `AuthClientResponse`       |
| POST   | `/api/auth/logout`           | —                                     | 204                        |
| GET    | `/api/auth/me`               | Bearer token                          | `MeResponse`               |
| PUT    | `/api/auth/profile`          | `{ name, email }`                     | `MeResponse`               |
| PUT    | `/api/auth/password`         | `{ currentPassword, newPassword, confirmPassword }` | — |

```ts
interface AuthClientResponse {
  accessToken:  string;
  expiresIn:    number;   // segundos — 900 (15 min)
  userId:       string;
  name:         string;
  email:        string;
  mfaRequired:  boolean;  // false em dev
  challengeId?: string;
}

interface MeResponse {
  userId:           string;
  name:             string;
  email:            string;
  isEmailConfirmed: boolean;
  isMfaEnabled:     boolean;
  role:             'User' | 'Admin';
  status:           'Active' | 'Suspended';
}
```

**Refresh Token no Mobile:**
O servidor usa cookie HttpOnly `aurora.refresh`. No React Native, cookies de resposta precisam ser tratados manualmente:

```ts
// Ao fazer login: extrair Set-Cookie da resposta
const setCookie = response.headers.get('set-cookie') ?? '';
const match = setCookie.match(/aurora\.refresh=([^;]+)/);
if (match) await SecureStore.setItemAsync('refresh_token', match[1]);

// Ao chamar refresh: enviar cookie manualmente
const refreshToken = await SecureStore.getItemAsync('refresh_token');
fetch(`${BASE_URL}/api/auth/refresh`, {
  method: 'POST',
  headers: { Cookie: `aurora.refresh=${refreshToken}` },
});
```

### Today

| Método | Endpoint                        | Body                                              |
|--------|---------------------------------|---------------------------------------------------|
| GET    | `/api/today`                    | `?date=YYYY-MM-DD`                                |
| POST   | `/api/today`                    | `{ title, notes?, priority, date }`               |
| PUT    | `/api/today/{id}`               | `{ title, notes?, priority }`                     |
| PATCH  | `/api/today/{id}/complete`      | —                                                 |
| PATCH  | `/api/today/{id}/reopen`        | —                                                 |
| DELETE | `/api/today/{id}`               | —                                                 |
| GET    | `/api/today/backlog`            | —                                                 |
| POST   | `/api/today/backlog`            | `{ title, notes?, priority }`                     |
| PATCH  | `/api/today/{id}/move-to-today` | —                                                 |

```ts
interface DailyTask {
  id: string; title: string; notes?: string;
  priority: 'Low' | 'Medium' | 'High';
  status:   'Pending' | 'Completed';
  date: string; completedAt?: string;
  sourceModule?: string; sourceId?: string;
  createdAt: string;
}
```

### Habits

| Método | Endpoint                       | Body                                                      |
|--------|--------------------------------|-----------------------------------------------------------|
| GET    | `/api/habits`                  | `?activeOnly=true`                                        |
| POST   | `/api/habits`                  | `{ name, description?, area, frequencyType, difficulty, timesPerWeek?, color? }` |
| PUT    | `/api/habits/{id}`             | mesmo do POST                                             |
| DELETE | `/api/habits/{id}`             | —                                                         |
| PATCH  | `/api/habits/{id}/pause`       | —                                                         |
| PATCH  | `/api/habits/{id}/resume`      | —                                                         |
| POST   | `/api/habits/{id}/check-in`    | `{ date: 'YYYY-MM-DD', notes? }`                          |
| GET    | `/api/habits/{id}/stats`       | `?year=&month=`                                           |

```ts
// area values (web usa):
type HabitArea = 'Saúde' | 'Mente' | 'Finanças' | 'Relacionamentos' |
  'Carreira' | 'Espiritualidade' | 'Lazer' | 'Aprendizado' | 'Outro';

// difficulty: 'Fácil' | 'Médio' | 'Difícil'
// frequencyType: 'daily' | 'weekly'
```

### Goals

| Método | Endpoint                                       | Body                                  |
|--------|------------------------------------------------|---------------------------------------|
| GET    | `/api/goals`                                   | `?status=Active\|Completed\|Abandoned`|
| GET    | `/api/goals/{id}`                              | —                                     |
| POST   | `/api/goals`                                   | `{ title, description?, targetDate?, category? }` |
| PUT    | `/api/goals/{id}`                              | mesmo do POST                         |
| DELETE | `/api/goals/{id}`                              | —                                     |
| PATCH  | `/api/goals/{id}/progress`                     | `{ progress: number }`  (0–100)       |
| PATCH  | `/api/goals/{id}/status`                       | `{ status: 'Active'\|'Completed'\|'Abandoned' }` |
| POST   | `/api/goals/{id}/milestones`                   | `{ title }`                           |
| PATCH  | `/api/goals/{id}/milestones/{milestoneId}/complete` | —                                |
| PATCH  | `/api/goals/{id}/milestones/{milestoneId}/reopen`   | —                                |

### Diary

| Método | Endpoint                  | Body                             |
|--------|---------------------------|----------------------------------|
| GET    | `/api/diary`              | `?page=1&pageSize=30`            |
| GET    | `/api/diary/date/{date}`  | date = `YYYY-MM-DD`              |
| POST   | `/api/diary`              | `{ title, content, mood?, tags? }` |
| PUT    | `/api/diary/{id}`         | mesmo do POST                    |
| DELETE | `/api/diary/{id}`         | —                                |

```ts
// mood: 1 (😞) | 2 (😕) | 3 (😐) | 4 (🙂) | 5 (😄)
```

### Weekly Planning

| Método | Endpoint                           | Body                                       |
|--------|------------------------------------|--------------------------------------------|
| GET    | `/api/weekly-planning/current`     | —                                          |
| GET    | `/api/weekly-planning`             | `?limit=8`                                 |
| POST   | `/api/weekly-planning`             | `{ weekStart, goals, intentions?, theme? }`|
| PUT    | `/api/weekly-planning/{id}`        | mesmo do POST                              |
| PATCH  | `/api/weekly-planning/{id}/close`  | `{ review, score }`                        |

### Dashboard / Home

| Método | Endpoint                               | Params               |
|--------|----------------------------------------|----------------------|
| GET    | `/api/home`                            | `?month=&year=`      |
| GET    | `/api/dashboard/monthly-summary`       | `?month=&year=`      |
| GET    | `/api/dashboard/upcoming-dues`         | `?days=7&status=pending` |
| GET    | `/api/dashboard/category-expenses`     | `?month=&year=`      |
| GET    | `/api/dashboard/cash-flow`             | `?year=`             |
| GET    | `/api/dashboard/financing-summary`     | —                    |

### Finanças

| Recurso      | GET list             | POST             | PUT          | DELETE       |
|--------------|----------------------|------------------|--------------|--------------|
| Accounts     | `/api/accounts`      | `/api/accounts`  | `/{id}`      | `/{id}`      |
| Categories   | `/api/categories`    | `/api/categories`| `/{id}`      | `/{id}`      |
| Transactions | `/api/transactions?page=&pageSize=&month=&year=&accountId=&categoryId=` | `/api/transactions` | `/{id}` | `/{id}` |
| Transfers    | `/api/transfers`     | `/api/transfers` | —            | `/{id}`      |
| Budgets      | `/api/budgets?month=&year=` | `/api/budgets` | — | `/{id}` |
| Financings   | `/api/financings`    | `/api/financings`| `/{id}`      | `/{id}`      |
| Invoices     | `/api/invoices`      | —                | —            | —            |

```ts
// Transaction type:   1=Income | 2=Expense
// Transaction status: 1=Paid   | 2=Pending | 3=Overdue | 4=Cancelled
// Account type:       1=Corrente | 2=Poupança | 3=Dinheiro | 4=Investimento | 5=CartãoCredito
```

### Retrospectivas

| Método | Endpoint                           | Params                     |
|--------|------------------------------------|----------------------------|
| GET    | `/api/retrospectives/weekly`       | `?weekStart=YYYY-MM-DD`    |
| GET    | `/api/retrospectives/monthly`      | `?month=&year=`            |

---

## 4. Navegação

### Estrutura de navegação mobile

O web usa sidebar. No mobile, use **Tab Bar** para as telas principais + **Stack** para sub-telas.

```
Root Stack
├── (auth) Stack
│   ├── /login
│   └── /register
│
└── (app) — guard: requer accessToken
    ├── (tabs) Tab Bar
    │   ├── Tab 1: Home (/index)        ← ícone: Home
    │   ├── Tab 2: Hoje (/today)        ← ícone: CheckSquare
    │   ├── Tab 3: Hábitos (/habits)    ← ícone: Flame
    │   ├── Tab 4: Metas (/goals)       ← ícone: Target
    │   └── Tab 5: Mais (/more)         ← ícone: Menu (drawer/modal com resto)
    │
    ├── Stack sobre as tabs:
    │   ├── /backlog
    │   ├── /diary
    │   ├── /weekly
    │   ├── /retrospectives
    │   ├── /evolution
    │   ├── /studies
    │   ├── /timeline
    │   ├── /transactions
    │   ├── /accounts
    │   ├── /categories
    │   ├── /budgets
    │   ├── /financings
    │   ├── /financings/[id]
    │   └── /settings
    │
    └── Tab 5 "Mais" → Modal/Drawer com grid de atalhos:
        Diário | Minha Semana | Retrospectiva | Transações
        Contas | Categorias  | Orçamentos   | Financiamentos
        Evolução | Estudos   | Timeline     | Configurações
```

### Tab Bar — especificação visual

```
Fundo: colors.card
Borda: 1px topo, colors.border
Altura: 60px + SafeAreaInsets.bottom
Item ativo: colors.primary (ícone + label)
Item inativo: colors.muted
Fonte label: 11px Inter_500Medium
Ícone: 24px
```

---

## Etapa 1 — Fundação

**Objetivo:** App abre, usuário faz login/register, tokens ficam salvos, todas telas protegidas.

### Telas

#### Login
Equivalente ao `AuthPage.tsx` web em modo login.

```
┌─────────────────────────────────┐
│                                 │
│   [Logo Aurora]                 │
│   Bem-vindo de volta            │
│   Entre na sua conta            │
│                                 │
│   [Input: Email]                │
│   [Input: Senha 🔒]             │
│                                 │
│   [Button primary: Entrar]      │
│                                 │
│   ─── ou ───                   │
│   Não tem conta? Criar conta    │
│                                 │
└─────────────────────────────────┘
```

**UX:**
- Input email: `keyboardType="email-address"`, `autoCapitalize="none"`
- Input senha: `secureTextEntry`, botão olho toggle
- Botão desabilitado enquanto `isLoading`; exibe ActivityIndicator
- Em erro 401/422: toast error com mensagem da API
- Após sucesso: salvar `accessToken` + `refreshToken` no SecureStore, navegar para `/(tabs)`

#### Register

```
┌─────────────────────────────────┐
│   ← Voltar                      │
│   Criar conta                   │
│   [Input: Nome]                 │
│   [Input: Email]                │
│   [Input: Senha 🔒]             │
│   [Button: Criar conta]         │
│   Já tem conta? Entrar          │
└─────────────────────────────────┘
```

### Componentes a criar nesta etapa

- `AppText` — base de tipografia
- `AppCard` — container padrão
- `AppButton` — variantes default, outline, ghost, destructive
- `AppInput` — com label, error state, secureTextEntry toggle
- `ToastProvider` + `useToast`
- `lib/api.ts` — httpClient com Bearer token + refresh automático
- `lib/auth.ts` — SecureStore wrapper (salvar/ler/limpar tokens)
- `hooks/useAuth.ts` — estado global (zustand): user, login(), logout(), isAuthenticated

### Fluxo de refresh token

```ts
// Em lib/api.ts — interceptor automático:
// 1. Antes de cada request: verificar se accessToken expira em < 60s
//    Se sim: chamar /api/auth/refresh antes
// 2. Em qualquer 401: tentar refresh uma vez
//    Se refresh falhar: logout() + redirecionar para /login
```

---

## Etapa 2 — Meu Dia + Backlog

**Objetivo:** Tela principal do dia, criar/completar/deletar tarefas. Backlog para captura rápida.

### Today Screen

Equivalente ao `TodayPage.tsx` web.

```
┌─────────────────────────────────┐
│ Meu Dia          [←] [→] [+ ]  │
│ Segunda, 2 jun                  │
├─────────────────────────────────┤
│ ⚠ Atrasadas (2)                │  ← seção amber bg
│ ○ Revisar relatório    [Alta]   │
│ ○ Ligar para banco     [Média]  │
├─────────────────────────────────┤
│ Pendentes (3)                   │
│ ○ Estudar React Native [Média]  │
│ ○ Exercitar 30min      [Baixa]  │
│ ○ Planejar semana      [Alta]   │
├─────────────────────────────────┤
│ Concluídas (1)                  │
│ ✓ Ler emails           [Baixa]  │  ← texto muted, riscado
└─────────────────────────────────┘
```

**Detalhes UX:**
- Seleção de data: navegar dia anterior/próximo com `←` `→`. Tap no título abre DatePicker
- Cada item: tap no círculo à esquerda → complete/reopen (animação check)
- Swipe para a esquerda no item: revela botão vermelho Deletar (react-native-gesture-handler)
- Botão `+` no header abre bottom sheet de criação
- Seção "Atrasadas" com fundo `#FFFBEB` (amber-50) e borda esquerda `#F59E0B`
- Tarefas concluídas: texto com `textDecorationLine: 'line-through'`, cor muted

**Bottom Sheet — Nova Tarefa:**
```
┌─────────────────────────────────┐
│ ━━━━━ (drag handle)             │
│ Nova Tarefa                     │
│ [Input: Título *]               │
│ [Input: Notas (opcional)]       │
│ Prioridade:                     │
│ [Baixa] [Média] [Alta]  ← chips │
│                                 │
│ [Button: Salvar]                │
└─────────────────────────────────┘
```

**Prioridade chips** (segmented control visual):
- Baixa: borda/texto `#64748B`
- Média: borda/texto `#F59E0B`
- Alta: borda/texto `#EF4444`
- Selecionado: fundo preenchido na cor correspondente

### Backlog Screen

```
┌─────────────────────────────────┐
│ ← Backlog                [filtro]│
├─────────────────────────────────┤
│ Captura Rápida                  │
│ [Input: O que fazer?]           │
│ Prioridade: [B] [M] [A]         │
│ [Adicionar]                     │
├─────────────────────────────────┤
│ 8 tarefas                       │
│ ○ Criar testes unitários [Alta] │
│   [→ Mover p/ hoje]  [🗑]       │
│ ─────────────────────────────── │
│ ○ Refatorar componente [Média]  │
│   [→ Mover p/ hoje]  [🗑]       │
└─────────────────────────────────┘
```

### APIs consumidas

- `GET /api/today?date=`
- `POST /api/today` / `PUT /api/today/{id}`
- `PATCH /api/today/{id}/complete` / `/reopen`
- `DELETE /api/today/{id}`
- `GET /api/today/backlog`
- `POST /api/today/backlog`
- `PATCH /api/today/{id}/move-to-today`

### Componentes a criar

- `TaskItem` — item com círculo, título, badge prioridade, swipe delete
- `PriorityChips` — seletor de prioridade reutilizável
- `SectionHeader` — título de seção com contador
- `CreateTaskSheet` — bottom sheet com form
- `DateNavigation` — prev/next/picker de data

---

## Etapa 3 — Rituais (Hábitos)

**Objetivo:** Listar hábitos, fazer check-in diário, criar/editar/pausar.

### Habits Screen

Equivalente ao `HabitsPage.tsx` web.

```
┌─────────────────────────────────┐
│ Rituais                    [+]  │
│ [Ativos ▼]  [Filtrar]           │
├─────────────────────────────────┤
│ ┌───────────────────────────┐   │
│ │ 🔥 Exercitar              │   │
│ │ [Saúde] Diário · Difícil  │   │
│ │ ████████░░ 80%            │   │
│ │ 🔥 12 dias  🏆 30 dias    │   │
│ │           [✓ Check-in]   │   │
│ └───────────────────────────┘   │
│                                 │
│ ┌───────────────────────────┐   │
│ │ 📚 Ler 30 minutos         │   │
│ │ [Aprendizado] Diário      │   │
│ │ ██░░░░░░░░ 20%            │   │
│ │ 🔥 3 dias                 │   │
│ │           [✓ Check-in]   │   │
│ └───────────────────────────┘   │
└─────────────────────────────────┘
```

**Detalhes UX:**
- Cards em lista vertical (1 coluna) no mobile — mais fácil de ler e tocar
- Badge colorido por área (cada área tem sua cor)
- Barra de progresso para a semana atual
- Streak atual 🔥 e recorde 🏆 em destaque
- Botão "Check-in" ocupa direita inferior do card; após check-in do dia vira ✓ verde desabilitado
- Long press no card abre menu de ações: Editar / Pausar / Deletar
- Hábitos pausados: card com opacity 0.5, badge "Pausado"

**Bottom Sheet — Criar/Editar Hábito:**
```
┌─────────────────────────────────┐
│ Novo Ritual                     │
│ [Input: Nome *]                 │
│ [Input: Descrição]              │
│ Área:                           │
│ [Saúde][Mente][Finanças]...     │  ← wrap chips
│ Frequência:                     │
│ [Diário] [Semanal]              │
│ Dificuldade:                    │
│ [Fácil] [Médio] [Difícil]       │
│ [Salvar]                        │
└─────────────────────────────────┘
```

### Cores de área (igual ao web)

```ts
const AREA_COLORS: Record<string, string> = {
  'Saúde':           '#10B981',
  'Mente':           '#8B5CF6',
  'Finanças':        '#F59E0B',
  'Relacionamentos': '#EC4899',
  'Carreira':        '#6366F1',
  'Espiritualidade': '#14B8A6',
  'Lazer':           '#F97316',
  'Aprendizado':     '#3B82F6',
  'Outro':           '#64748B',
};
```

### APIs consumidas

- `GET /api/habits?activeOnly=true`
- `POST /api/habits` / `PUT /api/habits/{id}`
- `PATCH /api/habits/{id}/pause` / `/resume`
- `DELETE /api/habits/{id}`
- `POST /api/habits/{id}/check-in`

---

## Etapa 4 — Minha Jornada (Goals)

**Objetivo:** Listar metas, ver progresso, adicionar milestones, mudar status.

### Goals Screen

Equivalente ao `GoalsPage.tsx` web.

```
┌─────────────────────────────────┐
│ Minha Jornada              [+]  │
│ [Ativas] [Concluídas] [Todas]  │  ← filter tabs
├─────────────────────────────────┤
│ ┌───────────────────────────┐   │
│ │ 🎯 Correr 5km             │   │
│ │ [Ativa] · até Jun/2026    │   │
│ │ ████████░░ 75%            │   │
│ │ 3 / 5 milestones ✓        │   │
│ └───────────────────────────┘   │
└─────────────────────────────────┘
```

**Tela de Detalhe da Meta** (stack push):
```
┌─────────────────────────────────┐
│ ← Correr 5km          [editar] │
│ [Ativa]                         │
│ ████████░░ 75%    [atualizar]   │
│                                 │
│ Milestones                 [+]  │
│ ✓ Comprar tênis de corrida      │
│ ✓ Correr 1km sem parar          │
│ ✓ Correr 2km                    │
│ ○ Correr 3km                    │
│ ○ Correr 5km                    │
│                                 │
│ Descrição:                      │
│ Completar minha primeira 5km... │
│                                 │
│ [Marcar Concluída]              │
│ [Abandonar Meta]                │
└─────────────────────────────────┘
```

**Detalhes UX:**
- Status cores: Ativa=verde, Pausada=amber, Concluída=violet, Cancelada=slate
- Tap no card → push para detalhe
- Progress bar interativa: tap em "atualizar" abre modal com slider + input numérico
- Milestones: tap no círculo → complete/reopen (igual Today tasks)
- Long press no milestone → deletar
- Swipe bottom para ações destrutivas (abandonar/deletar meta)

### APIs consumidas

- `GET /api/goals?status=`
- `GET /api/goals/{id}`
- `POST /api/goals` / `PUT /api/goals/{id}`
- `DELETE /api/goals/{id}`
- `PATCH /api/goals/{id}/progress`
- `PATCH /api/goals/{id}/status`
- `POST /api/goals/{id}/milestones`
- `PATCH /api/goals/{id}/milestones/{milestoneId}/complete` / `/reopen`

---

## Etapa 5 — Diário

**Objetivo:** Registrar entradas diárias com humor, tags e conteúdo.

### Diary Screen

Equivalente ao `DiaryPage.tsx` web. No mobile, separar em duas views: **editor do dia** e **histórico** (via tab ou botão alternar).

```
┌─────────────────────────────────┐
│ Diário        [←] 02/jun [→]   │
│                   [histórico]   │
├─────────────────────────────────┤
│ Como você está hoje?            │
│ 😞  😕  😐  🙂  😄             │  ← mood picker (tap)
│           ↑ selecionado         │
├─────────────────────────────────┤
│ [TextArea: escreva aqui...]     │
│                                 │
│                                 │
│                                 │
├─────────────────────────────────┤
│ Tags: [+ tag]                   │
│ #reflexão  #trabalho  ×         │
├─────────────────────────────────┤
│ [Salvar entrada]                │
└─────────────────────────────────┘
```

**Histórico (modal/sheet):**
```
┌─────────────────────────────────┐
│ Entradas Recentes         ✕    │
│ 01/jun · 😄 · #gratidão        │
│ 31/mai · 😐 · #trabalho        │
│ 29/mai · 🙂                     │
│ 28/mai · 😕 · #cansaço         │
└─────────────────────────────────┘
```

**Detalhes UX:**
- Mood picker: 5 emojis em row, tap seleciona com highlight (fundo primary oval)
- Tags: input + enter para adicionar, × para remover; pills com fundo secondary
- TextArea: `multiline`, `minHeight: 160`, fundo card
- Auto-save: debounce de 1s após parar de digitar (OU botão manual)
- Se já existe entrada do dia: carregar conteúdo no editor; botão vira "Atualizar"
- Swipe down no histórico para fechar

### APIs consumidas

- `GET /api/diary?page=1&pageSize=30`
- `GET /api/diary/date/{date}`
- `POST /api/diary` / `PUT /api/diary/{id}`
- `DELETE /api/diary/{id}`

---

## Etapa 6 — Minha Semana

**Objetivo:** Ver planejamento da semana atual, criar e encerrar semanas.

### Weekly Screen

Equivalente ao `WeeklyPlanningPage.tsx` web.

```
┌─────────────────────────────────┐
│ Minha Semana               [+]  │
├─────────────────────────────────┤
│ Semana atual                    │
│ 02 – 06 jun 2026   [Em andamento]│
│                                 │
│ Foco Principal:                 │
│ "Finalizar módulo mobile"       │
│                                 │
│ Prioridades:                    │
│ • Implementar auth              │
│ • Criar design system           │
│ • Testar no dispositivo         │
│                                 │
│ [Encerrar Semana]               │
├─────────────────────────────────┤
│ Semanas anteriores              │
│ 26–30 mai · 4.5/5 ⭐            │
│ 19–23 mai · 3/5 ⭐              │
└─────────────────────────────────┘
```

**Encerrar Semana (bottom sheet):**
```
┌─────────────────────────────────┐
│ Como foi a semana?              │
│ [TextArea: revisão...]          │
│ Nota (1-5):                     │
│ ○ ○ ○ ○ ○  (star rating)        │
│ [Encerrar]                      │
└─────────────────────────────────┘
```

**Criar Nova Semana (bottom sheet):**
```
┌─────────────────────────────────┐
│ Nova Semana                     │
│ Início da semana: [DatePicker]  │
│ [Input: Foco principal]         │
│ Prioridades:                    │
│ [Input + botão adicionar]       │
│ • item 1  ×                     │
│ • item 2  ×                     │
│ [Salvar]                        │
└─────────────────────────────────┘
```

---

## Etapa 7 — Dashboard Home

**Objetivo:** Tela inicial com visão geral de todos os módulos.

### Home Screen

Equivalente ao `DashboardPage.tsx` / `HomeController` web.

```
┌─────────────────────────────────┐
│ Bom dia, Willian ☀️             │
│ Jun 2026          [← →]        │
├─────────────────────────────────┤
│ ┌──────────────────────────┐   │
│ │ ⚡ Nível 7 · 2.340 XP    │   │
│ │ ████████░░ 82%           │   │
│ │ Faltam 510 XP p/ nível 8│   │
│ └──────────────────────────┘   │
│                                 │
│ HOJE                            │
│ ┌──────────────────────────┐   │
│ │ ✓ 3/6 tarefas            │   │
│ │ • Estudar React Native   │   │
│ │ • Exercitar 30min        │   │
│ │            [Ver todas →] │   │
│ └──────────────────────────┘   │
│                                 │
│ RITUAIS                         │
│ ┌──────────────────────────┐   │
│ │ 🔥 4/5 check-ins hoje    │   │
│ │ Exercitar · Ler · Meditar│   │
│ │            [Ver rituais →]│   │
│ └──────────────────────────┘   │
│                                 │
│ FINANÇAS                        │
│ ┌──────────────────────────┐   │
│ │ Saldo  R$ 4.250,00       │   │
│ │ ↑ R$ 5k  ↓ R$ 2.3k      │   │
│ │            [Transações →]│   │
│ └──────────────────────────┘   │
│                                 │
│ HUMOR                           │
│ ┌──────────────────────────┐   │
│ │ Hoje: 🙂  Média: 3.8/5   │   │
│ │ ─ ─ ▄ ▄ █ ─ 🙂           │  ← mini gráfico 7 dias
│ └──────────────────────────┘   │
└─────────────────────────────────┘
```

**Detalhes UX:**
- Cards clicáveis → navega para a tela respectiva
- XP card: gradient de cor por nível (azul, verde, roxo, dourado, vermelho)
- Saudação muda por hora (Bom dia/Boa tarde/Boa noite)
- Navegação de mês: muda resumo financeiro e progresso
- ScrollView vertical com `refreshControl` (pull-to-refresh)

---

## Etapa 8 — Finanças (núcleo)

**Objetivo:** Criar transações, ver lista filtrada, contas e categorias.

### Transactions Screen

Equivalente ao `TransactionsPage.tsx` web.

```
┌─────────────────────────────────┐
│ Transações                 [+]  │
│ Jun 2026   [← →]  [filtros ▾]  │
├─────────────────────────────────┤
│ Resumo do mês                   │
│ ↑ R$ 5.000  ↓ R$ 2.300  = R$ 2.700│
├─────────────────────────────────┤
│ 02/jun                          │
│ [💚] Salário      R$ +5.000,00  │  ← Conta · Categoria
│ [🔴] Supermercado R$ -180,00    │
├─────────────────────────────────┤
│ 31/mai                          │
│ [🔴] Luz          R$ -120,00    │
│      ⏳ Pendente                │
└─────────────────────────────────┘
```

**Detalhes UX:**
- Agrupado por data (SectionList)
- Cor: verde para receita, vermelho para despesa
- Ícone da categoria na esquerda
- Badge de status (Pago/Pendente/Atrasado) quando relevante
- Swipe esquerdo → Deletar; Swipe direito → Marcar Pago
- Tap no item → bottom sheet com detalhe + ações (editar, deletar, mudar status)

**Filtros (bottom sheet):**
```
┌─────────────────────────────────┐
│ Filtros                    ✕   │
│ Tipo:  [Todos][Receita][Despesa]│
│ Status:[Todos][Pago][Pendente]  │
│ Conta: [Select...]              │
│ Categ: [Select...]              │
│ [Limpar]          [Aplicar]     │
└─────────────────────────────────┘
```

**Bottom Sheet — Nova Transação:**
```
┌─────────────────────────────────┐
│ Nova Transação                  │
│ Tipo: [Receita] [Despesa] [Transf.]│
│ [Input: Descrição *]            │
│ [Input: Valor *]  R$            │
│ Conta: [Select...]              │
│ Categoria: [Select...]          │
│ Data: [DatePicker]              │
│ Status: [Pago] [Pendente]       │
│ [Salvar]                        │
└─────────────────────────────────┘
```

### Accounts Screen

```
┌─────────────────────────────────┐
│ ← Contas                   [+] │
├─────────────────────────────────┤
│ Saldo Total: R$ 12.450,00      │
├─────────────────────────────────┤
│ ┌────────────────────────────┐  │
│ │ 🟦 Nubank Corrente         │  │
│ │ R$ 4.250,00                │  │
│ └────────────────────────────┘  │
│ ┌────────────────────────────┐  │
│ │ 🟩 Poupança BB             │  │
│ │ R$ 8.200,00                │  │
│ └────────────────────────────┘  │
└─────────────────────────────────┘
```

### Budgets Screen

```
┌─────────────────────────────────┐
│ ← Orçamentos   Jun 2026 [← →] │
├─────────────────────────────────┤
│ ┌──────────────────────────┐   │
│ │ 🍔 Alimentação           │   │
│ │ ████████░░ R$810/R$1000  │   │
│ │ 81% · R$ 190 restante    │   │  ← cor warning (amber)
│ └──────────────────────────┘   │
│ ┌──────────────────────────┐   │
│ │ 🚗 Transporte            │   │
│ │ ████░░░░░░ R$200/R$500   │   │
│ │ 40%                      │   │  ← cor good (verde)
│ └──────────────────────────┘   │
└─────────────────────────────────┘
```

**Budget status colors:**
- < 70% → `#10B981` (verde)
- 70–100% → `#F59E0B` (amber)
- > 100% → `#EF4444` (vermelho)

---

## Etapa 9 — Finanças (avançado)

**Objetivo:** Financiamentos, faturas de cartão.

### Financings Screen

Listar financiamentos com parcelas pagas/total e botão de pagar próxima parcela.

### Categories Screen

CRUD de categorias com ícone e cor selecionáveis.

---

## Etapa 10 — Retrospectiva, Evolução, Estudos

**Objetivo:** Completar paridade com web para módulos secundários.

### Retrospectivas

```
┌─────────────────────────────────┐
│ ← Retrospectiva                 │
│ [Semanal] [Mensal]  ← tabs      │
├─────────────────────────────────┤
│ Semana 26–30 mai                │
│ Tarefas: 12/15  Hábitos: 80%   │
│ Metas: 1 concluída              │
│ Nota da semana: 4/5 ⭐          │
│ Review: "Semana produtiva..."   │
└─────────────────────────────────┘
```

### Tela "Mais" (Tab 5)

Grid de acesso rápido a todos os módulos secundários:

```
┌─────────────────────────────────┐
│ Mais                            │
├───────────────┬─────────────────┤
│ 📖 Diário     │ 📅 Minha Semana │
│ 🔄 Retrospect.│ 📈 Evolução     │
│ 📚 Estudos    │ 🗓 Timeline     │
│ 💳 Transações │ 🏦 Contas       │
│ 🏷 Categorias │ 📊 Orçamentos   │
│ 🏠 Financiamentos               │
├─────────────────────────────────┤
│ ⚙️ Configurações                │
│ 🚪 Sair                         │
└─────────────────────────────────┘
```

---

## Padrões de UX Mobile

Estes padrões devem ser **consistentes em todas as telas**. São a chave para paridade com o web.

### 1. Bottom Sheets (substituem modais do web)

Use para: criar/editar itens, formulários, filtros, detalhes.

```ts
// Biblioteca: @gorhom/bottom-sheet
// Snap points: ['50%', '90%']
// Background: colors.card
// Handle: View 4x40, bg colors.muted, borderRadius 2
// Keyboard avoiding: keyboardBehavior="interactive"
```

### 2. Swipe Actions (substituem dropdowns de ação)

Use para: deletar (swipe esquerdo, fundo vermelho), ação rápida (swipe direito, fundo verde/azul).

```ts
// Biblioteca: react-native-gesture-handler (Swipeable)
// Delete: renderRightActions → View vermelho com ícone lixeira
// Complete/Pay: renderLeftActions → View verde com ícone check
```

### 3. Pull-to-Refresh

Em todas as listas e telas de dados.

```ts
<ScrollView
  refreshControl={
    <RefreshControl
      refreshing={isRefetching}
      onRefresh={refetch}
      tintColor={colors.primary}
    />
  }
/>
```

### 4. Estados de Loading

```ts
// Skeleton: SkeletonBox com pulse animation
// Usar exatamente o mesmo número de itens que o conteúdo real
// Ex: se lista tem ~5 itens, mostrar 5 SkeletonBox
```

### 5. Empty States

```ts
// <EmptyState>
// Ícone Lucide 48px, cor colors.muted
// Title: 16px semibold, colors.foreground
// Description: 14px, colors.muted
// Action button (quando aplicável)
```

### 6. Toasts

```ts
// Posição: bottom (acima do tab bar)
// Tipos: success (verde), error (vermelho), warning (amber), info (azul)
// Auto-dismiss: 4000ms
// Ícone à esquerda + mensagem + botão X
```

### 7. Confirmação de Delete

```ts
// Alert nativo: Alert.alert() OU
// ConfirmModal (bottom sheet vermelho)
// Sempre: título + descrição do que será deletado
// Botões: "Cancelar" (outline) + "Deletar" (destructive, vermelho)
```

### 8. Formulários

```ts
// react-hook-form + zod
// Erros: texto vermelho abaixo do input, 12px
// Input com foco: borda colors.primary (2px)
// Input com erro: borda colors.destructive
// Labels: 13px semibold, acima do input
// Botão submit: desabilitado durante loading, mostra ActivityIndicator
```

### 9. Navegação de Data

Para telas com conteúdo por data (Today, Diary, Transactions, Budgets):

```ts
// Header com [ ← ] [ título da data ] [ → ]
// Tap no título: abre DateTimePicker
// Formato: "Seg, 2 jun" (Today), "Jun 2026" (Transactions/Budgets)
```

### 10. Cores semânticas (badges)

Usar as mesmas do web:

```ts
const badgeColors = {
  success: { bg: '#D1FAE5', text: '#065F46' },
  warning: { bg: '#FEF3C7', text: '#92400E' },
  danger:  { bg: '#FEE2E2', text: '#991B1B' },
  info:    { bg: '#EDE9FE', text: '#5B21B6' },
  muted:   { bg: '#F1F5F9', text: '#475569' },
};
```

---

## Checklist de Paridade

Use para rastrear o progresso de cada etapa.

### Etapa 1 — Fundação
- [ ] Design system (`colors.ts`, `typography.ts`, `spacing.ts`)
- [ ] Componentes base: `AppText`, `AppCard`, `AppButton`, `AppInput`, `AppBadge`
- [ ] `ToastProvider` + `useToast`
- [ ] `lib/api.ts` com interceptor de auth + retry
- [ ] `lib/auth.ts` com SecureStore
- [ ] `hooks/useAuth.ts` (zustand)
- [ ] Tela Login
- [ ] Tela Register
- [ ] Root layout com guard de autenticação
- [ ] Tratamento do cookie de refresh token

### Etapa 2 — Meu Dia + Backlog
- [ ] Tela Today com 3 seções (atrasadas, pendentes, concluídas)
- [ ] Navegação de data com prev/next/picker
- [ ] Check-in com animação (circle → check)
- [ ] Swipe-to-delete em task items
- [ ] Bottom sheet criar tarefa
- [ ] `PriorityChips` component
- [ ] Tela Backlog com captura rápida
- [ ] Botão "Mover para hoje"
- [ ] Pull-to-refresh em ambas as telas

### Etapa 3 — Rituais
- [ ] Lista de hábitos com cards
- [ ] Badge colorido por área
- [ ] Barra de progresso semanal
- [ ] Streak atual e recorde
- [ ] Botão check-in (desabilitado após check-in do dia)
- [ ] Long press → menu (editar/pausar/deletar)
- [ ] Bottom sheet criar/editar hábito
- [ ] Hábitos pausados com visual diferente
- [ ] Filtro ativo/todos

### Etapa 4 — Minha Jornada
- [ ] Lista de metas com progresso
- [ ] Filtro por status (tabs)
- [ ] Tela de detalhe da meta
- [ ] Milestones com complete/reopen (tap)
- [ ] Modal atualizar progresso (slider + input)
- [ ] Mudar status (concluir/abandonar)
- [ ] Bottom sheet criar meta

### Etapa 5 — Diário
- [ ] Editor do dia com mood picker
- [ ] TextArea responsiva
- [ ] Sistema de tags (adicionar/remover)
- [ ] Detecção de entrada existente (criar vs atualizar)
- [ ] Navegação de data
- [ ] Modal/sheet de histórico
- [ ] Deletar entrada

### Etapa 6 — Minha Semana
- [ ] Plano da semana atual
- [ ] Lista de prioridades
- [ ] Bottom sheet criar semana
- [ ] Bottom sheet encerrar semana (review + rating)
- [ ] Histórico de semanas anteriores

### Etapa 7 — Dashboard
- [ ] Card XP/nível com progresso
- [ ] Card Hoje (resumo tarefas)
- [ ] Card Rituais (check-ins do dia)
- [ ] Card Finanças (saldo + receita/despesa)
- [ ] Card Humor (mini gráfico 7 dias)
- [ ] Navegação de mês
- [ ] Pull-to-refresh
- [ ] Saudação dinâmica por hora

### Etapa 8 — Finanças (núcleo)
- [ ] Tela Transactions agrupada por data (SectionList)
- [ ] Resumo do mês (receita/despesa/saldo)
- [ ] Swipe: deletar / marcar pago
- [ ] Bottom sheet detalhes + ações
- [ ] Bottom sheet criar transação
- [ ] Bottom sheet criar transferência
- [ ] Filtros avançados
- [ ] Navegação de mês
- [ ] Tela Contas com saldo total
- [ ] CRUD de contas
- [ ] Tela Orçamentos com BudgetBar
- [ ] CRUD de orçamentos
- [ ] Tela Categorias
- [ ] CRUD de categorias

### Etapa 9 — Finanças (avançado)
- [ ] Tela Financiamentos
- [ ] Tela detalhe financiamento
- [ ] Pagar parcela
- [ ] Tela Faturas (cartão de crédito)

### Etapa 10 — Módulos secundários
- [ ] Tela Retrospectivas (semanal/mensal)
- [ ] Tela Evolução
- [ ] Tela Estudos
- [ ] Tela Timeline
- [ ] Tela "Mais" (grid de atalhos)
- [ ] Tela Configurações (perfil, senha, logout)
- [ ] Dark mode toggle

---

> **Dica de implementação:** Entregue cada etapa do início ao fim antes de avançar. Sempre valide no dispositivo físico (não só no web do Expo) — o comportamento de teclado, touch e SafeArea é diferente.
