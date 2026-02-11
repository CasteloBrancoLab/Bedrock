# Research: Auth User Data Access Layer (PostgreSQL)

**Feature Branch**: `001-auth-data-postgres`
**Date**: 2026-02-11

## Research Tasks

### R1: Template Pattern Mapping

**Decision**: Follow `src/templates/Infra.Data.PostgreSql/` pattern exactly, adapted to ShopDemo naming.

**Rationale**: Constitution BB-XI mandates templates as normative reference. The template provides a complete, tested pattern for all persistence artifacts. Deviation would contradict the constitution.

**Mapping**:

| Template Artifact | ShopDemo Auth Equivalent |
|-------------------|--------------------------|
| `Templates.Infra.Data.PostgreSql` namespace | `ShopDemo.Auth.Infra.Persistence` namespace |
| `SimpleAggregateRootDataModel` | `UserDataModel` |
| `SimpleAggregateRootDataModelMapper` | `UserDataModelMapper` |
| `SimpleAggregateRootDataModelFactory` | `UserDataModelFactory` |
| `SimpleAggregateRootFactory` | `UserFactory` |
| `SimpleAggregateRootDataModelAdapter` | `UserDataModelAdapter` |
| `ISimpleAggregateRootDataModelRepository` | `IUserDataModelRepository` |
| `SimpleAggregateRootDataModelRepository` | `UserDataModelRepository` |
| `ISimpleAggregateRootPostgreSqlRepository` | `IUserPostgreSqlRepository` |
| `SimpleAggregateRootPostgreSqlRepository` | `UserPostgreSqlRepository` |
| `ITemplatesPostgreSqlConnection` | `IAuthPostgreSqlConnection` |
| `TemplatesPostgreSqlConnection` | `AuthPostgreSqlConnection` |
| `ITemplatesPostgreSqlUnitOfWork` | `IAuthPostgreSqlUnitOfWork` |
| `TemplatesPostgreSqlUnitOfWork` | `AuthPostgreSqlUnitOfWork` |

**Alternatives considered**: EF Core — rejected because the Bedrock framework uses raw Npgsql with DataModelMapperBase for zero-allocation SQL generation.

### R2: PasswordHash Storage Strategy

**Decision**: Store password hash as `bytea` (PostgreSQL binary) using `NpgsqlDbType.Bytea`.

**Rationale**: The `PasswordHash` value object wraps `ReadOnlyMemory<byte>`. Storing as binary avoids encoding/decoding overhead and preserves the raw hash bytes exactly as produced by Argon2id. FR-011 mandates no interpretation at the persistence layer.

**Mapping**:
- Domain: `PasswordHash.Value` → `ReadOnlyMemory<byte>`
- DataModel: `byte[] PasswordHash` property
- PostgreSQL: `bytea` column
- Npgsql: `NpgsqlDbType.Bytea`

**Alternatives considered**: Base64 text — rejected because it adds encoding overhead and increases storage size by ~33%.

### R3: UserStatus Storage Strategy

**Decision**: Store as `smallint` using `NpgsqlDbType.Smallint`, cast to/from `byte`.

**Rationale**: `UserStatus` is `enum UserStatus : byte` with values 1, 2, 3. Storing as smallint (2 bytes) is the standard PostgreSQL mapping for byte-backed enums. The mapper writes `(short)model.Status` and the factory reads `(UserStatus)dataModel.Status`.

**Alternatives considered**: PostgreSQL enum type — rejected because it requires DDL management and provides no benefit for a small, stable enum.

### R4: Custom Query Methods (GetByEmail, GetByUsername, ExistsByEmail, ExistsByUsername)

**Decision**: Implement custom query methods in `UserDataModelRepository` by overriding or adding new methods that use the mapper's SQL generation capabilities. The custom methods build SQL with WHERE clauses for email/username + tenant_code.

**Rationale**: The base `DataModelRepositoryBase` provides CRUD by ID only. The `IUserRepository` contract requires email/username-based lookups. These custom queries must enforce tenant isolation (FR-007) by including `tenant_code` in WHERE clauses.

**Pattern**: The `UserDataModelRepository` extends `DataModelRepositoryBase<UserDataModel>` and adds custom methods:
- `GetByEmailAsync` → `SELECT ... WHERE email = @email AND tenant_code = @tenant`
- `GetByUsernameAsync` → `SELECT ... WHERE username = @username AND tenant_code = @tenant`
- `ExistsByEmailAsync` → `SELECT EXISTS(SELECT 1 FROM ... WHERE email = @email AND tenant_code = @tenant)`
- `ExistsByUsernameAsync` → `SELECT EXISTS(SELECT 1 FROM ... WHERE username = @username AND tenant_code = @tenant)`

These are exposed on `IUserDataModelRepository` and consumed by `UserPostgreSqlRepository` which transforms DataModels to domain entities.

**Alternatives considered**: LINQ-based queries — not applicable (no EF Core).

### R5: Table and Column Naming

**Decision**: Table `auth_users` in `public` schema. Columns in snake_case per template convention.

**Rationale**: Template uses `public` schema with snake_case table names (e.g., `simple_aggregate_roots`). The `auth_` prefix disambiguates from potential future user-related tables in other contexts.

**Column mapping**:

| DataModel Property | PostgreSQL Column | NpgsqlDbType |
|--------------------|-------------------|--------------|
| (inherited) Id | id | Uuid |
| (inherited) TenantCode | tenant_code | Uuid |
| (inherited) CreatedBy | created_by | Varchar |
| (inherited) CreatedAt | created_at | TimestampTz |
| (inherited) LastChangedBy | last_changed_by | Varchar |
| (inherited) LastChangedAt | last_changed_at | TimestampTz |
| (inherited) LastChangedExecutionOrigin | last_changed_execution_origin | Varchar |
| (inherited) LastChangedCorrelationId | last_changed_correlation_id | Uuid |
| (inherited) LastChangedBusinessOperationCode | last_changed_business_operation_code | Varchar |
| (inherited) EntityVersion | entity_version | Bigint |
| Username | username | Varchar |
| Email | email | Varchar |
| PasswordHash | password_hash | Bytea |
| Status | status | Smallint |

### R6: Connection String Configuration Key

**Decision**: `ConnectionStrings:AuthPostgreSql`

**Rationale**: Follows template pattern (`ConnectionStrings:TemplatesPostgreSql`) with domain-specific prefix. Allows independent database configuration per bounded context.

### R7: Infra.Persistence .csproj Dependencies

**Decision**: Add `Bedrock.BuildingBlocks.Observability` project reference to the existing `.csproj`.

**Rationale**: The existing `ShopDemo.Auth.Infra.Persistence.csproj` references `Infra.Data`, `Domain.Entities`, and `Persistence.PostgreSql`. The template also includes `Core` and `Observability` references. Since `Persistence.PostgreSql` already transitively brings `Core`, only `Observability` needs to be added (required by `DataModelRepositoryBase` for logging).
