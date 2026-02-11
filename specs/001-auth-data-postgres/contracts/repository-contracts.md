# Repository Contracts: Auth User Data Access Layer

**Feature Branch**: `001-auth-data-postgres`
**Date**: 2026-02-11

## Layer 1: Domain Repository (existing)

```csharp
// File: samples/ShopDemo/Auth/Domain/Repositories/Interfaces/IUserRepository.cs
// Status: ALREADY IMPLEMENTED

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken);

    Task<User?> GetByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);
}
```

## Layer 2: PostgreSQL Repository Interface (new)

```csharp
// File: samples/ShopDemo/Auth/Infra.Persistence/Repositories/Interfaces/IUserPostgreSqlRepository.cs

namespace ShopDemo.Auth.Infra.Persistence.Repositories.Interfaces;

public interface IUserPostgreSqlRepository
    : IPostgreSqlRepository<User>
{
    Task<User?> GetByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken);

    Task<User?> GetByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);
}
```

## Layer 3: DataModel Repository Interface (new)

```csharp
// File: samples/ShopDemo/Auth/Infra.Persistence/DataModelsRepositories/Interfaces/IUserDataModelRepository.cs

namespace ShopDemo.Auth.Infra.Persistence.DataModelsRepositories.Interfaces;

public interface IUserDataModelRepository
    : IPostgreSqlDataModelRepository<UserDataModel>
{
    Task<UserDataModel?> GetByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken);

    Task<UserDataModel?> GetByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);
}
```

## Layer 4: Connection and UnitOfWork Interfaces (new)

```csharp
// File: samples/ShopDemo/Auth/Infra.Persistence/Connections/Interfaces/IAuthPostgreSqlConnection.cs

namespace ShopDemo.Auth.Infra.Persistence.Connections.Interfaces;

public interface IAuthPostgreSqlConnection
    : IPostgreSqlConnection
{
}
```

```csharp
// File: samples/ShopDemo/Auth/Infra.Persistence/UnitOfWork/Interfaces/IAuthPostgreSqlUnitOfWork.cs

namespace ShopDemo.Auth.Infra.Persistence.UnitOfWork.Interfaces;

public interface IAuthPostgreSqlUnitOfWork
    : IPostgreSqlUnitOfWork
{
}
```

## Contract Dependency Chain

```
IUserRepository (Domain)
    ↑ implemented by
UserRepository (Infra.Data) → wraps IUserPostgreSqlRepository
    ↑ delegates to
IUserPostgreSqlRepository (Infra.Persistence) → uses Factories for entity↔DataModel
    ↑ delegates to
IUserDataModelRepository (Infra.Persistence) → raw DataModel CRUD + custom queries
    ↑ uses
IAuthPostgreSqlUnitOfWork → IAuthPostgreSqlConnection → PostgreSQL
```
