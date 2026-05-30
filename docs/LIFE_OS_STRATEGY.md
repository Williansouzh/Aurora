# Aurora Life OS - Produto e Plano Estrategico

> Documento guia para evoluir o Aurora, aos poucos e um modulo por vez, de um app financeiro pessoal para um Life OS: uma rede social privada da propria evolucao.
> Ultima atualizacao: 2026-05-29.
> Controle de acesso, planos e Super Admin: ver [ACCESS_CONTROL_PLAN.md](./ACCESS_CONTROL_PLAN.md).

---

## 1. Norte do Produto

O Aurora deve deixar de ser apenas um sistema financeiro e se tornar um sistema operacional pessoal para organizar, registrar e visualizar a evolucao da vida do usuario.

O posicionamento principal:

> Aurora e o lugar onde voce acompanha a evolucao da sua vida.

A experiencia deve combinar:

- dashboard pessoal
- diario visual
- rede social privada
- planner diario e semanal
- habitos, metas e streaks
- controle financeiro
- historico de fotos e evolucao
- retrospectivas semanais, mensais e anuais
- gamificacao com XP, niveis e conquistas

O diferencial nao e competir diretamente com apps isolados de tarefas, habitos, financas ou fotos. O diferencial e juntar esses sinais em uma narrativa pessoal continua: "quem eu estou me tornando?".

---

## 2. Principio Central

### Single-player social network

O Aurora deve parecer uma rede social, mas sem depender de publico externo. O usuario registra a propria vida para si mesmo.

Exemplo de feed privado:

```text
Hoje

- 12 dias de consistencia no treino
- Foto adicionada em "Evolucao fisica"
- 3 habitos concluidos
- R$ 42 gastos hoje
- Meta "ler 12 livros" avancou para 35%
- Reflexao: "Hoje consegui manter o foco melhor"
```

Regras de produto:

- Tudo nasce privado por padrao.
- Compartilhamento futuro deve ser sempre explicito.
- A timeline deve misturar eventos automaticos e registros manuais.
- O dashboard deve mostrar estado atual, nao apenas atalhos.
- Cada modulo deve alimentar a Timeline e a gamificacao quando fizer sentido.

---

## 3. Estrategia de Migracao

O Aurora ja possui uma base financeira funcional. A evolucao para Life OS deve preservar essa base e adicionar modulos de forma incremental, sem tentar reconstruir tudo de uma vez.

Ordem recomendada:

1. Consolidar a fundacao atual: auth, usuario, seguranca, layout, padroes de API e testes.
2. Criar o nucleo emocional: Hoje, Habitos, Metas e Timeline.
3. Conectar fotos, diario e planejamento semanal.
4. Reposicionar Financas como um modulo dentro do Life OS.
5. Adicionar retrospectivas e gamificacao mais rica.
6. Somente depois avaliar social opcional, IA e automacoes.

Cada modulo novo deve entregar uma fatia completa:

- entidades de dominio
- comandos e queries
- endpoints
- tela utilizavel
- eventos para Timeline
- regras basicas de XP quando aplicavel
- testes proporcionais ao risco

---

## 4. Mapa de Modulos

| Modulo tecnico | Nome no produto | Papel no Life OS | Prioridade |
| --- | --- | --- | --- |
| Dashboard | Aurora Home | Visao do dia, progresso e sinais importantes | Alta |
| Today | Meu Dia | Execucao diaria: prioridades, tarefas, habitos e check-in | Alta |
| Habits | Rituais | Consistencia, streaks, XP e estatisticas | Alta |
| Goals | Minha Jornada | Metas de vida, progresso e milestones | Alta |
| Timeline | Linha da Vida | Feed privado de eventos e memorias | Alta |
| WeeklyPlanning | Minha Semana | Planejamento e revisao semanal | Media |
| Diary | Diario | Reflexoes, humor, tags e fotos | Media |
| EvolutionPhotos | Evolucao | Albuns, antes/depois e progresso visual | Media |
| Finances | Dinheiro | Orcamento, transacoes e fechamento mensal | Ja existe / evoluir |
| Retrospectives | Retrospectiva | Resumos semanais, mensais e anuais | Media |
| Gamification | Progresso | XP, niveis, conquistas e streaks globais | Transversal |

---

## 5. Arquitetura Alvo

Manter a Clean Architecture atual:

```text
src/
  Aurora.API
  Aurora.Application
  Aurora.Domain
  Aurora.Infrastructure
```

Organizacao recomendada para novas features:

```text
src/Aurora.Application/Features/
  Auth/
  Dashboard/
  Today/
  Tasks/
  Habits/
  WeeklyPlanning/
  Goals/
  Timeline/
  Diary/
  EvolutionPhotos/
  Finances/
  Retrospectives/
  Gamification/
```

Novos agregados devem seguir o padrao existente:

- entidade em `Aurora.Domain/Entities`
- enum em `Aurora.Domain/Enums`
- interface de repositorio em `Aurora.Application/Abstractions/Persistence`
- repositorio Mongo em `Aurora.Infrastructure/Persistence/Repositories`
- comandos/queries em `Aurora.Application/Features/<Module>/<UseCase>`
- controller fino em `Aurora.API/Controllers`
- componentes e paginas em `client/src`

---

## 6. Modelos de Dominio Iniciais

### Habit

```text
Habit
- Id
- UserId
- Name
- Description
- FrequencyType
- DaysOfWeek
- TimesPerWeek
- Difficulty
- XPReward
- CurrentStreak
- BestStreak
- IsActive
- CreatedAt
- UpdatedAt
```

### HabitCheckIn

```text
HabitCheckIn
- Id
- HabitId
- UserId
- Date
- Status
- Note
- PhotoUrl
- XPGenerated
- CreatedAt
```

Regras:

- Habito diario conta streak por dia.
- Habito semanal conta streak por semana.
- Check-in duplicado no mesmo periodo nao deve gerar XP infinito.
- Foto em check-in pode gerar evento na Timeline.

### Goal

```text
Goal
- Id
- UserId
- Title
- Description
- Area
- Status
- StartDate
- TargetDate
- Progress
- MetricType
- TargetValue
- CurrentValue
- CoverImage
- CreatedAt
- UpdatedAt
```

### Milestone

```text
Milestone
- Id
- GoalId
- Title
- IsRequired
- IsCompleted
- CompletedAt
```

Regras:

- Meta so deve ser concluida automaticamente quando milestones obrigatorios estiverem completos.
- Conclusao forcada deve exigir motivo.
- Progresso pode ser manual no inicio e automatico depois.

### TimelineEvent

```text
TimelineEvent
- Id
- UserId
- Type
- Area
- Title
- Description
- OccurredAt
- SourceModule
- SourceId
- Visibility
- MediaUrls
- Metadata
- IsHidden
- IsFavorite
- CreatedAt
```

Tipos iniciais:

- HabitCheckedIn
- TaskCompleted
- GoalProgressed
- GoalCompleted
- DiaryWritten
- EvolutionPhotoAdded
- WeeklyReviewClosed
- MonthlyBudgetClosed
- AchievementUnlocked
- ManualPost

### DiaryEntry

```text
DiaryEntry
- Id
- UserId
- Date
- Content
- Mood
- Tags
- Photos
- IsPrivate
- CreatedAt
- UpdatedAt
```

### EvolutionAlbum

```text
EvolutionAlbum
- Id
- UserId
- Title
- Area
- Description
- CoverImage
- IsPrivate
- CreatedAt
```

### EvolutionPhoto

```text
EvolutionPhoto
- Id
- AlbumId
- UserId
- ImageUrl
- Caption
- Date
- Tags
- LinkedGoalId
- LinkedHabitId
- CreatedAt
```

### WeeklyPlan

```text
WeeklyPlan
- Id
- UserId
- WeekStart
- WeekEnd
- MainFocus
- Goals
- Priorities
- Notes
- Status
- Review
- XPGenerated
- CreatedAt
- ClosedAt
```

Status:

```text
NotStarted
InProgress
Closed
```

---

## 7. Requisitos por Modulo

### Aurora Home

Objetivo: mostrar o estado atual da vida do usuario.

Deve exibir, progressivamente:

- resumo do dia
- top 3 prioridades
- habitos pendentes
- XP atual
- streaks ativos
- humor recente
- saldo financeiro do mes
- metas em destaque
- fotos recentes
- radar do futuro

MVP:

- resumo financeiro atual reaproveitando o dashboard existente
- card "Meu Dia"
- card de habitos pendentes
- card de metas em destaque
- card de eventos recentes da Timeline

### Meu Dia

Objetivo: ser a tela de execucao das proximas 24h.

Deve conter:

- top 3 prioridades
- tarefas do dia
- habitos do dia
- check-in de humor
- mini diario
- captura rapida

Regras:

- Backlog nao deve poluir essa tela.
- Tarefas vencidas devem ir para revisao, nao acumular infinitamente.
- Tudo em "Meu Dia" deve ser executavel hoje.

### Rituais

Objetivo: criar consistencia por habitos.

Deve permitir:

- criar, editar, pausar e excluir habitos
- fazer check-in
- visualizar streak atual e melhor streak
- visualizar calendario de consistencia
- anexar nota e foto ao check-in

### Minha Jornada

Objetivo: acompanhar metas de vida.

Deve permitir:

- criar meta
- definir area da vida
- definir metrica e valor alvo
- adicionar milestones
- atualizar progresso
- concluir meta
- anexar foto ou vincular habito/tarefa

### Linha da Vida

Objetivo: feed privado da vida.

Deve permitir:

- listar eventos paginados
- filtrar por area, tipo e periodo
- criar post manual
- ocultar evento automatico
- favoritar memoria
- anexar fotos em posts manuais

### Diario

Objetivo: reflexao privada com contexto emocional.

Deve permitir:

- texto livre
- humor de 1 a 5
- tags
- fotos
- busca por palavra, tag e humor
- retrospectiva emocional

### Evolucao

Objetivo: tornar progresso visual.

Deve permitir:

- criar albuns
- adicionar fotos
- comparar antes/depois
- filtrar por periodo
- vincular foto a meta ou habito

### Dinheiro

Objetivo: reposicionar o financeiro atual como modulo do Life OS.

Ja existe:

- contas
- categorias
- transacoes
- transferencias
- orcamentos
- faturas
- financiamentos
- dashboard financeiro

Evolucao recomendada:

- fechamento mensal
- eventos financeiros na Timeline
- resumo financeiro no Aurora Home
- metas financeiras conectadas a Goals
- retrospectiva mensal com gastos, economia e progresso

---

## 8. Gamificacao

### XP

O XP deve incentivar consistencia, nao volume artificial.

Fontes iniciais:

- concluir habito: XP por dificuldade
- concluir tarefa importante: XP baixo
- escrever reflexao diaria: XP baixo e limitado
- avancar milestone: XP medio
- concluir meta: XP alto
- fechar planejamento semanal: XP medio
- adicionar foto de evolucao: XP baixo e limitado
- fechar mes financeiro: XP medio

Regras:

- Limite diario por tipo de acao.
- Check-in duplicado nao gera XP extra.
- Tarefas repetitivas tem retorno reduzido.
- Eventos de XP devem ser auditaveis.

### Niveis

```text
Level 1 - Inicio
Level 5 - Organizado
Level 10 - Consistente
Level 20 - Focado
Level 30 - Em evolucao
Level 50 - Modo Aurora
```

### Conquistas

Exemplos:

- 7 dias de um habito
- 30 dias registrando humor
- primeira revisao semanal
- primeira meta concluida
- primeiro mes financeiro fechado
- 10 fotos de evolucao
- 100 tarefas concluidas

---

## 9. Privacidade e Seguranca

Regras obrigatorias para o Life OS:

- Diario privado por padrao.
- Fotos privadas por padrao.
- Financas privadas por padrao.
- Timeline privada por padrao.
- Compartilhamento futuro sempre explicito.
- Logs sem conteudo sensivel.
- Dados financeiros e emocionais tratados como sensiveis no design do produto.

Seguranca minima:

- JWT + refresh token HttpOnly.
- BCrypt para senha.
- Rate limit em login e refresh.
- Exportacao e exclusao de dados do usuario.
- Auditoria para eventos sensiveis.

---

## 10. Roadmap Estrategico

### Fase 0 - Preparacao

Objetivo: preparar a base para crescer sem bagunca.

Entregas:

- manter documentacao de produto atualizada
- confirmar padrao de nomes dos modulos
- criar convencao para eventos de Timeline
- criar convencao para XP
- revisar rotas e layout frontend para receber novas areas

Criterio de pronto:

- Documento Life OS aprovado como norte.
- Proximo modulo escolhido.
- Contratos iniciais do modulo desenhados.

### Fase 1 - Aurora Home como ponte

Objetivo: reposicionar o dashboard atual para comecar a parecer Life OS.

Escopo:

- manter widgets financeiros atuais
- adicionar espacos para "Meu Dia", "Rituais", "Minha Jornada" e "Linha da Vida"
- criar estrutura de widgets configuraveis simples
- preparar UI para estado vazio de modulos ainda nao implementados

Valor:

- O usuario ja sente que o Aurora esta virando um sistema pessoal, sem quebrar o financeiro existente.

### Fase 2 - Meu Dia + Tarefas simples

Objetivo: criar a tela diaria de execucao.

Escopo:

- entidade DailyTask ou Task
- top 3 prioridades
- tarefas de hoje
- concluir tarefa
- revisar vencidas
- mini check-in de humor simples
- eventos `TaskCompleted` e `MoodCheckedIn` na Timeline futuramente

Criterio de pronto:

- Usuario consegue abrir o Aurora e saber o que precisa fazer hoje.

### Fase 3 - Rituais

Objetivo: adicionar habitos com streak e XP simples.

Escopo:

- Habit
- HabitCheckIn
- CRUD de habitos
- check-in diario/semanal
- current streak e best streak
- XP limitado por periodo
- card de habitos no Aurora Home

Criterio de pronto:

- Usuario consegue criar habitos, marcar progresso e ver consistencia.

### Fase 4 - Linha da Vida basica

Objetivo: criar o feed privado que conecta tudo.

Escopo:

- TimelineEvent
- listagem paginada
- post manual
- eventos automaticos de tarefas e habitos
- filtros por tipo e periodo
- ocultar evento
- favoritar memoria

Criterio de pronto:

- O Aurora passa a registrar uma narrativa da evolucao do usuario.

### Fase 5 - Minha Jornada

Objetivo: metas de vida com milestones.

Escopo:

- Goal
- Milestone
- progresso manual
- vinculo com habitos e tarefas
- conclusao com validacao de milestones obrigatorios
- eventos na Timeline
- XP por milestone e meta concluida

Criterio de pronto:

- Usuario consegue acompanhar objetivos de medio e longo prazo.

### Fase 6 - Minha Semana

Objetivo: planejamento e revisao semanal.

Escopo:

- WeeklyPlan
- foco da semana
- prioridades
- metas relacionadas
- habitos prioritarios
- fechamento/revisao
- congelamento de metricas ao encerrar
- XP por revisao fechada

Criterio de pronto:

- Usuario consegue planejar a semana e revisar como foi.

### Fase 7 - Diario

Objetivo: reflexao privada e historico emocional.

Escopo:

- DiaryEntry
- humor de 1 a 5
- tags
- busca simples
- fotos opcionais
- card de humor recente no Aurora Home
- eventos discretos na Timeline, sem expor conteudo por padrao

Criterio de pronto:

- Usuario consegue entender padroes emocionais ao longo do tempo.

### Fase 8 - Evolucao Visual

Objetivo: fotos como memoria de progresso.

Escopo:

- EvolutionAlbum
- EvolutionPhoto
- upload/storage de imagem
- legenda e tags
- vinculo com meta/habito
- antes/depois simples
- eventos na Timeline

Criterio de pronto:

- Usuario consegue enxergar visualmente sua evolucao.

### Fase 9 - Dinheiro dentro do Life OS

Objetivo: evoluir o financeiro atual sem deixar ele dominar o produto.

Escopo:

- fechamento mensal
- resumo financeiro na Timeline
- metas financeiras conectadas a Goals
- card financeiro no Aurora Home
- retrospectiva financeira mensal

Criterio de pronto:

- Financas viram uma area da vida, integrada ao restante do sistema.

### Fase 10 - Retrospectivas

Objetivo: criar o "wrapped da vida".

Escopo:

- resumo semanal
- resumo mensal
- melhores fotos
- habitos mais consistentes
- XP ganho
- metas avancadas
- humor medio
- gastos e economia

Criterio de pronto:

- Usuario consegue fechar ciclos e visualizar progresso com emocao.

---

## 11. Sequencia Recomendada de Implementacao Tecnica

Para cada modulo:

1. Criar entidades, enums e invariantes no Domain.
2. Criar interfaces de repositorio na Application.
3. Criar repositorios Mongo e indices na Infrastructure.
4. Criar commands/queries com validators.
5. Criar controller com rotas pequenas.
6. Criar testes de dominio e handlers principais.
7. Criar pagina e componentes no frontend.
8. Conectar modulo ao Aurora Home.
9. Emitir eventos para Timeline quando aplicavel.
10. Adicionar XP somente depois da regra principal estar estavel.

Ordem tecnica sugerida:

```text
Today/Tasks
Habits
Timeline
Goals
WeeklyPlanning
Diary
EvolutionPhotos
FinanceLifeOsIntegration
Retrospectives
GamificationAdvanced
```

---

## 12. MVP Life OS

O primeiro MVP Life OS deve conter:

- Auth existente
- Aurora Home simples
- Meu Dia
- Rituais
- Minha Jornada
- Linha da Vida basica
- Evolucao simples
- Dinheiro como resumo integrado
- XP simples

Fora do primeiro MVP:

- IA
- social publico
- grupos
- marketplace
- automacoes complexas
- retrospectiva anual completa
- edicao avancada de fotos
- importacao bancaria

---

## 13. Decisoes Pendentes

- O Aurora sera primeiro local/self-hosted, SaaS ou ambos?
- Imagens serao armazenadas localmente no MVP ou em storage externo?
- O frontend deve migrar para TypeScript antes dos modulos Life OS?
- A Timeline deve salvar snapshots textuais dos eventos ou sempre consultar a origem?
- XP deve ser recalculavel ou registrado como ledger imutavel?
- Quais areas de vida serao padrao?
- O modulo de tarefas sera simples ou tera projetos/backlog?

Recomendacao inicial:

- Timeline deve salvar um snapshot basico do evento para preservar memoria historica.
- XP deve ser registrado em ledger para evitar inconsistencias e permitir auditoria.
- Areas padrao devem ser simples: Saude, Trabalho, Estudos, Dinheiro, Relacionamentos, Casa, Lazer, Espiritualidade, Projetos.
- Tarefas devem comecar simples e focadas no dia.

---

## 14. Regra de Ouro

Nao implementar o sistema inteiro de uma vez.

O Aurora deve evoluir como produto vivo:

```text
Meu Dia
+ Rituais
+ Minha Jornada
+ Linha da Vida
+ Evolucao
+ Retrospectiva
+ Dinheiro integrado
```

Cada modulo precisa aumentar a sensacao de continuidade, identidade e progresso. Se uma funcionalidade nao ajuda o usuario a se organizar, se entender ou visualizar evolucao, ela deve esperar.
