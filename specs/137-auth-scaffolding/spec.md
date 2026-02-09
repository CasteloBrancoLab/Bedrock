# Feature Specification: Auth - Estrutura dos Projetos (Scaffolding)

**Feature Branch**: `137-auth-scaffolding`
**Created**: 2026-02-08
**Status**: Draft
**Input**: Issue #137 — Sub-issue de #136 (Auth Service Design Completo)
**Type**: Chore (scaffolding de infraestrutura)

## User Scenarios & Testing *(mandatory)*

<!--
  NOTA: Esta é uma issue de scaffolding (type:chore), não uma feature com jornadas de usuário.
  Os "user stories" aqui representam etapas de setup de infraestrutura que devem ser
  validadas independentemente pelo desenvolvedor/code agent.
-->

### User Story 1 - Criar Projetos src do Auth (Priority: P1)

Como desenvolvedor, preciso que a estrutura de projetos src do Auth exista em `samples/ShopDemo/Auth/` seguindo as convenções do ShopDemo (namespace `ShopDemo.Auth.*`), com as 5 camadas definidas na issue #136 e referências corretas entre camadas e aos BuildingBlocks.

**Why this priority**: Sem os projetos src, nenhuma das 20 sub-issues subsequentes (#138-#157) pode começar. É o pré-requisito absoluto de toda a implementação do Auth Service.

**Independent Test**: Pode ser testado executando `dotnet build` na solution — todos os projetos devem compilar sem erros.

**Acceptance Scenarios**:

1. **Given** a solution Bedrock sem projetos Auth, **When** os projetos são criados em `samples/ShopDemo/Auth/`, **Then** existem 5 projetos src (`Domain.Entities`, `Application`, `Infra.Data`, `Infra.Persistence`, `Api`) com namespaces `ShopDemo.Auth.*`
2. **Given** os projetos src criados, **When** `dotnet build` é executado, **Then** a compilação passa sem erros
3. **Given** os projetos src criados, **When** as referências são inspecionadas, **Then** cada camada referencia apenas as camadas permitidas (Domain não referencia nada acima; Application referencia Domain; Infra.Data referencia Domain; Infra.Persistence referencia Infra.Data e Domain; Api referencia Application)
4. **Given** os projetos src criados, **When** as referências aos BuildingBlocks são inspecionadas, **Then** cada projeto referencia o BuildingBlock correspondente à sua camada

---

### User Story 2 - Criar Projetos de Testes Unitários (Priority: P2)

Como desenvolvedor, preciso que exista um projeto de testes unitários para cada projeto src do Auth, seguindo a relação 1:1 obrigatória do Bedrock e as convenções de nomenclatura do CLAUDE.md.

**Why this priority**: A relação 1:1 é obrigatória para compatibilidade com Stryker.NET e a pipeline de testes. Sem os projetos de teste, o código não pode ser validado pela pipeline.

**Independent Test**: Pode ser testado executando `dotnet test` — os projetos de teste devem compilar e executar (mesmo sem testes implementados, a execução não deve falhar).

**Acceptance Scenarios**:

1. **Given** os projetos src do Auth existem, **When** os projetos de testes são criados, **Then** existem 5 projetos de testes unitários com nomenclatura `ShopDemo.UnitTests.Auth.*` em relação 1:1
2. **Given** os projetos de testes criados, **When** `dotnet test` é executado, **Then** a execução passa sem erros
3. **Given** os projetos de testes criados, **When** as referências são inspecionadas, **Then** cada projeto de teste referencia seu projeto src correspondente e o BuildingBlock `Testing`

---

### User Story 3 - Criar Projetos de Testes de Mutação (Priority: P3)

Como desenvolvedor, preciso que exista um projeto de testes de mutação para cada projeto de testes unitários, com configuração `stryker-config.json` correta, seguindo os thresholds do Bedrock (100%).

**Why this priority**: Testes de mutação garantem a qualidade dos testes unitários. Sem a configuração correta do Stryker, a pipeline não consegue validar mutantes.

**Independent Test**: Pode ser testado verificando que cada `stryker-config.json` é válido e referencia corretamente o projeto src e o projeto de testes correspondente.

**Acceptance Scenarios**:

1. **Given** os projetos de testes unitários existem, **When** os projetos de mutação são criados, **Then** existem 5 diretórios de mutação com `stryker-config.json`
2. **Given** os `stryker-config.json` criados, **When** o conteúdo é inspecionado, **Then** cada arquivo referencia corretamente o `.csproj` do src e do teste, com thresholds `high: 100, low: 100, break: 100`

---

### User Story 4 - Integrar na Solution e Pipeline (Priority: P4)

Como desenvolvedor, preciso que todos os projetos sejam adicionados à solution Bedrock e que `./scripts/pipeline.sh` passe com a estrutura vazia.

**Why this priority**: Sem integração na solution, os projetos não participam do build nem da pipeline CI/CD.

**Independent Test**: Pode ser testado executando `./scripts/pipeline.sh` — a pipeline completa deve passar.

**Acceptance Scenarios**:

1. **Given** todos os projetos Auth criados, **When** são adicionados à solution, **Then** `dotnet sln list` mostra todos os projetos Auth
2. **Given** os projetos integrados na solution, **When** `./scripts/pipeline.sh` é executado, **Then** a pipeline passa completamente (build, testes, cobertura, mutação)
3. **Given** o projeto `ShopDemo.Auth.Domain.Entities` criado, **When** o path é adicionado ao `DomainEntitiesArchFixture.GetProjectPaths()`, **Then** os testes de arquitetura (DE001-DE058) incluem o Auth na validação

---

### Edge Cases

- O que acontece se um projeto Auth já existir na solution? Verificar antes de adicionar e pular se já existir.
- Como a estrutura vazia afeta métricas de cobertura? Projetos vazios (sem código) não devem quebrar a pipeline — podem ser ignorados pelo Coverlet/Stryker se não tiverem código testável.
- As referências relativas entre camadas funcionam a partir de `samples/ShopDemo/Auth/`? Sim, as referências aos BuildingBlocks devem usar caminhos relativos corretos (ex: `../../../../src/BuildingBlocks/...`).

## Clarifications

### Session 2026-02-08

- Q: O path de Domain.Entities do Auth deve ser registrado no teste de arquitetura? → A: Sim. O path `samples/ShopDemo/Auth/Domain.Entities/ShopDemo.Auth.Domain.Entities.csproj` DEVE ser adicionado ao `DomainEntitiesArchFixture.GetProjectPaths()` em `tests/ArchitectureTests/Templates/Domain.Entities/Fixtures/DomainEntitiesArchFixture.cs` para que as 58 regras arquiteturais (DE001-DE058) validem as entidades do Auth.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Sistema DEVE criar 6 projetos src em `samples/ShopDemo/Auth/` com namespaces `ShopDemo.Auth.*`:
  - `ShopDemo.Auth.Domain.Entities` — Entities, Value Objects, regras de negócio puras
  - `ShopDemo.Auth.Domain` — Repository interfaces, Domain Services, abstrações de integração
  - `ShopDemo.Auth.Application` — Use Cases, DTOs, Application Services
  - `ShopDemo.Auth.Infra.Data` — Repository implementations, Data Model parsing
  - `ShopDemo.Auth.Infra.Persistence` — PostgreSQL Data Models, EF Core DbContext, Migrations
  - `ShopDemo.Auth.Api` — Controllers, Middleware, Configuration
- **FR-002**: Sistema DEVE criar referências entre camadas seguindo a arquitetura limpa:
  - `Domain.Entities` → `ShopDemo.Core.Entities` + `Bedrock.BuildingBlocks.Domain.Entities`
  - `Domain` → `Domain.Entities` + `Bedrock.BuildingBlocks.Domain`
  - `Application` → `Domain` + `Domain.Entities`
  - `Infra.Data` → `Domain` + `Domain.Entities` + `Bedrock.BuildingBlocks.Data`
  - `Infra.Persistence` → `Infra.Data` + `Domain.Entities` + `Bedrock.BuildingBlocks.Persistence.PostgreSql`
  - `Api` → `Application` + `Bedrock.BuildingBlocks.Observability`
- **FR-003**: Sistema DEVE criar 6 projetos de testes unitários em relação 1:1 com nomenclatura `ShopDemo.UnitTests.Auth.*`
- **FR-004**: Sistema DEVE criar 6 configurações de testes de mutação com `stryker-config.json` e thresholds de 100%
- **FR-005**: Todos os projetos DEVEM usar .NET 10.0 (`net10.0`), `ImplicitUsings: enable`, `Nullable: enable`
- **FR-006**: Todos os projetos DEVEM ser adicionados à solution `Bedrock.sln`
- **FR-007**: `./scripts/pipeline.sh` DEVE passar com a estrutura vazia
- **FR-008**: O path do projeto `ShopDemo.Auth.Domain.Entities` DEVE ser registrado no fixture de testes de arquitetura (`DomainEntitiesArchFixture.GetProjectPaths()`) para que as 58 regras arquiteturais (DE001-DE058) validem as entidades do Auth

### Key Entities

- Nenhuma entidade de domínio é criada nesta issue. O scaffolding cria apenas a estrutura de projetos vazia.
- As entidades do Auth (User, Credentials, Role, Claim, Token, etc.) serão implementadas nas sub-issues subsequentes (#138+).

### Estrutura Esperada

```
samples/ShopDemo/Auth/
  Domain.Entities/
    ShopDemo.Auth.Domain.Entities.csproj
    GlobalUsings.cs
  Domain/
    ShopDemo.Auth.Domain.csproj
    GlobalUsings.cs
  Application/
    ShopDemo.Auth.Application.csproj
    GlobalUsings.cs
  Infra.Data/
    ShopDemo.Auth.Infra.Data.csproj
    GlobalUsings.cs
  Infra.Persistence/
    ShopDemo.Auth.Infra.Persistence.csproj
    GlobalUsings.cs
  Api/
    ShopDemo.Auth.Api.csproj
    GlobalUsings.cs

tests/
  UnitTests/ShopDemo/Auth/
    Domain.Entities/
      ShopDemo.UnitTests.Auth.Domain.Entities.csproj
    Domain/
      ShopDemo.UnitTests.Auth.Domain.csproj
    Application/
      ShopDemo.UnitTests.Auth.Application.csproj
    Infra.Data/
      ShopDemo.UnitTests.Auth.Infra.Data.csproj
    Infra.Persistence/
      ShopDemo.UnitTests.Auth.Infra.Persistence.csproj
    Api/
      ShopDemo.UnitTests.Auth.Api.csproj
  MutationTests/ShopDemo/Auth/
    Domain.Entities/
      stryker-config.json
    Domain/
      stryker-config.json
    Application/
      stryker-config.json
    Infra.Data/
      stryker-config.json
    Infra.Persistence/
      stryker-config.json
    Api/
      stryker-config.json
```

### Convenções de Nomenclatura

| Convenção | Padrão |
|-----------|--------|
| Namespace src | `ShopDemo.Auth.{Camada}` |
| Namespace testes | `ShopDemo.UnitTests.Auth.{Camada}` |
| Pasta src | `samples/ShopDemo/Auth/{Camada}/` |
| Pasta testes | `tests/UnitTests/ShopDemo/Auth/{Camada}/` |
| Pasta mutação | `tests/MutationTests/ShopDemo/Auth/{Camada}/` |
| Target framework | `net10.0` |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: `dotnet build Bedrock.sln` compila sem erros com todos os projetos Auth incluídos
- **SC-002**: `dotnet test` executa sem erros (mesmo com zero testes implementados)
- **SC-003**: `./scripts/pipeline.sh` passa completamente com a estrutura vazia
- **SC-004**: Todas as referências entre camadas estão corretas e seguem a arquitetura limpa (nenhuma referência circular ou violação de dependência)
- **SC-005**: Todos os `stryker-config.json` referenciam corretamente os projetos src e teste correspondentes com thresholds de 100%
- **SC-006**: A estrutura está pronta para receber a implementação da issue #138 (User + Credentials) sem necessidade de alterações estruturais
- **SC-007**: Os testes de arquitetura (DE001-DE058) incluem o projeto `ShopDemo.Auth.Domain.Entities` na validação e passam com a estrutura vazia
