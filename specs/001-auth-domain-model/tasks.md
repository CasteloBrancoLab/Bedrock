# Tasks: Auth Domain Model — User Entity com Credenciais

**Input**: Design documents from `/specs/001-auth-domain-model/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/domain-contracts.md, quickstart.md

**Tests**: Included — FR-016, FR-017 and SC-007 explicitly require 100% coverage and 100% mutation score for all projects.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source (BuildingBlocks)**: `src/BuildingBlocks/Security/`
- **Source (Auth Domain.Entities)**: `src/ShopDemo/Auth/Domain.Entities/`
- **Source (Auth Domain)**: `src/ShopDemo/Auth/Domain/`
- **Unit Tests (Security)**: `tests/UnitTests/BuildingBlocks/Security/`
- **Unit Tests (Auth Domain.Entities)**: `tests/UnitTests/ShopDemo/Auth/Domain.Entities/`
- **Unit Tests (Auth Domain)**: `tests/UnitTests/ShopDemo/Auth/Domain/`
- **Mutation Tests**: `tests/MutationTests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the new Security building block project, configure test projects, and add NuGet dependencies. Auth Domain.Entities and Auth Domain projects already exist (scaffolded in issue #137).

- [x] T001 Create Security building block project with .csproj referencing Core and Konscious.Security.Cryptography NuGet package in `src/BuildingBlocks/Security/Bedrock.BuildingBlocks.Security.csproj`
- [x] T002 Create GlobalUsings.cs for Security building block in `src/BuildingBlocks/Security/GlobalUsings.cs`
- [x] T003 Create Security unit test project with .csproj referencing Testing and Security in `tests/UnitTests/BuildingBlocks/Security/Bedrock.UnitTests.BuildingBlocks.Security.csproj`
- [x] T004 Create stryker-config.json for Security mutation tests in `tests/MutationTests/BuildingBlocks/Security/stryker-config.json`
- [x] T005 Add project reference to Security building block from Auth Domain .csproj in `src/ShopDemo/Auth/Domain/ShopDemo.Auth.Domain.csproj`
- [x] T006 Add all new projects to the solution file `Bedrock.sln` and verify build with `./scripts/build.sh`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement value objects, enums, input objects, interfaces, and metadata that ALL user stories depend on. These are the building blocks of the User entity.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Value Objects & Enums (no interdependencies)

- [x] T007 [P] Implement UserStatus enum with values Active=1, Suspended=2, Blocked=3 in `src/ShopDemo/Auth/Domain.Entities/Users/Enums/UserStatus.cs`
- [x] T008 [P] Implement PasswordHash readonly struct with ReadOnlyMemory\<byte\>, CreateNew factory, constant-time Equals, and `[REDACTED]` ToString in `src/ShopDemo/Auth/Domain.Entities/Users/PasswordHash.cs`

### Input Objects (depend on T007, T008)

- [x] T009 [P] Implement RegisterNewInput readonly record struct (EmailAddress, PasswordHash) in `src/ShopDemo/Auth/Domain.Entities/Users/Inputs/RegisterNewInput.cs`
- [x] T010 [P] Implement CreateFromExistingInfoInput readonly record struct (EntityInfo, string Username, EmailAddress, PasswordHash, UserStatus) in `src/ShopDemo/Auth/Domain.Entities/Users/Inputs/CreateFromExistingInfoInput.cs`
- [x] T011 [P] Implement ChangeStatusInput readonly record struct (UserStatus NewStatus) in `src/ShopDemo/Auth/Domain.Entities/Users/Inputs/ChangeStatusInput.cs`
- [x] T012 [P] Implement ChangeUsernameInput readonly record struct (string NewUsername) in `src/ShopDemo/Auth/Domain.Entities/Users/Inputs/ChangeUsernameInput.cs`
- [x] T013 [P] Implement ChangePasswordHashInput readonly record struct (PasswordHash NewPasswordHash) in `src/ShopDemo/Auth/Domain.Entities/Users/Inputs/ChangePasswordHashInput.cs`

### Interface & Metadata (depend on T007, T008)

- [x] T014 [P] Implement IUser interface extending IEntity with Username, Email, PasswordHash, Status properties in `src/ShopDemo/Auth/Domain.Entities/Users/Interfaces/IUser.cs`
- [x] T015 [P] Implement UserMetadata static class with validation properties (UsernameMinLength=1, UsernameMaxLength=255, PasswordHashMaxLength=128, etc.) and ChangeMetadata method in `src/ShopDemo/Auth/Domain.Entities/Users/UserMetadata.cs`

**Checkpoint**: All foundational types compiled — User entity implementation can now begin

---

## Phase 3: User Story 1 — Registrar Novo Usuário com Email e Senha (Priority: P1) 🎯 MVP

**Goal**: Implement the User aggregate root entity with RegisterNew factory method, CreateFromExistingInfo, and all change methods following the SimpleAggregateRoot template. This is the core of the entire domain model.

**Independent Test**: Create User via `RegisterNew` with valid email and password hash (byte[]), verify Id is generated, email stored, username = email, hash stored, status = Active, audit fields populated. No crypto dependency needed in tests.

### Implementation for User Story 1

- [x] T016 [US1] Implement User sealed class extending EntityBase\<User\>, IAggregateRoot, IUser with private constructors, all properties (Username, Email, PasswordHash, Status), RegisterNew factory method (validates all fields with & operator, sets Username=Email), CreateFromExistingInfo factory method (no validation), and all static Validate* methods in `src/ShopDemo/Auth/Domain.Entities/Users/User.cs`
- [x] T017 [US1] Implement ChangeStatus method with state machine validation (Active↔Suspended, Active↔Blocked, Suspended→Blocked allowed; Blocked→Suspended forbidden) using Clone-Modify-Return pattern in `src/ShopDemo/Auth/Domain.Entities/Users/User.cs`
- [x] T018 [US1] Implement ChangeUsername and ChangePasswordHash methods using Clone-Modify-Return pattern in `src/ShopDemo/Auth/Domain.Entities/Users/User.cs`

### Tests for User Story 1

- [x] T019 [P] [US1] Implement UserStatus enum tests (values, names, all 3 members) in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/UserStatusTests.cs`
- [x] T020 [P] [US1] Implement PasswordHash value object tests (CreateNew, IsEmpty, Length, constant-time Equals, ToString returns REDACTED, equality operators, GetHashCode) in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/PasswordHashTests.cs`
- [x] T021 [P] [US1] Implement Input object tests (all 5 readonly record structs — construction, equality, property access) in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/Inputs/InputObjectTests.cs`
- [x] T022 [P] [US1] Implement IUser interface tests (contract verification) in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/IUserTests.cs`
- [x] T023 [P] [US1] Implement UserMetadata tests (default values, ChangeMetadata, thread safety) in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/UserMetadataTests.cs`
- [x] T024 [US1] Implement User entity tests — RegisterNew: valid data returns User with correct fields (Id, Username=Email, Email, PasswordHash, Status=Active, audit fields); invalid email returns null with error; invalid hash returns null with error; multiple invalid fields returns null with ALL errors (bitwise AND); in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/UserTests.cs`
- [x] T025 [US1] Implement User entity tests — CreateFromExistingInfo: reconstructs User with all fields, no validation performed; in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/UserTests.cs`
- [x] T026 [US1] Implement User entity tests — ChangeStatus: all valid transitions succeed (Active→Suspended, Active→Blocked, Suspended→Active, Suspended→Blocked, Blocked→Active), Blocked→Suspended fails, original instance unchanged (immutability), EntityVersion incremented; in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/UserTests.cs`
- [x] T027 [US1] Implement User entity tests — ChangeUsername: valid username succeeds, empty/null/too-long fails, original unchanged; ChangePasswordHash: valid hash succeeds, empty fails, original unchanged; in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/UserTests.cs`
- [x] T028 [US1] Implement User entity tests — ValidateUsername, ValidateEmail, ValidatePasswordHash, ValidateStatusTransition static methods with valid and invalid inputs; in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/UserTests.cs`
- [x] T029 [US1] Run `./scripts/pipeline.sh` and fix any coverage gaps or surviving mutants for Domain.Entities project until 100% coverage and 100% mutation score

**Checkpoint**: User entity fully functional and testable — core MVP complete. Domain.Entities has zero dependencies on Security.

---

## Phase 4: User Story 5 — Operações de Hashing de Senha via Building Block de Segurança (Priority: P5, moved up)

**Goal**: Implement the Security building block with Argon2id password hashing, pepper management, and password policy validation. This phase is moved ahead of US2 because the Authentication Service (US2) depends on it.

**Independent Test**: Generate hash for known password, verify same password validates against hash. Verify different passwords produce different hashes (via salt). Verify incorrect passwords fail verification.

**Note**: This phase is reordered from P5 to Phase 4 because US2 (VerifyCredentials) depends on IPasswordHasher. The Security BB has no dependency on User Story 1 — only on Core.

### Implementation for User Story 5

- [x] T030 [P] [US5] Implement PasswordHashResult readonly record struct (byte[] Hash, int PepperVersion) in `src/BuildingBlocks/Security/Passwords/PasswordHashResult.cs`
- [x] T031 [P] [US5] Implement PasswordVerificationResult readonly record struct (bool IsValid, bool NeedsRehash) in `src/BuildingBlocks/Security/Passwords/PasswordVerificationResult.cs`
- [x] T032 [P] [US5] Implement PasswordPolicyMetadata static class (MinLength=12, MaxLength=128, AllowSpaces=true, ChangeMetadata method) in `src/BuildingBlocks/Security/Passwords/PasswordPolicyMetadata.cs`
- [x] T033 [US5] Implement PasswordPolicy static class with ValidatePassword method (validates length min/max, adds errors to ExecutionContext) in `src/BuildingBlocks/Security/Passwords/PasswordPolicy.cs`
- [x] T034 [US5] Implement PepperConfiguration class (ActivePepperVersion, Peppers dictionary, validation that at least one pepper exists and active version is in dictionary) in `src/BuildingBlocks/Security/Passwords/PepperConfiguration.cs`
- [x] T035 [US5] Implement IPasswordHasher interface (HashPassword, VerifyPassword, NeedsRehash) in `src/BuildingBlocks/Security/Passwords/IPasswordHasher.cs`
- [x] T036 [US5] Implement PasswordHasher class — HashPassword: HMAC-SHA256(pepper, password) then Argon2id with OWASP params (19 MiB memory, 2 iterations, 1 parallelism), produces pepper_version(1 byte) + salt(16 bytes) + hash(32 bytes) = 49 bytes; VerifyPassword: extracts pepper version and salt from stored hash, recomputes and compares; NeedsRehash: checks pepper version against active version; in `src/BuildingBlocks/Security/Passwords/PasswordHasher.cs`

### Tests for User Story 5

- [x] T037 [P] [US5] Implement PasswordPolicyMetadata tests (default values, ChangeMetadata) in `tests/UnitTests/BuildingBlocks/Security/Passwords/PasswordPolicyMetadataTests.cs`
- [x] T038 [P] [US5] Implement PasswordPolicy tests (valid password 12-128 chars, too short fails, too long fails, spaces allowed, null/empty fails, boundary values 12 and 128) in `tests/UnitTests/BuildingBlocks/Security/Passwords/PasswordPolicyTests.cs`
- [x] T039 [P] [US5] Implement PepperConfiguration tests (valid config, no peppers fails, active version not in dictionary fails, multiple versions) in `tests/UnitTests/BuildingBlocks/Security/Passwords/PepperConfigurationTests.cs`
- [x] T040 [US5] Implement PasswordHasher tests — HashPassword: produces non-empty 49-byte hash, same password different hashes (salt uniqueness), correct pepper version embedded; VerifyPassword: correct password validates, incorrect password fails; NeedsRehash: old pepper version returns true, current returns false; pepper rotation: hash with v1, configure v2 active, verify still works but NeedsRehash=true; empty/null pepper rejected; in `tests/UnitTests/BuildingBlocks/Security/Passwords/PasswordHasherTests.cs`
- [x] T041 [US5] Implement PasswordHashResult and PasswordVerificationResult tests (construction, properties) in `tests/UnitTests/BuildingBlocks/Security/Passwords/PasswordHashResultTests.cs`
- [x] T042 [US5] Run `./scripts/pipeline.sh` and fix any coverage gaps or surviving mutants for Security project until 100% coverage and 100% mutation score

**Checkpoint**: Security building block fully functional — password hashing, verification, policy, and pepper rotation all tested independently

---

## Phase 5: User Story 2 — Verificar Credenciais de Usuário (Priority: P2)

**Goal**: Implement the domain service that orchestrates User entity + IPasswordHasher + IUserRepository for authentication flows: register user (validate policy → hash → create entity) and verify credentials (find by email → verify hash → re-hash if needed).

**Independent Test**: Using mocked IPasswordHasher and IUserRepository, verify that AuthenticationService correctly orchestrates: registration creates User with hashed password, credential verification returns User on correct password, returns null on wrong password with generic error, triggers re-hash when NeedsRehash=true.

**Dependencies**: Requires Phase 3 (User entity) and Phase 4 (Security BB) to be complete.

### Implementation for User Story 2

- [x] T043 [US2] Implement IAuthenticationService interface (RegisterUserAsync, VerifyCredentialsAsync) in `src/ShopDemo/Auth/Domain/Services/IAuthenticationService.cs`
- [x] T044 [US2] Implement AuthenticationService — RegisterUserAsync: validate password policy → hash password via IPasswordHasher → create User via RegisterNew → persist via IUserRepository; VerifyCredentialsAsync: find User by email → verify password via IPasswordHasher → re-hash and update if NeedsRehash → return User or null with generic error; in `src/ShopDemo/Auth/Domain/Services/AuthenticationService.cs`

### Tests for User Story 2

- [x] T045 [US2] Implement AuthenticationService tests — RegisterUserAsync: valid email+password creates User, invalid password (policy violation) returns null, hashing is delegated to IPasswordHasher mock, persistence delegated to IUserRepository mock; in `tests/UnitTests/ShopDemo/Auth/Domain/Services/AuthenticationServiceTests.cs`
- [x] T046 [US2] Implement AuthenticationService tests — VerifyCredentialsAsync: correct credentials returns User, wrong password returns null with generic "credenciais inválidas" error (anti-enumeration), non-existent email returns null with same generic error, NeedsRehash=true triggers re-hash and ChangePasswordHash on User; in `tests/UnitTests/ShopDemo/Auth/Domain/Services/AuthenticationServiceTests.cs`
- [x] T047 [US2] Run `./scripts/pipeline.sh` and fix any coverage gaps or surviving mutants for Auth Domain project until 100% coverage and 100% mutation score

**Checkpoint**: Authentication service fully functional with mocked dependencies — register and verify credential flows tested

---

## Phase 6: User Story 3 — Modificar Status do Usuário (Priority: P3)

**Goal**: This user story's implementation is already complete in Phase 3 (T017 — ChangeStatus method with state machine validation). This phase only ensures comprehensive test coverage of the state machine edge cases.

**Independent Test**: Create User with Active status, change to Suspended — verify new instance returned with updated status, original unchanged. Test all valid and invalid transitions exhaustively.

**Note**: The ChangeStatus implementation and basic tests were done in Phase 3 (T017, T026). This phase adds exhaustive edge case coverage if any gaps remain after pipeline validation.

- [x] T048 [US3] Review pipeline results for state machine coverage — ensure all 6 transitions (5 valid + 1 invalid) are tested, including same-status transitions (Active→Active) and edge cases; add any missing tests in `tests/UnitTests/ShopDemo/Auth/Domain.Entities/Users/UserTests.cs`

**Checkpoint**: All state transitions exhaustively tested — no surviving mutants in state machine logic

---

## Phase 7: User Story 4 — Consultar Usuário por Email (Priority: P4)

**Goal**: Define the IUserRepository interface extending IRepository\<User\> with additional query methods for email/username lookup and existence checks.

**Independent Test**: Verify IUserRepository defines the correct contract with all method signatures matching the domain contracts.

### Implementation for User Story 4

- [x] T049 [US4] Implement IUserRepository interface extending IRepository\<User\> with GetByEmailAsync, GetByUsernameAsync, ExistsByEmailAsync, ExistsByUsernameAsync in `src/ShopDemo/Auth/Domain/Repositories/IUserRepository.cs`

### Tests for User Story 4

- [x] T050 [US4] Implement IUserRepository contract tests — verify interface extends IRepository\<User\>, verify method signatures match contracts (parameter types, return types, CancellationToken), verify inherited methods from IRepository\<User\> in `tests/UnitTests/ShopDemo/Auth/Domain/Repositories/IUserRepositoryTests.cs`

**Checkpoint**: Repository contract defined — persistence implementation deferred to future issue

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final pipeline validation, mutation tests, and verification that all success criteria are met.

- [x] T051 Run full `./scripts/pipeline.sh` and verify all 3 projects pass: Domain.Entities (100% coverage, 100% mutation), Security (100% coverage, 100% mutation), Domain (100% coverage, 100% mutation)
- [x] T052 Verify SC-009: Domain.Entities project compiles and tests pass with zero references to Bedrock.BuildingBlocks.Security (check .csproj and GlobalUsings.cs)
- [x] T053 Verify SC-006: All authentication error messages are generic ("credenciais inválidas") — no email/password differentiation in AuthenticationService
- [x] T054 Run quickstart.md validation — verify all components listed in quickstart.md exist and build correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **US1 - User Entity (Phase 3)**: Depends on Phase 2 — MVP delivery
- **US5 - Security BB (Phase 4)**: Depends on Phase 1 only (not on Phase 2/3) — can run in parallel with Phases 2+3
- **US2 - Verify Credentials (Phase 5)**: Depends on Phase 3 AND Phase 4 (needs both User entity and Security BB)
- **US3 - Status Management (Phase 6)**: Depends on Phase 3 (gap analysis only)
- **US4 - Repository Contract (Phase 7)**: Depends on Phase 3 (needs User type defined)
- **Polish (Phase 8)**: Depends on all previous phases

### User Story Dependencies

```
Phase 1 (Setup)
    │
    ├──────────────────────────────┐
    ▼                              ▼
Phase 2 (Foundational)        Phase 4 (US5 - Security BB) ←── independent of Domain.Entities
    │                              │
    ▼                              │
Phase 3 (US1 - User Entity)       │
    │                              │
    ├──────────────┬───────────────┤
    ▼              ▼               ▼
Phase 6 (US3)  Phase 7 (US4)  Phase 5 (US2 - Auth Service)
    │              │               │
    └──────────────┴───────────────┘
                   │
                   ▼
            Phase 8 (Polish)
```

### Within Each User Story

- Models/types before entity implementation
- Entity implementation before tests
- Tests before pipeline validation
- Pipeline must pass before moving to next phase

### Parallel Opportunities

- **Phase 1**: T001-T005 can all run in parallel (different files)
- **Phase 2**: T007-T008 in parallel (enum + VO), then T009-T015 all in parallel (inputs + interface + metadata)
- **Phase 2 + Phase 4**: Phase 4 (Security BB) can start immediately after Phase 1, in parallel with Phase 2 + Phase 3
- **Phase 3 Tests**: T019-T023 all in parallel (different test files), then T024-T028 sequential (same file)
- **Phase 4**: T030-T032 in parallel (result types + metadata), T037-T039 in parallel (test files)
- **Phase 6 + Phase 7**: Can run in parallel after Phase 3

---

## Parallel Example: Phases 2+4 (Maximum Parallelism)

```text
# After Phase 1 completes, launch TWO parallel tracks:

# Track A: Domain.Entities foundation → User entity (Phases 2+3)
Task T007: UserStatus enum
Task T008: PasswordHash value object
  ↓ (wait)
Task T009-T015: All input objects + interface + metadata (parallel)
  ↓ (wait)
Task T016-T018: User entity implementation
  ↓ (wait)
Task T019-T023: Foundation tests (parallel)
Task T024-T028: User entity tests (sequential — same file)

# Track B: Security building block (Phase 4)
Task T030-T032: Result types + metadata (parallel)
  ↓ (wait)
Task T033: PasswordPolicy
Task T034: PepperConfiguration
Task T035: IPasswordHasher
Task T036: PasswordHasher
  ↓ (wait)
Task T037-T041: Security tests

# Both tracks converge → Phase 5 (AuthenticationService)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (value objects, enums, inputs)
3. Complete Phase 3: User Entity (US1)
4. **STOP and VALIDATE**: Run `./scripts/pipeline.sh` — Domain.Entities 100% coverage + mutation
5. The User entity is fully functional and independently testable

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add User Entity (US1) → Test independently → **MVP!**
3. Add Security BB (US5) → Test independently → Password hashing ready
4. Add Auth Service (US2) → Test independently → Full authentication flow
5. Add Status Management (US3) → Test edge cases → State machine hardened
6. Add Repository Contract (US4) → Test contract → Ready for persistence layer
7. Each phase adds value without breaking previous phases

### Optimal Single-Developer Strategy

1. Phase 1 (Setup) → Phase 2 (Foundational) → Phase 3 (US1 - User Entity) → validate
2. Phase 4 (US5 - Security BB) → validate
3. Phase 5 (US2 - Auth Service) → validate
4. Phase 6 (US3) + Phase 7 (US4) in any order → validate
5. Phase 8 (Polish) → final validation → commit

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each phase passes pipeline validation
- The SimpleAggregateRoot template at `src/Templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs` is the reference implementation for User entity
- Domain.Entities must NEVER reference Security building block — this is verified in Phase 8 (T052)
- Auth mutation test stryker configs already exist from issue #137 scaffolding
