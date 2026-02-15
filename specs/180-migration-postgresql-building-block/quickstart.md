# Quickstart: PostgreSQL Migrations BuildingBlock

**Branch**: `feature/180-migration-postgresql-building-block` | **Date**: 2026-02-14

## Prerequisites

- .NET 10.0 SDK
- Docker (for integration tests with Testcontainers)
- PostgreSQL instance (for manual testing)

## Step 1: Verify the BuildingBlock project compiles

```bash
dotnet build src/BuildingBlocks/Persistence.PostgreSql.Migrations/Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj
```

**Expected**: Build succeeds with no errors.

## Step 2: Verify unit tests pass

```bash
dotnet test tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj
```

**Expected**: All unit tests pass. Tests cover:
- `SqlScriptAttribute` construction and validation
- `MigrationInfo` value semantics
- `MigrationStatus` construction and properties
- `SqlScriptMigrationBase` script loading and validation
- `MigrationManagerBase` runner configuration and orchestration (with mocked runner)

## Step 3: Verify integration tests pass

```bash
dotnet test tests/IntegrationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj
```

**Expected**: All integration tests pass (requires Docker). Tests use Testcontainers to spin up a PostgreSQL instance and verify:
- MigrateUpAsync applies pending migrations
- MigrateDownAsync rolls back to target version
- GetStatusAsync reports correct applied/pending lists
- Transactional atomicity on script failure
- Concurrent execution locking

## Step 4: Verify architecture tests pass

```bash
dotnet test tests/ArchitectureTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Bedrock.ArchitectureTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj
```

**Expected**: All architecture rule tests pass (CS001-CS003, IN001-IN016).

## Step 5: Run the full pipeline

```bash
./scripts/pipeline.sh
```

**Expected**: Pipeline completes successfully with:
- 100% code coverage on new code
- 100% mutation score
- Zero architecture violations
- Zero SonarCloud issues (or justified exclusions)

## Manual Validation (optional)

To manually test against a local PostgreSQL:

1. Create a test database:
   ```sql
   CREATE DATABASE migration_test;
   ```

2. Create a sample migration script `V202602141200__create_test_table.sql`:
   ```sql
   CREATE TABLE test_table (
       id BIGINT PRIMARY KEY,
       name TEXT NOT NULL,
       created_at TIMESTAMPTZ DEFAULT NOW()
   );
   ```

3. Create the corresponding DOWN script:
   ```sql
   DROP TABLE IF EXISTS test_table;
   ```

4. Create a test migration class, embed the scripts, and run `MigrateUpAsync()`.

5. Verify `test_table` exists in the database.

6. Run `MigrateDownAsync(0)` and verify `test_table` no longer exists.
