# Aurora - Product Overview

## 1. Resumo Executivo

Aurora e uma aplicacao web de controle financeiro pessoal. O produto permite que uma pessoa cadastre contas, categorias e transacoes, acompanhe saldo total, entradas, saidas, resultado mensal, gastos por categoria e fluxo anual.

O objetivo atual do produto e oferecer uma base simples para organizacao financeira individual, com autenticacao, dashboard e CRUD financeiro basico.

## 2. Proposta de Valor

Aurora ajuda usuarios a responder perguntas como:

- Quanto dinheiro eu tenho distribuido nas minhas contas?
- Quanto entrou e saiu neste mes?
- Quais categorias mais consumiram meu dinheiro?
- Quais transacoes estao pagas, pendentes, atrasadas ou canceladas?
- Como meu fluxo de caixa se comporta ao longo do ano?

## 3. Publico-Alvo

### Usuario Principal

Pessoa fisica que deseja controlar as financas sem depender de planilhas.

Necessidades:

- Registrar receitas e despesas rapidamente.
- Visualizar saldo e resultado mensal.
- Separar gastos por categoria.
- Acompanhar pendencias financeiras.
- Consultar historico por mes, ano, tipo e status.

### Usuario Secundario Futuro

Pequeno empreendedor/autonomo que quer acompanhar movimentacoes simples sem um ERP completo.

Necessidades futuras:

- Relatorios por periodo.
- Separacao de contas pessoais e profissionais.
- Exportacao de dados.
- Recorrencia de receitas/despesas.

## 4. Funcionalidades Atuais

### Autenticacao

- Cadastro de usuario.
- Login com e-mail e senha.
- Token JWT persistido no frontend.
- Rota autenticada para buscar usuario logado.

Rotas:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Contas

O usuario pode criar, listar, editar, arquivar e excluir contas financeiras.

Tipos de conta:

- Conta corrente
- Poupanca
- Dinheiro
- Investimento
- Cartao de credito

Campos principais:

- Nome
- Tipo
- Saldo inicial
- Saldo atual
- Cor
- Status de arquivamento

Rotas:

- `GET /api/accounts`
- `GET /api/accounts/{id}`
- `POST /api/accounts`
- `PUT /api/accounts/{id}`
- `PATCH /api/accounts/{id}/archive`
- `DELETE /api/accounts/{id}`

### Categorias

O usuario pode listar, criar, editar e excluir categorias. Ao registrar um usuario, o backend cria categorias padrao.

Tipos:

- Receita
- Despesa

Campos principais:

- Nome
- Tipo
- Cor
- Icone
- Indicador de categoria padrao

Rotas:

- `GET /api/categories`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`

### Transacoes

O usuario pode criar, listar, editar, excluir e alterar status de transacoes.

Tipos:

- Receita
- Despesa
- Transferencia

Status:

- Paga
- Pendente
- Atrasada
- Cancelada

Campos principais:

- Conta
- Categoria
- Descricao
- Valor
- Tipo
- Status
- Data
- Vencimento
- Notas

Filtros atuais:

- Mes
- Ano
- Tipo
- Status
- Categoria
- Conta

Rotas:

- `GET /api/transactions`
- `GET /api/transactions/{id}`
- `POST /api/transactions`
- `PUT /api/transactions/{id}`
- `DELETE /api/transactions/{id}`
- `PATCH /api/transactions/{id}/mark-as-paid`
- `PATCH /api/transactions/{id}/mark-as-pending`

### Dashboard

O dashboard resume a situacao financeira por mes e ano.

Indicadores atuais:

- Saldo total
- Receita mensal paga
- Despesa mensal paga
- Resultado mensal
- Receita pendente
- Despesa pendente
- Quantidade de transacoes pagas
- Quantidade de transacoes pendentes
- Transacoes recentes
- Gastos por categoria
- Fluxo de caixa anual

Rotas:

- `GET /api/dashboard/monthly-summary?month={month}&year={year}`
- `GET /api/dashboard/category-expenses?month={month}&year={year}`
- `GET /api/dashboard/cash-flow?year={year}`

### Financiamentos

O usuario pode simular e gerenciar financiamentos de imovel, veiculo ou outros bens. O modulo calcula tabela de amortizacao nos sistemas SAC e Price, exibe juros, amortizacao, seguros, taxas, saldo devedor e permite marcar parcelas como pagas.

Tipos:

- Imovel
- Veiculo
- Outro

Sistemas de amortizacao:

- SAC
- Price

Campos principais:

- Nome do financiamento
- Tipo do bem
- Instituicao financeira
- Valor do bem
- Entrada
- Valor financiado
- Taxa de juros anual
- CET anual informado
- Seguro mensal
- Taxas mensais
- Prazo em meses
- Primeiro vencimento
- Parcelas geradas

Rotas:

- `GET /api/financings`
- `GET /api/financings/{id}`
- `POST /api/financings`
- `POST /api/financings/simulate`
- `PATCH /api/financings/{id}/installments/{number}/mark-as-paid`
- `DELETE /api/financings/{id}`

## 5. Fluxos Principais

### Cadastro e Primeiro Acesso

1. Usuario cria conta com nome, e-mail e senha.
2. Backend cria usuario e categorias padrao.
3. Usuario recebe token JWT.
4. Frontend redireciona para a area autenticada.
5. Usuario cadastra sua primeira conta financeira.
6. Usuario registra transacoes.
7. Dashboard passa a exibir resumo financeiro.

### Registro de Transacao

1. Usuario acessa tela de transacoes.
2. Seleciona conta e categoria.
3. Informa descricao, valor, tipo, status e data.
4. Se a transacao for paga, o saldo da conta e atualizado.
5. Dashboard e listas passam a refletir a nova transacao.

### Analise Mensal

1. Usuario acessa dashboard.
2. Seleciona mes e ano.
3. Visualiza saldo total, receitas, despesas e resultado.
4. Consulta gastos por categoria.
5. Consulta fluxo anual e transacoes recentes.

## 6. Arquitetura Atual

### Backend

Stack:

- .NET 8 Web API
- MediatR
- FluentValidation
- MongoDB
- Redis
- JWT
- Swagger / Swashbuckle

Camadas:

- `Aurora.API`: controllers, middlewares, configuracao HTTP.
- `Aurora.Application`: casos de uso, DTOs, validadores, interfaces.
- `Aurora.Domain`: entidades, enums e excecoes de dominio.
- `Aurora.Infrastructure`: MongoDB, Redis, repositorios, JWT, seguranca.

Padroes usados:

- Controllers finos.
- CQRS leve com MediatR.
- Repositorios por agregado.
- Middleware global de excecoes.
- Cache para dashboard mensal.

### Frontend

Stack:

- React
- Vite
- React Router
- CSS global
- Nginx para servir build estatico no Docker

Estrutura atual:

- `src/App.jsx`: roteamento principal.
- `src/pages`: paginas de Login, Dashboard, Contas, Categorias e Transacoes.
- `src/components`: componentes de layout, UI e tabela de transacoes.
- `src/hooks`: hooks de autenticacao e carregamento de dados.
- `src/services`: HTTP client e persistencia de autenticacao.
- `src/constants`: listas fixas de tipos, status, meses, cores e icones.
- `src/utils`: formatadores e helpers.
- `src/interfaces`: contratos JSDoc dos modelos.

### Infraestrutura Local

Orquestracao:

- Docker Compose

Servicos:

- Frontend: `http://localhost:5174`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- MongoDB: `localhost:27017`
- Redis: `localhost:6379`

## 7. Estado Atual da Experiencia

O produto ja permite um ciclo basico completo:

1. Cadastrar usuario.
2. Fazer login.
3. Criar contas.
4. Criar categorias.
5. Criar transacoes.
6. Consultar dashboard.
7. Editar/excluir dados financeiros.
8. Simular e acompanhar financiamentos.

Pontos positivos:

- Escopo funcional coerente para um MVP.
- Backend separado em camadas.
- Swagger ativo.
- Docker Compose com frontend, API, MongoDB e Redis.
- Frontend separado em paginas, componentes, hooks e services.
- Dashboard ja entrega valor inicial.
- Modulo de financiamentos calcula SAC/Price e tabela de amortizacao.

## 8. Limitacoes Conhecidas

### Produto

- Nao ha onboarding guiado para primeiro uso.
- Nao ha metas, orcamentos ou limites por categoria.
- Nao ha transacoes recorrentes.
- Nao ha importacao de extrato bancario.
- Nao ha exportacao CSV/PDF.
- Nao ha relatorios avancados por periodo customizado.
- Nao ha conciliacao bancaria.
- Nao ha suporte multi-moeda.
- Nao ha perfil/configuracoes do usuario.
- Financiamentos ainda nao geram transacoes automaticamente no fluxo financeiro mensal.

### UX/UI

- Formularios ainda sao simples.
- Nao ha confirmacao visual rica para sucesso/erro.
- Exclusoes podem acontecer sem modal de confirmacao dedicado.
- Filtros poderiam ser mais poderosos.
- Dashboard poderia ter visualizacoes mais claras e comparativas.
- Nao ha skeleton loading ou estados vazios muito elaborados.
- Tela de financiamentos ainda mostra apenas um recorte inicial da tabela de parcelas.

### Tecnico

- Frontend ainda esta em JavaScript; TypeScript poderia melhorar contratos.
- Nao ha testes automatizados no frontend.
- Nao ha testes automatizados end-to-end.
- Nao ha refresh token.
- JWT fica no `localStorage`, o que simplifica o MVP mas tem riscos de XSS.
- A API aceita comandos como body diretamente em alguns endpoints, o que acopla contrato HTTP aos comandos internos.
- Nao ha migrations porque MongoDB e usado diretamente.
- Docker do frontend usa variavel Vite em build-time, nao runtime.
- Modulo de financiamentos ainda nao possui testes cobrindo formulas SAC/Price.

## 9. Possiveis Melhorias de Produto

### Alto Impacto

- Criar onboarding de primeiro uso:
  - Criar primeira conta.
  - Confirmar categorias padrao.
  - Registrar primeira transacao.

- Adicionar orcamentos por categoria:
  - Limite mensal por categoria.
  - Percentual consumido.
  - Alertas visuais.

- Adicionar transacoes recorrentes:
  - Salario mensal.
  - Aluguel.
  - Assinaturas.
  - Parcelamentos.

- Melhorar dashboard:
  - Comparativo com mes anterior.
  - Tendencia de economia.
  - Top categorias.
  - Proximas contas a vencer.

- Integrar financiamentos ao fluxo mensal:
  - Gerar transacoes pendentes para parcelas futuras.
  - Mostrar proximas parcelas no dashboard.
  - Permitir simular amortizacao extraordinaria.
  - Comparar reduzir prazo versus reduzir parcela.

### Medio Impacto

- Exportar transacoes em CSV.
- Importar CSV de bancos.
- Adicionar busca textual em transacoes.
- Adicionar anexos/comprovantes.
- Criar pagina de configuracoes.
- Criar metas financeiras.
- Criar notificacoes de vencimento.

### Melhorias Tecnicas

- Migrar frontend para TypeScript.
- Criar camada de API por dominio:
  - `authApi`
  - `accountsApi`
  - `categoriesApi`
  - `transactionsApi`
  - `dashboardApi`

- Adicionar TanStack Query ou SWR para cache e sincronizacao.
- Adicionar React Hook Form + Zod para formularios.
- Adicionar testes:
  - Unitarios para utils/hooks.
  - Component tests.
  - E2E com Playwright.

- Separar DTOs HTTP de Commands internos no backend.
- Adicionar refresh token com cookie HttpOnly.
- Adicionar rate limiting em auth.
- Adicionar logs estruturados.
- Adicionar healthchecks na API.
- Criar testes automatizados para calculo SAC, Price e marcacao de parcelas.

## 10. Riscos e Decisoes Pendentes

- Definir se Aurora sera um produto pessoal simples ou uma ferramenta para autonomos/pequenos negocios.
- Definir se transferencias devem movimentar saldo entre duas contas ou continuar como tipo simples de transacao.
- Definir como lidar com cartao de credito:
  - Conta comum?
  - Fatura mensal?
  - Limite?
  - Fechamento e vencimento?

- Definir estrategia de seguranca:
  - JWT no localStorage para MVP.
  - Cookie HttpOnly + refresh token para produto mais serio.

- Definir estrategia de monetizacao, se houver:
  - Gratuito local/self-hosted.
  - SaaS.
  - Premium com relatorios/automacoes.

## 11. Perguntas Para Avaliacao Externa

Use estas perguntas ao jogar este documento em outra IA:

1. Quais sao os maiores gaps de produto para transformar Aurora em um MVP convincente?
2. Quais funcionalidades deveriam ser priorizadas nas proximas 2 semanas?
3. Quais fluxos de UX parecem incompletos ou confusos?
4. Como melhorar o dashboard para entregar mais valor ao usuario?
5. Quais riscos tecnicos podem atrapalhar a evolucao do produto?
6. O modelo de dados atual cobre bem contas, categorias e transacoes?
7. Como deveria ser modelado cartao de credito de forma correta?
8. Que testes automatizados deveriam ser criados primeiro?
9. Quais melhorias de seguranca sao mais urgentes?
10. Que arquitetura frontend seria ideal para crescer sem bagunca?

## 12. Prompt Sugerido Para Claude

```text
Voce e um especialista em produto, UX e arquitetura de software. Analise a documentacao abaixo do produto Aurora.

Objetivos:
1. Identificar gaps de produto e UX.
2. Propor melhorias priorizadas por impacto e esforco.
3. Sugerir melhorias tecnicas para frontend, backend e infraestrutura.
4. Apontar riscos de seguranca, modelagem e escalabilidade.
5. Propor um roadmap pratico para as proximas 2 a 4 semanas.

Considere que o produto ainda esta em fase MVP e que o objetivo e ter uma aplicacao de controle financeiro pessoal simples, mas bem feita.

Responda com:
- Diagnostico geral.
- Top 10 melhorias priorizadas.
- Roadmap por semana.
- Recomendacoes de arquitetura.
- Recomendacoes de UX.
- Riscos e mitigacoes.
```
