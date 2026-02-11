# Quickstart: Auth User Data Access Layer (PostgreSQL)

**Feature Branch**: `001-auth-data-postgres`
**Date**: 2026-02-11

## Prerequisites

- .NET 10.0 SDK
- Bedrock solution builds successfully: `dotnet build Bedrock.sln`
- Auth domain model implemented (issue #138 domain scope)
- Auth project scaffolding in place (issue #137)

## Implementation Order

Follow this dependency-ordered sequence. Each step should compile before moving to the next.

### Step 1: Infrastructure plumbing (Connection + UnitOfWork)

These have no dependencies on domain types.

1. `Infra.Persistence/Connections/Interfaces/IAuthPostgreSqlConnection.cs`
2. `Infra.Persistence/Connections/AuthPostgreSqlConnection.cs`
3. `Infra.Persistence/UnitOfWork/Interfaces/IAuthPostgreSqlUnitOfWork.cs`
4. `Infra.Persistence/UnitOfWork/AuthPostgreSqlUnitOfWork.cs`

**Verify**: `dotnet build ShopDemo.Auth.Infra.Persistence.csproj`

### Step 2: DataModel + Mapper

5. `Infra.Persistence/DataModels/UserDataModel.cs`
6. `Infra.Persistence/Mappers/UserDataModelMapper.cs`

**Verify**: `dotnet build ShopDemo.Auth.Infra.Persistence.csproj`

### Step 3: Factories + Adapter

7. `Infra.Persistence/Factories/UserDataModelFactory.cs`
8. `Infra.Persistence/Factories/UserFactory.cs`
9. `Infra.Persistence/Adapters/UserDataModelAdapter.cs`

**Verify**: `dotnet build ShopDemo.Auth.Infra.Persistence.csproj`

### Step 4: DataModel Repository

10. `Infra.Persistence/DataModelsRepositories/Interfaces/IUserDataModelRepository.cs`
11. `Infra.Persistence/DataModelsRepositories/UserDataModelRepository.cs`

**Verify**: `dotnet build ShopDemo.Auth.Infra.Persistence.csproj`

### Step 5: PostgreSQL Repository

12. `Infra.Persistence/Repositories/Interfaces/IUserPostgreSqlRepository.cs`
13. `Infra.Persistence/Repositories/UserPostgreSqlRepository.cs`

**Verify**: `dotnet build ShopDemo.Auth.Infra.Persistence.csproj`

### Step 6: Data Repository (abstraction layer)

14. `Infra.Data/Repositories/UserRepository.cs`

**Verify**: `dotnet build ShopDemo.Auth.Infra.Data.csproj`

### Step 7: Unit Tests

15. Create test project `ShopDemo.Auth.UnitTests.Infra.Persistence.csproj`
16. Create test project `ShopDemo.Auth.UnitTests.Infra.Data.csproj`
17. Write tests for each class (see plan.md for file list)

**Verify**: `dotnet test` for each test project

### Step 8: Mutation Tests

18. Create `stryker-config.json` for each test project
19. Run mutation tests

**Verify**: `dotnet stryker` for each configuration

### Step 9: Pipeline

20. Run `./scripts/pipeline.sh` — must pass 100% coverage + 100% mutation

## Key Patterns to Follow

- **Template reference**: `src/templates/Infra.Data.PostgreSql/` is the normative source
- **Namespace**: `ShopDemo.Auth.Infra.Persistence.*` (not `Infra.Data.PostgreSql`)
- **GlobalUsings**: Already exists — `ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext`
- **Sealed classes**: All implementation classes must be `sealed`
- **Interface subfolder**: All interfaces in `Interfaces/` subdirectory
- **Static lambdas**: All lambdas passed to project methods must use `static` modifier (CS002)
