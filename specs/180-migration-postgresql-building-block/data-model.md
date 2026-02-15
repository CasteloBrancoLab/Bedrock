# Data Model: PostgreSQL Migrations BuildingBlock

**Branch**: `feature/180-migration-postgresql-building-block` | **Date**: 2026-02-14

## Overview

This BuildingBlock does NOT define domain entities. It is purely infrastructure for schema evolution. The "entities" below are internal types used by the BuildingBlock to represent migration metadata and status.

## Types

### SqlScriptAttribute

**Purpose**: Custom attribute that links a migration class to its UP and DOWN SQL script embedded resource paths.

| Property | Type | Description |
|----------|------|-------------|
| UpScriptResourceName | `string` | Relative path to the UP script embedded resource (e.g., `Up/V202602141200__create_users.sql`) |
| DownScriptResourceName | `string?` | Relative path to the DOWN script embedded resource. Nullable — empty DOWN scripts represent irreversible migrations. |

**Constraints**:
- `UpScriptResourceName` is required (cannot be null or empty).
- `DownScriptResourceName` is optional (nullable for irreversible migrations).
- Attribute targets: `Class` only.
- Not inheritable.

### MigrationInfo

**Purpose**: Represents the record of a single migration (applied or pending).

| Property | Type | Description |
|----------|------|-------------|
| Version | `long` | Migration version number (timestamp format YYYYMMDDHHmm) |
| Description | `string` | Human-readable description extracted from script name |
| AppliedOn | `DateTimeOffset?` | Timestamp when migration was applied. Null if pending. |

**Constraints**:
- `readonly record struct` (zero allocation, value semantics).
- `Version` must be positive.
- `Description` is derived from the script filename (snake_case to readable).

### MigrationStatus

**Purpose**: Aggregated status of all migrations for a bounded context.

| Property | Type | Description |
|----------|------|-------------|
| AppliedMigrations | `IReadOnlyList<MigrationInfo>` | List of migrations already applied, ordered by version ascending |
| PendingMigrations | `IReadOnlyList<MigrationInfo>` | List of migrations not yet applied, ordered by version ascending |
| LastAppliedVersion | `long?` | Version of the most recently applied migration, or null if none |
| HasPendingMigrations | `bool` | True if there are unapplied migrations |

**Constraints**:
- `sealed class` (contains reference-type collections).
- Collections are immutable (`IReadOnlyList<T>`).

### MigrationManagerBase (abstract)

**Purpose**: Public API for migration operations. Each bounded context creates a concrete implementation.

**Abstract members** (to be implemented by BC):

| Member | Type | Description |
|--------|------|-------------|
| ConnectionString | `string` (abstract property) | PostgreSQL connection string for the target database |
| TargetSchema | `string` (abstract property) | Schema where migrations are applied (e.g., `public`) |
| MigrationAssembly | `Assembly` (abstract property) | Assembly containing migration classes and embedded SQL scripts |

**Public API**:

| Method | Signature | Description |
|--------|-----------|-------------|
| MigrateUpAsync | `Task MigrateUpAsync(ExecutionContext, CancellationToken)` | Apply all pending migrations in version order |
| MigrateDownAsync | `Task MigrateDownAsync(ExecutionContext, long targetVersion, CancellationToken)` | Rollback to the specified target version |
| GetStatusAsync | `Task<MigrationStatus> GetStatusAsync(ExecutionContext, CancellationToken)` | Query applied and pending migrations without changes |

**Constructor**: Receives `ILogger<MigrationManagerBase>`.

**Constraints**:
- `ExecutionContext` is the first parameter of all public methods (BB-IV).
- `CancellationToken` is the last parameter of all async methods (BB-IV).
- All operations use distributed tracing logging via `LogForDistributedTracing` extension methods.

### SqlScriptMigrationBase (abstract)

**Purpose**: Base class for individual migration classes. Inherits from FluentMigrator's `Migration`. Reads `[SqlScript]` attribute and executes embedded SQL scripts.

| Method | Description |
|--------|-------------|
| Up() | Reads UP script path from `[SqlScript]`, loads embedded resource from assembly, executes via `Execute.EmbeddedScript()` |
| Down() | Reads DOWN script path from `[SqlScript]`, loads embedded resource from assembly, executes via `Execute.EmbeddedScript()`. If DOWN path is null, throws `InvalidOperationException`. |

**Constraints**:
- Inherits from `FluentMigrator.Migration`.
- Sealed by convention (each migration class is final).
- Validates script existence in constructor or `Up()`/`Down()` (FR-012).

## Database Schema (managed by FluentMigrator)

FluentMigrator automatically creates and manages its version tracking table:

### VersionInfo (auto-created by FluentMigrator)

| Column | Type | Description |
|--------|------|-------------|
| Version | `bigint` | Migration version number |
| AppliedOn | `timestamp with time zone` | When the migration was applied |
| Description | `text` | Migration description |

This table is NOT managed by Bedrock code — FluentMigrator handles it entirely.

## Relationships

```
MigrationManagerBase (abstract)
    │
    ├── creates → FluentMigrator Runner (internal)
    │                 │
    │                 └── discovers → SqlScriptMigrationBase subclasses
    │                                     │
    │                                     └── reads → [SqlScript] attribute
    │                                                     │
    │                                                     └── references → Embedded SQL scripts
    │
    └── returns → MigrationStatus
                      │
                      └── contains → MigrationInfo[]
```
