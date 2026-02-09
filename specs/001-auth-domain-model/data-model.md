# Data Model: Auth Domain Model

**Feature**: 001-auth-domain-model
**Date**: 2026-02-09

## Entities

### User (Aggregate Root)

**Project**: `ShopDemo.Auth.Domain.Entities`
**Archetype**: SimpleAggregateRoot (sealed)
**Implements**: `EntityBase<User>`, `IAggregateRoot`, `IUser`

| Field | Type | Required | Validation | Notes |
|-------|------|----------|------------|-------|
| EntityInfo | EntityInfo | Yes | Managed by base class | Id, TenantInfo, audit, version |
| Username | string | Yes | MinLength: 1, MaxLength: 255 | Unique. Default = Email (lowercase) |
| Email | EmailAddress | Yes | RFC 5321 ASCII, lowercase | Unique. Value object do Core |
| PasswordHash | PasswordHash | Yes | Non-empty, max 128 bytes | Value object opaco (byte[]) |
| Status | UserStatus | Yes | Valid enum value | Default: Active on RegisterNew |

**Factory Methods**:

| Method | Returns | Validates | Use Case |
|--------|---------|-----------|----------|
| `RegisterNew(ExecutionContext, RegisterNewInput)` | `User?` | Yes (all fields) | Criar novo usuário |
| `CreateFromExistingInfo(CreateFromExistingInfoInput)` | `User` | No | Reconstituir de dados persistidos |

**Change Methods (Clone-Modify-Return)**:

| Method | Returns | Input | Notes |
|--------|---------|-------|-------|
| `ChangeStatus(ExecutionContext, ChangeStatusInput)` | `User?` | NewStatus | Valida transição permitida |
| `ChangeUsername(ExecutionContext, ChangeUsernameInput)` | `User?` | NewUsername | Valida min/max length |
| `ChangePasswordHash(ExecutionContext, ChangePasswordHashInput)` | `User?` | NewPasswordHash | Valida non-empty |

**Static Validation Methods**:

| Method | Parameters | Returns |
|--------|------------|---------|
| `ValidateUsername(ExecutionContext, string?)` | context, username | bool |
| `ValidateEmail(ExecutionContext, EmailAddress?)` | context, email | bool |
| `ValidatePasswordHash(ExecutionContext, PasswordHash?)` | context, hash | bool |
| `ValidateStatusTransition(ExecutionContext, UserStatus, UserStatus)` | context, from, to | bool |

---

### UserMetadata (Static Configuration)

**Project**: `ShopDemo.Auth.Domain.Entities`

| Property | Type | Default | Changeable |
|----------|------|---------|------------|
| UsernameIsRequired | bool | true | Yes |
| UsernameMinLength | int | 1 | Yes |
| UsernameMaxLength | int | 255 | Yes |
| EmailIsRequired | bool | true | Yes |
| PasswordHashIsRequired | bool | true | Yes |
| PasswordHashMaxLength | int | 128 | Yes |
| StatusIsRequired | bool | true | Yes |

---

## Value Objects

### PasswordHash (New — Domain.Entities)

**Project**: `ShopDemo.Auth.Domain.Entities` (ou `Bedrock.BuildingBlocks.Core` se for genérico o suficiente)
**Type**: `readonly struct`

| Member | Type | Description |
|--------|------|-------------|
| Value | ReadOnlyMemory\<byte\> | Hash opaco (immutable view of byte[]) |
| IsEmpty | bool | True se Value é vazio |
| Length | int | Comprimento em bytes |
| CreateNew(byte[]) | static factory | Cria instância (cópia defensiva) |
| Equals(PasswordHash) | bool | Comparação em tempo constante |
| ToString() | string | Sempre retorna "[REDACTED]" |
| GetHashCode() | int | Hash do conteúdo |
| == / != | operators | Delegam para Equals |

**Key Design Decisions**:
- `ReadOnlyMemory<byte>` em vez de `byte[]` para garantir imutabilidade
- `CryptographicOperations.FixedTimeEquals` para comparação segura
- `ToString()` nunca expõe o conteúdo (segurança)
- Cópia defensiva no factory method (isolamento)

---

### EmailAddress (Existing — Core)

**Project**: `Bedrock.BuildingBlocks.Core` (já existente)
**Reuse**: O value object `EmailAddress` já existe no Core. A validação de formato RFC 5321 é feita nos métodos `Validate*` da entidade User, não no value object (padrão Bedrock: VO é container, entidade valida).

---

## Enumerations

### UserStatus

**Project**: `ShopDemo.Auth.Domain.Entities`

| Value | Name | Description |
|-------|------|-------------|
| 1 | Active | Pode autenticar normalmente |
| 2 | Suspended | Temporariamente impedido de autenticar |
| 3 | Blocked | Permanentemente impedido até intervenção administrativa |

**State Machine**:

```
          ┌──────────┐
    ┌────→│  Active  │←────┐
    │     └──────────┘     │
    │       │      │       │
    │       ▼      ▼       │
┌──────────┐  ┌──────────┐
│ Suspended│  │ Blocked  │
└──────────┘  └──────────┘
    │              │
    └──────────────┘
         (Suspended → Blocked ✓)
         (Blocked → Suspended ✗)
```

**Valid Transitions**:
- Active → Suspended
- Active → Blocked
- Suspended → Active
- Suspended → Blocked
- Blocked → Active

**Invalid Transitions**:
- Blocked → Suspended (must go through Active first)

---

## Input Objects

### RegisterNewInput

```
readonly record struct RegisterNewInput(
    EmailAddress Email,
    PasswordHash PasswordHash
)
```

**Note**: Username is derived from Email (business rule). The factory method `RegisterNew` sets `Username = Email.Value.ToLowerInvariant()`.

### CreateFromExistingInfoInput

```
readonly record struct CreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string Username,
    EmailAddress Email,
    PasswordHash PasswordHash,
    UserStatus Status
)
```

### ChangeStatusInput

```
readonly record struct ChangeStatusInput(
    UserStatus NewStatus
)
```

### ChangeUsernameInput

```
readonly record struct ChangeUsernameInput(
    string NewUsername
)
```

### ChangePasswordHashInput

```
readonly record struct ChangePasswordHashInput(
    PasswordHash NewPasswordHash
)
```

---

## Security Building Block

### IPasswordHasher (Interface)

**Project**: `Bedrock.BuildingBlocks.Security`

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| HashPassword | ExecutionContext, string password | PasswordHashResult | Gera hash com Argon2id + pepper ativo |
| VerifyPassword | ExecutionContext, string password, byte[] storedHash | PasswordVerificationResult | Verifica senha contra hash armazenado |
| NeedsRehash | byte[] storedHash | bool | True se pepper version do hash não é a mais recente |

### PasswordHashResult

```
readonly record struct PasswordHashResult(
    byte[] Hash,
    int PepperVersion
)
```

### PasswordVerificationResult

```
readonly record struct PasswordVerificationResult(
    bool IsValid,
    bool NeedsRehash
)
```

**Note**: `NeedsRehash = true` quando a senha é válida mas o hash foi gerado com uma versão anterior do pepper. O caller deve re-hash e atualizar a entidade.

### PasswordPolicy (Static Validation)

| Method | Parameters | Returns |
|--------|------------|---------|
| ValidatePassword | ExecutionContext, string password | bool |

### PasswordPolicyMetadata

| Property | Type | Default |
|----------|------|---------|
| MinLength | int | 12 |
| MaxLength | int | 128 |
| AllowSpaces | bool | true |

### PepperConfiguration

| Property | Type | Description |
|----------|------|-------------|
| ActivePepperVersion | int | Versão atual para novos hashes |
| Peppers | IReadOnlyDictionary\<int, byte[]\> | Todas as versões ativas (version → key) |

---

## Interfaces

### IUser

**Project**: `ShopDemo.Auth.Domain.Entities` (ou `ShopDemo.Core.Entities` se compartilhado)

```
interface IUser : IEntity
{
    string Username { get; }
    EmailAddress Email { get; }
    PasswordHash PasswordHash { get; }
    UserStatus Status { get; }
}
```

### IUserRepository

**Project**: `ShopDemo.Auth.Domain`

```
interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(ExecutionContext, EmailAddress, CancellationToken)
    Task<User?> GetByUsernameAsync(ExecutionContext, string, CancellationToken)
    Task<bool> ExistsByEmailAsync(ExecutionContext, EmailAddress, CancellationToken)
    Task<bool> ExistsByUsernameAsync(ExecutionContext, string, CancellationToken)
}
```

**Note**: Base `IRepository<User>` already provides `GetByIdAsync`, `ExistsAsync`, `RegisterNewAsync`, `EnumerateAllAsync`, `EnumerateModifiedSinceAsync`.

### IAuthenticationService

**Project**: `ShopDemo.Auth.Domain`

```
interface IAuthenticationService
{
    Task<User?> RegisterUserAsync(ExecutionContext, string email, string password, CancellationToken)
    Task<User?> VerifyCredentialsAsync(ExecutionContext, string email, string password, CancellationToken)
}
```

---

## Dependency Graph

```
Bedrock.BuildingBlocks.Core (existing)
  ↑
Bedrock.BuildingBlocks.Security (NEW)
  ↑                          ↑
  │                          │
ShopDemo.Auth.Domain.Entities  ← (only depends on Core, NOT Security)
  ↑
ShopDemo.Auth.Domain ← (depends on Domain.Entities + Core + Security + BB.Domain)
```
