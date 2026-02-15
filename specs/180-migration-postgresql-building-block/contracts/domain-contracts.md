# Domain Contracts: PostgreSQL Migrations BuildingBlock

**Branch**: `feature/180-migration-postgresql-building-block` | **Date**: 2026-02-14

## Overview

This BuildingBlock has no REST/GraphQL API contracts. It is a library consumed programmatically by bounded context migration runners (pipeline-only, not deployed as a service). The contracts below define the public C# API surface.

## Public API Contract: MigrationManagerBase

### Namespace
`Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations`

### Abstract Members (implemented by BC)

```csharp
// Connection string for the target PostgreSQL database
protected abstract string ConnectionString { get; }

// Schema where migrations are applied (e.g., "public")
protected abstract string TargetSchema { get; }

// Assembly containing migration classes and embedded SQL scripts
protected abstract Assembly MigrationAssembly { get; }
```

### Public Methods

```csharp
// Apply all pending migrations in ascending version order.
// Logs each migration applied via distributed tracing.
// Throws if any script is missing or invalid.
public Task MigrateUpAsync(
    ExecutionContext executionContext,
    CancellationToken cancellationToken = default);

// Rollback migrations to the specified target version (exclusive).
// Executes DOWN scripts in descending version order.
// Throws InvalidOperationException if a DOWN script is missing.
public Task MigrateDownAsync(
    ExecutionContext executionContext,
    long targetVersion,
    CancellationToken cancellationToken = default);

// Query the current migration status without making changes.
// Returns applied and pending migrations.
public Task<MigrationStatus> GetStatusAsync(
    ExecutionContext executionContext,
    CancellationToken cancellationToken = default);
```

## Public API Contract: SqlScriptAttribute

### Namespace
`Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes`

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class SqlScriptAttribute : Attribute
{
    public string UpScriptResourceName { get; }
    public string? DownScriptResourceName { get; }

    public SqlScriptAttribute(
        string upScriptResourceName,
        string? downScriptResourceName = null);
}
```

## Public API Contract: SqlScriptMigrationBase

### Namespace
`Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations`

```csharp
// Base class for SQL-script-driven migrations.
// Developers inherit from this class and decorate with
// [Migration(version)] and [SqlScript("Up/...", "Down/...")].
// The class body is empty — all logic is in the base.
public abstract class SqlScriptMigrationBase : FluentMigrator.Migration
{
    public override void Up();   // Executes UP embedded script
    public override void Down(); // Executes DOWN embedded script
}
```

## Public API Contract: MigrationInfo

### Namespace
`Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Models`

```csharp
public readonly record struct MigrationInfo(
    long Version,
    string Description,
    DateTimeOffset? AppliedOn);
```

## Public API Contract: MigrationStatus

### Namespace
`Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Models`

```csharp
public sealed class MigrationStatus
{
    public IReadOnlyList<MigrationInfo> AppliedMigrations { get; }
    public IReadOnlyList<MigrationInfo> PendingMigrations { get; }
    public long? LastAppliedVersion { get; }
    public bool HasPendingMigrations { get; }
}
```

## Usage Contract (Developer in Bounded Context)

### 1. Create migration scripts

```
MyBoundedContext.Infra.Data.PostgreSql/
└── Migrations/
    └── Scripts/
        ├── Up/
        │   └── V202602141200__create_users_table.sql
        └── Down/
            └── V202602141200__create_users_table.sql
```

### 2. Embed scripts in .csproj

```xml
<ItemGroup>
  <EmbeddedResource Include="Migrations\Scripts\**\*.sql" />
</ItemGroup>
```

### 3. Create migration class

```csharp
[Migration(202602141200)]
[SqlScript("Up/V202602141200__create_users_table.sql",
           "Down/V202602141200__create_users_table.sql")]
public sealed class V202602141200_CreateUsersTable : SqlScriptMigrationBase { }
```

### 4. Create MigrationManager for the bounded context

```csharp
public sealed class AuthMigrationManager : MigrationManagerBase
{
    protected override string ConnectionString => _connectionString;
    protected override string TargetSchema => "public";
    protected override Assembly MigrationAssembly => typeof(AuthMigrationManager).Assembly;

    private readonly string _connectionString;

    public AuthMigrationManager(
        ILogger<AuthMigrationManager> logger,
        string connectionString)
        : base(logger)
    {
        _connectionString = connectionString;
    }
}
```

### 5. Execute in pipeline

```csharp
var executionContext = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),
    tenantInfo: TenantInfo.Create(Guid.Empty),
    executionUser: "migration-pipeline",
    executionOrigin: "CI/CD",
    businessOperationCode: "MIGRATION_UP",
    minimumMessageType: MessageType.Information,
    timeProvider: TimeProvider.System);

var manager = new AuthMigrationManager(logger, connectionString);
await manager.MigrateUpAsync(executionContext, CancellationToken.None);
```
