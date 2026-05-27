// Aurora seed — conta williansouza4041@gmail.com
const userId    = ObjectId('6a16e82fbe603c1440491a66');
const userIdStr = '6a16e82fbe603c1440491a66';

// Categories (já existem)
const catSalario   = '6a16e82fbe603c1440491a67';
const catFreelance = '6a16e82fbe603c1440491a68';
const catInvest    = '6a16e82fbe603c1440491a69';
const catMoradia   = '6a16e82fbe603c1440491a6a';
const catAlim      = '6a16e82fbe603c1440491a6b';
const catTransp    = '6a16e82fbe603c1440491a6c';
const catSaude     = '6a16e82fbe603c1440491a6d';
const catLazer     = '6a16e82fbe603c1440491a6e';
const catEduc      = '6a16e82fbe603c1440491a6f';
const catSubs      = '6a16e82fbe603c1440491a70';
const catOutros    = '6a16e82fbe603c1440491a71';

const PAID    = 0;
const PENDING = 1;
const INCOME  = 0;
const EXPENSE = 1;

// Account IDs
const contaId = new ObjectId();
const poupId  = new ObjectId();
const nuId    = new ObjectId();

const contaIdStr = contaId.toString();
const poupIdStr  = poupId.toString();
const nuIdStr    = nuId.toString();

// ─── TRANSACTIONS ─────────────────────────────────────────────────────────────
// [catId, description, amount, type, status, date]
const txDefs = [
  // ===== JANEIRO 2026 =====
  [catSalario,   'Salário Janeiro',                 9200.00, INCOME,  PAID,    '2026-01-05'],
  [catMoradia,   'Aluguel',                         1800.00, EXPENSE, PAID,    '2026-01-10'],
  [catSubs,      'Netflix',                           55.00, EXPENSE, PAID,    '2026-01-10'],
  [catSubs,      'Spotify',                           27.00, EXPENSE, PAID,    '2026-01-10'],
  [catAlim,      'Mercado Municipal',                680.00, EXPENSE, PAID,    '2026-01-12'],
  [catSaude,     'Plano de Saude',                  220.00, EXPENSE, PAID,    '2026-01-15'],
  [catTransp,    'Gasolina',                        280.00, EXPENSE, PAID,    '2026-01-18'],
  [catLazer,     'Restaurante com familia',          180.00, EXPENSE, PAID,    '2026-01-20'],
  [catTransp,    'Uber',                            120.00, EXPENSE, PAID,    '2026-01-22'],
  [catSaude,     'Farmacia',                         85.00, EXPENSE, PAID,    '2026-01-25'],
  [catSubs,      'Amazon Prime',                     19.90, EXPENSE, PAID,    '2026-01-28'],
  [catFreelance, 'Projeto Freelance - Site empresa', 1500.00, INCOME, PAID,   '2026-01-30'],

  // ===== FEVEREIRO 2026 =====
  [catSalario,   'Salario Fevereiro',               9200.00, INCOME,  PAID,    '2026-02-05'],
  [catMoradia,   'Aluguel',                         1800.00, EXPENSE, PAID,    '2026-02-10'],
  [catSubs,      'Netflix',                           55.00, EXPENSE, PAID,    '2026-02-10'],
  [catSubs,      'Spotify',                           27.00, EXPENSE, PAID,    '2026-02-10'],
  [catAlim,      'Supermercado',                    720.00, EXPENSE, PAID,    '2026-02-14'],
  [catLazer,     'Jantar especial',                 280.00, EXPENSE, PAID,    '2026-02-14'],
  [catSaude,     'Plano de Saude',                  220.00, EXPENSE, PAID,    '2026-02-15'],
  [catTransp,    'Gasolina',                        310.00, EXPENSE, PAID,    '2026-02-18'],
  [catEduc,      'Curso Dev Full Stack',             129.00, EXPENSE, PAID,    '2026-02-20'],
  [catSaude,     'Consulta medica',                  95.00, EXPENSE, PAID,    '2026-02-25'],
  [catSubs,      'Amazon Prime',                     19.90, EXPENSE, PAID,    '2026-02-28'],

  // ===== MARÇO 2026 =====
  [catSalario,   'Salario Marco',                   9200.00, INCOME,  PAID,    '2026-03-05'],
  [catFreelance, 'Projeto Freelance - App Mobile',  2200.00, INCOME,  PAID,    '2026-03-08'],
  [catMoradia,   'Aluguel',                         1800.00, EXPENSE, PAID,    '2026-03-10'],
  [catSubs,      'Netflix',                           55.00, EXPENSE, PAID,    '2026-03-10'],
  [catSubs,      'Spotify',                           27.00, EXPENSE, PAID,    '2026-03-10'],
  [catAlim,      'Mercado Central',                  850.00, EXPENSE, PAID,    '2026-03-12'],
  [catSaude,     'Plano de Saude',                  220.00, EXPENSE, PAID,    '2026-03-15'],
  [catTransp,    'Gasolina',                        290.00, EXPENSE, PAID,    '2026-03-18'],
  [catSaude,     'Dentista',                        200.00, EXPENSE, PAID,    '2026-03-20'],
  [catLazer,     'Cinema e jantar',                 220.00, EXPENSE, PAID,    '2026-03-22'],
  [catAlim,      'Padaria e acougue',               320.00, EXPENSE, PAID,    '2026-03-25'],
  [catSubs,      'Amazon Prime',                     19.90, EXPENSE, PAID,    '2026-03-28'],

  // ===== ABRIL 2026 =====
  [catSalario,   'Salario Abril',                   9200.00, INCOME,  PAID,    '2026-04-05'],
  [catMoradia,   'Aluguel',                         1800.00, EXPENSE, PAID,    '2026-04-10'],
  [catSubs,      'Netflix',                           55.00, EXPENSE, PAID,    '2026-04-10'],
  [catSubs,      'Spotify',                           27.00, EXPENSE, PAID,    '2026-04-10'],
  [catAlim,      'Supermercado',                    750.00, EXPENSE, PAID,    '2026-04-12'],
  [catSaude,     'Plano de Saude',                  220.00, EXPENSE, PAID,    '2026-04-15'],
  [catTransp,    'Gasolina',                        320.00, EXPENSE, PAID,    '2026-04-18'],
  [catEduc,      'Curso Dev Full Stack',             129.00, EXPENSE, PAID,    '2026-04-20'],
  [catLazer,     'Bar e restaurante com amigos',    350.00, EXPENSE, PAID,    '2026-04-22'],
  [catTransp,    'Revisao carro',                   450.00, EXPENSE, PAID,    '2026-04-25'],
  [catSubs,      'Amazon Prime',                     19.90, EXPENSE, PAID,    '2026-04-28'],
  [catFreelance, 'Consultoria tecnica',             1800.00, INCOME,  PAID,    '2026-04-29'],

  // ===== MAIO 2026 (mes atual) =====
  [catSalario,   'Salario Maio',                    9200.00, INCOME,  PAID,    '2026-05-05'],
  [catMoradia,   'Aluguel',                         1800.00, EXPENSE, PAID,    '2026-05-10'],
  [catSubs,      'Netflix',                           55.00, EXPENSE, PAID,    '2026-05-10'],
  [catSubs,      'Spotify',                           27.00, EXPENSE, PAID,    '2026-05-10'],
  [catTransp,    'Gasolina',                        260.00, EXPENSE, PAID,    '2026-05-20'],
  [catAlim,      'Supermercado',                    680.00, EXPENSE, PAID,    '2026-05-22'],
  [catSaude,     'Plano de Saude',                  220.00, EXPENSE, PENDING, '2026-05-28'],
  [catSubs,      'Amazon Prime',                     19.90, EXPENSE, PENDING, '2026-05-30'],
  [catEduc,      'Curso Dev Full Stack',             129.00, EXPENSE, PENDING, '2026-05-30'],
  [catLazer,     'Shows e eventos',                 280.00, EXPENSE, PENDING, '2026-05-31'],
];

// Compute checking balance from paid transactions
let contaBalance = 5000;
for (const [,,amount,type,status] of txDefs) {
  if (status === PAID) contaBalance += (type === INCOME ? amount : -amount);
}
// Subtract monthly transfers to poupanca
const transferAmt = 1500;
const numTransfers = 5;
contaBalance -= transferAmt * numTransfers;
const poupBalance = transferAmt * numTransfers;

print('Conta balance: R$ ' + contaBalance.toFixed(2));
print('Poupanca balance: R$ ' + poupBalance.toFixed(2));

// ─── INSERT ACCOUNTS ──────────────────────────────────────────────────────────
db.accounts.insertMany([
  {
    _id: contaId,
    UserId: userIdStr,
    Name: 'Conta Principal',
    Type: 0,
    InitialBalance: NumberDecimal('5000.00'),
    CurrentBalance: NumberDecimal(contaBalance.toFixed(2)),
    Color: '#6366f1',
    IsArchived: false,
    CreditLimit: NumberDecimal('0.00'),
    ClosingDay: 10,
    DueDay: 15,
    CreatedAt: new Date('2026-01-01T00:00:00Z'),
    UpdatedAt: new Date(),
  },
  {
    _id: poupId,
    UserId: userIdStr,
    Name: 'Poupanca',
    Type: 1,
    InitialBalance: NumberDecimal('0.00'),
    CurrentBalance: NumberDecimal(poupBalance.toFixed(2)),
    Color: '#10b981',
    IsArchived: false,
    CreditLimit: NumberDecimal('0.00'),
    ClosingDay: 0,
    DueDay: 0,
    CreatedAt: new Date('2026-01-01T00:00:00Z'),
    UpdatedAt: new Date(),
  },
  {
    _id: nuId,
    UserId: userIdStr,
    Name: 'Nubank',
    Type: 4,
    InitialBalance: NumberDecimal('0.00'),
    CurrentBalance: NumberDecimal('0.00'),
    Color: '#7c3aed',
    IsArchived: false,
    CreditLimit: NumberDecimal('5000.00'),
    ClosingDay: 20,
    DueDay: 27,
    CreatedAt: new Date('2026-01-01T00:00:00Z'),
    UpdatedAt: new Date(),
  },
]);
print('Accounts inseridos: 3');

// ─── INSERT TRANSACTIONS ──────────────────────────────────────────────────────
const txDocs = txDefs.map(([catId, desc, amount, type, status, dateStr]) => {
  const d = new Date(dateStr + 'T12:00:00Z');
  return {
    _id: new ObjectId(),
    UserId: userIdStr,
    AccountId: contaIdStr,
    CategoryId: catId,
    Description: desc,
    Amount: NumberDecimal(amount.toFixed(2)),
    Type: type,
    Status: status,
    Date: d,
    DueDate: d,
    Notes: null,
    CreditCardInvoiceId: null,
    IsRecurring: false,
    RecurrenceType: null,
    RecurrenceInterval: 1,
    RecurrenceEndDate: null,
    RecurrenceGroupId: null,
    RecurrenceIndex: null,
    TotalInstallments: null,
    CreatedAt: d,
    UpdatedAt: new Date(),
  };
});
db.transactions.insertMany(txDocs);
print('Transacoes inseridas: ' + txDocs.length);

// ─── INSERT TRANSFERS ─────────────────────────────────────────────────────────
const transferDates = ['2026-01-31','2026-02-28','2026-03-31','2026-04-30','2026-05-25'];
const transferDocs  = transferDates.map(d => ({
  _id: new ObjectId(),
  UserId: userIdStr,
  FromAccountId: contaIdStr,
  ToAccountId: poupIdStr,
  Amount: NumberDecimal('1500.00'),
  Date: new Date(d + 'T12:00:00Z'),
  Description: 'Transferencia para poupanca',
  Status: 0,
  CreatedAt: new Date(d + 'T12:00:00Z'),
  UpdatedAt: new Date(),
}));
db.transfers.insertMany(transferDocs);
print('Transferencias inseridas: ' + transferDocs.length);

// ─── INSERT BUDGETS (Maio 2026) ───────────────────────────────────────────────
const budgetDefs = [
  [catAlim,   900.00],
  [catTransp, 400.00],
  [catSaude,  350.00],
  [catLazer,  400.00],
  [catEduc,   200.00],
  [catSubs,   150.00],
];
const budgetDocs = budgetDefs.map(([catId, limit]) => ({
  _id: new ObjectId(),
  UserId: userIdStr,
  CategoryId: catId,
  Month: 5,
  Year: 2026,
  LimitAmount: NumberDecimal(limit.toFixed(2)),
  CreatedAt: new Date('2026-05-01T00:00:00Z'),
  UpdatedAt: new Date(),
}));
db.budgets.insertMany(budgetDocs);
print('Budgets inseridos: ' + budgetDocs.length);

// ─── INSERT FINANCING (Imovel SAC) ────────────────────────────────────────────
function buildSAC(principal, annualRate, insurance, fees, termMonths, firstDueDate) {
  const monthlyRate = annualRate / 100 / 12;
  const baseAmort   = Math.round(principal / termMonths * 100) / 100;
  const rows = [];
  let balance = principal;
  for (let n = 1; n <= termMonths; n++) {
    const opening     = Math.round(balance * 100) / 100;
    const interest    = Math.round(opening * monthlyRate * 100) / 100;
    const currentAmort = n === termMonths ? opening : baseAmort;
    balance = Math.round((opening - currentAmort) * 100) / 100;
    if (balance < 0) balance = 0;
    const total = Math.round((currentAmort + interest + insurance + fees) * 100) / 100;
    const due   = new Date(firstDueDate);
    due.setUTCMonth(due.getUTCMonth() + (n - 1));
    rows.push({
      Number: n,
      DueDate: due,
      OpeningBalance: NumberDecimal(opening.toFixed(2)),
      Amortization:  NumberDecimal(currentAmort.toFixed(2)),
      Interest:      NumberDecimal(interest.toFixed(2)),
      Insurance:     NumberDecimal(insurance.toFixed(2)),
      Fees:          NumberDecimal(fees.toFixed(2)),
      TotalPayment:  NumberDecimal(total.toFixed(2)),
      ClosingBalance: NumberDecimal(balance.toFixed(2)),
      Status: n <= 16 ? 1 : 0,
      PaidAt: n <= 16 ? due : null,
      PaidAmount: n <= 16 ? NumberDecimal(total.toFixed(2)) : null,
      LinkedTransactionId: null,
    });
  }
  return rows;
}

const firstDue     = new Date('2024-02-01T00:00:00Z');
const installments = buildSAC(360000, 11.5, 120, 35, 360, firstDue);

db.financings.insertOne({
  _id: new ObjectId(),
  UserId: userIdStr,
  Name: 'Financiamento Imovel',
  Type: 0,
  AmortizationSystem: 0,
  Status: 0,
  Institution: 'Caixa Economica Federal',
  AssetValue:         NumberDecimal('450000.00'),
  DownPayment:        NumberDecimal('90000.00'),
  FinancedAmount:     NumberDecimal('360000.00'),
  AnnualInterestRate: NumberDecimal('11.50'),
  MonthlyInsurance:   NumberDecimal('120.00'),
  MonthlyFees:        NumberDecimal('35.00'),
  CetAnnualRate: null,
  TermMonths: 360,
  FirstDueDate: firstDue,
  Installments: installments,
  LinkedAccountId: null,
  Notes: 'Apartamento no centro - 65m2',
  PropertyAddress: 'Rua das Flores, 123 - Centro',
  PropertyRegistration: '12.345-6',
  VehicleBrand: null,
  VehicleModel: null,
  VehicleYear: null,
  VehiclePlate: null,
  CreatedAt: new Date('2024-01-20T00:00:00Z'),
  UpdatedAt: new Date(),
});
print('Financiamento inserido (' + installments.length + ' parcelas, 16 pagas)');
print('=== Seed concluido! ===');
