# Aurora — Arquitetura, Padrões e Roadmap de Qualidade

> Documento de referência para refactor e evolução do backend.
> Última atualização: 2026-05-27.

---

## 1. Visão geral

Aurora é um sistema financeiro pessoal (contas, transações, cartões, orçamentos, transferências, financiamentos, dashboards) com:

- **Backend** .NET 8, ASP.NET Core Web API, MongoDB (driver oficial), Redis (cache + rate limit), MediatR (CQRS), FluentValidation, BCrypt, JWT + Refresh Token (HttpOnly cookie).
- **Frontend** React + Vite (Tailwind v3 + shadcn/ui) — fora do escopo deste documento.

A arquitetura segue **Clean Architecture** em 4 projetos:

```
src/
 ├─ Aurora.API             → controllers, middleware, composição (Program.cs)
 ├─ Aurora.Application     → CQRS (MediatR), DTOs, validators, interfaces
 ├─ Aurora.Domain          → entidades, enums, exceptions, value objects
 └─ Aurora.Infrastructure  → Mongo, Auth, Cache, Repositories, Security
tests/Aurora.Tests         → xUnit (unit tests de handlers)
```

A dependência é unidirecional: **API → Application → Domain ← Infrastructure**. Domínio não conhece infraestrutura.

---

## 2. Diagnóstico do estado atual

### Pontos positivos a preservar
- Separação em camadas correta (Clean Architecture).
- CQRS com MediatR já adotado (`IRequest`, `IRequestHandler`, `ISender`).
- `ValidationBehavior` rodando como pipeline behavior do MediatR.
- Refresh token HttpOnly + `SameSite=Strict` + `Secure` em produção.
- Índices Mongo bem definidos (incluindo TTL para refresh tokens).
- BCrypt para senha; SHA-256 para hash do refresh token (token aleatório de 64 bytes).
- `ApiResponse<T>` padronizado.
- Rate limiting em `login` e `refresh`.

### Problemas identificados

| # | Problema | Onde | Impacto |
|---|---|---|---|
| 1 | God files: 10 entidades em `Entities.cs`, 8 repositórios em `Repositories.cs`, ~30 classes em `FinanceFeatures.cs` | Domain/Application/Infrastructure | Navegação, code review, merge conflicts, violação do SRP |
| 2 | Linhas extensas com `;` colando classes/atributos | Entities.cs, FinanceFeatures.cs | Quebra de Clean Code, ilegível |
| 3 | Sem `EntityBase`/`BaseRepository` genérico reutilizável | Domain & Infrastructure | Duplicação de CRUD em 8 repositórios |
| 4 | Domain anêmico (entidades só com setters; lógica financeira em handlers e `TransactionPostingService`) | Domain & Application | Invariantes não garantidas; difícil testar regras isoladas |
| 5 | Sem Unit of Work / transações reais. `CreateTransferHandler` faz compensação manual com try/catch | Application | Risco de inconsistência financeira em falha entre updates |
| 6 | Validators incompletos (5 de ~20 commands); senha mínima 6 caracteres | Application/Validators | Dados inválidos entram no domain |
| 7 | Sem MFA, sem "esqueci minha senha", sem confirmação de e-mail | — | Bloqueia produção / boas práticas |
| 8 | Sem criptografia at-rest de PII (nome, e-mail); sem audit log; sem masking em logs | Domain & Infrastructure | LGPD: minimização, segurança e auditoria |
| 9 | Sem logging estruturado / correlation ID / OpenTelemetry | API | Difícil diagnosticar em produção |
| 10 | `GlobalExceptionMiddleware` vaza `ex.Message` de `Exception` genérico | API/Middlewares | Information disclosure |
| 11 | Rate limiting apenas em `login` e `refresh` | API | DoS surface em endpoints públicos |
| 12 | Cobertura de testes baixa (4 handlers em ~50) | tests/Aurora.Tests | Sem TDD efetivo |
| 13 | Mistura de PT-BR em mensagens dentro do Domain (Aurora.Domain deveria ser idioma único / mensagens em recurso) | Application/handlers, Validators | i18n e clareza |
| 14 | Senha do refresh token hasheada por SHA-256 sem HMAC com pepper | Application/Auth | Aceitável (token aleatório), mas frágil em leak do DB |

> **Sobre injeção (SQL/NoSQL):** o uso de `Builders<T>.Filter` no driver Mongo já protege contra injeção. O único input direto usado em filtro é `Regex.Escape` em `TransactionRepository.BuildFilter` — já mitigado. Falta apenas limitar tamanho de `Search` (≤ 100 chars) para evitar ReDoS.

---

## 3. Nova organização de pastas (target)

```
src/
├─ Aurora.Domain/
│  ├─ Common/
│  │  ├─ EntityBase.cs              # Id, CreatedAt, UpdatedAt, DomainEvents
│  │  ├─ AggregateRoot.cs           # marca raízes de agregado
│  │  ├─ ValueObject.cs             # base record/equality
│  │  └─ Result.cs                  # Result<T> para erros sem exception
│  ├─ Entities/                     # 1 arquivo por entidade
│  │  ├─ User.cs
│  │  ├─ Account.cs
│  │  ├─ Category.cs
│  │  ├─ Transaction.cs
│  │  ├─ CreditCardInvoice.cs
│  │  ├─ Budget.cs
│  │  ├─ Transfer.cs
│  │  ├─ Financing.cs
│  │  ├─ FinancingInstallment.cs
│  │  ├─ RefreshToken.cs
│  │  └─ MfaChallenge.cs            # novo (Fase 3)
│  ├─ ValueObjects/                 # novo
│  │  ├─ Email.cs
│  │  ├─ Money.cs
│  │  └─ PasswordHash.cs
│  ├─ Enums/                        # 1 arquivo por enum
│  │  ├─ AccountType.cs
│  │  └─ ...
│  ├─ Events/                       # Domain Events (Fase 2)
│  │  ├─ IDomainEvent.cs
│  │  ├─ TransactionCreatedEvent.cs
│  │  └─ UserRegisteredEvent.cs
│  └─ Exceptions/                   # 1 arquivo por exception
│     ├─ DomainException.cs
│     └─ ...
│
├─ Aurora.Application/
│  ├─ Abstractions/                 # interfaces puras
│  │  ├─ Persistence/
│  │  │  ├─ IRepository.cs          # genérico
│  │  │  ├─ IUnitOfWork.cs
│  │  │  ├─ IUserRepository.cs
│  │  │  └─ ...
│  │  ├─ Security/
│  │  │  ├─ IPasswordHasher.cs
│  │  │  ├─ IJwtTokenService.cs
│  │  │  ├─ IEncryptionService.cs   # Fase 3 — AES-GCM
│  │  │  └─ IMfaCodeGenerator.cs    # Fase 3
│  │  ├─ Messaging/
│  │  │  ├─ IEmailSender.cs         # Fase 3
│  │  │  └─ IDomainEventDispatcher.cs
│  │  └─ Common/
│  │     ├─ ICacheService.cs
│  │     ├─ IUserContext.cs
│  │     ├─ IDateTimeProvider.cs    # Fase 2 — testabilidade
│  │     └─ IRateLimiter.cs
│  ├─ Features/                     # vertical slice por agregado
│  │  ├─ Auth/
│  │  │  ├─ Register/
│  │  │  │  ├─ RegisterUserCommand.cs
│  │  │  │  ├─ RegisterUserHandler.cs
│  │  │  │  └─ RegisterUserValidator.cs
│  │  │  ├─ Login/
│  │  │  ├─ RefreshToken/
│  │  │  ├─ Logout/
│  │  │  ├─ Me/
│  │  │  ├─ UpdateProfile/
│  │  │  ├─ UpdatePassword/
│  │  │  ├─ ForgotPassword/         # Fase 3
│  │  │  ├─ ResetPassword/          # Fase 3
│  │  │  ├─ ChallengeMfa/           # Fase 3
│  │  │  ├─ VerifyMfa/              # Fase 3
│  │  │  ├─ Common/
│  │  │  │  ├─ AuthResult.cs
│  │  │  │  └─ TokenHelper.cs
│  │  ├─ Accounts/
│  │  │  ├─ Create/
│  │  │  ├─ Update/
│  │  │  ├─ Archive/
│  │  │  ├─ Delete/
│  │  │  ├─ GetAll/
│  │  │  ├─ GetById/
│  │  │  └─ Common/AccountDto.cs
│  │  ├─ Transactions/
│  │  ├─ Categories/
│  │  ├─ Budgets/
│  │  ├─ Transfers/
│  │  ├─ Financings/
│  │  ├─ CreditCardInvoices/
│  │  └─ Dashboard/
│  ├─ Behaviors/                    # pipeline MediatR
│  │  ├─ ValidationBehavior.cs
│  │  ├─ LoggingBehavior.cs         # Fase 2
│  │  ├─ UnitOfWorkBehavior.cs      # Fase 2
│  │  └─ PerformanceBehavior.cs     # Fase 2
│  └─ Common/
│     ├─ ApiResponse.cs
│     └─ PagedResult.cs
│
├─ Aurora.Infrastructure/
│  ├─ Persistence/
│  │  ├─ Mongo/
│  │  │  ├─ MongoContext.cs
│  │  │  ├─ MongoSettings.cs
│  │  │  ├─ MongoIndexInitializer.cs
│  │  │  └─ BsonConfiguration.cs
│  │  ├─ Repositories/
│  │  │  ├─ MongoRepositoryBase.cs   # genérico + tenant filter por UserId
│  │  │  ├─ UserRepository.cs
│  │  │  └─ ... (1 por entidade)
│  │  └─ UnitOfWork/
│  │     └─ MongoUnitOfWork.cs       # Fase 2 — IClientSessionHandle wrapping
│  ├─ Security/
│  │  ├─ BCryptPasswordHasher.cs
│  │  ├─ JwtTokenService.cs
│  │  ├─ JwtSettings.cs
│  │  ├─ AesGcmEncryptionService.cs  # Fase 3
│  │  ├─ EmailMfaService.cs          # Fase 3
│  │  └─ EncryptedStringSerializer.cs# Fase 3 (BSON serializer)
│  ├─ Cache/
│  │  └─ RedisCacheService.cs
│  ├─ RateLimiting/
│  │  └─ RedisRateLimiter.cs
│  ├─ Messaging/
│  │  ├─ SmtpEmailSender.cs          # Fase 3
│  │  └─ Templates/
│  ├─ Time/
│  │  └─ SystemDateTimeProvider.cs   # Fase 2
│  └─ DependencyInjection/
│     └─ ServiceCollectionExtensions.cs
│
└─ Aurora.API/
   ├─ Controllers/                   # 1 por agregado, magrinho
   │  ├─ AuthController.cs
   │  ├─ AccountsController.cs
   │  ├─ TransactionsController.cs
   │  └─ ...
   ├─ Middlewares/
   │  ├─ GlobalExceptionMiddleware.cs
   │  ├─ CorrelationIdMiddleware.cs  # Fase 2
   │  └─ SecurityHeadersMiddleware.cs
   ├─ Filters/
   │  └─ ValidateModelFilter.cs
   ├─ Configuration/                 # extension methods de setup
   │  ├─ AuthenticationSetup.cs
   │  ├─ SwaggerSetup.cs
   │  ├─ CorsSetup.cs
   │  └─ RateLimitSetup.cs           # Fase 3 — global
   └─ Program.cs

tests/
├─ Aurora.UnitTests/                 # Domain + Handlers, mocks
├─ Aurora.IntegrationTests/          # Testcontainers Mongo + Redis
└─ Aurora.ArchitectureTests/         # NetArchTest — força regras de camada
```

---

## 4. Design Patterns adotados

| Padrão | Onde | Por quê |
|---|---|---|
| **Repository + Generic Repository** | `MongoRepositoryBase<T>` com filtro automático por `UserId` (multi-tenant safety) | Elimina ~70% do código repetido de CRUD |
| **Unit of Work** | `IUnitOfWork` envolvendo `IClientSessionHandle` do Mongo | Atomicidade em `CreateTransfer`, `PayInvoice`, `Register+SeedCategories` |
| **Specification** | `Specification<Transaction>` para `TransactionFilter` | Tira a lógica de `BuildFilter` do repositório; reutilizável e testável |
| **Domain Events** | `Transaction.MarkAsPaid()` publica `TransactionPaidEvent` → handler atualiza saldo e invalida cache | Desacopla efeitos colaterais; reduz handlers gigantes |
| **CQRS (MediatR)** | Já adotado | Manter, com organização por feature folder |
| **Result Pattern** | `Result<TransactionDto>` para fluxos esperados | Exception = excepcional; performance |
| **Value Object** | `Email`, `Money`, `Cpf` | Validação no construtor, imutável, invariantes garantidas |
| **Factory** | `Transaction.Create(...)` static factory | Centraliza criação válida (entidade rica) |
| **Strategy** | `IAmortizationCalculator` (SAC, Price) | Já existe implícito; tornar plugável |
| **Decorator (Pipeline Behaviors)** | Logging, UoW, Validation, Performance | Já tem Validation; adicionar os demais |
| **Outbox Pattern** | Eventos disparam e-mails (MFA, reset) | Entrega exactly-once (Fase 3+) |
| **Options Pattern** | `JwtSettings`, `MongoSettings` | Já adotado |
| **Builder** | `TransactionFilterBuilder` para queries complexas | Legibilidade |

---

## 5. Segurança & Compliance

### Autenticação e Identidade
- **MFA por e-mail**: entidade `MfaChallenge { UserId, CodeHash, ExpiresAt, Attempts, Purpose }`. Login retorna `{ mfaRequired: true, challengeId }` → `POST /auth/mfa/verify` retorna tokens. Código de 6 dígitos, 5 minutos, máx 5 tentativas.
- **Recuperação de senha**: token único hasheado com TTL de 30 min, single-use. Endpoint `forgot-password` sempre retorna 200 (evita user enumeration).
- **Confirmação de e-mail** no cadastro.
- **Política de senha**: mínimo 10 caracteres com complexidade (atualmente 6 — fraco).
- **Lockout progressivo** por usuário, não só por IP (Redis).
- **JWT com `kid`** + rotação de chaves (cache no Redis).
- **Revogar todos os refresh tokens** ao trocar senha.

### LGPD — Proteção de Dados Pessoais
- **Criptografia at-rest** de PII (`User.Name`, `User.Email`) com **AES-256-GCM**; chave em Azure Key Vault / AWS KMS / variável protegida.
- **Email indexável**: `EmailHash` (HMAC) para lookup + `EmailEncrypted` para uso.
- **Audit log** (`AuditEntry { UserId, Action, EntityType, EntityId, At, IpHash }`) para login, troca de senha, exclusão.
- **Direito ao esquecimento**: `DELETE /me` anonimiza (substitui PII por hash, preserva integridade contábil).
- **Portabilidade**: `GET /me/export` (JSON ou CSV completo).
- **Consentimento**: `User.ConsentVersion + ConsentAcceptedAt`.
- **Logs sem PII**: filtro Serilog mascarando e-mails, CPFs, valores monetários.
- **Cookies**: já HttpOnly + Secure + SameSite=Strict.

### SQL/NoSQL Injection
- MongoDB com driver tipado (`Builders<T>.Filter`) — protegido.
- Único input livre: `Search` em `TransactionRepository.BuildFilter` → já usa `Regex.Escape`.
- **Pendência**: limitar tamanho do `Search` a 100 caracteres (anti-ReDoS).

### Defesas extras
- **Rate limiting global** com `Microsoft.AspNetCore.RateLimiting` (.NET 8).
- **CSRF**: cookies SameSite=Strict + double-submit token em mutations do refresh.
- **CSP** além dos headers atuais (`X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`).
- **Detecção de credenciais vazadas** (haveibeenpwned API) opcional na troca de senha.

---

## 6. Clean Code

- **1 tipo público por arquivo**.
- **Linhas ≤ 120 colunas**, sem `;` colando declarações.
- **Métodos ≤ 30 linhas**, classes ≤ 250 linhas.
- **Nomes em inglês** no código; mensagens i18n em recursos da Application.
- **`EditorConfig` + `dotnet format` + `Roslynator` + `StyleCop.Analyzers`**.
- **`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`** nos csproj.
- **Nullable habilitado** em todos os projetos.

---

## 7. TDD e Qualidade de Testes

```
        E2E (poucos, fluxos críticos: register→login→createTx)
        ── Integration (TestContainers: repos + Mongo real)
   ── Unit (Domain rich entities + handlers com mocks)
```

- **Unit tests do Domain** primeiro (entidades ricas → fácil testar invariantes).
- **Handlers** com `NSubstitute`.
- **Architecture tests** com `NetArchTest` — ex: "Domain não pode referenciar Infrastructure".
- **Mutation testing** com `Stryker.NET` em handlers financeiros.
- **Cobertura alvo**: Domain 95%, Application 80%, Infrastructure 50%.

---

## 8. Roadmap de Implementação

### Fase 1 — Refactor estrutural (sem mudar comportamento)
1. Quebrar god files (`Entities.cs`, `Enums.cs`, `Repositories.cs`, `*Features.cs`, `Interfaces.cs`, `DTOs.cs`) em arquivos individuais.
2. Criar `EntityBase`, `AggregateRoot`, `IRepository<T>`, `MongoRepositoryBase<T>`.
3. Adotar feature folder em `Application/Features/<Aggregate>/<UseCase>/`.
4. Adicionar `EditorConfig`, formatters, analyzers.
5. Architecture tests garantindo o layout.

### Fase 2 — Qualidade & Domain enrichment
6. Value Objects (`Email`, `Money`).
7. Mover regras de saldo/posting para métodos das entidades (`Account.Debit/Credit`, `Transaction.MarkAsPaid`).
8. Domain Events + dispatcher.
9. Unit of Work com Mongo sessions.
10. Completar validators (todos os commands).
11. `IDateTimeProvider` para testabilidade.
12. Logging estruturado (Serilog) + correlation ID + `LoggingBehavior`.

### Fase 3 — Segurança & LGPD
13. MFA por e-mail + Forgot/Reset password + Email confirmation.
14. `IEncryptionService` (AES-GCM) + `EncryptedString` BSON serializer + migração de e-mail (hash + encrypted).
15. Audit log + delete-me (anonymization) + export-me.
16. Rate limiting global, política de senha, lockout por usuário, JWT kid + rotation.
17. Cobertura de testes 70%+ (integration com Testcontainers).
