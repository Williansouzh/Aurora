# Handoff: Aurora Life OS — Finance module, Super Admin & "Quiet" design system

## Overview
Aurora is a personal **Life OS** that starts from a personal-finance core and grows module by module (My Day, Rituals, Journey, Studies, Money, plus a Super Admin for access control). This package documents the **desktop** product (with a strong focus on the **Finance module** and **Super Admin**), the **mobile** counterparts, and the **"Quiet" design system** that governs all of it.

The product voice: a calm, private space where someone watches themselves become who they want to be. The UI stays out of the way — warm paper neutrals, editorial serif headlines, a single indigo accent for action, and gamification (levels, streaks, XP) that whispers rather than shouts.

## About the Design Files
The files in this bundle are **design references created in HTML** — prototypes that show the intended look, layout and behavior. They are **not production code to copy directly**. They use a small in-house runtime (`support.js`, the `.dc.html` "Design Component" format) purely so the prototypes could be authored quickly.

**The task is to recreate these designs in the target codebase's environment** (React, Vue, SwiftUI, native, etc.), using its established component library, routing and state patterns. If no codebase exists yet, choose an appropriate stack (e.g. React + your preferred styling solution) and implement there. **Do not ship the `.dc.html` files or `support.js`** — treat them as visual + behavioral specs.

How to view them: open any `.dc.html` file in a browser. The interactive ones (Aurora.dc.html, Aurora - Finance.dc.html) have a working sidebar and a Light/Dark toggle in the top bar. The mobile files show several phone frames side-by-side on a gray canvas.

## Fidelity
**High-fidelity.** Final colors, typography, spacing and interaction states are all specified below and in the files. Recreate the UI pixel-accurately using the codebase's libraries. The exact hex values, font sizes and the two font families are intentional and listed in the Design Tokens section.

---

## Design Tokens

### Fonts
- **Display / numerals:** `Newsreader` (serif), weights 400–500, letter-spacing −0.01em. Used for headlines, screen titles, greetings, and all key numbers (balances, streak counts).
- **Text / UI:** `Hanken Grotesk` (sans), weights 400 / 500 / 600 / 700. Body, labels, nav, table data.
- Load both from Google Fonts. Uppercase overline labels use `letter-spacing: 0.12em; text-transform: uppercase; font-size: 12–13px; font-weight: 600`.

### Color — Light theme (default)
| Token | Hex | Use |
|---|---|---|
| `--bg` | `#f3f1ec` | App background (warm paper) |
| `--panel` | `#fcfcfb` | Cards / panels |
| `--side` | `#faf9f7` | Sidebar / table header / subtle fills |
| `--line` | `#eceae5` | Borders |
| `--line2` | `#f1efea` | Inner row dividers |
| `--ink` | `#26241f` | Primary text |
| `--ink2` | `#2b2925` | Strong text |
| `--mut` | `#6b665d` | Muted text |
| `--mut2` | `#8a857c` | Secondary muted |
| `--faint` | `#a8a299` | Faint labels / placeholders |
| `--chipline` | `#e6e3dc` | Chip / control borders |
| `--accent` | `#3d4eac` | Indigo — primary action, active nav, transfers |
| `--accentsoft` | `#eeece6` | Active nav fill / highlighted card |
| `--good` | `#6f8f6a` | Income / positive |
| `--warn` | `#c1976a` | Pending / caution |
| `--bad` | `#c1796a` | Expense / overdue / negative |
| `--track` | `#eceae5` | Progress-bar track / inactive dot |
| `--goodsoft` | `#e9f0e8` | "Paid" badge bg |
| `--warnsoft` | `#f6f0e6` | "Pending" badge bg |
| `--badsoft` | `#f6e9e6` | "Overdue" badge bg |

### Color — Dark theme
| Token | Hex |
|---|---|
| `--bg` | `#131214` |
| `--panel` | `#1b1a1f` |
| `--side` | `#171619` |
| `--line` | `#2a2930` |
| `--line2` | `#232228` |
| `--ink` | `#f1efe8` |
| `--ink2` | `#e8e6df` |
| `--mut` | `#a39e94` |
| `--mut2` | `#8a857c` |
| `--faint` | `#6f6a61` |
| `--chipline` | `#2f2e36` |
| `--accent` | `#8c93e8` |
| `--accentsoft` | `#26263a` |
| `--good` | `#8fae89` |
| `--warn` | `#d4ab78` |
| `--bad` | `#d18f86` |
| `--track` | `#2a2930` |
| `--goodsoft` | `#1e2820` |
| `--warnsoft` | `#2c2820` |
| `--badsoft` | `#2c211f` |

Implement theming as CSS variables (or your framework's token system) so a single toggle swaps the whole map. Account "color tags" use a fixed extra palette: `#7b5cd6` (purple), `#6f8f6a`, `#3d4eac`, `#c1976a`, `#c1796a`.

### Spacing scale (px)
`4, 8, 12, 16, 22, 32, 40, 56`. Card padding is typically 18–22px; screen padding 32–40px; gaps between cards 14–22px.

### Radius (px)
- Chips / progress bars / pills: `999px`
- Inputs & buttons: `8px`
- Stat tiles: `10px`
- Cards: `12–14px`
- Phone screen inner: `36px`; bezel `46px`

### Type scale
| Role | Family | Size / weight |
|---|---|---|
| Display XL (hero) | Newsreader | 46 / 400 |
| Display (screen title) | Newsreader | 32–33 / 400 |
| Numeral (balance, streak) | Newsreader | 26–34 / 400 |
| Card title | Hanken Grotesk | 16–18 / 600 |
| Body | Hanken Grotesk | 14–15 / 400 |
| Label / nav / row | Hanken Grotesk | 13–14 / 500–600 |
| Overline | Hanken Grotesk | 11–13 / 600, UPPERCASE, 0.12em |

### Shadows
Cards on the flat app surface use a **1px border, no shadow**. Elevated elements: FAB `0 6px 18px rgba(61,78,172,.4)`; mobile bezel `0 18px 50px rgba(0,0,0,.16)`. Keep shadows minimal — this is a flat, calm system.

---

## App shell (desktop) — used by Finance and the Life OS app
- **Layout:** fixed left **sidebar 240px** + flexible main column. Main column = a 60px top bar (border-bottom `--line`) over a scrollable content area, content `max-width` ~1180–1240px, padding 32–34px / 40px.
- **Sidebar:** logo (24px indigo radial-gradient circle + "Aurora" in Newsreader 22px), an uppercase section label, then nav items. **Nav item:** 9×12px padding, radius 8px, a 7px leading dot. **Active state:** background `--accentsoft`, text `--ink2`, weight 600, dot filled `--accent`. Inactive: text `--mut`, dot `--track`. Footer: avatar + name/email or a small summary card.
- **Top bar:** left = current screen title (14px/600). Right = contextual controls (month stepper, primary button) + a **theme toggle pill** ("Light"/"Dark", a 13px ring icon that fills when dark).
- **Theme toggle** swaps the entire CSS-variable map; persist the choice.

---

## Screens / Views

### FINANCE MODULE (primary focus) — `Aurora - Finance.dc.html` (desktop), `Aurora - Finance Mobile.dc.html` (mobile)
Sidebar: Dashboard · Transactions · Accounts · Categories · Financings. Top bar carries a **month stepper (◀ June 2026 ▶)** and **"+ New transaction"**. A sidebar footer card shows the month result (+$2,260, "12% better than May").

**1. Dashboard**
- Title "Financial overview" + subtitle (month · 4 accounts · 28 transactions).
- **4 KPI cards** (grid): Total balance `$14,280` (▲2.1% vs May), Income paid `$6,200` (good color, "$800 pending"), Expenses paid `$3,940` (bad color, "$1,262 pending"), **Result `+$2,260`** (accent card with `--accentsoft` bg, ▲12%).
- Two-column body. Left (flex 1.6): **Cash flow · 2026** — a 12-month grouped bar chart, each month = an income bar (`--good`) + expense bar (`--bad`); the current month's label is bold accent; future months are `--track` at 32% opacity. Below it: **Recent transactions** card (description, status badge, amount; "View all →" routes to Transactions).
- Right (flex 1): **Spending by category** (labeled progress bars: Housing 90%, Food 38%, Leisure 26%, Transport 22%, Subscriptions 10%, each its own color) and **Upcoming & overdue** (Rent today, Credit card Jun 28, Gym 2d late, Spotify Jun 25 — with colored date chips).

**2. Transactions**
- Title + subtitle ("28 records · $6,200 in · $3,940 out"). "+ New transaction" button.
- **Filter row:** dropdown chips Month / Year / All types / All status / All categories + a flexible **search field** ("Search description…").
- **Table** (panel, rounded, header row on `--side`): columns `Date | Description | Category | Account | Status | Amount`. Description has a leading 7px **type dot** — green=income, indigo=transfer, red=expense. Amount colored by type (income green, transfer muted, expense ink). **Status badge** one of Paid/Pending/Overdue/Cancelled (see below). 14 example rows are in the file, including a cancelled and a transfer row.

**3. Accounts**
- Title ("4 active · 1 archived · $14,280 total"). "+ New account".
- **Grid of account cards**, each with a **top border in the account's color** (3px), name + type/bank subtitle, a colored square tag, the **current balance** (Newsreader), and an initial-vs-current delta (▲/▼). Five types: Checking (`#7b5cd6`), Savings (`--good`), Investment (`--accent`), Cash (`--warn`), Credit card (`--bad`, shows negative balance + limit/used). A **dashed "Add account"** tile closes the grid.
- **Archived** section below: a single muted row with a "Restore" link.

**4. Categories**
- Title ("4 income · 8 expense"). "+ New category".
- **Two columns** (Income / Expense), each a panel with a colored header dot + uppercase label, then rows. **Row:** 30px rounded **icon chip** tinted with the category color (color at ~12% alpha bg, full color glyph), name, a sub-line (amount this month), and an optional "default" tag. Income: Salary, Freelance, Investments, Other. Expense: Housing, Food, Transport, Leisure, Subscriptions, Health, Education, Other.

**5. Financings**
- Title ("Apartment · SAC · 360 installments · 14 paid"). "+ Simulate financing".
- **4 KPI cards:** Asset value `$320,000`, Financed `$256,000` ($64k down · 9.5%/yr), Outstanding `$246,044` (bad color), Paid off `3.9%` (accent card, "14 of 360").
- Left (flex 1.7): **Amortization · SAC** table — columns `# | Due | Amort. | Interest | Fees | Total | Status`. SAC = fixed amortization (~$711), decreasing interest, decreasing total; status badges (first rows Paid, rest Pending). Showing installments 15–22 of 360.
- Right (flex 1): **SAC vs Price** comparison (SAC active card highlighted: 1st $2,738, total interest $117,440; Price: fixed $2,152, total interest $518,720) and a **Details** key/value list (Institution Caixa, Term 360 months, Interest 9.5%/yr, CET 10.2%/yr, Monthly insurance $48, First due May 2025) + "Simulate extra payment".

**Finance mobile** (`Aurora - Finance Mobile.dc.html`): same five views as 348×780 phone frames. Dashboard leads with a **dark balance card** (`#26241f` bg, light text, In/Out/Result). Transactions = search + segmented filter (All/Income/Expense/Pending) + transactions **grouped by day**, each row with a tinted category icon chip; a circular **FAB +** bottom-right. Bottom tab bar: Home / Activity / Accounts / Loans.

### SUPER ADMIN (focus) — inside `Aurora.dc.html`, nav item "Super Admin"
- Title "Super Admin" + subtitle ("Access control · plans · ship modules one at a time"). "+ Invite user".
- **Tab bar:** Users (active) · Plans · Modules · Life areas · Audit log. (Only Users is designed in hi-fi; the others are tabs to build.)
- **4 KPI cards:** Total users 248, Active plans 5, Modules live 7/12, Beta overrides 31.
- Left (flex 1.7): **Users table** — columns `User | Plan | Role | Status | Last seen`. User cell = avatar + name + email. Status badge bordered (Active green, Suspended red, Invited warn). The selected row is highlighted with `--accentsoft`. Six example users; roles include User / SuperAdmin / Support / Admin; plans Free / Early Access / Pro / Founder.
- Right (flex 1): **User detail panel** for the selected user (Ana Costa) — **Module access matrix**: rows = modules (Home, My Day, Rituals, Studies, Money, Evolution, Super Admin), columns `Plan` (✓/✗) and **Final** (Allowed / Beta / "Beta ⚑" override / Upgrade / Denied). This encodes the launch model: a module's availability = plan grant, optionally overridden per-user for beta. Buttons: "Change plan", "Grant module".

### LIFE OS (context) — `Aurora.dc.html` (desktop), `Aurora - Mobile.dc.html` (mobile)
Same shell. Sidebar: Home · My Day · Rituals · Journey · Studies · Money, then a "System" section with Super Admin. All have Light/Dark.
- **Home:** greeting (Newsreader), Level pill; left column Today·Top 3 checklist, Rituals-today streak tiles, Timeline feed; right column This-month money summary, In-focus goals (progress bars), Mood-this-week bars.
- **My Day:** Top-3 priority cards (big serif rank numeral watermark), full task checklist with times, an "Overdue · move to review" block, mood selector (5 circles), quick-capture input, mini-diary textarea.
- **Rituals:** 4 KPI cards (Done today 3/4, Longest streak 21, This week 86%, Consistency 82%); a **weekly grid** (ritual rows × 7 day cells, filled accent square = done) and a Today's-rituals checklist with 🔥 streak counts; an encouragement card.
- **Journey:** filter chips by life area; a 2×2 grid of **goal cards** (area label, target date, title, milestone count, progress bar in the area's color) + a detail panel (description, milestones checklist, linked rituals/skills).
- **Studies:** 4 KPI cards; "Next block" highlighted start-session card; **Active priorities** ranked list with scores; **Progress by stage** (Obtain→Organize→Memorize→Apply→Teach) bar chart; Reviews-due (spaced repetition, D+n), Top bottlenecks, Recent sessions (Feynman ratings).
- **Money (Life OS summary):** lighter version of the finance dashboard (month stepper, cash-flow bars, spending-by-category, accounts list, upcoming, recent).

**Life OS mobile** (`Aurora - Mobile.dc.html`): phone frames for Home, My Day, Studies, Journey, Money, Rituals, Super Admin. Bottom tab bar + FAB where a primary create action exists.

---

## Interactions & Behavior
- **Navigation:** sidebar items switch the main screen (single-page; no reload). Active item gets the accent treatment described above. Mobile uses a bottom tab bar with the same accent-dot active state.
- **Theme toggle:** swaps the full token map instantly; persist (localStorage or app setting).
- **Month stepper:** ◀ / ▶ change the active month for Finance/Money data.
- **Checkboxes:** unchecked = 1.5px `--track` border, 6px radius; checked = filled `--accent` with a white check; completed task text is `--faint` with strikethrough.
- **Filters:** dropdown chips and a segmented pill control (active segment = filled accent, others = bordered).
- **Buttons:** Primary = filled `--accent`, white text, radius 8, 600. Secondary = `--panel` bg + `--chipline` border. Text link = accent, no bg, often with "→". Destructive = `--bad` outline. Mobile FAB = 52px accent circle with the elevation shadow.
- **Hover (desktop):** nav items lighten toward `--accentsoft`; rows may use `--side`. Keep transitions subtle (~150ms).
- **Empty/loading/error states:** not yet designed — apply the calm system (muted `--faint` text, a thin illustration or icon, a single primary action). Flag these to design if needed.

## State Management
- `theme: 'light' | 'dark'` (persisted).
- `activeScreen` per app (router or local state).
- `activeMonth` for finance data.
- Finance data shapes (from the prototypes): **Account** {name, type, institution, color, initialBalance, currentBalance, limit?}, **Category** {name, kind: income|expense, color, icon, isDefault}, **Transaction** {date, description, categoryId, accountId, type: income|expense|transfer, status: paid|pending|overdue|cancelled, amount}, **Financing** {asset, method: SAC|Price, principal, downPayment, annualRate, termMonths, installments[] {n, due, amortization, interest, fees, total, status}}.
- Admin: **User** {name, email, avatar, plan, role, status: active|suspended|invited, lastSeen}, **ModuleAccess** {module, planGrants: bool, finalState: allowed|beta|upgrade|denied, override?: bool}.

## Assets
No raster images or external icon set were used — avatars are flat color circles and the few glyphs are Unicode placeholders. In the real app, use the codebase's existing icon library for nav, categories and actions, and real user avatars. The Aurora logo is a simple radial-gradient circle (`radial-gradient(circle at 30% 30%, #a7b0d6, #3d4eac)`) next to "Aurora" set in Newsreader — replace with a real logo if one exists.

## Files
- `Aurora - Finance.dc.html` — **desktop Finance module** (5 screens, nav, light/dark).
- `Aurora - Finance Mobile.dc.html` — **mobile Finance module** (5 phone frames).
- `Aurora.dc.html` — **desktop Life OS app incl. Super Admin** (7 screens, nav, light/dark).
- `Aurora - Mobile.dc.html` — **mobile Life OS** (7 phone frames).
- `Aurora - Design System.dc.html` — the **"Quiet" design system reference** (color tokens, type, components, patterns, dark theme).
- `support.js` — prototype runtime only; **do not port**.

Open any file in a browser to view. The Design System page is the fastest way to pull exact tokens and component styling.
