# Tasks: PostgreSQL Migrations BuildingBlock

**Input**: Design documents from `/specs/180-migration-postgresql-building-block/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/domain-contracts.md

**Tests**: Tests are REQUIRED per Constitution Principle I (100% coverage, 100% mutation). Tests are included in each user story phase.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, NuGet packages, and solution structure

- [X] T001 Add FluentMigrator packages to `Directory.Packages.props` under `<!-- Database -->` section: `FluentMigrator`, `FluentMigrator.Runner`, `FluentMigrator.Runner.Postgres`
- [X] T002 Create main project `src/BuildingBlocks/Persistence.PostgreSql.Migrations/Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj` with ProjectReferences to Core and Observability, PackageReferences to FluentMigrator packages, InternalsVisibleTo for unit test project
- [X] T003 [P] Create `src/BuildingBlocks/Persistence.PostgreSql.Migrations/GlobalUsings.cs` with global using for ExecutionContext alias, Observability.ExtensionMethods, Core.Utils, Core.Extensions
- [X] T004 [P] Create unit test project `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj` with reference to main project and test libraries (xUnit, Shouldly, Moq, Bogus, Humanizer.Core)
- [X] T005 [P] Create integration test project `tests/IntegrationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj` with reference to main project, Testing BuildingBlock, and Testcontainers.PostgreSql
- [X] T006 [P] Create architecture test project `tests/ArchitectureTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Bedrock.ArchitectureTests.BuildingBlocks.Persistence.PostgreSql.Migrations.csproj` with reference to Testing BuildingBlock
- [X] T007 [P] Create mutation test config `tests/MutationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/stryker-config.json` with project pointing to main .csproj and test-projects pointing to unit test .csproj, thresholds 100/100/100
- [X] T008 Add all new projects to `Bedrock.sln` using `dotnet sln add`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core types that ALL user stories depend on. MUST be complete before any user story.

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T009 [P] Create `SqlScriptAttribute` in `src/BuildingBlocks/Persistence.PostgreSql.Migrations/Attributes/SqlScriptAttribute.cs` — sealed attribute with `UpScriptResourceName` (string, required) and `DownScriptResourceName` (string?, optional), targeting Class only, not inheritable
- [X] T010 [P] Create `MigrationInfo` in `src/BuildingBlocks/Persistence.PostgreSql.Migrations/Models/MigrationInfo.cs` — `readonly record struct` with Version (long), Description (string), AppliedOn (DateTimeOffset?)
- [X] T011 [P] Create `MigrationStatus` in `src/BuildingBlocks/Persistence.PostgreSql.Migrations/Models/MigrationStatus.cs` — sealed class with AppliedMigrations (IReadOnlyList<MigrationInfo>), PendingMigrations (IReadOnlyList<MigrationInfo>), LastAppliedVersion (long?), HasPendingMigrations (bool)
- [X] T012 Create `SqlScriptMigrationBase` in `src/BuildingBlocks/Persistence.PostgreSql.Migrations/SqlScriptMigrationBase.cs` — abstract class inheriting FluentMigrator.Migration, reads [SqlScript] attribute via reflection, implements Up() and Down() using Execute.EmbeddedScript(), validates script existence (FR-012)
- [X] T013 [P] Create unit tests for SqlScriptAttribute in `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Attributes/SqlScriptAttributeTests.cs` — construction with valid params, null/empty UpScript validation, nullable DownScript, attribute usage constraints
- [X] T014 [P] Create unit tests for MigrationInfo in `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Models/MigrationInfoTests.cs` — value semantics, equality, construction, default values, record struct behavior
- [X] T015 [P] Create unit tests for MigrationStatus in `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Models/MigrationStatusTests.cs` — construction, immutable collections, LastAppliedVersion logic, HasPendingMigrations logic, empty lists edge case
- [X] T016 Create unit tests for SqlScriptMigrationBase in `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/SqlScriptMigrationBaseTests.cs` — Up/Down script loading, missing script error, null Down script throws on Down(), attribute reflection

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Criar e executar migrations de schema (Priority: P1) MVP

**Goal**: Developer can create UP/DOWN SQL scripts, annotated migration classes, and execute `MigrateUpAsync()` to apply pending migrations to the database.

**Independent Test**: Create a pair of UP/DOWN scripts, a migration class, and execute MigrationManagerBase against a test database via Testcontainers. The schema MUST reflect the UP script changes.

### Implementation for User Story 1

- [X] T017 [US1] Implement `MigrationManagerBase` in `src/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerBase.cs` — abstract class with: abstract properties (ConnectionString, TargetSchema, MigrationAssembly), ILogger constructor, internal FluentMigrator ServiceProvider setup via ConfigureRunner(rb => rb.AddPostgres()), `MigrateUpAsync(ExecutionContext, CancellationToken)` that builds runner and calls MigrateUp(), distributed tracing logging for start/end/each migration, error handling with ExecutionContext messages
- [X] T018 [US1] Create embedded test SQL scripts for unit/integration testing: `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/TestResources/Up/V202602140001__create_test_table.sql` and corresponding Down script, plus a second pair `V202602140002__add_test_column.sql` for multi-migration scenarios. Configure as EmbeddedResource in test .csproj
- [X] T019 [US1] Create test migration classes in unit test project: `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/TestMigrations/` — `V202602140001_CreateTestTable.cs` and `V202602140002_AddTestColumn.cs` using SqlScriptMigrationBase with [Migration] and [SqlScript] attributes
- [X] T020 [US1] Create unit tests for MigrationManagerBase.MigrateUpAsync in `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerBaseTests.cs` — tests runner configuration, logging calls, error propagation, ExecutionContext message recording, null parameter validation
- [X] T021 [US1] Create integration test fixture in `tests/IntegrationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Fixtures/MigrationFixture.cs` — extends ServiceCollectionFixture, ConfigureEnvironments with Testcontainers PostgreSQL, provides CreateExecutionContext(), CreateMigrationManager(connectionString), embedded test scripts
- [X] T022 [US1] Create integration tests for MigrateUpAsync in `tests/IntegrationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerIntegrationTests.cs` — Scenario 1: single migration applies schema changes; Scenario 2: multiple pending migrations applied in order; Scenario 3: no pending migrations is no-op; Scenario 4: invalid SQL causes failure with rollback

**Checkpoint**: User Story 1 fully functional — MigrateUpAsync works end-to-end with Testcontainers

---

## Phase 4: User Story 2 — Reverter migrations (Priority: P2)

**Goal**: Developer can rollback applied migrations to a target version using `MigrateDownAsync()`.

**Independent Test**: Apply migrations UP, then execute MigrateDownAsync to a target version. The schema MUST return to the state before the rolled-back migrations.

### Implementation for User Story 2

- [X] T023 [US2] Add `MigrateDownAsync(ExecutionContext, long targetVersion, CancellationToken)` to `MigrationManagerBase` in `src/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerBase.cs` — builds runner, calls MigrateDown(targetVersion), distributed tracing logging, validates DOWN script existence before execution, throws InvalidOperationException for missing DOWN scripts
- [X] T024 [US2] Add unit tests for MigrateDownAsync in `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerBaseTests.cs` — rollback logging, error propagation, null param validation, target version validation
- [X] T025 [US2] Add integration tests for MigrateDownAsync in `tests/IntegrationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerIntegrationTests.cs` — Scenario 1: rollback single migration; Scenario 2: rollback multiple migrations in descending order; Scenario 3: rollback on empty DB is no-op

**Checkpoint**: User Stories 1 AND 2 both work independently

---

## Phase 5: User Story 3 — Consultar status das migrations (Priority: P3)

**Goal**: Developer can query applied and pending migrations without making any changes.

**Independent Test**: Apply some migrations, then query status. The result MUST correctly list applied (with dates) and pending migrations.

### Implementation for User Story 3

- [X] T026 [US3] Add `GetStatusAsync(ExecutionContext, CancellationToken)` to `MigrationManagerBase` in `src/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerBase.cs` — queries FluentMigrator's IVersionLoader and IMigrationInformationLoader, builds MigrationStatus with applied and pending lists, distributed tracing logging
- [X] T027 [US3] Add unit tests for GetStatusAsync in `tests/UnitTests/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerBaseTests.cs` — returns correct applied/pending split, empty DB returns all pending, fully migrated DB returns no pending, null param validation
- [X] T028 [US3] Add integration tests for GetStatusAsync in `tests/IntegrationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerIntegrationTests.cs` — Scenario 1: mixed applied/pending; Scenario 2: new DB reports all pending

**Checkpoint**: All user stories (1, 2, 3) independently functional

---

## Phase 6: User Story 4 — Configurar MigrationManagerBase por bounded context (Priority: P4)

**Goal**: Developer can create a concrete MigrationManagerBase with custom connection string, schema, and assembly configuration.

**Independent Test**: Create a concrete MigrationManagerBase with specific configuration and verify that FluentMigrator is configured with the provided parameters.

### Implementation for User Story 4

- [X] T029 [US4] Add integration tests verifying bounded context configuration in `tests/IntegrationTests/BuildingBlocks/Persistence.PostgreSql.Migrations/MigrationManagerIntegrationTests.cs` — Scenario 1: concrete class with custom connection string and schema applies migrations to configured schema; Scenario 2: concrete class with specific assembly scans only that assembly's migrations

**Checkpoint**: All 4 user stories independently functional and testable

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Architecture validation, mutation testing, and pipeline verification

- [X] T030 [P] Create architecture test fixture in `tests/ArchitectureTests/BuildingBlocks/Persistence.PostgreSql.Migrations/Fixtures/ArchFixture.cs` — sealed class inheriting RuleFixture, GetProjectPaths() pointing to main .csproj, sealed ArchCollection with Collection("Arch")
- [X] T031 [P] Create CodeStyleRuleTests in `tests/ArchitectureTests/BuildingBlocks/Persistence.PostgreSql.Migrations/CodeStyleRuleTests.cs` — sealed class inheriting CodeStyleRuleTestsBase<ArchFixture>, primary constructor with (ArchFixture fixture, ITestOutputHelper output), [Collection("Arch")]
- [X] T032 [P] Create InfrastructureRuleTests in `tests/ArchitectureTests/BuildingBlocks/Persistence.PostgreSql.Migrations/InfrastructureRuleTests.cs` — sealed class inheriting InfrastructureRuleTestsBase<ArchFixture>, primary constructor, [Collection("Arch")]
- [X] T033 Run `dotnet build` to verify all projects compile
- [X] T034 Run `dotnet test` on all 3 test projects (unit, integration, architecture) to verify all tests pass
- [X] T035 Run full pipeline `./scripts/pipeline.sh` and resolve any pending items in `artifacts/pending/SUMMARY.txt` (architecture violations, surviving mutants, SonarCloud issues)
- [X] T036 Verify quickstart.md steps can be followed successfully

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) — MVP
- **User Story 2 (Phase 4)**: Depends on US1 (MigrationManagerBase must exist)
- **User Story 3 (Phase 5)**: Depends on US1 (MigrationManagerBase must exist)
- **User Story 4 (Phase 6)**: Depends on US1 (MigrationManagerBase must exist)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — MVP, creates MigrationManagerBase
- **User Story 2 (P2)**: Can start after US1 (adds MigrateDownAsync to existing MigrationManagerBase)
- **User Story 3 (P3)**: Can start after US1 (adds GetStatusAsync to existing MigrationManagerBase)
- **User Story 4 (P4)**: Can start after US1 (tests extensibility of existing MigrationManagerBase)

> **Note**: US2, US3, and US4 all depend on US1 because they add methods to or test the `MigrationManagerBase` class created in US1. However, US2, US3, and US4 are independent of each other.

### Within Each User Story

- Implementation before tests (when adding to existing class)
- Unit tests before integration tests
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks T003-T007 can run in parallel
- All Foundational type tasks T009-T011 can run in parallel
- All Foundational unit test tasks T013-T015 can run in parallel
- US2, US3, and US4 can run in parallel after US1 completes (if team capacity allows)
- All Polish architecture tasks T030-T032 can run in parallel

---

## Parallel Example: Phase 2 (Foundational)

```bash
# Launch all type definitions in parallel:
Task: "Create SqlScriptAttribute in src/.../Attributes/SqlScriptAttribute.cs"
Task: "Create MigrationInfo in src/.../Models/MigrationInfo.cs"
Task: "Create MigrationStatus in src/.../Models/MigrationStatus.cs"

# Then SqlScriptMigrationBase (depends on SqlScriptAttribute):
Task: "Create SqlScriptMigrationBase in src/.../SqlScriptMigrationBase.cs"

# Launch all foundational unit tests in parallel:
Task: "Unit tests for SqlScriptAttribute in tests/.../SqlScriptAttributeTests.cs"
Task: "Unit tests for MigrationInfo in tests/.../MigrationInfoTests.cs"
Task: "Unit tests for MigrationStatus in tests/.../MigrationStatusTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (MigrateUpAsync)
4. **STOP and VALIDATE**: Test MigrateUpAsync independently with Testcontainers
5. Proceed to remaining stories

### Incremental Delivery

1. Complete Setup + Foundational -> Foundation ready
2. Add User Story 1 -> Test independently (MVP!)
3. Add User Story 2 -> Test rollback independently
4. Add User Story 3 -> Test status query independently
5. Add User Story 4 -> Test extensibility independently
6. Each story adds value without breaking previous stories

### Sequential Execution (recommended for single developer)

1. Phase 1: Setup (T001-T008)
2. Phase 2: Foundational types + tests (T009-T016)
3. Phase 3: US1 MigrateUpAsync + tests (T017-T022)
4. Phase 4: US2 MigrateDownAsync + tests (T023-T025)
5. Phase 5: US3 GetStatusAsync + tests (T026-T028)
6. Phase 6: US4 Configuration tests (T029)
7. Phase 7: Architecture + mutation + pipeline (T030-T036)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All tests follow BB-IX (TestBase, AAA, Shouldly) and BB-X (IntegrationTestBase, UseEnvironment, Testcontainers)
- MigrationManagerBase is a single file that grows across US1-US3; each story adds a method
