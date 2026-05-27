# Aurora (API + Client) com Docker Compose

Projeto Aurora com backend (.NET 8) e frontend (React + Vite), além de MongoDB e Redis, tudo orquestrado via Docker Compose.

## Stack
- Backend: .NET 8 Web API
- Frontend: React + Vite
- Banco: MongoDB 7
- Cache: Redis 7
- Orquestração: Docker Compose

## Subir tudo com Docker
```bash
docker compose up --build
```

## Serviços e URLs
- Frontend: `http://localhost:5173`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- MongoDB: `localhost:27017`
- Redis: `localhost:6379`

## Testes

### Backend (xUnit + FluentAssertions + Moq)
```bash
dotnet test tests/Aurora.Tests/Aurora.Tests.csproj
```

### Frontend (Vitest + React Testing Library)
```bash
cd client
npm test            # execução única
npm run test:watch  # modo watch
npm run test:coverage # com relatório de cobertura
```

## Observações
- O frontend roda em modo dev (`vite --host 0.0.0.0 --port 5173`) dentro do container.
- A API usa `Cors__FrontendUrl=http://localhost:5173` no Compose.
- O backend já está configurado para acessar MongoDB e Redis por nome de serviço (`mongodb` e `redis`).
