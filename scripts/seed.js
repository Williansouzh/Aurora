// Aurora seed — mongosh aurora /tmp/seed.js
// C# MongoDB driver serializes with PascalCase field names

var db = db.getSiblingDB('aurora');

var user = db.users.findOne({ Email: 'williansouza4041@gmail.com' });
if (!user) { print('ERROR: user not found'); quit(1); }
var uid = user._id.toString();
print('Seeding for userId: ' + uid);

// ─── accounts ────────────────────────────────────────────────────────────────
var existingAccounts = db.accounts.find({ UserId: uid }).toArray();
var acc = {};  // name → id string
if (existingAccounts.length > 0) {
  existingAccounts.forEach(function(a) { acc[a.Name] = a._id.toString(); });
  print('Using existing accounts: ' + Object.keys(acc).join(', '));
} else {
  var now = new Date();
  var acctDocs = [
    { _id: new ObjectId(), UserId: uid, Name: 'Conta Corrente', Type: 0, InitialBalance: 5000,  CurrentBalance: 5000,  Color: '#6366f1', IsArchived: false, CreditLimit: 0,     ClosingDay: 10, DueDay: 15, CreatedAt: now, UpdatedAt: now },
    { _id: new ObjectId(), UserId: uid, Name: 'Poupança',        Type: 1, InitialBalance: 12000, CurrentBalance: 12000, Color: '#0f766e', IsArchived: false, CreditLimit: 0,     ClosingDay: 10, DueDay: 15, CreatedAt: now, UpdatedAt: now },
    { _id: new ObjectId(), UserId: uid, Name: 'Cartão Nubank',   Type: 4, InitialBalance: 0,     CurrentBalance: 0,     Color: '#7c3aed', IsArchived: false, CreditLimit: 10000, ClosingDay: 5,  DueDay: 12, CreatedAt: now, UpdatedAt: now },
    { _id: new ObjectId(), UserId: uid, Name: 'Investimentos',   Type: 3, InitialBalance: 40000, CurrentBalance: 40000, Color: '#0891b2', IsArchived: false, CreditLimit: 0,     ClosingDay: 10, DueDay: 15, CreatedAt: now, UpdatedAt: now },
  ];
  db.accounts.insertMany(acctDocs);
  acctDocs.forEach(function(a) { acc[a.Name] = a._id.toString(); });
  print('Created accounts: ' + Object.keys(acc).join(', '));
}

// pick account ids by type
var contaId = null, poupancaId = null, cartaoId = null, investId = null;
Object.keys(acc).forEach(function(name) {
  var a = db.accounts.findOne({ _id: ObjectId(acc[name]) });
  if (a.Type === 0) contaId    = acc[name];
  if (a.Type === 1) poupancaId = acc[name];
  if (a.Type === 4) cartaoId   = acc[name];
  if (a.Type === 3) investId   = acc[name];
});
if (!contaId)    contaId    = Object.values(acc)[0];
if (!poupancaId) poupancaId = contaId;
if (!cartaoId)   cartaoId   = contaId;
if (!investId)   investId   = contaId;
print('contaId=' + contaId + ' cartaoId=' + cartaoId);

// ─── categories ──────────────────────────────────────────────────────────────
var cat = {};
db.categories.find({ UserId: uid }).forEach(function(c) { cat[c.Name] = c._id.toString(); });
if (Object.keys(cat).length === 0) {
  print('WARNING: no categories – seeding defaults');
  var catDocs = [
    { _id: new ObjectId(), UserId: uid, Name: 'Salário',      Type: 0, Color: '#0f766e', Icon: 'Salario',  IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Freelance',    Type: 0, Color: '#0891b2', Icon: 'Pix',      IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Investimentos',Type: 0, Color: '#2563eb', Icon: 'Outros',   IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Moradia',      Type: 1, Color: '#dc2626', Icon: 'Casa',     IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Alimentação',  Type: 1, Color: '#ea580c', Icon: 'Mercado',  IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Transporte',   Type: 1, Color: '#7c3aed', Icon: 'Carro',    IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Saúde',        Type: 1, Color: '#0891b2', Icon: 'Saude',    IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Lazer',        Type: 1, Color: '#4f46e5', Icon: 'Lazer',    IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Educação',     Type: 1, Color: '#475569', Icon: 'Outros',   IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Assinaturas',  Type: 1, Color: '#6366f1', Icon: 'Outros',   IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
    { _id: new ObjectId(), UserId: uid, Name: 'Outros',       Type: 1, Color: '#94a3b8', Icon: 'Outros',   IsDefault: true, CreatedAt: new Date(), UpdatedAt: new Date() },
  ];
  db.categories.insertMany(catDocs);
  catDocs.forEach(function(c) { cat[c.Name] = c._id.toString(); });
}
print('Categories: ' + Object.keys(cat).join(', '));

function catId(name, fallbackType) {
  if (cat[name]) return cat[name];
  var fb = db.categories.findOne({ UserId: uid, Type: fallbackType });
  return fb ? fb._id.toString() : Object.values(cat)[0];
}

var cSalario   = catId('Salário',      0);
var cFreelance = catId('Freelance',    0);
var cInvest    = catId('Investimentos',0);
var cMoradia   = catId('Moradia',      1);
var cAlimen    = catId('Alimentação',  1);
var cTransp    = catId('Transporte',   1);
var cSaude     = catId('Saúde',        1);
var cLazer     = catId('Lazer',        1);
var cEduc      = catId('Educação',     1);
var cAssine    = catId('Assinaturas',  1);
var cOutros    = catId('Outros',       1);

// ─── clear previous seed ──────────────────────────────────────────────────────
db.transactions.deleteMany({ UserId: uid, Notes: 'seed' });
db.budgets.deleteMany({ UserId: uid });
print('Cleared previous seed transactions & budgets');

// ─── transaction factory ─────────────────────────────────────────────────────
function tx(accountId, categoryId, description, amount, type, status, dateStr) {
  return {
    _id: new ObjectId(),
    UserId: uid,
    AccountId: accountId,
    CategoryId: categoryId,
    Description: description,
    Amount: amount,
    Type: type,    // 0=Income 1=Expense
    Status: status, // 0=Paid 1=Pending 2=Overdue
    Date: new Date(dateStr + 'T12:00:00Z'),
    DueDate: null,
    Notes: 'seed',
    CreditCardInvoiceId: null,
    IsRecurring: false,
    RecurrenceType: null,
    RecurrenceInterval: 1,
    RecurrenceEndDate: null,
    RecurrenceGroupId: null,
    RecurrenceIndex: null,
    TotalInstallments: null,
    CreatedAt: new Date(),
    UpdatedAt: new Date()
  };
}

var txs = [];

// ═══════════════════════ JANEIRO 2026 ═════════════════════════════════════════
txs.push(tx(contaId,  cSalario,  'Salário janeiro',              9200,  0, 0, '2026-01-05'));
txs.push(tx(contaId,  cMoradia,  'Aluguel',                      2400,  1, 0, '2026-01-08'));
txs.push(tx(contaId,  cAlimen,   'Supermercado',                  680,  1, 0, '2026-01-04'));
txs.push(tx(cartaoId, cAlimen,   'iFood',                         145,  1, 0, '2026-01-20'));
txs.push(tx(contaId,  cTransp,   'Combustível',                   280,  1, 0, '2026-01-07'));
txs.push(tx(cartaoId, cAssine,   'Netflix + Spotify + Academia',  187,  1, 0, '2026-01-10'));
txs.push(tx(contaId,  cMoradia,  'Condomínio',                    480,  1, 0, '2026-01-05'));
txs.push(tx(cartaoId, cLazer,    'Cinema e jantar',               220,  1, 0, '2026-01-18'));

// ═══════════════════════ FEVEREIRO 2026 ══════════════════════════════════════
txs.push(tx(contaId,  cSalario,  'Salário fevereiro',            9200,  0, 0, '2026-02-05'));
txs.push(tx(contaId,  cFreelance,'Projeto extra',                1500,  0, 0, '2026-02-15'));
txs.push(tx(contaId,  cMoradia,  'Aluguel',                      2400,  1, 0, '2026-02-08'));
txs.push(tx(contaId,  cMoradia,  'Condomínio',                    480,  1, 0, '2026-02-05'));
txs.push(tx(contaId,  cAlimen,   'Supermercado Pão de Açúcar',    710,  1, 0, '2026-02-03'));
txs.push(tx(cartaoId, cAlimen,   'Restaurante japonês',           195,  1, 0, '2026-02-22'));
txs.push(tx(contaId,  cTransp,   'Combustível',                   295,  1, 0, '2026-02-06'));
txs.push(tx(cartaoId, cAssine,   'Netflix + Spotify + Academia',  187,  1, 0, '2026-02-10'));
txs.push(tx(cartaoId, cSaude,    'Consulta médica',               200,  1, 0, '2026-02-14'));
txs.push(tx(cartaoId, cLazer,    'Carnaval - viagem',             1100, 1, 0, '2026-02-28'));

// ═══════════════════════ MARÇO 2026 ═══════════════════════════════════════════
txs.push(tx(contaId,  cSalario,  'Salário março',                 9200,  0, 0, '2026-03-05'));
txs.push(tx(contaId,  cFreelance,'Projeto freelance',             1800,  0, 0, '2026-03-12'));
txs.push(tx(contaId,  cMoradia,  'Aluguel',                       2400,  1, 0, '2026-03-08'));
txs.push(tx(contaId,  cMoradia,  'Condomínio',                     480,  1, 0, '2026-03-05'));
txs.push(tx(contaId,  cAlimen,   'Supermercado',                   685,  1, 0, '2026-03-04'));
txs.push(tx(cartaoId, cAlimen,   'iFood',                          185,  1, 0, '2026-03-20'));
txs.push(tx(contaId,  cTransp,   'Combustível',                    280,  1, 0, '2026-03-07'));
txs.push(tx(cartaoId, cTransp,   'Uber',                            95,  1, 0, '2026-03-22'));
txs.push(tx(cartaoId, cLazer,    'Bar com amigos',                 220,  1, 0, '2026-03-29'));
txs.push(tx(cartaoId, cAssine,   'Netflix + Spotify + Academia',   187,  1, 0, '2026-03-10'));
txs.push(tx(contaId,  cSaude,    'Farmácia',                        98,  1, 0, '2026-03-18'));
txs.push(tx(cartaoId, cEduc,     'Curso Udemy',                     79,  1, 0, '2026-03-02'));

// ═══════════════════════ ABRIL 2026 ═══════════════════════════════════════════
txs.push(tx(contaId,  cSalario,  'Salário abril',                  9200,  0, 0, '2026-04-05'));
txs.push(tx(contaId,  cFreelance,'Consultoria TI',                 2500,  0, 0, '2026-04-18'));
txs.push(tx(contaId,  cMoradia,  'Aluguel',                        2400,  1, 0, '2026-04-08'));
txs.push(tx(contaId,  cMoradia,  'Condomínio',                      480,  1, 0, '2026-04-05'));
txs.push(tx(contaId,  cAlimen,   'Supermercado Pão de Açúcar',      720,  1, 0, '2026-04-03'));
txs.push(tx(cartaoId, cAlimen,   'iFood',                           210,  1, 0, '2026-04-19'));
txs.push(tx(cartaoId, cAlimen,   'Restaurante fino',                380,  1, 0, '2026-04-25'));
txs.push(tx(contaId,  cTransp,   'Combustível',                     310,  1, 0, '2026-04-06'));
txs.push(tx(cartaoId, cLazer,    'Show Rock in Rio',                580,  1, 0, '2026-04-11'));
txs.push(tx(cartaoId, cLazer,    'Viagem fim de semana',            850,  1, 0, '2026-04-20'));
txs.push(tx(cartaoId, cAssine,   'Netflix + Spotify + Academia',    187,  1, 0, '2026-04-10'));
txs.push(tx(contaId,  cSaude,    'Consulta + exames',               345,  1, 0, '2026-04-14'));
txs.push(tx(cartaoId, cEduc,     'Assinatura Alura',                119,  1, 0, '2026-04-08'));

// ═══════════════════════ MAIO 2026 (mês atual) ════════════════════════════════
txs.push(tx(contaId,  cSalario,  'Salário maio',                   9200,  0, 0, '2026-05-05'));
txs.push(tx(contaId,  cFreelance,'Projeto e-commerce',             3200,  0, 0, '2026-05-10'));
txs.push(tx(contaId,  cMoradia,  'Aluguel',                        2400,  1, 0, '2026-05-08'));
txs.push(tx(contaId,  cMoradia,  'Condomínio',                      480,  1, 0, '2026-05-05'));
txs.push(tx(contaId,  cMoradia,  'Conta de luz',                    185,  1, 0, '2026-05-12'));
txs.push(tx(contaId,  cAlimen,   'Supermercado Pão de Açúcar',      695,  1, 0, '2026-05-03'));
txs.push(tx(cartaoId, cAlimen,   'Supermercado Extra',              420,  1, 0, '2026-05-13'));
txs.push(tx(cartaoId, cAlimen,   'iFood',                           165,  1, 0, '2026-05-17'));
txs.push(tx(cartaoId, cAlimen,   'Restaurante Figueira Rubaiyat',   520,  1, 0, '2026-05-21'));
txs.push(tx(contaId,  cTransp,   'Combustível',                     295,  1, 0, '2026-05-06'));
txs.push(tx(cartaoId, cTransp,   'Uber',                             75,  1, 0, '2026-05-19'));
txs.push(tx(cartaoId, cLazer,    'Ingresso show Coldplay',          320,  1, 0, '2026-05-09'));
txs.push(tx(cartaoId, cAssine,   'Netflix',                          55.9, 1, 0, '2026-05-10'));
txs.push(tx(cartaoId, cAssine,   'Spotify',                          21.9, 1, 0, '2026-05-10'));
txs.push(tx(cartaoId, cAssine,   'Academia Smart Fit',              109.9, 1, 0, '2026-05-10'));
txs.push(tx(cartaoId, cAssine,   'Amazon Prime + YouTube Premium',   39.9, 1, 0, '2026-05-10'));
txs.push(tx(contaId,  cSaude,    'Plano de saúde',                  420,  1, 0, '2026-05-05'));
txs.push(tx(cartaoId, cSaude,    'Farmácia',                         87,  1, 0, '2026-05-15'));
txs.push(tx(cartaoId, cEduc,     'Curso inglês online',             350,  1, 0, '2026-05-01'));
txs.push(tx(contaId,  cOutros,   'Presente aniversário mãe',        180,  1, 0, '2026-05-22'));
// pendentes
txs.push(tx(contaId,  cTransp,   'IPVA 2ª parcela',                 490,  1, 1, '2026-05-30'));
txs.push(tx(cartaoId, cOutros,   'Compra Amazon',                   350,  1, 1, '2026-05-30'));

db.transactions.insertMany(txs);
print('Inserted ' + txs.length + ' transactions');

// ─── budgets May 2026 ─────────────────────────────────────────────────────────
var budgets = [
  { _id: new ObjectId(), UserId: uid, CategoryId: cAlimen, Month: 5, Year: 2026, LimitAmount: 1800, CreatedAt: new Date(), UpdatedAt: new Date() },
  { _id: new ObjectId(), UserId: uid, CategoryId: cLazer,  Month: 5, Year: 2026, LimitAmount: 600,  CreatedAt: new Date(), UpdatedAt: new Date() },
  { _id: new ObjectId(), UserId: uid, CategoryId: cTransp, Month: 5, Year: 2026, LimitAmount: 500,  CreatedAt: new Date(), UpdatedAt: new Date() },
  { _id: new ObjectId(), UserId: uid, CategoryId: cAssine, Month: 5, Year: 2026, LimitAmount: 280,  CreatedAt: new Date(), UpdatedAt: new Date() },
  { _id: new ObjectId(), UserId: uid, CategoryId: cSaude,  Month: 5, Year: 2026, LimitAmount: 600,  CreatedAt: new Date(), UpdatedAt: new Date() },
  { _id: new ObjectId(), UserId: uid, CategoryId: cMoradia,Month: 5, Year: 2026, LimitAmount: 3500, CreatedAt: new Date(), UpdatedAt: new Date() },
];
db.budgets.insertMany(budgets);
print('Inserted ' + budgets.length + ' budgets');

// ─── financing ────────────────────────────────────────────────────────────────
if (db.financings.countDocuments({ UserId: uid }) === 0) {
  var principal = 320000;
  var annualRate = 9.5;
  var mr = annualRate / 100 / 12;
  var term = 360;
  var firstDue = new Date('2023-06-01T00:00:00Z');
  var basePayment = principal * mr / (1 - Math.pow(1 + mr, -term));
  basePayment = Math.round(basePayment * 100) / 100;

  var installments = [];
  var balance = principal;
  for (var n = 1; n <= term; n++) {
    var opening = balance;
    var interest = Math.round(opening * mr * 100) / 100;
    var amort = n === term ? opening : Math.round((basePayment - interest) * 100) / 100;
    balance = Math.round((opening - amort) * 100) / 100;
    var dueDate = new Date(firstDue);
    dueDate.setMonth(firstDue.getMonth() + (n - 1));
    var isPaid = n <= 36; // 36 meses pagos (jun/2023 a mai/2026)
    var total = Math.round((amort + interest + 95 + 35) * 100) / 100;
    installments.push({
      Number: n,
      DueDate: dueDate,
      OpeningBalance: opening,
      Amortization: amort,
      Interest: interest,
      Insurance: 95,
      Fees: 35,
      TotalPayment: total,
      ClosingBalance: balance,
      Status: isPaid ? 1 : 0,   // 1=Paid 0=Planned
      PaidAt: isPaid ? new Date(dueDate.getTime() + 86400000 * 5) : null,
      PaidAmount: isPaid ? total : null,
      LinkedTransactionId: null
    });
  }

  db.financings.insertOne({
    _id: new ObjectId(),
    UserId: uid,
    Name: 'Apartamento Jardim Europa',
    Type: 0, AmortizationSystem: 1, Status: 0,
    Institution: 'Itaú BBA',
    AssetValue: 480000, DownPayment: 160000, FinancedAmount: principal,
    AnnualInterestRate: annualRate, MonthlyInsurance: 95, MonthlyFees: 35,
    CetAnnualRate: 10.2, TermMonths: term, FirstDueDate: firstDue,
    Installments: installments,
    LinkedAccountId: null,
    Notes: 'Apartamento 2 quartos 75m². Matrícula 12.345-SP.',
    PropertyAddress: 'Rua das Flores, 123 - Jardim Europa, São Paulo/SP',
    PropertyRegistration: '12.345-SP',
    VehicleBrand: null, VehicleModel: null, VehicleYear: null, VehiclePlate: null,
    CreatedAt: new Date(), UpdatedAt: new Date()
  });
  print('Financing inserted: 36 of 360 installments marked paid');
} else {
  print('Financing already exists, skipping');
}

// ─── update account balances ──────────────────────────────────────────────────
// Conta Corrente: initialBalance=5000 + receitas - despesas
var contaTxs = db.transactions.find({ UserId: uid, AccountId: contaId, Status: 0 }).toArray();
var ccIncome = 0, ccExpense = 0;
contaTxs.forEach(function(t) {
  if (t.Type === 0) ccIncome  += t.Amount;
  if (t.Type === 1) ccExpense += t.Amount;
});
var ccBalance = 5000 + ccIncome - ccExpense;
db.accounts.updateOne({ _id: ObjectId(contaId) }, { $set: { CurrentBalance: Math.round(ccBalance * 100) / 100, UpdatedAt: new Date() } });
print('Conta Corrente balance: R$ ' + Math.round(ccBalance * 100) / 100);

// Poupança: keep at 15.200
db.accounts.updateOne({ _id: ObjectId(poupancaId) }, { $set: { CurrentBalance: 15200, UpdatedAt: new Date() } });

// Investimentos: keep at 47.500
db.accounts.updateOne({ _id: ObjectId(investId) }, { $set: { CurrentBalance: 47500, UpdatedAt: new Date() } });

print('\n✓ Seed concluído com sucesso!');
