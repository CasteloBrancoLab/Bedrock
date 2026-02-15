# Tasks: Configuration BuildingBlock

**Input**: Design documents from `/specs/001-configuration-building-block/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/public-api.md, quickstart.md

**Tests**: REQUIRED — Constitution mandates 100% coverage + 100% mutation score. Tests are included in each user story phase.

**Organization**: Tasks grouped by user story (P1→P4) for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1, US2, US3, US4)
- Exact file paths included in all descriptions

## Path Conventions

- **Source**: `src/BuildingBlocks/Configuration/`
- **Unit Tests**: `tests/UnitTests/BuildingBlocks/Configuration/`
- **Mutation Tests**: `tests/MutationTests/BuildingBlocks/Configuration/`
- **Architecture Tests**: `tests/ArchitectureTests/BuildingBlocks/Configuration/`

---

## Phase 1: Setup (Project Scaffolding)

**Purpose**: Create all project files, add to solution, verify build

- [x] T001 Create source project `src/BuildingBlocks/Configuration/Bedrock.BuildingBlocks.Configuration.csproj` with PackageReferences (Configuration.Abstractions, DI.Abstractions, Logging.Abstractions) and ProjectReference to Core. Add InternalsVisibleTo for unit test project. Create `GlobalUsings.cs`.
- [x] T002 [P] Create unit test project `tests/UnitTests/BuildingBlocks/Configuration/Bedrock.UnitTests.BuildingBlocks.Configuration.csproj` with references to source project and Bedrock.BuildingBlocks.Testing
- [x] T003 [P] Create mutation test config `tests/MutationTests/BuildingBlocks/Configuration/stryker-config.json` with thresholds 100/100/100
- [x] T004 [P] Create architecture test project `tests/ArchitectureTests/BuildingBlocks/Configuration/Bedrock.ArchitectureTests.BuildingBlocks.Configuration.csproj` with `Fixtures/ArchFixture.cs`, `CodeStyleRuleTests.cs`, and `InfrastructureRuleTests.cs`
- [x] T005 Add DI.Abstractions to `Directory.Packages.props` if not present. Add all 3 new projects to `Bedrock.sln` in correct solution folders.
- [x] T006 Verify all projects build: `dotnet build Bedrock.sln`

**Checkpoint**: All projects compile. Architecture tests and unit tests are runnable (empty or placeholder).

---

## Phase 2: Foundational (Model Types)

**Purpose**: Implement value types and enums used across all user stories. MUST complete before any user story.

- [x] T007 [P] Implement `LoadStrategy` enum in `src/BuildingBlocks/Configuration/Handlers/Enums/LoadStrategy.cs` — values: StartupOnly (0), LazyStartupOnly (1), AllTime (2) with XML docs
- [x] T008 [P] Implement `ConfigurationPath` readonly struct in `src/BuildingBlocks/Configuration/ConfigurationPath.cs` — properties: Section, Property, FullPath. Static factory `Create(string, string)`. Implement IEquatable, Equals, GetHashCode, ==, !=, ToString.
- [x] T009 [P] Implement `HandlerScope` readonly struct and `ScopeType` enum in `src/BuildingBlocks/Configuration/Handlers/HandlerScope.cs` — ScopeType (Global/Class/Property), PathPattern, Matches(key). Static factories: Global(), ForClass(sectionPath), ForProperty(fullPath). Implement IEquatable, Equals, GetHashCode, ==, !=.
- [x] T010 [P] Write unit tests for `ConfigurationPath` in `tests/UnitTests/BuildingBlocks/Configuration/ConfigurationPathTests.cs` — factory, FullPath derivation, equality, hash code, ToString, null/empty inputs
- [x] T011 [P] Write unit tests for `HandlerScope` in `tests/UnitTests/BuildingBlocks/Configuration/Handlers/HandlerScopeTests.cs` — Global matches all, ForClass matches prefix, ForProperty matches exact, non-matching keys, equality, factories, null inputs
- [x] T012 Implement `ConfigurationOptions` (MapSection only) in `src/BuildingBlocks/Configuration/Registration/ConfigurationOptions.cs` — MapSection<T>(sectionPath), internal section mappings dictionary. AddHandler deferred to US2.

**Checkpoint**: Foundation ready. Model types tested. ConfigurationOptions supports section mapping. User story implementation can begin.

---

## Phase 3: User Story 1 — Leitura de Configuracao com Fontes Padrao (Priority: P1) MVP

**Goal**: Developers can read typed configuration objects from IConfiguration with automatic path derivation (class + property name). No custom handlers — behavior equivalent to IConfiguration.

**Independent Test**: Create a test ConfigurationManagerBase subclass with section mappings, provide an in-memory IConfiguration, and verify Get<TSection>() returns correctly populated objects including arrays, nullable types, and properties with same name in different classes.

**Spec Coverage**: RF-001, RF-004, RF-005, RF-011, RF-015, RF-016, RF-017, RF-018, RF-019, RF-021. Acceptance scenarios P1-1 through P1-10.

### Implementation for User Story 1

- [x] T013 [US1] Implement `ConfigurationManagerBase` core in `src/BuildingBlocks/Configuration/ConfigurationManagerBase.cs` — protected constructor (IConfiguration, ILogger) with null checks, Initialize() calls ConfigureInternal(options), section mapping registry from ConfigurationOptions, path cache (ConcurrentDictionary)
- [x] T014 [US1] Implement expression-based path derivation in `ConfigurationManagerBase` — extract property name from `Expression<Func<TSection, TProperty>>`, combine with section from mappings, cache in static ConcurrentDictionary keyed by (Type, propertyName)
- [x] T015 [US1] Implement `Get<TSection>()` in `ConfigurationManagerBase` — enumerate properties of TSection via reflection (cached), derive path per property, read from IConfiguration, populate new TSection instance. Support: primitives, string, arrays (string[], int[]), nullable types (int?, string?). Throw InvalidOperationException if section not mapped.
- [x] T016 [US1] Implement `Get<TSection, TProperty>()` in `ConfigurationManagerBase` — derive path from expression, read single value from IConfiguration via pipeline, return typed value
- [x] T017 [US1] Implement basic `ServiceCollectionExtensions.AddBedrockConfiguration<TManager>()` in `src/BuildingBlocks/Configuration/Registration/ServiceCollectionExtensions.cs` — register TManager as Singleton, resolve IConfiguration and ILogger from container
- [x] T018 [US1] Write unit tests for `ConfigurationManagerBase` in `tests/UnitTests/BuildingBlocks/Configuration/ConfigurationManagerBaseTests.cs` — constructor null checks, Initialize calls ConfigureInternal, section mapping registration, unmapped section throws, ILogger received
- [x] T019 [US1] Write unit tests for `Get<TSection>()` in `ConfigurationManagerBaseTests.cs` — read typed object from in-memory IConfiguration, verify all properties populated, arrays resolve correctly, nullable returns null when key missing, non-nullable returns default, empty array (not null), two classes with same property name don't collide (P1-7), path auto-derivation (P1-6)
- [x] T020 [US1] Write unit tests for `Get<TSection, TProperty>()` — expression compilation, path derivation caching (same expression twice uses cache), different property types (string, int, bool, string[], int?), missing key behavior
- [x] T021 [US1] Write unit tests for `ConfigurationOptions.MapSection` and `ServiceCollectionExtensions` in `tests/UnitTests/BuildingBlocks/Configuration/Registration/ConfigurationOptionsTests.cs` and `ServiceCollectionExtensionsTests.cs` — MapSection stores mapping, duplicate section warning/error, DI resolves manager correctly

**Checkpoint**: User Story 1 fully functional. Get<TSection>() reads typed configuration from IConfiguration with auto-derived paths. Arrays, nullable, and collision prevention work. No handlers yet.

---

## Phase 4: User Story 2 — Extensao do Pipeline com Handlers Customizados (Priority: P2)

**Goal**: Developers add custom handlers to Get pipeline via fluent API, with scope targeting (global, class, property) using type-safe expressions. Handlers receive key + value and can transform/replace/pass-through.

**Independent Test**: Register test handlers with different scopes, verify only matching handlers execute for each key, verify handler chain order, verify scoped handlers add to pipeline (don't replace).

**Spec Coverage**: RF-002, RF-003, RF-008, RF-009, RF-012, RF-013, RF-014, RF-020. Acceptance scenarios P2-1 through P2-10.

### Implementation for User Story 2

- [x] T022 [P] [US2] Implement `ConfigurationHandlerBase` abstract class in `src/BuildingBlocks/Configuration/Handlers/ConfigurationHandlerBase.cs` — protected constructor(LoadStrategy), public LoadStrategy property, abstract HandleGet(key, value), abstract HandleSet(key, value)
- [x] T023 [US2] Implement `ConfigurationPipeline` internal sealed class in `src/BuildingBlocks/Configuration/Pipeline/ConfigurationPipeline.cs` — PipelineEntry(handler, scope, position), ordered entries list, ExecuteGet(key, initialValue) iterates entries checking scope match then calling HandleGet, propagates errors with handler context
- [x] T024 [US2] Implement `ConfigurationHandlerBuilder<T>` in `src/BuildingBlocks/Configuration/Registration/ConfigurationHandlerBuilder.cs` — AtPosition(int), WithLoadStrategy(LoadStrategy), ForGet(), ForSet(), ForBoth() (default), ToClass<TClass>() returns ClassScopeBuilder
- [x] T025 [US2] Implement `ClassScopeBuilder<TClass>` in same file as ConfigurationHandlerBuilder — ToProperty<TProp>(Expression) extracts property name and combines with class section path to create HandlerScope.ForProperty. Without ToProperty, creates HandlerScope.ForClass.
- [x] T026 [US2] Extend `ConfigurationOptions` with `AddHandler<T>()` in `src/BuildingBlocks/Configuration/Registration/ConfigurationOptions.cs` — returns ConfigurationHandlerBuilder, stores handler registrations, Build() validates no duplicate positions per pipeline (RF-014), builds ConfigurationPipeline
- [x] T027 [US2] Integrate pipeline into `ConfigurationManagerBase.Get` methods — after reading from IConfiguration, pass value through _getPipeline.ExecuteGet(path, initialValue). Empty pipeline (no handlers) returns initial value unchanged.
- [x] T028 [US2] Extend `ServiceCollectionExtensions` to register handlers from ConfigurationOptions in DI container
- [x] T029 [P] [US2] Write unit tests for `ConfigurationHandlerBase` in `tests/UnitTests/BuildingBlocks/Configuration/Handlers/ConfigurationHandlerBaseTests.cs` — constructor sets LoadStrategy, abstract methods invokable via test subclass
- [x] T030 [US2] Write unit tests for `ConfigurationPipeline` in `tests/UnitTests/BuildingBlocks/Configuration/Pipeline/ConfigurationPipelineTests.cs` — handler execution order, scope matching (global executes for all, class for section prefix, property for exact key), handler skipped when scope doesn't match, error propagation with handler context, empty pipeline passthrough, handler ignores value and returns new one (RF-013)
- [x] T031 [US2] Write unit tests for fluent API in `tests/UnitTests/BuildingBlocks/Configuration/Registration/ConfigurationHandlerBuilderTests.cs` — builder chain (AtPosition, WithLoadStrategy, ToClass, ToProperty), duplicate position rejection (RF-014), ForGet/ForSet/ForBoth pipeline selection, ClassScopeBuilder expression extraction
- [x] T032 [US2] Write integration tests for full pipeline in `ConfigurationManagerBaseTests.cs` — handler registered for exact property executes only for that key (P2-5), handler for section prefix executes for all keys in section (P2-6), global handler executes for all (P2-7), fluent API .ToClass<T>().ToProperty(expr) derives correct scope (P2-8/P2-9), multiple handlers chain in order (P2-2), scoped handlers add to pipeline not replace (clarification #2)

**Checkpoint**: User Stories 1 AND 2 work. Get with handlers transforms values through scoped pipeline. Fluent API with type-safe expressions works.

---

## Phase 5: User Story 3 — Configuracao de LoadStrategy (Priority: P3)

**Goal**: Handlers can be configured with LoadStrategy controlling when they execute: StartupOnly (once at init, fail-fast), LazyStartupOnly (once on first access, retry on failure), AllTime (every access).

**Independent Test**: Create handlers with each strategy, verify StartupOnly caches after init, LazyStartupOnly caches after first access and retries on failure, AllTime executes every time.

**Spec Coverage**: RF-010. Acceptance scenarios P3-1 through P3-5. Edge cases 3, 4.

### Implementation for User Story 3

- [x] T033 [US3] Implement StartupOnly caching in `ConfigurationPipeline` — during pipeline initialization (called from ConfigurationManagerBase.Initialize), pre-execute StartupOnly handlers and cache results. On failure, propagate exception (fail-fast). Cached results used on all subsequent Get calls.
- [x] T034 [US3] Implement LazyStartupOnly with `Lazy<T>` in `ConfigurationPipeline` — wrap handler execution in Lazy<object?> per (handlerIndex, key). LazyThreadSafetyMode.ExecutionAndPublication ensures single execution. Exception NOT cached (retry on next access per spec edge case 4).
- [x] T035 [US3] Implement AllTime execution in `ConfigurationPipeline` — no caching, handler executes on every Get call. Verify this is the default behavior when no caching is applied.
- [x] T036 [US3] Write unit tests for LoadStrategy behaviors in `ConfigurationPipelineTests.cs` — StartupOnly: handler executes once at init, cached on second Get (P3-1), exception during init propagates (P3-5). LazyStartupOnly: handler executes on first Get, cached on second Get (P3-2), concurrent first access only one execution, failure NOT cached - retry works (edge case 4). AllTime: handler executes on every Get (P3-3). Handler-specific options configurable independently (P3-4).

**Checkpoint**: All three LoadStrategy modes work correctly with proper caching and error handling.

---

## Phase 6: User Story 4 — Escrita de Configuracao (Priority: P4)

**Goal**: Developers use Set to write configuration values through the Set pipeline. Handlers can validate, transform, or persist values.

**Independent Test**: Write a value via Set, verify it flows through Set pipeline handlers in order. Verify subsequent Get returns updated value.

**Spec Coverage**: RF-003. Acceptance scenarios P4-1 through P4-3. Edge case 6.

### Implementation for User Story 4

- [x] T037 [US4] Implement `Set<TSection, TProperty>()` in `ConfigurationManagerBase` — derive path from expression, pass value through _setPipeline.ExecuteSet(path, value), store result in memory
- [x] T038 [US4] Implement Set pipeline execution in `ConfigurationPipeline.ExecuteSet` — iterate Set entries checking scope, call HandleSet(key, value), chain results. No Set handlers registered → apply value in memory without error (edge case 6).
- [x] T039 [US4] Write unit tests for Set in `ConfigurationManagerBaseTests.cs` — Set flows through handler chain (P4-1), handler persists to external source (P4-2), Get after Set returns updated value (P4-3), Set with no handlers applies in-memory (edge case 6), ForSet handler only in Set pipeline not Get

**Checkpoint**: All 4 user stories independently functional. Full Get/Set pipeline with handlers, scoping, and LoadStrategy.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Architecture validation, mutation testing, pipeline execution, final quality gates

- [x] T040 Run architecture tests: `dotnet test tests/ArchitectureTests/BuildingBlocks/Configuration/` — verify CodeStyle (CS001-CS003) and Infrastructure (IN001-IN016) rules pass. Fix any violations.
- [x] T041 Run unit tests with coverage: `dotnet test tests/UnitTests/BuildingBlocks/Configuration/ --collect:"XPlat Code Coverage"` — verify 100% line coverage. Add missing tests for uncovered lines.
- [x] T042 Run full pipeline: `./scripts/pipeline.sh` — build, test, coverage, mutation, SonarCloud. Read `artifacts/pending/SUMMARY.txt` and resolve all pending items.
- [x] T043 Verify 100% mutation score: read `artifacts/pending/mutant_*.txt`, kill surviving mutants. Apply `[ExcludeFromCodeCoverage]` + Stryker comments only for genuinely untestable code with pt-BR justification.
- [x] T044 [P] Verify all 21 RFs covered and all 26 acceptance scenarios testable. Cross-reference spec.md against implemented tests.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (T006 build success)
- **US1 (Phase 3)**: Depends on Phase 2 (model types ready)
- **US2 (Phase 4)**: Depends on Phase 3 (ConfigurationManagerBase exists)
- **US3 (Phase 5)**: Depends on Phase 4 (pipeline exists for caching)
- **US4 (Phase 6)**: Depends on Phase 4 (pipeline exists for Set)
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational only — can start immediately after Phase 2
- **US2 (P2)**: Depends on US1 — extends ConfigurationManagerBase with pipeline
- **US3 (P3)**: Depends on US2 — adds caching to existing pipeline
- **US4 (P4)**: Depends on US2 — adds Set execution to existing pipeline
- **US3 and US4**: Can run in parallel (both depend on US2, touch different areas)

### Within Each User Story

- Implementation tasks before their tests (tests validate implementation)
- Core types before dependent types (e.g., HandlerBase before Pipeline)
- Internal types before public API (e.g., Pipeline before ServiceCollectionExtensions)

### Parallel Opportunities

**Phase 1 (Setup)**: T002, T003, T004 can run in parallel (different project types)
**Phase 2 (Foundational)**: T007, T008, T009 in parallel (different files); T010, T011 in parallel (different test files)
**Phase 4 (US2)**: T022 (HandlerBase) in parallel with T024 (Builder) — different files
**Phase 5+6**: US3 and US4 can run in parallel after US2 completes
**Phase 7**: T040 and T044 in parallel (different concerns)

---

## Parallel Example: Phase 2

```bash
# Launch all model types in parallel (different files):
Task: T007 "Implement LoadStrategy enum"
Task: T008 "Implement ConfigurationPath readonly struct"
Task: T009 "Implement HandlerScope readonly struct"

# After models done, launch tests in parallel:
Task: T010 "Write tests for ConfigurationPath"
Task: T011 "Write tests for HandlerScope"
```

## Parallel Example: US3 + US4

```bash
# After US2 completes, launch US3 and US4 in parallel:
# Developer A: US3 (LoadStrategy caching in ConfigurationPipeline)
Task: T033 "Implement StartupOnly caching"
Task: T034 "Implement LazyStartupOnly with Lazy<T>"
Task: T035 "Implement AllTime execution"
Task: T036 "Write tests for LoadStrategy behaviors"

# Developer B: US4 (Set pipeline)
Task: T037 "Implement Set<TSection, TProperty>()"
Task: T038 "Implement Set pipeline in ConfigurationPipeline"
Task: T039 "Write tests for Set operations"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T006)
2. Complete Phase 2: Foundational (T007-T012)
3. Complete Phase 3: US1 — Leitura de Configuracao (T013-T021)
4. **STOP and VALIDATE**: Get<TSection>() works with auto-derived paths, arrays, nullable
5. Run `dotnet test` — all US1 tests pass

### Incremental Delivery

1. Setup + Foundational → Project compiles, model types tested
2. US1 → Get from IConfiguration with typed objects → MVP!
3. US2 → Add handler pipeline with fluent API and scoping
4. US3 + US4 (parallel) → LoadStrategy caching + Set operations
5. Polish → 100% coverage, 100% mutation, architecture rules pass

### Single Developer Strategy

1. Phase 1 → Phase 2 → Phase 3 (US1) → Phase 4 (US2) → Phase 5 (US3) → Phase 6 (US4) → Phase 7
2. Run `dotnet build` + `dotnet test` after each task
3. Run `./scripts/pipeline.sh` once at Phase 7

---

## Notes

- [P] tasks = different files, no shared state dependencies
- [USn] label maps task to user story for traceability
- Tests are REQUIRED (constitution: 100% coverage + 100% mutation)
- Use TestBase, AAA pattern, Shouldly assertions, Moq for mocking
- All logging in pt-BR (LogArrange, LogAct, LogAssert descriptions)
- ConfigurationPipeline is internal — use InternalsVisibleTo for unit tests
- Expression tree caching is critical for performance — test cache hit behavior
- Commit after each task or logical group
