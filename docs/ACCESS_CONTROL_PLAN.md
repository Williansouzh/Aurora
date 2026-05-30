# Aurora - Plano de Controle de Acesso, Planos e Super Admin

> Objetivo: permitir que o Aurora seja lancado modulo por modulo, com controle por usuario, plano e estado de rollout, sem misturar regra comercial dentro das telas.
> Ultima atualizacao: 2026-05-30.

---

## 1. Objetivo do Controle de Acesso

O Aurora precisa ter uma camada central para decidir:

- quem e super admin
- quais usuarios existem
- qual plano cada usuario possui
- quais modulos cada plano libera
- quais modulos cada usuario pode acessar por excecao
- quais modulos estao em beta, ativos, ocultos ou bloqueados
- quais areas da vida aparecem no produto

Isso permite lancar os modulos um de cada vez conforme ficam bons, sem precisar remover codigo, esconder manualmente telas ou criar varios builds diferentes.

---

## 2. Principios

1. Backend sempre decide acesso real.
2. Frontend apenas esconde, bloqueia ou mostra estados conforme permissao recebida.
3. Plano define o acesso padrao.
4. Usuario pode ter overrides individuais.
5. Super admin pode acessar a tela administrativa, mas nao deve ler diario/fotos/financas privadas sem uma regra explicita de suporte.
6. Modulo pode existir no codigo, mas ficar indisponivel para usuarios ate ser liberado.
7. Tudo que muda acesso deve gerar audit log.

---

## 3. Conceitos

### Super Admin

Usuario interno com permissao para operar o produto.

Pode:

- listar usuarios
- editar status do usuario
- definir plano do usuario
- liberar ou bloquear modulos para usuarios especificos
- configurar planos
- configurar catalogo de modulos
- configurar areas da vida disponiveis
- ver logs administrativos

Nao deve, por padrao:

- ler conteudo privado do diario
- abrir fotos privadas
- visualizar transacoes detalhadas
- editar dados pessoais do usuario sem auditoria

### Plano

Pacote comercial ou operacional de acesso.

Exemplos:

- Free
- Early Access
- Pro
- Founder
- Internal

Plano define:

- modulos liberados
- limites de uso
- features experimentais
- storage maximo
- quantidade de fotos
- historico permitido

### Modulo

Uma area funcional do produto.

Exemplos:

- Home
- Today
- Habits
- Goals
- Timeline
- Diary
- Evolution
- WeeklyPlanning
- Finances
- Retrospectives
- Admin

### Area da Vida

Categoria conceitual usada dentro dos modulos.

Exemplos:

- Saude
- Trabalho
- Estudos
- Dinheiro
- Relacionamentos
- Casa
- Lazer
- Espiritualidade
- Projetos

Area da vida nao e a mesma coisa que modulo. "Saude" pode aparecer em Habits, Goals, Diary, Timeline e Evolution.

---

## 4. Modelo de Permissao Recomendado

Usar tres camadas:

```text
Role -> Plan -> UserModuleOverride
```

### Role

Define poder administrativo.

```text
UserRole
- User
- Support
- Admin
- SuperAdmin
```

Uso:

- `User`: usuario normal
- `Support`: suporte limitado, sem acesso a dados sensiveis
- `Admin`: administra configuracoes operacionais
- `SuperAdmin`: controla usuarios, planos e modulos

### Plan

Define acesso comercial padrao.

```text
Plan
- Id
- Key
- Name
- Description
- Status
- MonthlyPrice
- YearlyPrice
- ModuleKeys
- Limits
- CreatedAt
- UpdatedAt
```

Status:

```text
Draft
Active
Deprecated
Archived
```

### ModuleCatalog

Catalogo central de modulos.

```text
ModuleCatalogItem
- Id
- Key
- Name
- ProductName
- Description
- Area
- Status
- ReleaseStage
- SortOrder
- Icon
- Route
- RequiredRole
- CreatedAt
- UpdatedAt
```

Status:

```text
Enabled
Disabled
Hidden
Archived
```

ReleaseStage:

```text
Internal
Alpha
Beta
Released
Deprecated
```

Exemplo:

```text
Key: habits
Name: Habits
ProductName: Rituais
Route: /habits
ReleaseStage: Beta
Status: Enabled
```

### UserSubscription

Plano atual do usuario.

```text
UserSubscription
- Id
- UserId
- PlanId
- Status
- StartedAt
- EndsAt
- TrialEndsAt
- CancelledAt
- CreatedAt
- UpdatedAt
```

Status:

```text
Trial
Active
PastDue
Cancelled
Expired
Internal
```

### UserModuleOverride

Excecao individual por usuario.

```text
UserModuleOverride
- Id
- UserId
- ModuleKey
- Access
- Reason
- ExpiresAt
- CreatedByUserId
- CreatedAt
- UpdatedAt
```

Access:

```text
Allow
Deny
Beta
Readonly
```

Regra:

- `Deny` individual vence sobre plano.
- `Allow` individual libera mesmo se o plano nao liberar.
- `Beta` libera modulo em beta para usuario selecionado.
- `Readonly` permite visualizar dados, mas nao criar/editar.

---

## 5. Algoritmo de Decisao

Para cada requisicao de modulo:

1. Usuario esta autenticado?
2. Usuario esta ativo e nao deletado?
3. Modulo existe no catalogo?
4. Modulo esta `Enabled`?
5. Role do usuario permite acesso administrativo, se for modulo admin?
6. Existe override individual `Deny` valido? Se sim, bloquear.
7. Existe override individual `Allow` ou `Beta` valido? Se sim, liberar conforme modo.
8. Plano atual inclui o modulo?
9. Release stage permite acesso para esse plano?
10. Limites de uso foram respeitados?

Resultado:

```text
Allowed
Denied
Readonly
UpgradeRequired
BetaOnly
ModuleDisabled
```

---

## 6. Backend

### Entidades novas

Adicionar no Domain:

```text
Plan
ModuleCatalogItem
UserSubscription
UserModuleOverride
LifeAreaCatalogItem
AdminAuditLog
```

Adicionar no User:

```text
Role
Status
```

Status recomendado:

```text
Active
Suspended
Invited
Deleted
```

### Claims no JWT

Adicionar claims leves:

```text
sub: userId
email
name
role
plan
```

Nao colocar todos os modulos no JWT como fonte principal, porque permissao pode mudar durante a sessao. Usar endpoint `/api/access/me` com cache curto.

### Servico de acesso

Criar:

```text
IAccessControlService
- GetAccessSnapshotAsync(userId)
- CanAccessModuleAsync(userId, moduleKey, action)
- EnsureCanAccessModuleAsync(userId, moduleKey, action)
```

Snapshot:

```text
AccessSnapshot
- UserId
- Role
- PlanKey
- Modules
- LifeAreas
- Limits
- GeneratedAt
```

### Authorization Policy

Criar policies:

```text
RequireSuperAdmin
RequireAdmin
RequireModule:<moduleKey>
```

Na pratica, em controllers:

```text
[Authorize]
[RequireModule("habits")]
```

ou dentro dos handlers, quando a regra depender da acao:

```text
access.EnsureCanAccessModuleAsync(userId, "habits", "write")
```

### Middlewares / Filters

Opcao recomendada:

- criar atributo `RequireModuleAttribute`
- implementar `IAsyncAuthorizationFilter`
- resolver `IAccessControlService`
- bloquear com `403` e payload padronizado

Payload:

```json
{
  "success": false,
  "error": "ModuleAccessDenied",
  "message": "Este modulo ainda nao esta disponivel para o seu plano.",
  "metadata": {
    "module": "habits",
    "reason": "UpgradeRequired"
  }
}
```

---

## 7. Frontend

### Access Provider

Ao carregar o app autenticado:

```text
GET /api/access/me
```

Guardar:

```text
access.modules
access.lifeAreas
access.role
access.plan
```

### Rotas

Cada rota deve declarar modulo:

```text
/today -> today
/habits -> habits
/goals -> goals
/timeline -> timeline
/diary -> diary
/evolution -> evolution
/weekly -> weekly-planning
/dashboard -> finances/home
/admin -> admin
```

Se bloqueado:

- esconder da sidebar quando `Hidden` ou sem acesso
- mostrar tela de "modulo em breve" se fizer sentido para marketing interno
- mostrar "fale comigo para liberar beta" no Early Access
- nunca confiar so no frontend

### Shell

O menu lateral deve ser gerado a partir de:

```text
ModuleCatalog + AccessSnapshot
```

Assim, quando o super admin libera um modulo, ele aparece sem deploy novo.

---

## 8. Tela Super Admin

Rota:

```text
/admin
```

Backend:

```text
/api/admin/users
/api/admin/plans
/api/admin/modules
/api/admin/life-areas
/api/admin/audit-logs
```

Todas exigem:

```text
RequireSuperAdmin
```

### Views principais

#### Usuarios

Tabela:

- nome
- email
- status
- role
- plano
- modulos liberados por override
- ultimo login
- criado em

Acoes:

- alterar plano
- suspender usuario
- reativar usuario
- alterar role
- liberar modulo
- bloquear modulo
- definir beta access
- remover override
- ver auditoria do usuario

#### Detalhe do Usuario

Abas:

- Perfil
- Plano
- Modulos
- Limites
- Auditoria

Em Modulos:

```text
Modulo | Plano libera? | Override | Acesso final | Expira em | Acao
```

#### Planos

Tabela:

- nome
- key
- status
- preco
- quantidade de usuarios
- modulos incluidos

Acoes:

- criar plano
- editar plano
- ativar/desativar
- duplicar plano
- escolher modulos
- configurar limites

#### Catalogo de Modulos

Tabela:

- nome no produto
- key
- rota
- status
- release stage
- ordem
- role minima

Acoes:

- ativar/desativar modulo globalmente
- mudar etapa de release
- marcar como visivel/invisivel
- definir ordem no menu
- definir se aparece como "em breve"

#### Areas da Vida

Tabela:

- nome
- key
- cor
- icone
- status
- ordem

Acoes:

- ativar/desativar area
- reordenar
- editar nome/cor/icone

---

## 9. Modulos e Chaves Padrao

Usar keys estaveis, em ingles, sem depender do texto exibido:

| Key | Nome tecnico | Nome no produto |
| --- | --- | --- |
| home | Home | Aurora Home |
| today | Today | Meu Dia |
| tasks | Tasks | Tarefas |
| habits | Habits | Rituais |
| goals | Goals | Minha Jornada |
| timeline | Timeline | Linha da Vida |
| diary | Diary | Diario |
| evolution | EvolutionPhotos | Evolucao |
| weekly-planning | WeeklyPlanning | Minha Semana |
| finances | Finances | Dinheiro |
| retrospectives | Retrospectives | Retrospectiva |
| admin | Admin | Super Admin |

---

## 10. Planos Iniciais

### Free

Modulos:

- home
- finances
- today limitado

Limites:

- poucas metas
- poucos habitos
- sem evolution photos

### Early Access

Modulos:

- home
- finances
- today
- habits
- goals
- timeline
- weekly-planning conforme liberacao

Uso:

- usuarios de teste
- feedback dos modulos novos

### Pro

Modulos:

- todos os modulos released
- alguns beta liberaveis por override

### Founder

Modulos:

- todos os released
- beta por padrao
- limites altos

### Internal

Modulos:

- todos os modulos
- admin conforme role

---

## 11. Rollout de Modulos

Cada modulo deve passar por estes estados:

```text
Internal -> Alpha -> Beta -> Released
```

### Internal

Somente `SuperAdmin` e usuarios internos.

### Alpha

Poucos usuarios via override `Allow` ou `Beta`.

### Beta

Usuarios Early Access e Founder.

### Released

Disponivel conforme plano.

### Hidden

Modulo existe, mas nao aparece para usuario comum.

---

## 12. Auditoria

Toda acao do super admin deve registrar:

```text
AdminAuditLog
- Id
- ActorUserId
- TargetUserId
- Action
- EntityType
- EntityId
- Before
- After
- Reason
- IpAddress
- UserAgent
- CreatedAt
```

Eventos auditaveis:

- alteracao de role
- alteracao de plano
- suspensao/reativacao
- liberacao/bloqueio de modulo
- alteracao de catalogo de modulos
- alteracao de areas da vida
- alteracao de limites de plano

---

## 13. Cache e Consistencia

Permissoes podem ser cacheadas por pouco tempo:

```text
access:{userId} -> 1 a 5 minutos
```

Invalidar cache quando:

- usuario troca de plano
- override muda
- modulo muda status
- plano muda modulos
- role muda

Para mudancas criticas, invalidar imediatamente.

---

## 14. Ordem de Implementacao

### Fase 1 - Base de Acesso

- Criar enums `UserRole`, `UserStatus`, `ModuleStatus`, `ReleaseStage`, `ModuleAccess`.
- Adicionar `Role` e `Status` em `User`.
- Criar entidades `ModuleCatalogItem`, `Plan`, `UserSubscription`, `UserModuleOverride`.
- Criar repositorios e indices.
- Criar `IAccessControlService`.
- Criar `/api/access/me`.

### Fase 2 - Protecao por Modulo

- Criar `RequireModuleAttribute`.
- Aplicar nos controllers existentes.
- Padronizar erro `ModuleAccessDenied`.
- Adaptar frontend para esconder/bloquear rotas sem acesso.
- Gerar menu a partir do snapshot de acesso.

### Fase 3 - Super Admin MVP

- Criar rota `/admin`.
- Criar listagem de usuarios.
- Criar detalhe de usuario.
- Permitir alterar plano.
- Permitir liberar/bloquear modulos por usuario.
- Criar audit log administrativo.

### Fase 4 - Planos e Catalogo

- Tela de planos.
- Tela de catalogo de modulos.
- Tela de areas da vida.
- Configurar limites por plano.
- Permitir rollout por `ReleaseStage`.

### Fase 5 - Comercial e Operacao

- Trial, cancelamento e expiracao.
- Webhooks de pagamento se virar SaaS.
- Relatorios de usuarios por plano.
- Metricas de uso por modulo.
- Feature flags avancadas.

---

## 15. Recomendacao de Primeiro Corte

Para comecar simples e certo:

1. `User.Role`
2. `User.Status`
3. `ModuleCatalogItem`
4. `Plan`
5. `UserSubscription`
6. `UserModuleOverride`
7. `/api/access/me`
8. `RequireModuleAttribute`
9. `/admin/users`
10. tela `/admin` com Usuarios e Modulos do Usuario

Esse primeiro corte ja permite:

- esconder modulos ainda nao prontos
- liberar beta por usuario
- bloquear modulo problematico rapidamente
- dar acesso total para usuario interno
- manter o produto organizado enquanto os modulos amadurecem

---

## 16. Riscos

### Risco: regra duplicada no frontend

Mitigacao: frontend usa snapshot so para experiencia; backend sempre valida.

### Risco: super admin com acesso excessivo a dados privados

Mitigacao: separar permissao operacional de leitura de dados sensiveis.

### Risco: plano e override virarem confusos

Mitigacao: tela de detalhe sempre mostrar "acesso final" e "motivo".

### Risco: cache de permissao atrasar bloqueio

Mitigacao: invalidar cache em qualquer alteracao administrativa.

### Risco: mudanca de modulo quebrar usuarios antigos

Mitigacao: manter catalogo versionado e evitar renomear `ModuleKey`.

---

## 17. Regra de Produto

Nenhum modulo novo deve ser considerado lancado so porque existe rota ou tela.

Modulo lancado e modulo que:

- esta no catalogo
- tem status correto
- tem permissao aplicada no backend
- aparece no menu conforme acesso
- possui estado bloqueado/upgrade/em breve
- tem rollout controlavel pelo Super Admin
