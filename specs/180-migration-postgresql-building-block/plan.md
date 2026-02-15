# Implementation Plan: PostgreSQL Migrations BuildingBlock

**Branch**: `feature/180-migration-postgresql-building-block` | **Date**: 2026-02-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/180-migration-postgresql-building-block/spec.md`

## Summary

Create a new BuildingBlock `Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations` that provides schema evolution capabilities for PostgreSQL databases via FluentMigrator. The BuildingBlock exposes an abstract `MigrationManagerBase` class as the public API, a custom `[SqlScript]` attribute for linking migration classes to embedded SQL scripts, and a `SqlScriptMigrationBase` class that automates script execution. Migrations run exclusively via pipeline (CI/CD), not as part of deployed applications.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: FluentMigrator 8.x, FluentMigrator.Runner, FluentMigrator.Runner.Postgres, Npgsql (transitive via runner)
**Storage**: PostgreSQL (target of migrations; FluentMigrator manages its own `VersionInfo` table)
**Testing**: xUnit + Shouldly + Moq + Bogus + Testcontainers.PostgreSql + Stryker.NET
**Target Platform**: Library (consumed by BC migration pipeline runners)
**Project Type**: BuildingBlock library (part of `src/BuildingBlocks/`)
**Performance Goals**: N/A — pipeline-only execution, not hot path
**Constraints**: Embedded resources for SQL scripts, no filesystem dependency at runtime
**Scale/Scope**: Single new BuildingBlock project + unit tests + integration tests + architecture tests + mutation tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Qualidade Inegociável | PASS | 100% coverage + mutation targets apply. Unit, integration, and architecture tests planned. |
| II. Simplicidade Deliberada | PASS | Single project, minimal abstractions (3 public types + 2 models). No over-engineering. |
| III. Observabilidade Nativa | PASS | All public methods receive `ExecutionContext`. Logging uses `*ForDistributedTracing` extension methods (CS003 compliant). |
| IV. Modularidade por Contrato | PASS | Own .csproj, 1:1 test project ratio. No circular dependencies. Interfaces in `Interfaces/` subfolders. |
| V. Automação como Garantia | PASS | Pipeline gates apply. Architecture tests verify compliance. |
| BB-I. Performance como Requisito | PASS | `MigrationInfo` is `readonly record struct`. Pipeline-only — no hot path concerns. |
| BB-II. Imutabilidade por Padrão | PASS | `MigrationInfo` immutable by design. `MigrationStatus` uses `IReadOnlyList<T>`. |
| BB-III. Estado Inválido Nunca Existe | N/A | No domain entities. Infrastructure types only. |
| BB-IV. Explícito sobre Implícito | PASS | `ExecutionContext` first param, `CancellationToken` last param on all async methods. |
| BB-V. Aggregate Root como Fronteira | N/A | No domain entities or repositories. |
| BB-VI. Separação Domain.Entities / Domain | N/A | No domain layer involvement. |
| BB-VII. Arquitetura Verificada por Código | PASS | Architecture test project with CS and IN rule categories. |
| BB-VIII. Camadas de Infraestrutura por Template Method | PASS | `MigrationManagerBase` follows Template Method pattern — abstract config members, concrete orchestration in base. |
| BB-IX. Disciplina de Testes Unitários | PASS | Tests follow TestBase, AAA with LogArrange/LogAct/LogAssert, Shouldly assertions. |
| BB-X. Disciplina de Testes de Integração | PASS | IntegrationTestBase with Testcontainers fixture, UseEnvironment, factory-based fixture. |
| BB-XI. Templates como Lei de Implementação | N/A | No domain entity templates apply. This is a new infrastructure pattern. |

## Project Structure

### Documentation (this feature)

```text
specs/180-migration-postgresql-building-block/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── domain-contracts.md  # Public API contracts
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── spec.md              # Feature specification
```

### Source Code (repository root)

```text
src/BuildingBlocks/Persistence.PostgreSql.Migrations/
├── Attributes/
│   └── SqlScriptAttribute.cs                    # [SqlScript] custom attribute
├── Interfaces/
│   └── IMigrationManagerBase.cs                 # Interface for testability (optional)
├── Models/
│   ├── MigrationInfo.cs                         # readonly record struct
│   └── MigrationStatus.cs                       # sealed class
├── SqlScriptMigrationBase.cs                    # Abstract migration base class
├── MigrationManagerBase.cs                      # Abstract manager base class
├── GlobalUsings.cs                              # Global using directives
└── Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj

tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/
├── Attributes/
│   └── SqlScriptAttributeTests.cs
├── Models/
│   ├── MigrationInfoTests.cs
│   └── MigrationStatusTests.cs
├── SqlScriptMigrationBaseTests.cs
├── MigrationManagerBaseTests.cs
└── Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj

tests/IntegrationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/
├── Fixtures/
│   └── MigrationFixture.cs                      # Testcontainers + migration setup
├── MigrationManagerIntegrationTests.cs
└── Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj

tests/ArchitectureTests/BuildingBlocks/Persistence.PostgreSql.Migrations/
├── Fixtures/
│   └── ArchFixture.cs
├── CodeStyleRuleTests.cs                        # CS001-CS003
├── InfrastructureRuleTests.cs                   # IN001-IN016
└── Bedrock.ArchitectureTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj

tests/MutationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/
└── stryker-config.json
```

**Structure Decision**: Single new BuildingBlock under `src/BuildingBlocks/` following the established 1:1 project-to-test pattern. The project sits alongside `Persistence.PostgreSql` in the dependency graph but depends only on `Core` and `Observability` (not on `Persistence.PostgreSql` itself, as migrations don't use DataModels or Mappers).

## Complexity Tracking

No constitution violations to justify.
