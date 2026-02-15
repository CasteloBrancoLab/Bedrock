# Feature Specification: PostgreSQL Migrations BuildingBlock

**Feature Branch**: `feature/180-migration-postgresql-building-block`
**Created**: 2026-02-14
**Status**: Draft
**Input**: Criar novo BuildingBlock para migration de PostgreSQL usando FluentMigrator, com MigrationManagerBase abstrato, scripts SQL UP/DOWN padronizados e classes de migration com anotações vinculando aos scripts.

## Clarifications

### Session 2026-02-14

- Q: Convenção de nomenclatura para classes abstratas base? → A: Toda classe abstrata base DEVE ter sufixo `Base` (ex: `MigrationManagerBase`, não `MigrationManager`).
- Q: Formato de nomenclatura dos scripts SQL? → A: `V{timestamp}__{descricao}.sql` (ex: `V202602141200__create_users_table.sql`). Versão é timestamp long, separador duplo underscore.
- Q: Onde residem os scripts SQL e como são carregados? → A: No projeto do BC (`Infra.Data.PostgreSql`), em `Migrations/Scripts/Up/` e `Migrations/Scripts/Down/`, embarcados como embedded resources no assembly.
- Q: MigrationManagerBase recebe ExecutionContext? → A: Sim (opção B), para padronização de log com distributed tracing. Migrations são executadas exclusivamente via pipeline (não fazem parte do software deployado em runtime).
- Q: Como o desenvolvedor define uma migration? → A: Classe por migration decorada com `[Migration(version)]` do FluentMigrator e `[SqlScript("Up/V...sql", "Down/V...sql")]` customizado do Bedrock. A classe é puro metadado declarativo — a base lê e executa o SQL do embedded resource.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Criar e executar migrations de schema (Priority: P1)

Um desenvolvedor de um bounded context (ex: ShopDemo.Auth) precisa
evoluir o schema do banco de dados PostgreSQL de forma versionada e
reproduzível. Ele cria scripts SQL de UP (aplicar) e DOWN (reverter)
seguindo a convenção de nomes, cria uma classe de migration anotada
que referencia esses scripts, e executa as migrations pendentes
através do MigrationManagerBase do seu bounded context.

**Why this priority**: Sem a capacidade de executar migrations para
frente (UP), nenhuma outra funcionalidade de migração tem valor.
Este é o cenário fundamental que viabiliza schema evolution.

**Independent Test**: Pode ser testado criando um par de scripts
UP/DOWN, uma classe de migration, e executando o MigrationManagerBase
contra um banco de teste via Testcontainers. O schema resultante
DEVE refletir as mudanças definidas no script UP.

**Acceptance Scenarios**:

1. **Given** um bounded context com um MigrationManagerBase configurado
   e um par de scripts SQL (UP/DOWN) seguindo a convenção de nomes,
   **When** o desenvolvedor executa `MigrateUpAsync()`,
   **Then** o script UP é aplicado ao banco, a versão da migration é
   registrada na tabela de controle e o schema reflete as mudanças.

2. **Given** um bounded context com múltiplas migrations pendentes,
   **When** o desenvolvedor executa `MigrateUpAsync()`,
   **Then** todas as migrations pendentes são aplicadas em ordem
   crescente de versão.

3. **Given** um bounded context com todas as migrations já aplicadas,
   **When** o desenvolvedor executa `MigrateUpAsync()`,
   **Then** nenhuma ação é tomada e a operação é bem-sucedida.

4. **Given** um script UP com SQL inválido,
   **When** o desenvolvedor executa `MigrateUpAsync()`,
   **Then** a migration falha, nenhuma mudança parcial é aplicada
   (rollback transacional) e o erro é reportado com detalhes.

---

### User Story 2 - Reverter migrations (Priority: P2)

Um desenvolvedor precisa reverter uma ou mais migrations aplicadas
ao schema do banco, seja para corrigir um problema em produção ou
para retornar a um estado anterior durante desenvolvimento.

**Why this priority**: Rollback é o complemento essencial do UP e
garante segurança durante deploys. Sem rollback, erros em migrations
podem deixar o banco em estado irrecuperável.

**Independent Test**: Pode ser testado aplicando uma migration UP e
em seguida executando o rollback. O schema DEVE retornar ao estado
anterior.

**Acceptance Scenarios**:

1. **Given** um banco com a migration versão N aplicada,
   **When** o desenvolvedor executa `MigrateDownAsync(targetVersion)`,
   **Then** o script DOWN da versão N é executado e a versão é
   removida da tabela de controle.

2. **Given** um banco com migrations versões 1, 2 e 3 aplicadas,
   **When** o desenvolvedor executa `MigrateDownAsync(1)`,
   **Then** os scripts DOWN das versões 3 e 2 são executados em
   ordem decrescente e apenas a versão 1 permanece registrada.

3. **Given** um banco sem nenhuma migration aplicada,
   **When** o desenvolvedor executa `MigrateDownAsync(0)`,
   **Then** nenhuma ação é tomada e a operação é bem-sucedida.

---

### User Story 3 - Consultar status das migrations (Priority: P3)

Um desenvolvedor ou operador precisa verificar quais migrations foram
aplicadas, quais estão pendentes e o estado geral do schema, sem
executar nenhuma alteração.

**Why this priority**: Visibilidade do estado é essencial para
diagnóstico em ambientes de produção e para validação em pipelines,
mas não bloqueia a execução de migrations.

**Independent Test**: Pode ser testado aplicando algumas migrations e
verificando que o status reportado lista corretamente as migrations
aplicadas e pendentes.

**Acceptance Scenarios**:

1. **Given** um banco com algumas migrations aplicadas e outras
   pendentes,
   **When** o desenvolvedor consulta o status das migrations,
   **Then** o sistema retorna a lista de migrations aplicadas (com
   data de aplicação) e a lista de migrations pendentes.

2. **Given** um banco novo sem tabela de controle,
   **When** o desenvolvedor consulta o status,
   **Then** todas as migrations são reportadas como pendentes.

---

### User Story 4 - Configurar MigrationManagerBase por bounded context (Priority: P4)

Um desenvolvedor de bounded context precisa criar seu próprio
MigrationManagerBase concreto que herda do MigrationManagerBase abstrato do
BuildingBlock, configurando: connection string, schema de destino,
assemblies a escanear e comportamento customizado.

**Why this priority**: A extensibilidade por bounded context é
fundamental para o modelo de multi-projeto do Bedrock, mas depende
da infraestrutura base (US1-US3) estar funcional.

**Independent Test**: Pode ser testado criando um MigrationManagerBase
concreto com configuração específica e verificando que o
comportamento customizado é aplicado durante a execução.

**Acceptance Scenarios**:

1. **Given** uma classe concreta que herda de MigrationManagerBase,
   **When** o desenvolvedor implementa os métodos abstratos de
   configuração,
   **Then** o FluentMigrator é configurado com os parâmetros
   fornecidos pelo bounded context.

2. **Given** um MigrationManagerBase concreto com configuração de
   connection string e schema,
   **When** as migrations são executadas,
   **Then** os scripts são aplicados ao schema configurado usando
   a connection string fornecida.

---

### Edge Cases

- O que acontece quando um script UP referenciado pela anotação da
  classe de migration não existe no diretório esperado? O sistema
  DEVE falhar com mensagem clara indicando o nome do script
  esperado e o caminho completo.
- O que acontece quando duas migrations têm o mesmo número de versão?
  O sistema DEVE detectar conflitos de versão e falhar antes de
  executar qualquer migration, listando as classes conflitantes.
- O que acontece quando o script DOWN está vazio ou ausente?
  O sistema DEVE permitir scripts DOWN vazios (migration
  irreversível) mas registrar um warning. Scripts DOWN ausentes
  DEVEM causar erro se rollback for tentado.
- O que acontece durante execução concorrente de migrations
  (múltiplas instâncias da aplicação)? O sistema DEVE garantir
  que apenas uma instância execute migrations por vez (locking
  no banco).
- O que acontece quando a tabela de controle de migrations não
  existe? O sistema DEVE criá-la automaticamente na primeira
  execução.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O BuildingBlock DEVE fornecer uma classe abstrata
  `MigrationManagerBase` que serve como ponto de entrada público para
  toda operação de migration (UP, DOWN, status).
- **FR-002**: O `MigrationManagerBase` DEVE expor métodos abstratos
  para que classes filhas de bounded contexts concretos configurem
  connection string, schema de destino e assemblies a escanear.
- **FR-003**: O sistema DEVE utilizar FluentMigrator como engine
  de migrations, encapsulando toda interação com a biblioteca dentro
  do BuildingBlock.
- **FR-004**: O sistema DEVE suportar scripts SQL em arquivos
  físicos separados, organizados dentro do projeto do bounded
  context (ex: `Infra.Data.PostgreSql`) na estrutura fixa
  `Migrations/Scripts/Up/` e `Migrations/Scripts/Down/`. Os
  scripts DEVEM ser embarcados como embedded resources no
  assembly, eliminando dependência de filesystem em runtime.
- **FR-005**: O sistema DEVE impor convenção de nomenclatura
  padronizada para scripts: `V{timestamp}__{descricao}.sql`
  (ex: `V202602141200__create_users_table.sql`). A versão é um
  timestamp long (YYYYMMDDHHmm), o separador é duplo underscore
  (`__`), e a descrição usa snake_case.
- **FR-006**: O sistema DEVE fornecer um atributo customizado
  `[SqlScript]` que permite ao desenvolvedor vincular uma classe
  de migration aos caminhos dos scripts UP e DOWN como embedded
  resources (ex: `[SqlScript("Up/V202602141200__create_users.sql",
  "Down/V202602141200__create_users.sql")]`). A classe de
  migration é puro metadado declarativo — herda de uma base que
  lê e executa o SQL do embedded resource automaticamente nos
  métodos `Up()` e `Down()`. O desenvolvedor NÃO escreve código
  C# de schema, apenas aponta para os scripts SQL.
- **FR-007**: O `MigrationManagerBase` DEVE ser capaz de descobrir
  automaticamente todas as classes de migration no assembly
  configurado e orquestrar a execução via FluentMigrator.
- **FR-008**: O sistema DEVE executar migrations dentro de
  transações, garantindo atomicidade (tudo ou nada) por migration
  individual.
- **FR-009**: O sistema DEVE registrar cada migration aplicada em
  uma tabela de controle no banco, incluindo versão, descrição e
  timestamp de aplicação.
- **FR-010**: O sistema DEVE suportar rollback (DOWN) até uma
  versão alvo específica, executando os scripts DOWN em ordem
  decrescente.
- **FR-011**: O sistema DEVE fornecer capacidade de consulta de
  status que retorna migrations aplicadas e pendentes sem executar
  alterações.
- **FR-012**: O sistema DEVE validar a integridade das migrations
  antes de executar: verificar existência dos scripts referenciados,
  detectar conflitos de versão e reportar problemas com mensagens
  claras.
- **FR-013**: O sistema DEVE usar logging estruturado com
  distributed tracing (via ExecutionContext) para todas as
  operações, permitindo rastreabilidade completa. Os métodos
  públicos do `MigrationManagerBase` DEVEM receber
  `ExecutionContext` como primeiro parâmetro (BB-IV).
- **FR-015**: Migrations são executadas exclusivamente via
  pipeline (CI/CD ou linha de comando), NÃO fazem parte do
  software deployado em runtime. O bounded context DEVE criar
  um `ExecutionContext` dedicado para execução de migrations.
- **FR-014**: O sistema DEVE suportar locking de banco para
  prevenir execução concorrente de migrations por múltiplas
  instâncias da aplicação.

### Key Entities

- **Migration**: Representa uma unidade de mudança de schema
  versionada. Possui um número de versão (long), descrição, e
  referências aos scripts SQL de UP e DOWN.
- **MigrationManagerBase**: Ponto de entrada abstrato que orquestra
  descoberta, validação e execução de migrations. Cada bounded
  context implementa sua versão concreta.
- **MigrationInfo**: Representa o registro de uma migration
  aplicada na tabela de controle: versão, descrição, data de
  aplicação.
- **MigrationStatus**: Resultado de consulta de status contendo
  listas de migrations aplicadas e pendentes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Um desenvolvedor consegue criar e executar uma
  migration completa (UP + DOWN) em menos de 5 minutos seguindo
  a documentação e convenções.
- **SC-002**: Toda migration é executada atomicamente — falha
  parcial NUNCA resulta em schema inconsistente.
- **SC-003**: O sistema detecta e reporta 100% dos erros de
  configuração (scripts ausentes, versões duplicadas) antes de
  executar qualquer alteração no banco.
- **SC-004**: Múltiplas instâncias da aplicação tentando migrar
  simultaneamente resultam em apenas uma execução bem-sucedida,
  sem erros para as demais (esperam ou ignoram).
- **SC-005**: O status de migrations é consultável em tempo
  constante, sem degradação com o aumento do número de migrations
  registradas (até 10.000 migrations).
- **SC-006**: O BuildingBlock se integra naturalmente no grafo de
  dependências do Bedrock sem introduzir dependências circulares.
