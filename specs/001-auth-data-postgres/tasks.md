# Tasks: Auth User Data Access Layer (PostgreSQL)

**Input**: Design documents from `/specs/001-auth-data-postgres/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included — required by Constitution I (100% coverage, 100% mutation) and spec SC-001.

**Organization**: Tasks are grouped by implementation phase, with user stories mapped to the phases that enable them. Due to the layered nature of this infrastructure feature, foundational components must be built bottom-up before any story can be independently tested.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `samples/ShopDemo/Auth/Infra.Data.PostgreSql/` and `samples/ShopDemo/Auth/Infra.Data/`
- **Unit Tests**: `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/` and `tests/UnitTests/ShopDemo/Auth/Infra.Data/`
- **Mutation Tests**: `tests/MutationTests/ShopDemo/Auth/Infra.Data.PostgreSql/` and `tests/MutationTests/ShopDemo/Auth/Infra.Data/`
- **Template Reference**: `src/templates/Infra.Data.PostgreSql/` (normative source per BB-XI)

---

## Phase 1: Setup (Project Scaffolding)

**Purpose**: Update existing .csproj files and create test project structure

- [ ] T001 Add Observability project reference to `samples/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.Auth.Infra.Data.PostgreSql.csproj` (per research R7)
- [ ] T002 Add Infra.Data.PostgreSql project reference to `samples/ShopDemo/Auth/Infra.Data/ShopDemo.Auth.Infra.Data.csproj` (UserRepository depends on IUserPostgreSqlRepository)
- [ ] T003 [P] Create unit test project `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.Auth.UnitTests.Infra.Data.PostgreSql.csproj` with references to xUnit, Shouldly, Moq, Bogus, Coverlet, Bedrock.BuildingBlocks.Testing, and ShopDemo.Auth.Infra.Data.PostgreSql
- [ ] T004 [P] Create unit test project `tests/UnitTests/ShopDemo/Auth/Infra.Data/ShopDemo.Auth.UnitTests.Infra.Data.csproj` with references to xUnit, Shouldly, Moq, Bogus, Coverlet, Bedrock.BuildingBlocks.Testing, and ShopDemo.Auth.Infra.Data
- [ ] T005 Register both new test projects in `Bedrock.sln`
- [ ] T006 Verify solution builds: `dotnet build Bedrock.sln`

---

## Phase 2: Foundational Infrastructure (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented. These are shared components used by all repository operations.

**CRITICAL**: No user story work can begin until this phase is complete.

### Connection + UnitOfWork (no domain dependencies)

- [ ] T007 [P] Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Connections/Interfaces/IAuthPostgreSqlConnection.cs` — interface extending `IPostgreSqlConnection` (follow `ITemplatesPostgreSqlConnection` template)
- [ ] T008 [P] Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/UnitOfWork/Interfaces/IAuthPostgreSqlUnitOfWork.cs` — interface extending `IPostgreSqlUnitOfWork` (follow `ITemplatesPostgreSqlUnitOfWork` template)
- [ ] T009 Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Connections/AuthPostgreSqlConnection.cs` — sealed class extending `PostgreSqlConnectionBase`, implementing `IAuthPostgreSqlConnection`, with config key `ConnectionStrings:AuthPostgreSql` (follow `TemplatesPostgreSqlConnection` template)
- [ ] T010 Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/UnitOfWork/AuthPostgreSqlUnitOfWork.cs` — sealed class extending `PostgreSqlUnitOfWorkBase`, implementing `IAuthPostgreSqlUnitOfWork`, with name `AuthPostgreSqlUnitOfWork` (follow `TemplatesPostgreSqlUnitOfWork` template)

### DataModel + Mapper

- [ ] T011 Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/DataModels/UserDataModel.cs` — class extending `DataModelBase` with properties: `Username` (string), `Email` (string), `PasswordHash` (byte[]), `Status` (short). Per data-model.md column mapping.
- [ ] T012 Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Mappers/UserDataModelMapper.cs` — sealed class extending `DataModelMapperBase<UserDataModel>`. Configure table `public.auth_users`, map 4 columns (Username, Email, PasswordHash, Status). Implement `MapBinaryImporter` with all 14 columns (10 base + 4 entity). Use NpgsqlDbType.Bytea for PasswordHash, NpgsqlDbType.Smallint for Status. Follow `SimpleAggregateRootDataModelMapper` template.

### Factories + Adapter

- [ ] T013 [P] Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Factories/UserDataModelFactory.cs` — static class with `Create(User entity)` using `DataModelBaseFactory.Create<>()` for base fields, then mapping Username, Email.Value, PasswordHash.Value.ToArray(), (short)Status. Follow `SimpleAggregateRootDataModelFactory` template.
- [ ] T014 [P] Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Factories/UserFactory.cs` — static class with `Create(UserDataModel dataModel)` reconstructing `EntityInfo` from DataModelBase fields, then calling `User.CreateFromExistingInfo()` with `EmailAddress.CreateNew(email)`, `PasswordHash.CreateNew(hash)`, `(UserStatus)status`. Follow `SimpleAggregateRootFactory` template.
- [ ] T015 [P] Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Adapters/UserDataModelAdapter.cs` — static class with `Adapt(UserDataModel, User)` using `DataModelBaseAdapter.Adapt()` for base fields, then mapping Username, Email.Value, PasswordHash.Value.ToArray(), (short)Status. Follow `SimpleAggregateRootDataModelAdapter` template.
- [ ] T016 Verify Infra.Data.PostgreSql compiles: `dotnet build samples/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.Auth.Infra.Data.PostgreSql.csproj`

**Checkpoint**: All foundational persistence infrastructure compiled. Ready for repository layer.

---

## Phase 3: User Story 1 + User Story 5 — Core Persistence (Persist + Retrieve by ID) (Priority: P1/P2) MVP

**Goal**: Enable persisting a new User and retrieving by ID — the minimal viable repository operations.

**Independent Test**: Create a User via `RegisterNew`, persist through repository chain, retrieve by ID, verify all fields round-trip correctly.

### Implementation

- [ ] T017 Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/Interfaces/IUserDataModelRepository.cs` — interface extending `IPostgreSqlDataModelRepository<UserDataModel>` with custom method signatures: `GetByEmailAsync`, `GetByUsernameAsync`, `ExistsByEmailAsync`, `ExistsByUsernameAsync` (all using raw types: string for email/username). Per contracts/repository-contracts.md Layer 3.
- [ ] T018 Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepository.cs` — sealed class extending `DataModelRepositoryBase<UserDataModel>`, implementing `IUserDataModelRepository`. Constructor receives `ILogger<UserDataModelRepository>`, `IAuthPostgreSqlUnitOfWork`, `IDataModelMapper<UserDataModel>`. Leave custom query methods with `throw new NotImplementedException()` for now (implemented in Phase 4/5). Follow `SimpleAggregateRootDataModelRepository` template for base class wiring.
- [ ] T019 Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/Interfaces/IUserPostgreSqlRepository.cs` — interface extending `IPostgreSqlRepository<User>` with custom method signatures: `GetByEmailAsync(ExecutionContext, EmailAddress, CancellationToken)`, `GetByUsernameAsync(ExecutionContext, string, CancellationToken)`, `ExistsByEmailAsync(ExecutionContext, EmailAddress, CancellationToken)`, `ExistsByUsernameAsync(ExecutionContext, string, CancellationToken)`. Per contracts/repository-contracts.md Layer 2.
- [ ] T020 Create `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepository.cs` — sealed class implementing `IUserPostgreSqlRepository`. Wraps `IUserDataModelRepository`. Implement base methods: `GetByIdAsync` (uses UserFactory.Create for reconstitution), `ExistsAsync` (delegates to DataModelRepository), `RegisterNewAsync` (uses UserDataModelFactory.Create then InsertAsync), `EnumerateAllAsync` and `EnumerateModifiedSinceAsync` with handler pattern. Leave custom query methods with `throw new NotImplementedException()` for now. Follow `SimpleAggregateRootPostgreSqlRepository` template.
- [ ] T021 Create `samples/ShopDemo/Auth/Infra.Data/Repositories/UserRepository.cs` — sealed class extending `RepositoryBase<User>`, implementing `IUserRepository`. Wraps `IUserPostgreSqlRepository`. Implement all abstract internal methods delegating to the PostgreSql repository. Implement custom methods (GetByEmailAsync, GetByUsernameAsync, ExistsByEmailAsync, ExistsByUsernameAsync) with try-catch error handling following `RepositoryBase` template method pattern. Follow `SimpleAggregateRootRepository` template pattern but with full implementation delegating to PostgreSql repository.
- [ ] T022 Verify both projects compile: `dotnet build samples/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.Auth.Infra.Data.PostgreSql.csproj && dotnet build samples/ShopDemo/Auth/Infra.Data/ShopDemo.Auth.Infra.Data.csproj`

### Unit Tests for US1 + US5

- [ ] T023 [P] [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/DataModels/UserDataModelTests.cs` — test all properties, default values, inheritance from DataModelBase. TestBase inheritance, Shouldly assertions, AAA with LogArrange/LogAct/LogAssert.
- [ ] T024 [P] [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Mappers/UserDataModelMapperTests.cs` — test ConfigureInternal (table name, column mappings), MapBinaryImporter (all 14 columns written correctly). Mock NpgsqlBinaryImporter if needed.
- [ ] T025 [P] [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Factories/UserDataModelFactoryTests.cs` — test Create(User entity) maps all fields correctly including PasswordHash as byte[] and Status as short. Use Bogus for test data.
- [ ] T026 [P] [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Factories/UserFactoryTests.cs` — test Create(UserDataModel) reconstitutes User via CreateFromExistingInfo with correct EntityInfo, EmailAddress, PasswordHash, UserStatus. Verify round-trip fidelity.
- [ ] T027 [P] [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Adapters/UserDataModelAdapterTests.cs` — test Adapt maps all fields correctly from entity to existing DataModel, including base fields via DataModelBaseAdapter.
- [ ] T028 [P] [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Connections/AuthPostgreSqlConnectionTests.cs` — test ConfigureInternal reads connection string from IConfiguration, throws on null/empty.
- [ ] T029 [P] [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/UnitOfWork/AuthPostgreSqlUnitOfWorkTests.cs` — test constructor passes correct name and connection to base.
- [ ] T030 [P] [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepositoryTests.cs` — test constructor validates parameters, base class wiring.
- [ ] T031 [P] [US5] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepositoryTests.cs` — test GetByIdAsync (found → factory conversion, not found → null), ExistsAsync (delegates), RegisterNewAsync (factory + insert), EnumerateAllAsync/EnumerateModifiedSinceAsync with handler pattern. Use Moq for IUserDataModelRepository.
- [ ] T032 [US1] Create `tests/UnitTests/ShopDemo/Auth/Infra.Data/Repositories/UserRepositoryTests.cs` — test all RepositoryBase methods delegate correctly to IUserPostgreSqlRepository. Test error handling (exception → log + return null/false). Use Moq for IUserPostgreSqlRepository and ILogger.
- [ ] T033 Run tests: `dotnet test tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/ && dotnet test tests/UnitTests/ShopDemo/Auth/Infra.Data/`

**Checkpoint**: Core persist + retrieve by ID works end-to-end through all layers. US1 and US5 are testable.

---

## Phase 4: User Story 2 — Retrieve User by Email (Priority: P1)

**Goal**: Enable looking up a user by email address — the primary authentication query path.

**Independent Test**: Persist a user, call GetByEmailAsync with the user's email, verify correct User aggregate root is returned with all fields intact.

### Implementation

- [ ] T034 [US2] Implement `GetByEmailAsync` in `samples/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepository.cs` — SQL: `SELECT * FROM auth_users WHERE email = @email AND tenant_code = @tenant LIMIT 1`. Use mapper for column names, UnitOfWork for connection. Include try-catch with logging following DataModelRepositoryBase error handling pattern.
- [ ] T035 [US2] Implement `GetByEmailAsync` in `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepository.cs` — delegate to DataModelRepository.GetByEmailAsync, convert result via UserFactory.Create if not null.
- [ ] T036 Verify compile: `dotnet build samples/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.Auth.Infra.Data.PostgreSql.csproj`

### Unit Tests for US2

- [ ] T037 [US2] Add GetByEmailAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepositoryTests.cs` — test found returns DataModel, not found returns null, tenant isolation enforced.
- [ ] T038 [US2] Add GetByEmailAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepositoryTests.cs` — test factory conversion on found, null passthrough.
- [ ] T039 [US2] Add GetByEmailAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data/Repositories/UserRepositoryTests.cs` — test delegation and error handling.
- [ ] T040 Run tests: `dotnet test tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/ && dotnet test tests/UnitTests/ShopDemo/Auth/Infra.Data/`

**Checkpoint**: US2 (retrieve by email) works independently. Login flow query path is functional.

---

## Phase 5: User Story 3 + User Story 4 — Username Queries + Existence Checks (Priority: P2)

**Goal**: Enable username-based lookup and lightweight existence checks for registration validation.

**Independent Test**: Persist a user, verify GetByUsernameAsync returns correct entity, ExistsByEmailAsync/ExistsByUsernameAsync return true for existing and false for non-existing.

### Implementation

- [ ] T041 [P] [US3] Implement `GetByUsernameAsync` in `samples/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepository.cs` — SQL: `SELECT * FROM auth_users WHERE username = @username AND tenant_code = @tenant LIMIT 1`. Same pattern as GetByEmailAsync.
- [ ] T042 [P] [US4] Implement `ExistsByEmailAsync` in `samples/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepository.cs` — SQL: `SELECT EXISTS(SELECT 1 FROM auth_users WHERE email = @email AND tenant_code = @tenant)`. Return boolean.
- [ ] T043 [P] [US4] Implement `ExistsByUsernameAsync` in `samples/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepository.cs` — SQL: `SELECT EXISTS(SELECT 1 FROM auth_users WHERE username = @username AND tenant_code = @tenant)`. Return boolean.
- [ ] T044 [US3] Implement `GetByUsernameAsync` in `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepository.cs` — delegate + factory conversion.
- [ ] T045 [US4] Implement `ExistsByEmailAsync` and `ExistsByUsernameAsync` in `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepository.cs` — direct delegation to DataModelRepository.
- [ ] T046 Verify compile: `dotnet build samples/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.Auth.Infra.Data.PostgreSql.csproj`

### Unit Tests for US3 + US4

- [ ] T047 [P] [US3] Add GetByUsernameAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepositoryTests.cs`
- [ ] T048 [P] [US4] Add ExistsByEmailAsync and ExistsByUsernameAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepositoryTests.cs`
- [ ] T049 [P] [US3] Add GetByUsernameAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepositoryTests.cs`
- [ ] T050 [P] [US4] Add ExistsByEmailAsync/ExistsByUsernameAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepositoryTests.cs`
- [ ] T051 [US3] Add GetByUsernameAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data/Repositories/UserRepositoryTests.cs`
- [ ] T052 [US4] Add ExistsByEmailAsync/ExistsByUsernameAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data/Repositories/UserRepositoryTests.cs`
- [ ] T053 Run tests: `dotnet test tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/ && dotnet test tests/UnitTests/ShopDemo/Auth/Infra.Data/`

**Checkpoint**: US3 (username lookup) and US4 (existence checks) work independently. Registration uniqueness validation is functional.

---

## Phase 6: User Story 6 — Update User State (Priority: P3)

**Goal**: Enable updating User mutable fields (username, password hash, status) with optimistic concurrency control.

**Independent Test**: Persist a user, change status via domain method, update through repository, re-retrieve and verify changes persisted. Verify stale version update fails.

### Implementation

- [ ] T054 [US6] Implement `UpdateAsync` method in `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepository.cs` — use UserDataModelAdapter.Adapt to update DataModel from entity, then delegate to DataModelRepository.UpdateAsync with expected entity version for optimistic concurrency.
- [ ] T055 Verify compile: `dotnet build samples/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.Auth.Infra.Data.PostgreSql.csproj`

### Unit Tests for US6

- [ ] T056 [US6] Add UpdateAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepositoryTests.cs` — test adapter usage, version passing for optimistic concurrency, success and failure paths.
- [ ] T057 [US6] Add UpdateAsync tests to `tests/UnitTests/ShopDemo/Auth/Infra.Data/Repositories/UserRepositoryTests.cs` — test delegation and error handling.
- [ ] T058 Run tests: `dotnet test tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/ && dotnet test tests/UnitTests/ShopDemo/Auth/Infra.Data/`

**Checkpoint**: US6 (update with concurrency control) works. All user stories are now functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Mutation tests, pipeline validation, and final quality gates

- [ ] T059 [P] Create `tests/MutationTests/ShopDemo/Auth/Infra.Data.PostgreSql/stryker-config.json` — project: `ShopDemo.Auth.Infra.Data.PostgreSql.csproj`, test-projects: `ShopDemo.Auth.UnitTests.Infra.Data.PostgreSql.csproj`, thresholds: 100/100/100
- [ ] T060 [P] Create `tests/MutationTests/ShopDemo/Auth/Infra.Data/stryker-config.json` — project: `ShopDemo.Auth.Infra.Data.csproj`, test-projects: `ShopDemo.Auth.UnitTests.Infra.Data.csproj`, thresholds: 100/100/100
- [ ] T061 Run full pipeline: `./scripts/pipeline.sh` — must pass 100% coverage, 100% mutation, zero applicable SonarCloud issues
- [ ] T062 If pipeline has pendencies: read `artifacts/pending/SUMMARY.txt`, fix issues, re-run (max 5 attempts per CLAUDE.md)
- [ ] T063 Verify all Roslyn rules pass (DE* and CS*) for new code — check `artifacts/` for any architecture violations

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **US1+US5 (Phase 3)**: Depends on Phase 2 — the MVP milestone
- **US2 (Phase 4)**: Depends on Phase 3 (needs base repository layer in place)
- **US3+US4 (Phase 5)**: Depends on Phase 3 (same pattern as Phase 4, can run in parallel with Phase 4)
- **US6 (Phase 6)**: Depends on Phase 3 (needs Adapter and base repository)
- **Polish (Phase 7)**: Depends on all Phases 3-6 complete

### User Story Dependencies

- **US1 (Persist)**: Requires all foundational infra + factories + base repository = Phase 2+3
- **US2 (Get by Email)**: Requires US1 infrastructure + custom email query method
- **US3 (Get by Username)**: Requires US1 infrastructure + custom username query method
- **US4 (Existence Checks)**: Requires US1 infrastructure + custom exists methods
- **US5 (Get by ID)**: Comes free with US1 (base repository provides GetByIdAsync)
- **US6 (Update)**: Requires US1 infrastructure + Adapter + UpdateAsync wiring

### Parallel Opportunities

- **Phase 1**: T003 and T004 (test project creation) can run in parallel
- **Phase 2**: T007+T008 (interfaces), T013+T014+T015 (factories+adapter) can run in parallel
- **Phase 3**: T023-T032 (unit tests) can mostly run in parallel (different files)
- **Phase 4+5**: Can run in parallel with each other (different query methods)
- **Phase 5**: T041+T042+T043 (custom DataModelRepo methods) can run in parallel

---

## Parallel Example: Phase 2 Foundational

```bash
# Step 1: Interfaces in parallel (different files)
Task: T007 "Create IAuthPostgreSqlConnection"
Task: T008 "Create IAuthPostgreSqlUnitOfWork"

# Step 2: Implementations (depend on interfaces)
Task: T009 "Create AuthPostgreSqlConnection"
Task: T010 "Create AuthPostgreSqlUnitOfWork"

# Step 3: DataModel + Mapper (sequential within, no interface dependency)
Task: T011 "Create UserDataModel"
Task: T012 "Create UserDataModelMapper" (depends on T011)

# Step 4: Factories + Adapter in parallel (depend on T011)
Task: T013 "Create UserDataModelFactory"
Task: T014 "Create UserFactory"
Task: T015 "Create UserDataModelAdapter"
```

## Parallel Example: Phase 3 Unit Tests

```bash
# All test files can be written in parallel (different files, no dependencies):
Task: T023 "UserDataModelTests"
Task: T024 "UserDataModelMapperTests"
Task: T025 "UserDataModelFactoryTests"
Task: T026 "UserFactoryTests"
Task: T027 "UserDataModelAdapterTests"
Task: T028 "AuthPostgreSqlConnectionTests"
Task: T029 "AuthPostgreSqlUnitOfWorkTests"
Task: T030 "UserDataModelRepositoryTests"
Task: T031 "UserPostgreSqlRepositoryTests"
Task: T032 "UserRepositoryTests"
```

---

## Implementation Strategy

### MVP First (US1 + US5 — Persist + Retrieve by ID)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational Infrastructure
3. Complete Phase 3: US1 + US5 (Core Persistence)
4. **STOP and VALIDATE**: Run unit tests, verify persist + retrieve works
5. This is the minimum viable data access layer

### Incremental Delivery

1. Phase 1+2 → Foundation ready (compiles, no tests yet)
2. Phase 3 → US1+US5 + tests → Persist and Retrieve by ID work (MVP)
3. Phase 4 → US2 + tests → Email lookup works (enables login flow)
4. Phase 5 → US3+US4 + tests → Username lookup + existence checks work (enables registration validation)
5. Phase 6 → US6 + tests → Updates work (enables password change, account management)
6. Phase 7 → Pipeline passes → Ready for PR

### Single Developer Strategy (Recommended)

Execute sequentially Phase 1 → 2 → 3 → 4 → 5 → 6 → 7, using `dotnet build` and `dotnet test` for rapid feedback between tasks. Run `./scripts/pipeline.sh` only once at Phase 7.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Template reference files are in `src/templates/Infra.Data.PostgreSql/` — consult them for exact code patterns
- All classes must be `sealed` (constitution BB-III)
- All interfaces must be in `Interfaces/` subdirectory (constitution BB-IV)
- All lambdas passed to project methods must use `static` modifier (Roslyn rule CS002)
- Test classes must inherit `TestBase`, use Shouldly assertions, AAA with LogArrange/LogAct/LogAssert (constitution BB-IX)
- Namespace for Infra.Data.PostgreSql: `ShopDemo.Auth.Infra.Data.PostgreSql.*`
- Namespace for Infra.Data: `ShopDemo.Auth.Infra.Data.*`
