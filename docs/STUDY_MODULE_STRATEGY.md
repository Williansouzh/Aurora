# Aurora Studies - Modulo de Estudos baseado em ciencia de aprendizagem

> Documento guia para criar o modulo de Estudos do Aurora Life OS.
> Base: analise do PDF "Aprendendo a aprender" enviado pelo usuario em 2026-06-01.
> Relacionado: [LIFE_OS_STRATEGY.md](./LIFE_OS_STRATEGY.md) e [ACCESS_CONTROL_PLAN.md](./ACCESS_CONTROL_PLAN.md).

---

## 1. Objetivo do Modulo

O modulo de Estudos deve ajudar o usuario a escolher melhor o que estudar, montar um setup de estudo realista, executar sessoes com foco e revisar o aprendizado com metodos validados pela ciencia cognitiva.

Ele nao deve ser apenas uma lista de cursos. O papel do modulo e responder:

- O que eu deveria estudar agora?
- Por que isso e prioridade?
- Qual e o meu setup de estudo?
- Qual e o proximo bloco executavel?
- Eu estou apenas consumindo conteudo ou realmente aprendendo?
- O que precisa ser revisado, aplicado ou ensinado?

Nome tecnico recomendado: `Studies`.
Nome no produto recomendado: `Estudos`.

---

## 2. Principios extraidos do PDF

O PDF traz uma estrutura muito boa para virar produto:

1. Escolher 10 coisas que gostaria de aprender e cortar para 3 prioridades.
2. Estudar com foco nas 3 habilidades prioritarias.
3. Criar um proposito visivel para quebrar pressupostos.
4. Usar PERMA como contexto de bem-estar: emocao positiva, engajamento, relacionamentos, significado e realizacao.
5. Dividir aprendizado em quatro etapas:
   - obter informacoes e avaliar importancia relativa
   - organizar a informacao de forma entendivel
   - memorizar a informacao
   - aplicar a informacao
6. Usar aprendizado ativo.
7. Usar tecnica de Feynman para explicar com simplicidade.
8. Aplicar gestao de tempo com Pareto, Parkinson, Pomodoro e plano semanal.

Esses pontos devem virar comportamento do sistema, nao apenas texto explicativo.

---

## 3. Ciencia de Aprendizagem que deve guiar o produto

### 3.1 Active recall

O usuario aprende melhor quando tenta recuperar a informacao da memoria, em vez de apenas reler ou assistir.

No produto:

- cada topico deve ter perguntas de recuperacao
- cada sessao pode terminar com "o que eu lembro sem olhar?"
- o dashboard deve diferenciar tempo passivo de tempo ativo

### 3.2 Spaced repetition

Revisoes espacadas reduzem esquecimento.

No produto:

- todo conceito importante pode gerar uma revisao futura
- revisoes vencidas aparecem no dashboard de Estudos
- o sistema sugere revisoes em D+1, D+3, D+7, D+14 e D+30

### 3.3 Interleaving

Alternar tipos de problema melhora discriminacao e transferencia, principalmente em matematica, programacao e idiomas.

No produto:

- um plano pode misturar pratica, revisao e aplicacao
- o sistema evita varios blocos iguais seguidos quando isso prejudica aprendizado

### 3.4 Deliberate practice

Melhora vem de pratica focada em pontos fracos, com feedback.

No produto:

- cada habilidade deve ter gargalos
- a sessao registra dificuldade percebida
- erros viram itens de revisao ou tarefas de pratica

### 3.5 Elaboracao e Feynman

Explicar com as proprias palavras ajuda a encontrar buracos de entendimento.

No produto:

- toda sessao pode terminar com uma explicacao Feynman
- o usuario marca a explicacao como clara, confusa ou incompleta
- explicacoes incompletas geram tarefas de reforco

### 3.6 Cognitive load

Estudo deve ser dividido em blocos pequenos para nao sobrecarregar memoria de trabalho.

No produto:

- blocos devem ter tempo recomendado
- topicos grandes precisam ser quebrados em subtarefas
- o sistema deve sugerir "proximo passo pequeno"

---

## 4. Fluxo Principal do Usuario

### 4.1 Setup inicial de estudos

Primeira experiencia do modulo:

1. Usuario lista ate 10 coisas que quer aprender.
2. Sistema pede objetivo, motivo e impacto de cada item.
3. Usuario escolhe ou o sistema recomenda as 3 prioridades.
4. Para cada prioridade, usuario define:
   - nivel atual
   - objetivo mensuravel
   - prazo
   - tempo disponivel por semana
   - recursos principais
   - forma de aplicacao pratica
5. Sistema cria o primeiro plano semanal de estudos.

### 4.2 Rotina diaria

Na tela de Estudos:

- bloco de hoje
- revisoes vencidas
- pratica principal
- tempo planejado vs realizado
- proxima acao recomendada
- registro rapido de sessao

### 4.3 Fechamento de sessao

Ao finalizar uma sessao, o usuario registra:

- duracao
- tipo: obter, organizar, memorizar, aplicar, ensinar
- energia/foco
- dificuldade
- o que aprendeu
- Feynman curto
- erros ou duvidas
- proxima revisao

Isso alimenta XP, timeline e dashboard.

---

## 5. Analise de Prioridades

O modulo deve ter um motor simples de prioridade. A ideia do PDF "liste 10 e corte para 3" vira uma ferramenta.

### 5.1 Criterios

Cada habilidade recebe notas de 1 a 5:

- Impacto: quanto muda a vida/projeto do usuario.
- Urgencia: existe prazo, prova, trabalho ou oportunidade?
- Alinhamento: combina com metas de vida atuais?
- Pre-requisito: desbloqueia outras habilidades?
- Motivacao: o usuario tem energia real para isso?
- Aplicabilidade: da para praticar no mundo real agora?
- Custo de manutencao: quanto tempo exige para nao esquecer?

### 5.2 Formula inicial

```text
PriorityScore =
  Impacto * 2
  + Urgencia * 1.5
  + Alinhamento * 2
  + PreRequisito * 1.5
  + Motivacao
  + Aplicabilidade * 1.5
  - CustoDeManutencao
```

Regras:

- o sistema recomenda ate 3 prioridades ativas
- outras habilidades entram em backlog
- para ativar uma quarta prioridade, o usuario precisa pausar uma das 3
- prioridades podem ser reavaliadas semanalmente

---

## 6. Areas e Tipos de Estudo

O PDF cita exemplos pessoais:

- ingles
- programacao
- matematica basica
- espanhol
- comunicacao
- violao
- portugues avancado
- fisica basica
- nadar
- UX design

No Aurora, isso deve virar habilidades genericas com categoria:

- Idiomas
- Tecnologia
- Matematica
- Comunicacao
- Arte/Musica
- Saude/Corpo
- Design
- Escola/Faculdade
- Carreira
- Outros

---

## 7. Metodo Aurora de Estudos

Cada habilidade deve ser planejada nas quatro etapas do PDF.

### 7.1 Obter

Objetivo: encontrar fontes e entender importancia relativa.

Exemplos:

- curso principal
- livro base
- playlist
- documentacao
- roadmap
- professor/mentor

Produto:

- cadastro de recursos
- nota de confiabilidade
- ordem recomendada
- campo "por que isso importa?"

### 7.2 Organizar

Objetivo: transformar informacao em estrutura entendivel.

Exemplos:

- resumo
- mapa mental
- anotacoes no Notion/caderno
- roteiro de estudo
- lista de conceitos

Produto:

- topicos e subtopicos
- notas conectadas
- mapa simples por topico
- checklist de entendimento

### 7.3 Memorizar

Objetivo: consolidar informacao.

Exemplos:

- flashcards
- revisao de mapas mentais
- revisao de resumos
- perguntas de recuperacao

Produto:

- revisoes espacadas
- fila de conceitos vencidos
- indicador de retencao

### 7.4 Aplicar

Objetivo: usar o conhecimento.

Exemplos:

- exercicios
- projetos
- gravar video explicando
- resolver problema real
- ler/assistir em outro idioma

Produto:

- tarefas praticas
- entregaveis
- diario de erros
- evidencias de aplicacao

---

## 8. Entidades de Dominio

### StudySkill

```text
StudySkill
- Id
- UserId
- Title
- Category
- Area = Studies
- Status
- PriorityRank
- PriorityScore
- Purpose
- CurrentLevel
- TargetLevel
- StartDate
- TargetDate
- WeeklyTimeBudgetMinutes
- CreatedAt
- UpdatedAt
```

Status:

```text
Backlog
Active
Paused
Completed
Archived
```

### StudyPriorityAssessment

```text
StudyPriorityAssessment
- Id
- SkillId
- UserId
- Impact
- Urgency
- Alignment
- PrerequisitePower
- Motivation
- Applicability
- MaintenanceCost
- Score
- Notes
- CreatedAt
```

### StudyResource

```text
StudyResource
- Id
- SkillId
- UserId
- Title
- Type
- Url
- Author
- Reliability
- Status
- SortOrder
```

Types:

```text
Course
Book
Video
Documentation
Article
ExerciseList
Mentor
Other
```

### StudyTopic

```text
StudyTopic
- Id
- SkillId
- UserId
- Title
- ParentTopicId
- Stage
- Status
- Importance
- Confidence
- Notes
```

Stage:

```text
Obtain
Organize
Memorize
Apply
Teach
```

### StudySession

```text
StudySession
- Id
- UserId
- SkillId
- TopicId
- Date
- PlannedMinutes
- ActualMinutes
- Stage
- FocusScore
- EnergyScore
- DifficultyScore
- Summary
- FeynmanExplanation
- NextAction
- XPGenerated
- CreatedAt
```

### StudyReview

```text
StudyReview
- Id
- UserId
- SkillId
- TopicId
- SourceSessionId
- DueDate
- CompletedAt
- Result
- ConfidenceBefore
- ConfidenceAfter
- NextDueDate
```

Result:

```text
Again
Hard
Good
Easy
```

### StudyPracticeTask

```text
StudyPracticeTask
- Id
- UserId
- SkillId
- TopicId
- Title
- Description
- Status
- DueDate
- EvidenceUrl
- Reflection
- CreatedAt
```

---

## 9. Dashboard de Estudos

O dashboard separado do modulo deve mostrar:

- habilidades ativas: no maximo 3
- tempo estudado na semana
- tempo planejado vs realizado
- revisoes vencidas
- proximo bloco recomendado
- progresso por etapa: obter, organizar, memorizar, aplicar, ensinar
- top gargalos
- streak de estudos
- XP de estudos
- timeline recente de sessoes

Cards recomendados:

1. Hoje nos Estudos
2. Revisoes Vencidas
3. Prioridades Ativas
4. Plano Semanal
5. Aplicacao Pratica
6. Feynman / Ensinar
7. Gargalos

---

## 10. Integracao com Life OS

### Hoje

O modulo deve enviar para "Meu Dia":

- bloco de estudo do dia
- revisoes vencidas
- pratica principal

### Minha Semana

O modulo deve enviar:

- tempo semanal planejado
- foco de estudo da semana
- top 3 resultados esperados

### Minha Jornada

Cada habilidade pode estar ligada a uma meta.

Exemplos:

- Meta: "Aprender ingles ate B1"
- Habilidade: Ingles
- Milestones: curso base, 500 flashcards, 20 horas de listening, 5 conversacoes

### Rituais

Estudo recorrente pode virar habito:

- "Estudar ingles 30 min"
- "Resolver 10 exercicios de matematica"
- "Revisar Anki"

### Timeline

Eventos automaticos:

- sessao de estudo concluida
- revisao importante feita
- topico aplicado em projeto
- habilidade subiu de nivel
- prioridade alterada

### Retrospectiva

Resumo semanal/mensal:

- horas estudadas
- melhor habilidade da semana
- revisoes concluidas
- entregaveis praticos
- conceitos mais dificeis
- evolucao de confianca

---

## 11. Gamificacao

XP deve premiar aprendizado real, nao consumo infinito.

Pontuacao inicial:

- sessao planejada concluida: 10 XP
- revisao vencida concluida: 8 XP
- pratica aplicada: 20 XP
- Feynman claro: 15 XP
- topico concluido: 25 XP
- habilidade finalizada: 100 XP

Anti-farm:

- limite diario de XP por releitura/passivo
- aplicar e ensinar valem mais do que assistir
- sessoes muito curtas nao devem gerar XP cheio
- repetir a mesma acao varias vezes reduz retorno

Conquistas:

- Primeira sessao de estudo
- 7 dias estudando
- 30 revisoes concluidas
- Primeiro Feynman claro
- Primeiro projeto aplicado
- 3 prioridades definidas
- 10 horas em uma habilidade

---

## 12. MVP Recomendado

### MVP 1 - Setup e Prioridades

Objetivo: ajudar o usuario a decidir o que estudar.

Entregas:

- modulo `Studies` no catalogo de acesso
- rota `/studies`
- tela de onboarding de estudos
- lista de ate 10 habilidades desejadas
- avaliacao de prioridade
- selecao de 3 habilidades ativas
- dashboard simples de Estudos

### MVP 2 - Sessoes

Objetivo: transformar prioridade em execucao.

Entregas:

- criar sessao planejada
- iniciar/finalizar sessao
- registrar etapa do estudo
- registrar foco, energia, dificuldade
- gerar evento na Timeline
- gerar XP simples

Status em 2026-06-01: implementado como primeiro corte funcional.

Implementado:

- entidade `StudySession`
- endpoint para listar sessoes
- endpoint para criar sessao planejada
- endpoint para finalizar sessao
- registro de etapa: obter, organizar, memorizar, aplicar e ensinar
- registro de minutos reais, foco, energia, dificuldade, resumo, Feynman e proxima acao
- XP simples ao finalizar sessao
- evento automatico na Timeline ao finalizar sessao
- cards de sessoes recentes na tela `/studies`
- modais de planejar e finalizar sessao

### MVP 3 - Revisoes

Objetivo: adicionar memoria de longo prazo.

Entregas:

- criar revisoes a partir de topicos/sessoes
- fila de revisoes vencidas
- agenda D+1, D+3, D+7, D+14, D+30
- indicador de confianca

Status em 2026-06-01: implementado como primeiro corte funcional.

Implementado:

- entidade `StudyReview`
- endpoint de revisoes vencidas
- endpoint para concluir revisao
- resultado da revisao: repetir, dificil, bom e facil
- confianca antes/depois
- agendamento automatico da proxima revisao
- criacao automatica de revisao D+1 ao finalizar sessao
- metricas de revisoes vencidas e revisoes concluidas na semana no dashboard de Estudos
- fila de revisoes vencidas na tela `/studies`
- evento automatico na Timeline ao concluir revisao
- XP simples ao concluir revisao

### MVP 4 - Topicos e recursos

Objetivo: organizar cada habilidade em uma trilha clara de estudo.

Entregas:

- criar topicos por habilidade
- classificar topicos por etapa: obter, organizar, memorizar, aplicar e ensinar
- registrar importancia e confianca por topico
- cadastrar recursos por habilidade
- classificar recursos por tipo: curso, livro, video, documentacao, artigo, lista de exercicios, mentor e outro
- registrar confiabilidade e status do recurso
- exibir trilha de estudos na tela `/studies`

Status em 2026-06-01: implementado como primeiro corte funcional.

Implementado:

- entidade `StudyTopic`
- entidade `StudyResource`
- endpoints para listar, criar e atualizar topicos
- endpoints para listar, criar e atualizar recursos
- cards de trilha de estudos na tela `/studies`
- seletor de habilidade para visualizar topicos e recursos por modulo de estudo
- modais de cadastro de topico e recurso
- indices MongoDB por usuario, habilidade, etapa e ordem

### MVP 5 - Aplicacao e Feynman

Objetivo: medir aprendizado real.

Entregas:

- tarefas praticas
- campo Feynman por sessao
- registro de erros/duvidas
- dashboard de gargalos

Status em 2026-06-01: implementado como primeiro corte funcional.

Implementado:

- entidade `StudyPracticeTask`
- endpoint para listar praticas por habilidade
- endpoint para criar tarefa pratica
- endpoint para concluir pratica com resultado
- registro de explicacao Feynman por pratica
- registro de erros percebidos e duvidas abertas
- XP ao concluir pratica
- revisao espacada automatica criada a partir da pratica
- evento automatico na Timeline ao concluir pratica
- metricas de praticas abertas e praticas concluidas na semana no dashboard de Estudos
- cards de pratica na trilha da habilidade em `/studies`
- modais para criar e concluir pratica

### MVP 6 - Integracao profunda

Objetivo: conectar Estudos ao Life OS inteiro.

Entregas:

- integracao com Hoje
- integracao com Minha Semana
- integracao com Metas
- retrospectiva de estudos
- graficos e comparativos

Status em 2026-06-01: implementado como primeiro corte funcional.

Implementado:

- sessoes planejadas de estudo geram tarefas automaticamente em `Meu Dia`
- praticas planejadas geram tarefas automaticamente em `Meu Dia`
- ao finalizar sessao de estudo, a tarefa vinculada em `Meu Dia` e marcada como concluida
- ao concluir pratica, a tarefa vinculada em `Meu Dia` e marcada como concluida
- tarefas vindas de Estudos recebem `SourceModule = Studies` e `SourceId` para evitar perda de contexto
- `Minha Semana` recebe resumo de Estudos no plano atual
- resumo semanal de Estudos inclui prioridades ativas, minutos planejados, minutos estudados, sessoes, revisoes e praticas
- Retrospectiva semanal inclui minutos estudados, sessoes concluidas, revisoes e praticas
- Retrospectiva mensal inclui minutos estudados, sessoes concluidas, revisoes e praticas
- Timeline usa eventos distintos para sessao, revisao e pratica
- XP continua sendo emitido por sessoes, revisoes e praticas concluidas

Fora deste corte:

- vincular Estudos diretamente a Metas
- graficos avancados de comparacao
- retrospectiva visual dedicada somente a Estudos

---

## 13. APIs Recomendadas

```text
GET    /api/studies/dashboard
GET    /api/studies/skills
POST   /api/studies/skills
PUT    /api/studies/skills/{id}
POST   /api/studies/skills/{id}/priority-assessment
PATCH  /api/studies/skills/{id}/activate
PATCH  /api/studies/skills/{id}/pause

GET    /api/studies/sessions
POST   /api/studies/sessions
PATCH  /api/studies/sessions/{id}/finish

GET    /api/studies/reviews/due
PATCH  /api/studies/reviews/{id}/complete

GET    /api/studies/skills/{skillId}/topics
POST   /api/studies/skills/{skillId}/topics
PUT    /api/studies/topics/{id}

GET    /api/studies/skills/{skillId}/resources
POST   /api/studies/skills/{skillId}/resources
PUT    /api/studies/resources/{id}

GET    /api/studies/skills/{skillId}/practice-tasks
POST   /api/studies/skills/{skillId}/practice-tasks
PATCH  /api/studies/practice-tasks/{id}/complete
```

Todos devem usar:

```text
[RequireModule(ModuleKeys.Studies)]
```

---

## 14. Frontend Recomendado

Rota:

```text
/studies
```

Abas:

- Dashboard
- Prioridades
- Sessoes
- Revisoes
- Topicos
- Recursos
- Pratica

Componentes principais:

- `StudyDashboardPage`
- `StudySkillPriorityWizard`
- `ActiveStudySkills`
- `StudySessionTimer`
- `StudyReviewQueue`
- `StudyStageBoard`
- `FeynmanReflectionForm`
- `StudySetupChecklist`

---

## 15. Plano Tecnico de Implementacao

### Backend

1. Adicionar `ModuleKeys.Studies`.
2. Atualizar `AccessControlSeeder` com modulo Estudos.
3. Adicionar `Studies` aos planos que devem receber o modulo.
4. Criar entidades de dominio.
5. Criar repositorios Mongo.
6. Criar comandos/queries em `Aurora.Application/Features/Studies`.
7. Criar `StudiesController`.
8. Criar eventos para Timeline e XP.

### Frontend

1. Adicionar menu `Estudos` em `Life OS`.
2. Adicionar rota protegida por modulo.
3. Criar dashboard inicial.
4. Criar wizard de prioridade.
5. Criar cadastro de sessoes.
6. Criar fila de revisoes.
7. Adicionar filtro de Estudos no dashboard geral.

### Dados

Seed inicial sugerido para usuario de teste:

- Ingles
- Programacao
- Matematica basica

Cada habilidade deve vir com:

- proposito
- recursos
- topicos iniciais
- uma sessao planejada
- algumas revisoes vencidas/futuras

---

## 16. Regras de Produto

- O usuario nao deve ter mais de 3 habilidades ativas por padrao.
- Estudo passivo deve contar, mas valer menos.
- Aplicar e ensinar devem ter destaque visual.
- Revisoes vencidas devem aparecer antes de novo conteudo.
- O sistema deve sugerir blocos pequenos e executaveis.
- O usuario deve conseguir pausar uma habilidade sem perder historico.
- O dashboard deve mostrar progresso real, nao apenas tempo gasto.
- Tudo deve alimentar a Timeline como registro privado da evolucao.

---

## 17. Decisao Recomendada

O modulo de Estudos deve entrar como um dos proximos modulos Life OS porque combina diretamente com:

- metas
- habitos
- planejamento semanal
- timeline
- retrospectivas
- gamificacao

Ordem recomendada depois da separacao atual por modulos:

1. Criar catalogo/acesso do modulo `Studies`.
2. Criar tela simples `/studies` com dashboard e wizard de prioridades.
3. Implementar sessoes de estudo.
4. Implementar revisoes espacadas.
5. Integrar com Hoje, Minha Semana e Timeline.
