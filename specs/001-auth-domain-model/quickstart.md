# Quickstart: Auth Domain Model

**Feature**: 001-auth-domain-model
**Date**: 2026-02-09

## Prerequisites

- .NET 10.0 SDK
- Solução Bedrock compilando (`./scripts/build.sh`)
- Scaffolding da issue #137 presente (projetos Auth vazios)

## Ordem de Implementação Recomendada

### Fase 1: Fundação (sem dependências externas)

1. **UserStatus enum** — `Auth/Domain.Entities/Users/Enums/UserStatus.cs`
   - 3 valores: Active, Suspended, Blocked
   - Sem lógica (apenas enum)

2. **PasswordHash value object** — `Auth/Domain.Entities/Users/PasswordHash.cs`
   - readonly struct wrapping ReadOnlyMemory<byte>
   - Factory method CreateNew(byte[])
   - Comparação em tempo constante
   - ToString() retorna "[REDACTED]"

3. **Input Objects** — `Auth/Domain.Entities/Users/Inputs/*.cs`
   - 5 readonly record structs (um por operação)

4. **IUser interface** — `Auth/Domain.Entities/Users/Interfaces/IUser.cs`

5. **UserMetadata** — `Auth/Domain.Entities/Users/UserMetadata.cs`
   - Propriedades estáticas de validação

6. **User entity** — `Auth/Domain.Entities/Users/User.cs`
   - Seguir SimpleAggregateRoot template
   - RegisterNew, CreateFromExistingInfo, ChangeStatus, ChangeUsername, ChangePasswordHash

### Fase 2: Building Block Security

7. **Projeto Security** — `src/BuildingBlocks/Security/`
   - Criar .csproj com referência a Core
   - Adicionar package Konscious.Security.Cryptography

8. **PasswordPolicyMetadata** — Metadata estático de política de senhas
9. **PasswordPolicy** — Validação de senha (min/max length)
10. **PepperConfiguration** — Gestão de versões de pepper
11. **IPasswordHasher + PasswordHasher** — Hash e verify com Argon2id + pepper

### Fase 3: Domain Services

12. **IUserRepository** — `Auth/Domain/Repositories/IUserRepository.cs`
    - Extends IRepository<User>
    - Adiciona GetByEmail, GetByUsername, ExistsByEmail, ExistsByUsername

13. **IAuthenticationService + AuthenticationService** — `Auth/Domain/Services/`
    - Orquestra User + IPasswordHasher + IUserRepository

### Fase 4: Testes + Pipeline

14. **UnitTests Domain.Entities** — Testes da entidade User, VO, enums
15. **UnitTests Security** — Testes do PasswordHasher, PasswordPolicy, Pepper
16. **UnitTests Domain** — Testes do AuthenticationService (com mocks)
17. **MutationTests** — 3 stryker-config.json (Domain.Entities, Security, Domain)
18. **Pipeline** — `./scripts/pipeline.sh` deve passar 100%

## Comandos Úteis

```bash
# Compilar solução
./scripts/build.sh

# Executar testes com cobertura
./scripts/test.sh

# Executar testes de mutação
./scripts/mutate.sh

# Pipeline completa
./scripts/pipeline.sh
```

## Referências

- [SimpleAggregateRoot Template](../../src/templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs)
- [IRepository Base](../../src/BuildingBlocks/Domain/Repositories/IRepository.cs)
- [EmailAddress Value Object](../../src/BuildingBlocks/Core/EmailAddresses/EmailAddress.cs)
- [Constitution](../../.specify/memory/constitution.md)
- [Spec](spec.md)
- [Data Model](data-model.md)
- [Research](research.md)
