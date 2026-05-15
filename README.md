# Aurora MVP

Aurora é um sistema web de finanças pessoais com arquitetura Clean Architecture (modular monolith), CQRS, JWT, MongoDB e cache distribuído.

## Stack
- Backend: .NET 8 Web API, MediatR, FluentValidation, MongoDB.Driver, JWT, BCrypt, Redis (`IDistributedCache`)
- Frontend: React + Vite, React Router, Axios
- Infra: Docker Compose (API, MongoDB, Redis, frontend)

## Arquitetura
- `Aurora.API`: Controllers, middlewares e setup.
- `Aurora.Application`: CQRS (commands/queries), DTOs, validações e interfaces.
- `Aurora.Domain`: entidades, enums e exceções.
- `Aurora.Infrastructure`: MongoDB, repositórios, JWT, cache e seed.

Decisões arquiteturais:
1. **Clean Architecture + CQRS** para separar domínio, aplicação e infraestrutura.
2. **MongoDB** com índices por `userId` e filtros de consulta para escala de leitura.
3. **Cache distribuído** no dashboard para reduzir agregações repetidas.
4. **Invalidação de cache por usuário** em mutações financeiras.

## Como rodar
```bash
docker compose up --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Frontend: `http://localhost:5173`

## Variáveis de ambiente (API)
- `ConnectionStrings__MongoDb`
- `ConnectionStrings__Redis`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Cors__FrontendUrl`

## Endpoints principais
- Auth: `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`
- Accounts: `GET/POST /api/accounts`, `GET/PUT/DELETE /api/accounts/{id}`, `PATCH /api/accounts/{id}/archive`
- Categories: `GET/POST /api/categories`, `PUT/DELETE /api/categories/{id}`
- Transactions: `GET/POST /api/transactions`, `GET/PUT/DELETE /api/transactions/{id}`, `PATCH /api/transactions/{id}/mark-as-paid`, `PATCH /api/transactions/{id}/mark-as-pending`
- Dashboard: `GET /api/dashboard/monthly-summary`, `GET /api/dashboard/category-expenses`, `GET /api/dashboard/cash-flow`

## Regras de negócio
- Pending não altera saldo.
- Paid altera saldo (Income +, Expense -).
- Editar/excluir/mudar status de transação reverte/aplica impacto corretamente.
- Apenas dados do usuário autenticado são acessíveis.
- Categoria/conta só podem ser removidas sem transações vinculadas.

## Índices MongoDB
- `users`: `email` unique
- `accounts`: `{userId}`, `{userId,name}`
- `categories`: `{userId}`, `{userId,type}`
- `transactions`: `{userId,date}`, `{userId,status}`, `{userId,accountId}`, `{userId,categoryId}`, `{userId,type}`, `{userId,date,type,status}`

## Próximos passos
- Multi-tenant formal com `TenantId`.
- Refresh token + revoke list.
- Observabilidade (OpenTelemetry).
- Testes automatizados unitários e integração.
