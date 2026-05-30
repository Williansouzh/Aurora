// Aurora Life OS demo seed
// Target user: williansouza4041@gmail.com
//
// This script is idempotent for the target user: it deletes demo data owned by
// the user and inserts a fresh, coherent scenario for finance + Life OS modules.

const TARGET_EMAIL = 'williansouza4041@gmail.com';
const dbName = db.getName();

const user = db.users.findOne({ Email: TARGET_EMAIL });
if (!user) {
  throw new Error(`User not found: ${TARGET_EMAIL}`);
}

const userId = String(user._id);
const now = new Date('2026-05-30T12:00:00Z');

print(`Seeding ${TARGET_EMAIL} (${userId}) on database ${dbName}`);

const ownedCollections = [
  'accounts',
  'transactions',
  'budgets',
  'transfers',
  'financings',
  'creditCardInvoices',
  'dailyTasks',
  'habits',
  'habitCheckIns',
  'timelineEvents',
  'goals',
  'weeklyPlans',
  'diaryEntries',
  'evolutionAlbums',
  'evolutionPhotos',
  'xpEntries',
];

for (const name of ownedCollections) {
  const result = db.getCollection(name).deleteMany({ UserId: userId });
  print(`${name}: removed ${result.deletedCount}`);
}

function D(value) {
  return NumberDecimal(Number(value).toFixed(2));
}

function at(date, hour = 12) {
  return new Date(`${date}T${String(hour).padStart(2, '0')}:00:00Z`);
}

function oid() {
  return new ObjectId();
}

function oidStr() {
  return String(oid());
}

function ensureCategories() {
  const existing = db.categories.find({ UserId: userId }).sort({ _id: 1 }).toArray();
  if (existing.length >= 8) return existing;

  const defaults = [
    ['Salario', 0, '#22c55e', 'wallet'],
    ['Freelance', 0, '#14b8a6', 'briefcase'],
    ['Investimentos', 0, '#0ea5e9', 'trending-up'],
    ['Moradia', 1, '#f97316', 'home'],
    ['Alimentacao', 1, '#ef4444', 'utensils'],
    ['Transporte', 1, '#6366f1', 'car'],
    ['Saude', 1, '#10b981', 'heart-pulse'],
    ['Lazer', 1, '#a855f7', 'party-popper'],
    ['Educacao', 1, '#3b82f6', 'book-open'],
    ['Assinaturas', 1, '#64748b', 'repeat'],
    ['Outros', 1, '#94a3b8', 'tag'],
  ];

  db.categories.insertMany(defaults.map(([name, type, color, icon]) => ({
    _id: oid(),
    UserId: userId,
    Name: name,
    Type: type,
    Color: color,
    Icon: icon,
    IsDefault: true,
    CreatedAt: now,
    UpdatedAt: now,
  })));

  return db.categories.find({ UserId: userId }).sort({ _id: 1 }).toArray();
}

const categories = ensureCategories();
const incomeCats = categories.filter(c => c.Type === 0);
const expenseCats = categories.filter(c => c.Type === 1);

const cat = {
  salario: String(incomeCats[0]._id),
  freelance: incomeCats[1] ? String(incomeCats[1]._id) : String(incomeCats[0]._id),
  invest: incomeCats[2] ? String(incomeCats[2]._id) : String(incomeCats[0]._id),
  moradia: String(expenseCats[0]._id),
  alimentacao: expenseCats[1] ? String(expenseCats[1]._id) : String(expenseCats[0]._id),
  transporte: expenseCats[2] ? String(expenseCats[2]._id) : String(expenseCats[0]._id),
  saude: expenseCats[3] ? String(expenseCats[3]._id) : String(expenseCats[0]._id),
  lazer: expenseCats[4] ? String(expenseCats[4]._id) : String(expenseCats[0]._id),
  educacao: expenseCats[5] ? String(expenseCats[5]._id) : String(expenseCats[0]._id),
  assinaturas: expenseCats[6] ? String(expenseCats[6]._id) : String(expenseCats[0]._id),
  outros: expenseCats[7] ? String(expenseCats[7]._id) : String(expenseCats[0]._id),
};

const checkingId = oid();
const savingsId = oid();
const cardId = oid();

const checkingIdStr = String(checkingId);
const savingsIdStr = String(savingsId);
const cardIdStr = String(cardId);

const PAID = 0;
const PENDING = 1;
const INCOME = 0;
const EXPENSE = 1;

const txDefs = [
  [cat.salario, 'Salario Janeiro', 9200, INCOME, PAID, '2026-01-05'],
  [cat.moradia, 'Aluguel', 1800, EXPENSE, PAID, '2026-01-10'],
  [cat.assinaturas, 'Netflix', 55, EXPENSE, PAID, '2026-01-10'],
  [cat.assinaturas, 'Spotify', 27, EXPENSE, PAID, '2026-01-10'],
  [cat.alimentacao, 'Mercado Municipal', 680, EXPENSE, PAID, '2026-01-12'],
  [cat.saude, 'Plano de saude', 220, EXPENSE, PAID, '2026-01-15'],
  [cat.transporte, 'Gasolina', 280, EXPENSE, PAID, '2026-01-18'],
  [cat.lazer, 'Restaurante com familia', 180, EXPENSE, PAID, '2026-01-20'],
  [cat.freelance, 'Freelance - site institucional', 1500, INCOME, PAID, '2026-01-30'],

  [cat.salario, 'Salario Fevereiro', 9200, INCOME, PAID, '2026-02-05'],
  [cat.moradia, 'Aluguel', 1800, EXPENSE, PAID, '2026-02-10'],
  [cat.assinaturas, 'Netflix', 55, EXPENSE, PAID, '2026-02-10'],
  [cat.alimentacao, 'Supermercado', 720, EXPENSE, PAID, '2026-02-14'],
  [cat.lazer, 'Jantar especial', 280, EXPENSE, PAID, '2026-02-14'],
  [cat.saude, 'Plano de saude', 220, EXPENSE, PAID, '2026-02-15'],
  [cat.transporte, 'Gasolina', 310, EXPENSE, PAID, '2026-02-18'],
  [cat.educacao, 'Curso Dev Full Stack', 129, EXPENSE, PAID, '2026-02-20'],

  [cat.salario, 'Salario Marco', 9200, INCOME, PAID, '2026-03-05'],
  [cat.freelance, 'Freelance - app mobile', 2200, INCOME, PAID, '2026-03-08'],
  [cat.moradia, 'Aluguel', 1800, EXPENSE, PAID, '2026-03-10'],
  [cat.assinaturas, 'Netflix', 55, EXPENSE, PAID, '2026-03-10'],
  [cat.alimentacao, 'Mercado Central', 850, EXPENSE, PAID, '2026-03-12'],
  [cat.saude, 'Dentista', 200, EXPENSE, PAID, '2026-03-20'],
  [cat.lazer, 'Cinema e jantar', 220, EXPENSE, PAID, '2026-03-22'],

  [cat.salario, 'Salario Abril', 9200, INCOME, PAID, '2026-04-05'],
  [cat.moradia, 'Aluguel', 1800, EXPENSE, PAID, '2026-04-10'],
  [cat.assinaturas, 'Netflix', 55, EXPENSE, PAID, '2026-04-10'],
  [cat.alimentacao, 'Supermercado', 750, EXPENSE, PAID, '2026-04-12'],
  [cat.transporte, 'Gasolina', 320, EXPENSE, PAID, '2026-04-18'],
  [cat.educacao, 'Curso Dev Full Stack', 129, EXPENSE, PAID, '2026-04-20'],
  [cat.lazer, 'Restaurante com amigos', 350, EXPENSE, PAID, '2026-04-22'],
  [cat.transporte, 'Revisao carro', 450, EXPENSE, PAID, '2026-04-25'],
  [cat.freelance, 'Consultoria tecnica', 1800, INCOME, PAID, '2026-04-29'],

  [cat.salario, 'Salario Maio', 9200, INCOME, PAID, '2026-05-05'],
  [cat.moradia, 'Aluguel', 1800, EXPENSE, PAID, '2026-05-10'],
  [cat.assinaturas, 'Netflix', 55, EXPENSE, PAID, '2026-05-10'],
  [cat.assinaturas, 'Spotify', 27, EXPENSE, PAID, '2026-05-10'],
  [cat.transporte, 'Gasolina', 260, EXPENSE, PAID, '2026-05-20'],
  [cat.alimentacao, 'Supermercado', 680, EXPENSE, PAID, '2026-05-22'],
  [cat.saude, 'Plano de saude', 220, EXPENSE, PENDING, '2026-05-28'],
  [cat.assinaturas, 'Amazon Prime', 19.9, EXPENSE, PENDING, '2026-05-30'],
  [cat.educacao, 'Curso Dev Full Stack', 129, EXPENSE, PENDING, '2026-05-30'],
  [cat.lazer, 'Shows e eventos', 280, EXPENSE, PENDING, '2026-05-31'],
];

let checkingBalance = 5000;
for (const [, , amount, type, status] of txDefs) {
  if (status === PAID) checkingBalance += type === INCOME ? amount : -amount;
}

const transferAmount = 1500;
const transferDates = ['2026-01-31', '2026-02-28', '2026-03-31', '2026-04-30', '2026-05-25'];
checkingBalance -= transferAmount * transferDates.length;
const savingsBalance = transferAmount * transferDates.length;

db.accounts.insertMany([
  {
    _id: checkingId,
    UserId: userId,
    Name: 'Conta Principal',
    Type: 0,
    InitialBalance: D(5000),
    CurrentBalance: D(checkingBalance),
    Color: '#2563eb',
    IsArchived: false,
    CreditLimit: D(0),
    ClosingDay: 10,
    DueDay: 15,
    CreatedAt: at('2026-01-01', 0),
    UpdatedAt: now,
  },
  {
    _id: savingsId,
    UserId: userId,
    Name: 'Reserva Aurora',
    Type: 1,
    InitialBalance: D(0),
    CurrentBalance: D(savingsBalance),
    Color: '#10b981',
    IsArchived: false,
    CreditLimit: D(0),
    ClosingDay: 0,
    DueDay: 0,
    CreatedAt: at('2026-01-01', 0),
    UpdatedAt: now,
  },
  {
    _id: cardId,
    UserId: userId,
    Name: 'Cartao Roxo',
    Type: 4,
    InitialBalance: D(0),
    CurrentBalance: D(0),
    Color: '#7c3aed',
    IsArchived: false,
    CreditLimit: D(5000),
    ClosingDay: 20,
    DueDay: 27,
    CreatedAt: at('2026-01-01', 0),
    UpdatedAt: now,
  },
]);

const txDocs = txDefs.map(([categoryId, description, amount, type, status, date]) => ({
  _id: oid(),
  UserId: userId,
  AccountId: checkingIdStr,
  CategoryId: categoryId,
  Description: description,
  Amount: D(amount),
  Type: type,
  Status: status,
  Date: at(date),
  DueDate: at(date),
  Notes: null,
  CreditCardInvoiceId: null,
  IsRecurring: false,
  RecurrenceType: null,
  RecurrenceInterval: 1,
  RecurrenceEndDate: null,
  RecurrenceGroupId: null,
  RecurrenceIndex: null,
  TotalInstallments: null,
  CreatedAt: at(date),
  UpdatedAt: now,
}));
db.transactions.insertMany(txDocs);

db.transfers.insertMany(transferDates.map(date => ({
  _id: oid(),
  UserId: userId,
  FromAccountId: checkingIdStr,
  ToAccountId: savingsIdStr,
  Amount: D(transferAmount),
  Date: at(date),
  Description: 'Transferencia mensal para reserva',
  Status: 0,
  CreatedAt: at(date),
  UpdatedAt: now,
})));

db.budgets.insertMany([
  [cat.alimentacao, 900],
  [cat.transporte, 400],
  [cat.saude, 350],
  [cat.lazer, 400],
  [cat.educacao, 250],
  [cat.assinaturas, 160],
].map(([categoryId, limit]) => ({
  _id: oid(),
  UserId: userId,
  CategoryId: categoryId,
  Month: 5,
  Year: 2026,
  LimitAmount: D(limit),
  CreatedAt: at('2026-05-01', 0),
  UpdatedAt: now,
})));

function buildSac(principal, annualRate, insurance, fees, termMonths, firstDueDate) {
  const monthlyRate = annualRate / 100 / 12;
  const baseAmort = Math.round(principal / termMonths * 100) / 100;
  const rows = [];
  let balance = principal;

  for (let n = 1; n <= termMonths; n++) {
    const opening = Math.round(balance * 100) / 100;
    const interest = Math.round(opening * monthlyRate * 100) / 100;
    const amortization = n === termMonths ? opening : baseAmort;
    balance = Math.round((opening - amortization) * 100) / 100;
    if (balance < 0) balance = 0;
    const due = new Date(firstDueDate);
    due.setUTCMonth(due.getUTCMonth() + n - 1);
    const total = Math.round((amortization + interest + insurance + fees) * 100) / 100;

    rows.push({
      Number: n,
      DueDate: due,
      OpeningBalance: D(opening),
      Amortization: D(amortization),
      Interest: D(interest),
      Insurance: D(insurance),
      Fees: D(fees),
      TotalPayment: D(total),
      ClosingBalance: D(balance),
      Status: n <= 16 ? 1 : 0,
      PaidAt: n <= 16 ? due : null,
      PaidAmount: n <= 16 ? D(total) : null,
      LinkedTransactionId: null,
    });
  }

  return rows;
}

db.financings.insertOne({
  _id: oid(),
  UserId: userId,
  Name: 'Apartamento Centro',
  Type: 0,
  AmortizationSystem: 0,
  Status: 0,
  Institution: 'Caixa Economica Federal',
  AssetValue: D(450000),
  DownPayment: D(90000),
  FinancedAmount: D(360000),
  AnnualInterestRate: D(11.5),
  MonthlyInsurance: D(120),
  MonthlyFees: D(35),
  CetAnnualRate: null,
  TermMonths: 360,
  FirstDueDate: at('2025-02-01', 0),
  Installments: buildSac(360000, 11.5, 120, 35, 360, at('2025-02-01', 0)),
  LinkedAccountId: checkingIdStr,
  Notes: 'Financiamento de apartamento para acompanhar amortizacao e parcelas.',
  PropertyAddress: 'Rua das Flores, 123 - Centro',
  PropertyRegistration: '12.345-6',
  VehicleBrand: null,
  VehicleModel: null,
  VehicleYear: null,
  VehiclePlate: null,
  CreatedAt: at('2025-01-20', 0),
  UpdatedAt: now,
});

const habitRunId = oidStr();
const habitReadId = oidStr();
const habitStudyId = oidStr();
const habitJournalId = oidStr();
const habitMoneyId = oidStr();

db.habits.insertMany([
  {
    _id: ObjectId(habitRunId),
    UserId: userId,
    Name: 'Treinar ou caminhar',
    Description: 'Movimento diario para energia e saude.',
    Area: 1,
    FrequencyType: 1,
    DaysOfWeek: [1, 2, 3, 4, 5, 6],
    TimesPerWeek: 5,
    Difficulty: 2,
    XpReward: 10,
    CurrentStreak: 12,
    BestStreak: 18,
    IsActive: true,
    CreatedAt: at('2026-05-01', 8),
    UpdatedAt: now,
  },
  {
    _id: ObjectId(habitReadId),
    UserId: userId,
    Name: 'Ler 20 minutos',
    Description: 'Leitura leve antes de dormir.',
    Area: 3,
    FrequencyType: 1,
    DaysOfWeek: [1, 2, 3, 4, 5, 6, 0],
    TimesPerWeek: 7,
    Difficulty: 1,
    XpReward: 5,
    CurrentStreak: 8,
    BestStreak: 14,
    IsActive: true,
    CreatedAt: at('2026-05-02', 8),
    UpdatedAt: now,
  },
  {
    _id: ObjectId(habitStudyId),
    UserId: userId,
    Name: 'Estudar o Aurora',
    Description: 'Evoluir um modulo por vez.',
    Area: 9,
    FrequencyType: 2,
    DaysOfWeek: [1, 3, 5],
    TimesPerWeek: 3,
    Difficulty: 3,
    XpReward: 20,
    CurrentStreak: 4,
    BestStreak: 4,
    IsActive: true,
    CreatedAt: at('2026-05-04', 8),
    UpdatedAt: now,
  },
  {
    _id: ObjectId(habitJournalId),
    UserId: userId,
    Name: 'Check-in emocional',
    Description: 'Registrar humor e uma reflexao curta.',
    Area: 8,
    FrequencyType: 1,
    DaysOfWeek: [1, 2, 3, 4, 5, 6, 0],
    TimesPerWeek: 7,
    Difficulty: 1,
    XpReward: 5,
    CurrentStreak: 5,
    BestStreak: 9,
    IsActive: true,
    CreatedAt: at('2026-05-10', 8),
    UpdatedAt: now,
  },
  {
    _id: ObjectId(habitMoneyId),
    UserId: userId,
    Name: 'Revisar gastos',
    Description: 'Olhar transacoes e evitar surpresas no fim do mes.',
    Area: 4,
    FrequencyType: 2,
    DaysOfWeek: [2, 6],
    TimesPerWeek: 2,
    Difficulty: 2,
    XpReward: 10,
    CurrentStreak: 3,
    BestStreak: 6,
    IsActive: true,
    CreatedAt: at('2026-05-01', 8),
    UpdatedAt: now,
  },
]);

const checkInDocs = [];
for (let day = 19; day <= 30; day++) {
  const date = `2026-05-${String(day).padStart(2, '0')}`;
  checkInDocs.push({
    _id: oid(),
    UserId: userId,
    HabitId: habitRunId,
    Date: at(date, 7),
    Status: 1,
    Note: day === 30 ? 'Treino curto, mas feito.' : null,
    PhotoUrl: null,
    XpGenerated: 10,
    CreatedAt: at(date, 7),
    UpdatedAt: now,
  });

  if (day >= 23) {
    checkInDocs.push({
      _id: oid(),
      UserId: userId,
      HabitId: habitReadId,
      Date: at(date, 22),
      Status: 1,
      Note: null,
      PhotoUrl: null,
      XpGenerated: 5,
      CreatedAt: at(date, 22),
      UpdatedAt: now,
    });
  }
}

['2026-05-20', '2026-05-22', '2026-05-25', '2026-05-27', '2026-05-29'].forEach(date => {
  checkInDocs.push({
    _id: oid(),
    UserId: userId,
    HabitId: habitStudyId,
    Date: at(date, 20),
    Status: 1,
    Note: 'Sessao focada no plano Life OS.',
    PhotoUrl: null,
    XpGenerated: 20,
    CreatedAt: at(date, 20),
    UpdatedAt: now,
  });
});

['2026-05-26', '2026-05-27', '2026-05-28', '2026-05-29', '2026-05-30'].forEach(date => {
  checkInDocs.push({
    _id: oid(),
    UserId: userId,
    HabitId: habitJournalId,
    Date: at(date, 21),
    Status: 1,
    Note: 'Check-in registrado.',
    PhotoUrl: null,
    XpGenerated: 5,
    CreatedAt: at(date, 21),
    UpdatedAt: now,
  });
});

['2026-05-24', '2026-05-27', '2026-05-30'].forEach(date => {
  checkInDocs.push({
    _id: oid(),
    UserId: userId,
    HabitId: habitMoneyId,
    Date: at(date, 18),
    Status: 1,
    Note: 'Gastos revisados e categorizados.',
    PhotoUrl: null,
    XpGenerated: 10,
    CreatedAt: at(date, 18),
    UpdatedAt: now,
  });
});

db.habitCheckIns.insertMany(checkInDocs);

db.dailyTasks.insertMany([
  ['Finalizar documento Life OS', 'Revisar estrategia e quebrar em proximos tickets.', 3, 2, '2026-05-29', '2026-05-29T18:00:00Z'],
  ['Ajustar dashboard financeiro', 'Melhorar leitura dos cards principais.', 2, 2, '2026-05-29', '2026-05-29T16:30:00Z'],
  ['Treino de costas', 'Manter consistencia do habito.', 2, 2, '2026-05-29', '2026-05-29T07:30:00Z'],
  ['Planejar o modulo Hoje', 'Definir entidades, endpoints e tela inicial.', 3, 1, '2026-05-30', null],
  ['Registrar reflexao do dia', 'Anotar energia, foco e aprendizados.', 2, 1, '2026-05-30', null],
  ['Revisar gastos pendentes', 'Conferir despesas de maio antes do fechamento.', 3, 1, '2026-05-30', null],
  ['Separar prioridades da semana', 'Preparar segunda-feira sem pressa.', 2, 1, '2026-05-31', null],
].map(([title, notes, priority, status, date, completedAt]) => ({
  _id: oid(),
  UserId: userId,
  Title: title,
  Notes: notes,
  Priority: priority,
  Status: status,
  Date: at(date, 9),
  CompletedAt: completedAt ? new Date(completedAt) : null,
  CreatedAt: at(date, 8),
  UpdatedAt: now,
})));

const goalAuroraId = oidStr();
const goalBooksId = oidStr();
const goalReserveId = oidStr();
const goalShapeId = oidStr();

db.goals.insertMany([
  {
    _id: ObjectId(goalAuroraId),
    UserId: userId,
    Title: 'Transformar Aurora em Life OS',
    Description: 'Evoluir o produto modulo por modulo ate virar uma rede privada da propria vida.',
    Area: 9,
    Status: 1,
    StartDate: at('2026-05-01', 0),
    TargetDate: at('2026-08-31', 0),
    MetricType: 2,
    TargetValue: D(100),
    CurrentValue: D(28),
    CoverImage: null,
    Milestones: [
      { Id: 'aurora-doc', Title: 'Documentacao estrategica criada', IsRequired: true, IsCompleted: true, CompletedAt: at('2026-05-29') },
      { Id: 'aurora-today', Title: 'Modulo Meu Dia funcional', IsRequired: true, IsCompleted: false, CompletedAt: null },
      { Id: 'aurora-habits', Title: 'Rituais com streak e XP', IsRequired: true, IsCompleted: false, CompletedAt: null },
      { Id: 'aurora-timeline', Title: 'Linha da Vida basica', IsRequired: true, IsCompleted: false, CompletedAt: null },
    ],
    CreatedAt: at('2026-05-01', 9),
    UpdatedAt: now,
  },
  {
    _id: ObjectId(goalBooksId),
    UserId: userId,
    Title: 'Ler 12 livros em 2026',
    Description: 'Construir repertorio com leitura constante.',
    Area: 3,
    Status: 1,
    StartDate: at('2026-01-01', 0),
    TargetDate: at('2026-12-31', 0),
    MetricType: 1,
    TargetValue: D(12),
    CurrentValue: D(4),
    CoverImage: null,
    Milestones: [
      { Id: 'book-1', Title: 'Primeiro livro concluido', IsRequired: true, IsCompleted: true, CompletedAt: at('2026-02-05') },
      { Id: 'book-4', Title: 'Quatro livros concluidos', IsRequired: true, IsCompleted: true, CompletedAt: at('2026-05-20') },
      { Id: 'book-8', Title: 'Oito livros concluidos', IsRequired: true, IsCompleted: false, CompletedAt: null },
      { Id: 'book-12', Title: 'Meta anual concluida', IsRequired: true, IsCompleted: false, CompletedAt: null },
    ],
    CreatedAt: at('2026-01-01', 9),
    UpdatedAt: now,
  },
  {
    _id: ObjectId(goalReserveId),
    UserId: userId,
    Title: 'Montar reserva de R$ 10.000',
    Description: 'Guardar dinheiro com consistencia mensal.',
    Area: 4,
    Status: 1,
    StartDate: at('2026-01-01', 0),
    TargetDate: at('2026-07-31', 0),
    MetricType: 1,
    TargetValue: D(10000),
    CurrentValue: D(savingsBalance),
    CoverImage: null,
    Milestones: [
      { Id: 'reserve-2500', Title: 'R$ 2.500 guardados', IsRequired: true, IsCompleted: true, CompletedAt: at('2026-02-28') },
      { Id: 'reserve-5000', Title: 'R$ 5.000 guardados', IsRequired: true, IsCompleted: true, CompletedAt: at('2026-04-30') },
      { Id: 'reserve-10000', Title: 'Reserva completa', IsRequired: true, IsCompleted: false, CompletedAt: null },
    ],
    CreatedAt: at('2026-01-01', 9),
    UpdatedAt: now,
  },
  {
    _id: ObjectId(goalShapeId),
    UserId: userId,
    Title: 'Treinar 4x por semana',
    Description: 'Ganhar energia e manter consistencia fisica.',
    Area: 1,
    Status: 1,
    StartDate: at('2026-05-01', 0),
    TargetDate: at('2026-06-30', 0),
    MetricType: 2,
    TargetValue: D(100),
    CurrentValue: D(62),
    CoverImage: null,
    Milestones: [
      { Id: 'shape-week-1', Title: 'Primeira semana completa', IsRequired: true, IsCompleted: true, CompletedAt: at('2026-05-10') },
      { Id: 'shape-week-2', Title: 'Duas semanas completas', IsRequired: true, IsCompleted: true, CompletedAt: at('2026-05-18') },
      { Id: 'shape-month', Title: 'Um mes consistente', IsRequired: true, IsCompleted: false, CompletedAt: null },
    ],
    CreatedAt: at('2026-05-01', 9),
    UpdatedAt: now,
  },
]);

db.weeklyPlans.insertMany([
  {
    _id: oid(),
    UserId: userId,
    WeekStart: at('2026-05-18', 0),
    WeekEnd: at('2026-05-24', 23),
    MainFocus: 'Organizar a base do Aurora Life OS',
    LinkedGoalIds: [goalAuroraId],
    Priorities: ['Documentar visao', 'Revisar dashboard', 'Manter treinos'],
    Notes: 'Semana para sair da ideia e virar plano concreto.',
    Status: 3,
    Review: 'Boa semana. A visao ficou clara e o financeiro ja serve como base.',
    XpGenerated: 30,
    ClosedAt: at('2026-05-24', 21),
    CreatedAt: at('2026-05-18', 8),
    UpdatedAt: now,
  },
  {
    _id: oid(),
    UserId: userId,
    WeekStart: at('2026-05-25', 0),
    WeekEnd: at('2026-05-31', 23),
    MainFocus: 'Comecar a transformar planejamento em produto',
    LinkedGoalIds: [goalAuroraId, goalReserveId],
    Priorities: ['Popular ambiente demo', 'Desenhar modulo Meu Dia', 'Fechar maio com clareza'],
    Notes: 'Foco em criar uma experiencia visual para validar o Life OS.',
    Status: 2,
    Review: null,
    XpGenerated: 0,
    ClosedAt: null,
    CreatedAt: at('2026-05-25', 8),
    UpdatedAt: now,
  },
]);

db.diaryEntries.insertMany([
  {
    _id: oid(),
    UserId: userId,
    Date: at('2026-05-26', 21),
    Content: 'Hoje eu consegui enxergar o Aurora como algo maior do que financeiro. A ideia de Linha da Vida parece forte.',
    Mood: 4,
    Tags: ['aurora', 'produto', 'clareza'],
    Photos: [],
    IsPrivate: true,
    CreatedAt: at('2026-05-26', 21),
    UpdatedAt: now,
  },
  {
    _id: oid(),
    UserId: userId,
    Date: at('2026-05-28', 21),
    Content: 'Dia corrido, mas mantive o treino e revisei gastos. Pequenas consistencias estao se acumulando.',
    Mood: 3,
    Tags: ['rotina', 'financas', 'treino'],
    Photos: [],
    IsPrivate: true,
    CreatedAt: at('2026-05-28', 21),
    UpdatedAt: now,
  },
  {
    _id: oid(),
    UserId: userId,
    Date: at('2026-05-30', 21),
    Content: 'Com dados reais de exemplo fica mais facil sentir o produto. Proximo passo: deixar o Meu Dia gostoso de usar.',
    Mood: 5,
    Tags: ['life-os', 'foco', 'planejamento'],
    Photos: [],
    IsPrivate: true,
    CreatedAt: at('2026-05-30', 21),
    UpdatedAt: now,
  },
]);

const albumShapeId = oidStr();
const albumProjectId = oidStr();
db.evolutionAlbums.insertMany([
  {
    _id: ObjectId(albumShapeId),
    UserId: userId,
    Title: 'Evolucao fisica',
    Area: 1,
    Description: 'Fotos e marcos da consistencia nos treinos.',
    CoverImage: '/demo/evolution/shape-2026-05-30.jpg',
    IsPrivate: true,
    CreatedAt: at('2026-05-01', 9),
    UpdatedAt: now,
  },
  {
    _id: ObjectId(albumProjectId),
    UserId: userId,
    Title: 'Aurora Life OS',
    Area: 9,
    Description: 'Registros visuais da evolucao do produto.',
    CoverImage: '/demo/evolution/aurora-dashboard.jpg',
    IsPrivate: true,
    CreatedAt: at('2026-05-20', 9),
    UpdatedAt: now,
  },
]);

db.evolutionPhotos.insertMany([
  {
    _id: oid(),
    UserId: userId,
    AlbumId: albumShapeId,
    ImageUrl: '/demo/evolution/shape-2026-05-10.jpg',
    Caption: 'Primeira semana consistente.',
    Date: at('2026-05-10', 8),
    Tags: ['treino', 'inicio'],
    LinkedGoalId: goalShapeId,
    LinkedHabitId: habitRunId,
    CreatedAt: at('2026-05-10', 8),
    UpdatedAt: now,
  },
  {
    _id: oid(),
    UserId: userId,
    AlbumId: albumShapeId,
    ImageUrl: '/demo/evolution/shape-2026-05-30.jpg',
    Caption: '12 dias de streak no treino.',
    Date: at('2026-05-30', 8),
    Tags: ['treino', 'streak'],
    LinkedGoalId: goalShapeId,
    LinkedHabitId: habitRunId,
    CreatedAt: at('2026-05-30', 8),
    UpdatedAt: now,
  },
  {
    _id: oid(),
    UserId: userId,
    AlbumId: albumProjectId,
    ImageUrl: '/demo/evolution/aurora-dashboard.jpg',
    Caption: 'Primeira visao do Aurora como Life OS.',
    Date: at('2026-05-29', 18),
    Tags: ['aurora', 'produto'],
    LinkedGoalId: goalAuroraId,
    LinkedHabitId: habitStudyId,
    CreatedAt: at('2026-05-29', 18),
    UpdatedAt: now,
  },
]);

const timelineDocs = [
  [1, 1, 'Treino concluido', '12 dias de consistencia no treino.', '2026-05-30', 'Habits', habitRunId, false],
  [1, 3, 'Leitura registrada', '20 minutos de leitura antes de dormir.', '2026-05-30', 'Habits', habitReadId, false],
  [2, 9, 'Tarefa concluida', 'Documento Life OS revisado e transformado em plano.', '2026-05-29', 'Today', null, true],
  [3, 9, 'Meta avancou', 'Transformar Aurora em Life OS chegou a 28%.', '2026-05-29', 'Goals', goalAuroraId, true],
  [5, 8, 'Reflexao escrita', 'Check-in emocional registrado no diario.', '2026-05-30', 'Diary', null, false],
  [6, 1, 'Foto de evolucao adicionada', 'Nova foto no album Evolucao fisica.', '2026-05-30', 'EvolutionPhotos', albumShapeId, true],
  [7, 9, 'Semana revisada', 'Revisao semanal fechada com foco em produto.', '2026-05-24', 'WeeklyPlanning', null, true],
  [10, 9, 'Ideia central definida', 'Aurora como rede social privada da propria evolucao.', '2026-05-29', 'Timeline', null, true],
  [10, 4, 'Radar financeiro', 'Reserva chegou a R$ 7.500 em maio.', '2026-05-25', 'Finances', savingsIdStr, false],
];

db.timelineEvents.insertMany(timelineDocs.map(([type, area, title, description, date, sourceModule, sourceId, favorite]) => ({
  _id: oid(),
  UserId: userId,
  Type: type,
  Area: area,
  Title: title,
  Description: description,
  OccurredAt: at(date, 12),
  SourceModule: sourceModule,
  SourceId: sourceId,
  Visibility: 1,
  MediaUrls: type === 6 ? ['/demo/evolution/shape-2026-05-30.jpg'] : [],
  IsHidden: false,
  IsFavorite: favorite,
  CreatedAt: at(date, 12),
  UpdatedAt: now,
})));

const xpEntries = [
  [1, 10, 'Check-in: Treinar ou caminhar', '2026-05-30'],
  [1, 5, 'Check-in: Ler 20 minutos', '2026-05-30'],
  [1, 20, 'Check-in: Estudar o Aurora', '2026-05-29'],
  [2, 30, 'Revisao semanal fechada', '2026-05-24'],
  [4, 40, 'Milestone: documentacao estrategica criada', '2026-05-29'],
  [6, 5, 'Reflexao diaria registrada', '2026-05-30'],
  [7, 5, 'Tarefa importante concluida', '2026-05-29'],
  [8, 50, 'Conquista: primeira semana revisada', '2026-05-24'],
];

let totalXp = 0;
const xpDocs = xpEntries.map(([source, amount, description, date]) => {
  totalXp += amount;
  return {
    _id: oid(),
    UserId: userId,
    Source: source,
    Amount: amount,
    Description: description,
    OccurredAt: at(date, 12),
    CreatedAt: at(date, 12),
    UpdatedAt: now,
  };
});
db.xpEntries.insertMany(xpDocs);

db.users.updateOne(
  { _id: user._id },
  {
    $set: {
      TotalXp: totalXp,
      Level: 2,
      Achievements: ['first_weekly_review', 'first_goal_milestone', 'habit_7_day_streak'],
      UpdatedAt: now,
    },
  },
);

print('Inserted:');
print(`- accounts: ${db.accounts.countDocuments({ UserId: userId })}`);
print(`- transactions: ${db.transactions.countDocuments({ UserId: userId })}`);
print(`- transfers: ${db.transfers.countDocuments({ UserId: userId })}`);
print(`- budgets: ${db.budgets.countDocuments({ UserId: userId })}`);
print(`- financings: ${db.financings.countDocuments({ UserId: userId })}`);
print(`- dailyTasks: ${db.dailyTasks.countDocuments({ UserId: userId })}`);
print(`- habits: ${db.habits.countDocuments({ UserId: userId })}`);
print(`- habitCheckIns: ${db.habitCheckIns.countDocuments({ UserId: userId })}`);
print(`- goals: ${db.goals.countDocuments({ UserId: userId })}`);
print(`- weeklyPlans: ${db.weeklyPlans.countDocuments({ UserId: userId })}`);
print(`- diaryEntries: ${db.diaryEntries.countDocuments({ UserId: userId })}`);
print(`- evolutionAlbums: ${db.evolutionAlbums.countDocuments({ UserId: userId })}`);
print(`- evolutionPhotos: ${db.evolutionPhotos.countDocuments({ UserId: userId })}`);
print(`- timelineEvents: ${db.timelineEvents.countDocuments({ UserId: userId })}`);
print(`- xpEntries: ${db.xpEntries.countDocuments({ UserId: userId })}`);
print(`Total XP: ${totalXp}`);
print('Seed completed.');
