# Aurora Backend MVP

Backend do Aurora (controle financeiro pessoal) com .NET 8, MongoDB, CQRS/MediatR, JWT, BCrypt e cache distribuído.

## Stack
- .NET 8 Web API
- MongoDB.Driver
- MediatR (CQRS)
- JWT Bearer Auth
- BCrypt password hash
- Redis com `IDistributedCache` via `ICacheService`
- Swagger
- Docker Compose (backend-only)

## Rodar
```bash
docker compose up --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`

## Endpoints implementados
### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Accounts
- `GET /api/accounts`
- `GET /api/accounts/{id}`
- `POST /api/accounts`
- `PUT /api/accounts/{id}`
- `PATCH /api/accounts/{id}/archive`
- `DELETE /api/accounts/{id}`

### Categories
- `GET /api/categories`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`

### Transactions
- `GET /api/transactions`
- `GET /api/transactions/{id}`
- `POST /api/transactions`
- `PUT /api/transactions/{id}`
- `DELETE /api/transactions/{id}`
- `PATCH /api/transactions/{id}/mark-as-paid`
- `PATCH /api/transactions/{id}/mark-as-pending`

### Dashboard
- `GET /api/dashboard/monthly-summary?month=5&year=2026`
- `GET /api/dashboard/category-expenses?month=5&year=2026`
- `GET /api/dashboard/cash-flow?year=2026`

## Regras
- `Amount` precisa ser positivo.
- `Pending` não impacta saldo.
- `Paid` impacta saldo (`Income` soma, `Expense` subtrai).
- Editar transação reverte impacto anterior e aplica novo impacto quando necessário.
- Excluir transação paga reverte saldo.
- Exclusão de conta/categoria é bloqueada quando há transações vinculadas.
- Isolamento total por `UserId` do JWT.

## Cache
- Dashboard mensal com TTL de 5 minutos.
- Invalidação por prefixo `aurora:dashboard:{userId}` em mutações de transações.

## Observação
- Neste repositório, o foco atual está no **backend MVP completo**. O frontend será desenvolvido depois.

## Qualidade
- FluentValidation com pipeline MediatR para validações de entrada.
- Exceções tipadas de domínio mapeadas para HTTP 400/404/409/500 no middleware global.

## Melhorias de performance e segurança
- **Performance**: somas/contagens do dashboard usam agregações no MongoDB para evitar materialização de listas grandes em memória da API.
- **Cache**: invalidação por prefixo implementada com Redis (`StackExchange.Redis`) para remover chaves `aurora:dashboard:{userId}*`.
- **Segurança HTTP**: cabeçalhos `X-Content-Type-Options`, `X-Frame-Options` e `Referrer-Policy` adicionados.
- **CORS**: restringido para origem configurada em `Cors:FrontendUrl`.
- **JWT**: validação de chave mínima de 32 caracteres no startup.
