# Implementation Plan: Auth - Estrutura dos Projetos (Scaffolding)

**Branch**: `137-auth-scaffolding` | **Date**: 2026-02-08 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/137-auth-scaffolding/spec.md`

## Summary

Criar a estrutura de projetos (scaffolding) para o Auth Service dentro de `samples/ShopDemo/Auth/`, incluindo 5 projetos src, 5 projetos de testes unitários, 5 configurações de testes de mutação (Stryker), integração na solution Bedrock e registro no fixture de testes de arquitetura. O resultado é uma estrutura vazia que compila e passa na pipeline, pronta para receber as entidades da issue #138+.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: Bedrock BuildingBlocks (Core, Domain.Entities, Data, Persistence.PostgreSql, Observability, Testing)
**Storage**: N/A (scaffolding apenas — sem entidades nem persistência nesta issue)
**Testing**: xUnit, Shouldly, Stryker.NET, Coverlet
**Target Platform**: .NET 10.0 class libraries (samples)
**Project Type**: Modular — segue convenção `samples/ShopDemo/{Module}/`
**Performance Goals**: N/A (scaffolding)
**Constraints**: Projetos vazios DEVEM compilar e a pipeline DEVE passar
**Scale/Scope**: 5 projetos src + 5 projetos teste + 5 configs Stryker + 1 alteração no fixture de arquitetura = 16 artefatos

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Status | Notas |
|-----------|--------|-------|
| I. Qualidade Inegociável | PASS | Pipeline DEVE passar com estrutura vazia. Thresholds Stryker 100%. |
| II. Simplicidade Deliberada | PASS | 5 camadas justificadas pela issue #136 (arquitetura Auth Service completa). |
| III. Observabilidade Nativa | N/A | Scaffolding — sem código de runtime. |
| IV. Modularidade por Contrato | PASS | Cada camada é um .csproj independente. Relação 1:1 src/testes. |
| V. Automação como Garantia | PASS | Pipeline local e CI validam a estrutura. |
| BB-I. Performance | N/A | Scaffolding — sem código. |
| BB-II. Imutabilidade | N/A | Scaffolding — sem entidades. |
| BB-III. Estado Inválido | N/A | Scaffolding — sem entidades. |
| BB-IV. Explícito sobre Implícito | N/A | Scaffolding — sem código. |
| BB-V. Aggregate Root | N/A | Scaffolding — sem entidades. |
| BB-VI. Separação Domain.Entities/Domain | PASS | `Domain.Entities` criado separadamente, seguindo convenção. |
| BB-VII. Arquitetura Verificada | PASS | Auth Domain.Entities registrado em `DomainEntitiesArchFixture` (FR-008). |
| BB-VIII. Template Method | N/A | Scaffolding — sem implementações de infraestrutura. |
| BB-IX. Disciplina Testes Unitários | PASS | Projetos de teste criados com relação 1:1, ref a Testing BuildingBlock. |
| BB-X. Disciplina Testes Integração | N/A | Scaffolding — sem testes de integração nesta issue. |
| BB-XI. Templates como Lei | N/A | Scaffolding — implementações seguirão templates em #138+. |

**Resultado**: PASS — nenhuma violação. Pode prosseguir.

## Project Structure

### Documentation (this feature)

```text
specs/137-auth-scaffolding/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output (minimal — scaffolding)
├── data-model.md        # Phase 1 output (N/A — sem entidades)
├── quickstart.md        # Phase 1 output
└── checklists/
    └── requirements.md  # Quality checklist
```

### Source Code (repository root)

```text
samples/ShopDemo/Auth/
├── Domain.Entities/
│   ├── ShopDemo.Auth.Domain.Entities.csproj
│   └── GlobalUsings.cs
├── Application/
│   ├── ShopDemo.Auth.Application.csproj
│   └── GlobalUsings.cs
├── Infra.Data/
│   ├── ShopDemo.Auth.Infra.Data.csproj
│   └── GlobalUsings.cs
├── Infra.Persistence/
│   ├── ShopDemo.Auth.Infra.Persistence.csproj
│   └── GlobalUsings.cs
└── Api/
    ├── ShopDemo.Auth.Api.csproj
    └── GlobalUsings.cs

tests/UnitTests/ShopDemo/Auth/
├── Domain.Entities/
│   └── ShopDemo.UnitTests.Auth.Domain.Entities.csproj
├── Application/
│   └── ShopDemo.UnitTests.Auth.Application.csproj
├── Infra.Data/
│   └── ShopDemo.UnitTests.Auth.Infra.Data.csproj
├── Infra.Persistence/
│   └── ShopDemo.UnitTests.Auth.Infra.Persistence.csproj
└── Api/
    └── ShopDemo.UnitTests.Auth.Api.csproj

tests/MutationTests/ShopDemo/Auth/
├── Domain.Entities/
│   └── stryker-config.json
├── Application/
│   └── stryker-config.json
├── Infra.Data/
│   └── stryker-config.json
├── Infra.Persistence/
│   └── stryker-config.json
└── Api/
    └── stryker-config.json
```

**Structure Decision**: Segue a convenção existente do ShopDemo (`samples/ShopDemo/{Module}/{Layer}/`), com nomenclatura `ShopDemo.Auth.*` análoga a `ShopDemo.Customers.*`, `ShopDemo.Orders.*`, etc.

### Referências entre Projetos

```text
ShopDemo.Auth.Domain.Entities
  → ShopDemo.Core.Entities
  → Bedrock.BuildingBlocks.Domain.Entities

ShopDemo.Auth.Application
  → ShopDemo.Auth.Domain.Entities

ShopDemo.Auth.Infra.Data
  → ShopDemo.Auth.Domain.Entities
  → Bedrock.BuildingBlocks.Data

ShopDemo.Auth.Infra.Persistence
  → ShopDemo.Auth.Infra.Data
  → ShopDemo.Auth.Domain.Entities
  → Bedrock.BuildingBlocks.Persistence.PostgreSql

ShopDemo.Auth.Api
  → ShopDemo.Auth.Application
  → Bedrock.BuildingBlocks.Observability
```

### Referências dos Projetos de Teste

```text
ShopDemo.UnitTests.Auth.{Layer}
  → ShopDemo.Auth.{Layer}          (projeto src correspondente)
  → Bedrock.BuildingBlocks.Testing (infraestrutura de teste)
```

### Caminhos Relativos (a partir de `samples/ShopDemo/Auth/Domain.Entities/`)

| Destino | Caminho Relativo |
|---------|-----------------|
| `ShopDemo.Core.Entities` | `..\..\Core\Entities\ShopDemo.Core.Entities.csproj` |
| `Bedrock.BuildingBlocks.Domain.Entities` | `..\..\..\..\src\BuildingBlocks\Domain.Entities\Bedrock.BuildingBlocks.Domain.Entities.csproj` |
| `Bedrock.BuildingBlocks.Data` | `..\..\..\..\src\BuildingBlocks\Data\Bedrock.BuildingBlocks.Data.csproj` |
| `Bedrock.BuildingBlocks.Persistence.PostgreSql` | `..\..\..\..\src\BuildingBlocks\Persistence.PostgreSql\Bedrock.BuildingBlocks.Persistence.PostgreSql.csproj` |
| `Bedrock.BuildingBlocks.Observability` | `..\..\..\..\src\BuildingBlocks\Observability\Bedrock.BuildingBlocks.Observability.csproj` |
| `Bedrock.BuildingBlocks.Testing` | `..\..\..\..\src\BuildingBlocks\Testing\Bedrock.BuildingBlocks.Testing.csproj` |

### Registro no Teste de Arquitetura

Arquivo: `tests/ArchitectureTests/Templates/Domain.Entities/Fixtures/DomainEntitiesArchFixture.cs`

Adicionar ao array retornado por `GetProjectPaths()`:

```csharp
Path.Combine(rootDir, "samples", "ShopDemo", "Auth", "Domain.Entities", "ShopDemo.Auth.Domain.Entities.csproj")
```

### Stryker Config Template

Cada `stryker-config.json` segue o padrão:

```json
{
  "$schema": "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/stryker-config.schema.json",
  "stryker-config": {
    "project": "ShopDemo.Auth.{Layer}.csproj",
    "test-projects": [
      "../../../../UnitTests/ShopDemo/Auth/{Layer}/ShopDemo.UnitTests.Auth.{Layer}.csproj"
    ],
    "reporters": ["html", "progress"],
    "thresholds": {
      "high": 100,
      "low": 100,
      "break": 100
    }
  }
}
```

## Complexity Tracking

> Nenhuma violação da constituição identificada.

| Decisão | Justificativa |
|---------|--------------|
| 5 projetos src | Definido pela arquitetura do Auth Service (issue #136). Cada camada tem responsabilidade clara. |
| Namespace `ShopDemo.Auth.*` em vez de `Bedrock.Auth.*` | Segue convenção existente do ShopDemo (análogo a `ShopDemo.Customers.*`). |
