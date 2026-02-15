# Research: PostgreSQL Migrations BuildingBlock

**Branch**: `feature/180-migration-postgresql-building-block` | **Date**: 2026-02-14

## R-001: FluentMigrator Integration Strategy

**Decision**: Use FluentMigrator 8.x with `FluentMigrator.Runner.Postgres` as the migration engine, encapsulated entirely within the BuildingBlock.

**Rationale**:
- FluentMigrator is the user-specified technology choice (FR-003).
- Version 8.x targets .NET 8+ and is compatible with .NET 10.0.
- The `FluentMigrator.Runner` provides `IMigrationRunner` which handles discovery, ordering, execution, and version tracking.
- FluentMigrator natively supports PostgreSQL via `FluentMigrator.Runner.Postgres`.
- Supports `Execute.EmbeddedScript()` for running SQL from embedded resources.
- Supports transactional migrations per migration (FR-008).
- Provides built-in version info table management (FR-009).

**Alternatives considered**:
- **DbUp**: Simpler but lacks annotation-based migration classes and version tracking granularity.
- **EF Core Migrations**: Tied to Entity Framework, which conflicts with Bedrock's DataModel/Mapper architecture.
- **Raw SQL scripts with custom runner**: Maximum control but reinvents FluentMigrator's well-tested orchestration.

## R-002: NuGet Packages Required

**Decision**: Add three FluentMigrator packages to `Directory.Packages.props`:

| Package | Purpose |
|---------|---------|
| `FluentMigrator` | Core library — `Migration` base class, `[Migration]` attribute |
| `FluentMigrator.Runner` | Runner infrastructure — `IMigrationRunner`, `IServiceCollection` extensions |
| `FluentMigrator.Runner.Postgres` | PostgreSQL-specific runner with Npgsql integration |

**Rationale**:
- Central package management is already configured via `Directory.Packages.props`.
- `FluentMigrator.Runner` provides the `AddFluentMigratorCore()` and `ConfigureRunner()` extension methods needed for DI setup.
- `FluentMigrator.Runner.Postgres` adds `AddPostgres()` to the runner configuration.
- These packages are referenced only by the new BuildingBlock project, keeping the dependency isolated.

**Alternatives considered**:
- `FluentMigrator.Extensions.Postgres`: Additional PostgreSQL-specific SQL generation. Not needed since we execute raw SQL scripts, not fluent API schema definitions.

## R-003: Embedded Resources Loading Pattern

**Decision**: SQL scripts are compiled as embedded resources in the bounded context's assembly. The `MigrationManagerBase` receives the assembly to scan and uses `Assembly.GetManifestResourceStream()` to load scripts at runtime.

**Rationale**:
- Eliminates filesystem dependency in CI/CD pipelines (FR-004).
- The `.csproj` of each bounded context includes a glob:
  ```xml
  <EmbeddedResource Include="Migrations\Scripts\**\*.sql" />
  ```
- FluentMigrator's `Execute.EmbeddedScript()` method reads from the assembly's embedded resources natively.
- Resource names follow the pattern: `{RootNamespace}.Migrations.Scripts.{Up|Down}.{FileName}`.

**Alternatives considered**:
- Filesystem-based scripts: Requires deployment of script files alongside the pipeline runner binary. Fragile in containerized environments.
- Content files: Still requires filesystem access. Embedded resources are self-contained.

## R-004: MigrationManagerBase Architecture

**Decision**: `MigrationManagerBase` is an abstract class that uses FluentMigrator's `IMigrationRunner` internally. It configures a `ServiceProvider` per execution, builds the FluentMigrator runner with the bounded context's parameters, and delegates migration orchestration to the runner.

**Rationale**:
- FluentMigrator uses DI (Microsoft.Extensions.DependencyInjection) internally.
- The `MigrationManagerBase` creates a scoped `ServiceProvider` per operation (MigrateUp, MigrateDown, GetStatus) to avoid shared state.
- Abstract methods allow each bounded context to provide: connection string, target schema, assemblies to scan.
- The base class handles: runner construction, logging, error handling, distributed tracing.
- The base class is `IAsyncDisposable` to clean up the internal `ServiceProvider`.

**Alternatives considered**:
- Static configuration: Cannot support multiple bounded contexts with different connection strings.
- Singleton runner: Shared state between BCs would cause isolation issues.

## R-005: SqlScriptAttribute and Migration Base Class

**Decision**: Create a `[SqlScript]` attribute and a `SqlScriptMigrationBase` class. The attribute stores the embedded resource paths for UP and DOWN scripts. The base class reads the attribute via reflection and executes the scripts in its `Up()` and `Down()` methods.

**Rationale**:
- Developers create migration classes that are pure metadata (FR-006):
  ```csharp
  [Migration(202602141200)]
  [SqlScript("Up/V202602141200__create_users.sql",
             "Down/V202602141200__create_users.sql")]
  public class CreateUsersTable : SqlScriptMigrationBase { }
  ```
- `SqlScriptMigrationBase` inherits from FluentMigrator's `Migration` class.
- In `Up()`: reads the UP script path from `[SqlScript]`, loads embedded resource, calls `Execute.EmbeddedScript()`.
- In `Down()`: same for the DOWN script path.
- Validation: The base class validates script existence on construction (FR-012).

**Alternatives considered**:
- Fluent API in migration classes: Defeats the purpose of SQL-first approach. Developers want raw SQL control.
- Convention-over-configuration (auto-detect scripts by class name): Less explicit, harder to debug mismatches.

## R-006: Concurrency Locking Strategy

**Decision**: Use FluentMigrator's built-in advisory lock support for PostgreSQL.

**Rationale**:
- FluentMigrator supports `WithGlobalConnectionString()` and advisory locking via `pg_advisory_lock` to prevent concurrent migrations (FR-014).
- The runner configuration provides `ConfigureRunner(rb => rb.AddPostgres().WithGlobalConnectionString(...))` which uses PostgreSQL advisory locks.
- This is a battle-tested approach used by production systems.

**Alternatives considered**:
- Custom distributed lock table: More code, already solved by FluentMigrator.
- File-based locks: Not viable in containerized CI/CD environments.

## R-007: Migration Status Query Implementation

**Decision**: Query FluentMigrator's version info table directly via `IMigrationInformationLoader` and `IVersionLoader` to build the `MigrationStatus` result.

**Rationale**:
- FluentMigrator stores applied migrations in a version info table (default: `VersionInfo`).
- `IVersionLoader.GetVersionInfo()` returns all applied versions.
- `IMigrationInformationLoader.LoadMigrations()` returns all discovered migration classes.
- The difference between loaded and applied gives pending migrations (FR-011).

**Alternatives considered**:
- Direct SQL query to VersionInfo: Bypasses FluentMigrator abstractions, less maintainable.
- Custom tracking table: Duplicates what FluentMigrator already provides.

## R-008: Project Position in Dependency Graph

**Decision**: The new BuildingBlock `Persistence.PostgreSql.Migrations` depends on:
- `Bedrock.BuildingBlocks.Core` (ExecutionContext, Id, TenantInfo)
- `Bedrock.BuildingBlocks.Observability` (distributed tracing logging)
- `FluentMigrator` + `FluentMigrator.Runner` + `FluentMigrator.Runner.Postgres`

It does NOT depend on:
- `Persistence.PostgreSql` (no data models, mappers, or repositories needed)
- `Domain` or `Domain.Entities` (migrations are infrastructure-only)
- `Data` (no domain repository abstractions needed)

**Rationale**:
- Migrations are a pipeline-only concern (FR-015). They don't interact with domain entities or repositories.
- The only Bedrock dependencies are `Core` (for ExecutionContext) and `Observability` (for logging with distributed tracing).
- This keeps the dependency graph minimal and avoids coupling migrations to the data access layer.

**Alternatives considered**:
- Depending on `Persistence.PostgreSql`: Would create tight coupling. Migrations don't use DataModels or Mappers.
- Depending on `Persistence.Abstractions`: No abstractions are needed. Migrations use their own connection string directly.
