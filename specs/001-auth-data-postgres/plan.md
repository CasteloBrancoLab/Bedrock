# Implementation Plan: Auth User Data Access Layer (PostgreSQL)

**Branch**: `001-auth-data-postgres` | **Date**: 2026-02-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-auth-data-postgres/spec.md`
**Parent Issue**: [#138 — Auth: User + Credentials](https://github.com/CasteloBrancoLab/Bedrock/issues/138)

## Summary

Implement the PostgreSQL data access layer for the User aggregate root (auth domain), following the established template project patterns. This covers: UserDataModel, Mapper, Factories (bidirectional), Adapter, DataModelRepository, PostgreSqlRepository, Connection, and UnitOfWork. The domain layer (User entity, PasswordHash value object, IUserRepository interface) is already implemented in `ShopDemo.Auth.Domain.Entities` and `ShopDemo.Auth.Domain`. The scaffolded but empty projects `ShopDemo.Auth.Infra.Data` and `ShopDemo.Auth.Infra.Persistence` will receive the implementation.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: Bedrock.BuildingBlocks.Core, Bedrock.BuildingBlocks.Domain.Entities, Bedrock.BuildingBlocks.Data, Bedrock.BuildingBlocks.Persistence.PostgreSql, Bedrock.BuildingBlocks.Observability, Npgsql
**Storage**: PostgreSQL (via Npgsql, no EF Core)
**Testing**: xUnit + Shouldly + Moq + Bogus + Coverlet + Stryker.NET
**Target Platform**: .NET 10.0 (cross-platform server)
**Project Type**: Framework library (BuildingBlocks pattern)
**Performance Goals**: Zero-allocation on hot paths (per BB-I); raw Npgsql with binary import
**Constraints**: 100% code coverage, 100% mutation score, no EF Core (direct Npgsql)
**Scale/Scope**: 2 projects (Infra.Data, Infra.Persistence) + 2 test projects

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Qualidade Inegociável | PASS | 100% coverage + 100% mutation planned. xUnit + Shouldly + Moq + Bogus. |
| II. Simplicidade Deliberada | PASS | Following existing template pattern exactly — no new abstractions. |
| III. Observabilidade Nativa | PASS | RepositoryBase provides structured logging + distributed tracing. |
| IV. Modularidade por Contrato | PASS | Interfaces in `Interfaces/` subdirectories. 1:1 src/test projects. |
| V. Automação como Garantia | PASS | Pipeline local before PR. GitHub Actions CI. |
| BB-I. Performance como Requisito | PASS | DataModelMapperBase provides zero-allocation SQL. Binary importer for bulk ops. |
| BB-II. Imutabilidade por Padrão | PASS | Domain entities are immutable (Clone-Modify-Return). DataModels are mutable (persistence shape). |
| BB-III. Estado Inválido Nunca Existe | PASS | Factory reconstitution via `CreateFromExistingInfo` (no revalidation). |
| BB-IV. Explícito sobre Implícito | PASS | ExecutionContext as first param, CancellationToken as last. |
| BB-V. Aggregate Root como Fronteira | PASS | Repository only for User aggregate root. Handler pattern for enumeration. |
| BB-VI. Separação Domain.Entities / Domain | PASS | Domain.Entities has entities, Domain has IUserRepository. Infra implements. |
| BB-VII. Arquitetura Verificada por Código | PASS | Roslyn rules DE* and CS* apply to new code. |
| BB-VIII. Camadas de Infraestrutura por Template Method | PASS | RepositoryBase wraps error handling. DataModelRepositoryBase provides CRUD. |
| BB-IX. Disciplina de Testes Unitários | PASS | TestBase inheritance, AAA with LogArrange/LogAct/LogAssert, Shouldly, regions. |
| BB-X. Disciplina de Testes de Integração | N/A | Integration tests are out of scope for this issue (unit tests only). |
| BB-XI. Templates como Lei de Implementação | PASS | Implementation follows `src/templates/Infra.Data.PostgreSql/` exactly. |

**Gate Result: PASS** — No violations. Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/001-auth-data-postgres/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (repository contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
samples/ShopDemo/Auth/
├── Infra.Data/                                    # Data access abstraction
│   ├── Repositories/
│   │   └── UserRepository.cs                      # RepositoryBase<User> → IUserRepository
│   ├── GlobalUsings.cs                            # (exists)
│   └── ShopDemo.Auth.Infra.Data.csproj            # (exists)
│
├── Infra.Persistence/                             # PostgreSQL implementation
│   ├── DataModels/
│   │   └── UserDataModel.cs                       # DataModelBase + User columns
│   ├── Mappers/
│   │   └── UserDataModelMapper.cs                 # Table/column mapping + binary importer
│   ├── Factories/
│   │   ├── UserFactory.cs                         # DataModel → Domain Entity
│   │   └── UserDataModelFactory.cs                # Domain Entity → DataModel
│   ├── Adapters/
│   │   └── UserDataModelAdapter.cs                # Update adapter
│   ├── DataModelsRepositories/
│   │   ├── Interfaces/
│   │   │   └── IUserDataModelRepository.cs        # IPostgreSqlDataModelRepository<UserDataModel> + custom queries
│   │   └── UserDataModelRepository.cs             # DataModelRepositoryBase<UserDataModel> + custom queries
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   └── IUserPostgreSqlRepository.cs       # IPostgreSqlRepository<User> + custom queries
│   │   └── UserPostgreSqlRepository.cs            # Wraps DataModelRepository, uses Factories
│   ├── Connections/
│   │   ├── Interfaces/
│   │   │   └── IAuthPostgreSqlConnection.cs       # IPostgreSqlConnection
│   │   └── AuthPostgreSqlConnection.cs            # PostgreSqlConnectionBase
│   ├── UnitOfWork/
│   │   ├── Interfaces/
│   │   │   └── IAuthPostgreSqlUnitOfWork.cs       # IPostgreSqlUnitOfWork
│   │   └── AuthPostgreSqlUnitOfWork.cs            # PostgreSqlUnitOfWorkBase
│   ├── GlobalUsings.cs                            # (exists)
│   └── ShopDemo.Auth.Infra.Persistence.csproj     # (exists, needs Observability ref)
│
tests/UnitTests/ShopDemo/Auth/
├── Infra.Data/
│   ├── Repositories/
│   │   └── UserRepositoryTests.cs
│   └── ShopDemo.Auth.UnitTests.Infra.Data.csproj
│
├── Infra.Persistence/
│   ├── DataModels/
│   │   └── UserDataModelTests.cs
│   ├── Mappers/
│   │   └── UserDataModelMapperTests.cs
│   ├── Factories/
│   │   ├── UserFactoryTests.cs
│   │   └── UserDataModelFactoryTests.cs
│   ├── Adapters/
│   │   └── UserDataModelAdapterTests.cs
│   ├── DataModelsRepositories/
│   │   └── UserDataModelRepositoryTests.cs
│   ├── Repositories/
│   │   └── UserPostgreSqlRepositoryTests.cs
│   ├── Connections/
│   │   └── AuthPostgreSqlConnectionTests.cs
│   ├── UnitOfWork/
│   │   └── AuthPostgreSqlUnitOfWorkTests.cs
│   └── ShopDemo.Auth.UnitTests.Infra.Persistence.csproj
│
tests/MutationTests/ShopDemo/Auth/
├── Infra.Data/
│   └── stryker-config.json
└── Infra.Persistence/
    └── stryker-config.json
```

**Structure Decision**: Follows existing ShopDemo.Auth scaffolding (issue #137). The two infrastructure projects (`Infra.Data` and `Infra.Persistence`) already exist with .csproj and GlobalUsings. This plan adds implementation files following the `src/templates/Infra.Data.PostgreSql/` pattern exactly, adapted to ShopDemo naming conventions (`Infra.Persistence` instead of `Infra.Data.PostgreSql`).

## Complexity Tracking

No violations to justify. The implementation follows the established template pattern 1:1.
