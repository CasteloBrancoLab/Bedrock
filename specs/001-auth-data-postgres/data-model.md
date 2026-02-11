# Data Model: Auth User Data Access Layer (PostgreSQL)

**Feature Branch**: `001-auth-data-postgres`
**Date**: 2026-02-11

## Entities

### UserDataModel (Persistence Shape)

Extends `DataModelBase` with User-specific columns.

| Property | Type | PostgreSQL Column | PostgreSQL Type | Nullable | Notes |
|----------|------|-------------------|-----------------|----------|-------|
| Id | `Guid` | id | uuid | No | PK (inherited from DataModelBase) |
| TenantCode | `Guid` | tenant_code | uuid | No | Multi-tenancy (inherited) |
| CreatedBy | `string` | created_by | varchar | No | Audit (inherited) |
| CreatedAt | `DateTimeOffset` | created_at | timestamptz | No | Audit (inherited) |
| LastChangedBy | `string?` | last_changed_by | varchar | Yes | Audit (inherited) |
| LastChangedAt | `DateTimeOffset?` | last_changed_at | timestamptz | Yes | Audit (inherited) |
| LastChangedExecutionOrigin | `string?` | last_changed_execution_origin | varchar | Yes | Audit (inherited) |
| LastChangedCorrelationId | `Guid?` | last_changed_correlation_id | uuid | Yes | Audit (inherited) |
| LastChangedBusinessOperationCode | `string?` | last_changed_business_operation_code | varchar | Yes | Audit (inherited) |
| EntityVersion | `long` | entity_version | bigint | No | Optimistic concurrency (inherited) |
| **Username** | `string` | username | varchar | No | User-specific |
| **Email** | `string` | email | varchar | No | User-specific |
| **PasswordHash** | `byte[]` | password_hash | bytea | No | User-specific (raw binary) |
| **Status** | `short` | status | smallint | No | User-specific (enum cast) |

### Table Definition

```
Table: public.auth_users
```

### Relationships

- **User (Domain Entity)** ↔ **UserDataModel**: Bidirectional mapping via factories.
  - `UserDataModelFactory.Create(User)` → `UserDataModel` (entity → persistence)
  - `UserFactory.Create(UserDataModel)` → `User` (persistence → entity)
  - `UserDataModelAdapter.Adapt(UserDataModel, User)` → `UserDataModel` (update)

### State Transitions

No state transitions at the persistence layer. The domain layer manages `UserStatus` transitions (Active ↔ Suspended ↔ Blocked). The persistence layer stores the current status value as a smallint.

## Factory Mapping Details

### UserDataModelFactory (Domain Entity → DataModel)

```
User.EntityInfo → DataModelBase fields (via DataModelBaseFactory)
User.Username   → UserDataModel.Username
User.Email.Value → UserDataModel.Email
User.PasswordHash.Value.ToArray() → UserDataModel.PasswordHash
(short)User.Status → UserDataModel.Status
```

### UserFactory (DataModel → Domain Entity)

```
DataModelBase fields → EntityInfo (Id, TenantInfo, audit fields, RegistryVersion)
UserDataModel.Username → CreateFromExistingInfoInput.Username
UserDataModel.Email → EmailAddress.CreateNew(email)
UserDataModel.PasswordHash → PasswordHash.CreateNew(hash)
(UserStatus)UserDataModel.Status → CreateFromExistingInfoInput.Status
→ User.CreateFromExistingInfo(input)
```

### UserDataModelAdapter (Update)

```
DataModelBaseAdapter.Adapt(dataModel, entity) → base fields
User.Username → UserDataModel.Username
User.Email.Value → UserDataModel.Email
User.PasswordHash.Value.ToArray() → UserDataModel.PasswordHash
(short)User.Status → UserDataModel.Status
```

## Custom Query Methods

Beyond base CRUD, the following custom queries are required:

| Method | SQL Pattern | Parameters |
|--------|------------|------------|
| `GetByEmailAsync` | `SELECT * FROM auth_users WHERE email = @email AND tenant_code = @tenant LIMIT 1` | email (varchar), tenant_code (uuid) |
| `GetByUsernameAsync` | `SELECT * FROM auth_users WHERE username = @username AND tenant_code = @tenant LIMIT 1` | username (varchar), tenant_code (uuid) |
| `ExistsByEmailAsync` | `SELECT EXISTS(SELECT 1 FROM auth_users WHERE email = @email AND tenant_code = @tenant)` | email (varchar), tenant_code (uuid) |
| `ExistsByUsernameAsync` | `SELECT EXISTS(SELECT 1 FROM auth_users WHERE username = @username AND tenant_code = @tenant)` | username (varchar), tenant_code (uuid) |

All custom queries enforce tenant isolation via `tenant_code` in WHERE clause.
