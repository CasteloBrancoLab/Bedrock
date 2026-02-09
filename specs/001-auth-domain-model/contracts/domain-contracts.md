# Domain Contracts: Auth Domain Model

**Feature**: 001-auth-domain-model
**Date**: 2026-02-09

## Note

Este feature é um **domain model** — não expõe APIs REST/GraphQL diretamente.
Os contratos abaixo definem as interfaces públicas das camadas de domínio que serão consumidas
pelas camadas superiores (Application, API) em issues futuras.

---

## 1. User Entity — Public API

### Factory Methods

```csharp
// Criar novo usuário (valida todos os campos)
public static User? RegisterNew(
    ExecutionContext executionContext,
    RegisterNewInput input);

// Reconstituir de dados persistidos (sem validação)
public static User CreateFromExistingInfo(
    CreateFromExistingInfoInput input);
```

### Change Methods (Clone-Modify-Return)

```csharp
// Alterar status (valida transição)
public User? ChangeStatus(
    ExecutionContext executionContext,
    ChangeStatusInput input);

// Alterar username
public User? ChangeUsername(
    ExecutionContext executionContext,
    ChangeUsernameInput input);

// Alterar hash de senha (re-hash após rotação de pepper)
public User? ChangePasswordHash(
    ExecutionContext executionContext,
    ChangePasswordHashInput input);
```

### Static Validation Methods

```csharp
public static bool ValidateUsername(
    ExecutionContext executionContext, string? username);

public static bool ValidateEmail(
    ExecutionContext executionContext, EmailAddress? email);

public static bool ValidatePasswordHash(
    ExecutionContext executionContext, PasswordHash? passwordHash);

public static bool ValidateStatusTransition(
    ExecutionContext executionContext, UserStatus from, UserStatus to);
```

### Properties (Read-Only)

```csharp
public EntityInfo EntityInfo { get; }     // Id, Tenant, Audit, Version
public string Username { get; }           // Unique
public EmailAddress Email { get; }        // Unique, RFC 5321, lowercase
public PasswordHash PasswordHash { get; } // Opaque byte array
public UserStatus Status { get; }         // Active, Suspended, Blocked
```

---

## 2. IUserRepository — Contract

```csharp
public interface IUserRepository : IRepository<User>
{
    // Buscar por email (autenticação)
    Task<User?> GetByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken);

    // Buscar por username
    Task<User?> GetByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);

    // Verificar existência por email (anti-duplicata)
    Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken);

    // Verificar existência por username (anti-duplicata)
    Task<bool> ExistsByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);
}
```

**Inherited from IRepository\<User\>**:
- `GetByIdAsync(ExecutionContext, Id, CancellationToken)`
- `ExistsAsync(ExecutionContext, Id, CancellationToken)`
- `RegisterNewAsync(ExecutionContext, User, CancellationToken)`
- `EnumerateAllAsync(ExecutionContext, PaginationInfo, Handler, CancellationToken)`
- `EnumerateModifiedSinceAsync(ExecutionContext, TimeProvider, DateTimeOffset, Handler, CancellationToken)`

---

## 3. IPasswordHasher — Contract

```csharp
public interface IPasswordHasher
{
    // Gerar hash (Argon2id + salt + pepper)
    PasswordHashResult HashPassword(
        ExecutionContext executionContext,
        string password);

    // Verificar senha contra hash armazenado
    PasswordVerificationResult VerifyPassword(
        ExecutionContext executionContext,
        string password,
        byte[] storedHash);

    // Verificar se hash precisa ser re-gerado (pepper version desatualizada)
    bool NeedsRehash(byte[] storedHash);
}
```

### Result Types

```csharp
public readonly record struct PasswordHashResult(
    byte[] Hash,
    int PepperVersion);

public readonly record struct PasswordVerificationResult(
    bool IsValid,
    bool NeedsRehash);
```

---

## 4. IAuthenticationService — Contract

```csharp
public interface IAuthenticationService
{
    // Registrar novo usuário
    // Orquestra: validar política → hash password → criar User
    Task<User?> RegisterUserAsync(
        ExecutionContext executionContext,
        string email,
        string password,
        CancellationToken cancellationToken);

    // Verificar credenciais
    // Orquestra: buscar User por email → verificar hash → re-hash se necessário
    Task<User?> VerifyCredentialsAsync(
        ExecutionContext executionContext,
        string email,
        string password,
        CancellationToken cancellationToken);
}
```

---

## 5. PasswordPolicy — Contract

```csharp
public static class PasswordPolicy
{
    // Validar senha antes do hashing
    public static bool ValidatePassword(
        ExecutionContext executionContext,
        string? password);
}

public static class PasswordPolicyMetadata
{
    public static int MinLength { get; private set; } // Default: 12
    public static int MaxLength { get; private set; } // Default: 128
    public static bool AllowSpaces { get; private set; } // Default: true

    public static void ChangeMetadata(int minLength, int maxLength, bool allowSpaces);
}
```

---

## 6. Input Objects — Contracts

```csharp
public readonly record struct RegisterNewInput(
    EmailAddress Email,
    PasswordHash PasswordHash);

public readonly record struct CreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string Username,
    EmailAddress Email,
    PasswordHash PasswordHash,
    UserStatus Status);

public readonly record struct ChangeStatusInput(
    UserStatus NewStatus);

public readonly record struct ChangeUsernameInput(
    string NewUsername);

public readonly record struct ChangePasswordHashInput(
    PasswordHash NewPasswordHash);
```
