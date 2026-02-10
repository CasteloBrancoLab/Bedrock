# Implementation Plan: Auth Domain Model — User Entity com Credenciais

**Branch**: `001-auth-domain-model` | **Date**: 2026-02-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-auth-domain-model/spec.md`

## Summary

Implementar a entidade User (aggregate root) com credenciais (email + hash de senha opaco) e status de ciclo de vida, seguindo os padrões do Bedrock (SimpleAggregateRoot template). Criar o building block `Bedrock.BuildingBlocks.Security` para encapsular hashing Argon2id com salt + pepper e rotação. Definir a interface IUserRepository e o domain service de autenticação no projeto Domain. Todos os projetos com 100% cobertura e 100% mutantes eliminados.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: Bedrock.BuildingBlocks.Core, Bedrock.BuildingBlocks.Domain, Bedrock.BuildingBlocks.Domain.Entities, Bedrock.BuildingBlocks.Testing, Konscious.Security.Cryptography (Argon2id)
**Storage**: N/A (domain model apenas — persistência é escopo de outra issue)
**Testing**: xUnit, Shouldly, Moq, Bogus, Humanizer, Coverlet, Stryker.NET
**Target Platform**: .NET 10.0 (cross-platform)
**Project Type**: Framework (BuildingBlocks + ShopDemo sample)
**Performance Goals**: Zero-allocation em value objects (readonly struct), métodos estáticos para handlers (sem closures)
**Constraints**: Domain.Entities com zero dependências de Security; hash como byte[] opaco; sealed classes; private constructors
**Scale/Scope**: 3 projetos src novos/modificados, 3 projetos de testes, 3 configs de mutação

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Status | Notas |
|-----------|--------|-------|
| I. Qualidade Inegociável | PASS | 100% cobertura + 100% mutação definidos em FR-016/FR-017/SC-007 |
| II. Simplicidade Deliberada | PASS | User é SimpleAggregateRoot (arquétipo mais simples). Security BB justificado por reuso cross-cutting |
| III. Observabilidade Nativa | PASS | ExecutionContext carrega correlation/audit. Logging estruturado via TestBase |
| IV. Modularidade por Contrato | PASS | Separação Domain.Entities/Domain/Security com interfaces explícitas |
| V. Automação como Garantia | PASS | Pipeline local obrigatória antes de commit |
| VI. Separação Domain.Entities/Domain | PASS | Domain.Entities → Core only; Domain → Core + Domain.Entities + Security |
| VII. Arquitetura Verificada | PASS | Regras Roslyn DE001+ aplicam-se automaticamente |
| VIII. Template Method (Infra) | N/A | Sem camada de persistência nesta issue |
| IX. Disciplina de Testes Unitários | PASS | TestBase, AAA com logging, Shouldly, nomenclatura padronizada |
| X. Disciplina de Testes Integração | N/A | Sem testes de integração nesta issue |
| XI. Templates como Lei | PASS | User segue SimpleAggregateRoot template |
| BB-I. Performance | PASS | Value objects como readonly struct, handlers static |
| BB-II. Imutabilidade | PASS | Clone-Modify-Return para todas alterações |
| BB-III. Estado Inválido Nunca Existe | PASS | RegisterNew retorna null se inválido |
| BB-IV. Explícito sobre Implícito | PASS | ExecutionContext como primeiro parâmetro |
| BB-V. Aggregate Root como Fronteira | PASS | User é o único AR, IUserRepository para AR only |

## Project Structure

### Documentation (this feature)

```text
specs/001-auth-domain-model/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
# BuildingBlock novo: Security
src/BuildingBlocks/Security/
├── Bedrock.BuildingBlocks.Security.csproj
├── GlobalUsings.cs
├── Passwords/
│   ├── IPasswordHasher.cs              # Interface pública do serviço
│   ├── PasswordHasher.cs               # Implementação Argon2id + pepper
│   ├── PasswordPolicy.cs               # Validação NIST 800-63B
│   ├── PasswordPolicyMetadata.cs       # Metadata estático (min/max length)
│   └── PepperConfiguration.cs          # Configuração de pepper com versões
└── PasswordHashResult.cs               # Resultado do hash (byte[] + pepper version)

# Auth Domain.Entities (existente — scaffolding issue #137)
samples/ShopDemo/Auth/Domain.Entities/
├── ShopDemo.Auth.Domain.Entities.csproj  # (existente)
├── GlobalUsings.cs                       # (existente)
└── Users/
    ├── User.cs                          # Entidade sealed, EntityBase<User>
    ├── UserMetadata.cs                  # Metadata estático de validação
    ├── Enums/
    │   └── UserStatus.cs                # Ativo, Suspenso, Bloqueado
    ├── Interfaces/
    │   └── IUser.cs                     # Interface pública da entidade
    └── Inputs/
        ├── RegisterNewInput.cs          # readonly record struct
        ├── CreateFromExistingInfoInput.cs
        ├── ChangeStatusInput.cs
        ├── ChangeUsernameInput.cs
        └── ChangePasswordHashInput.cs

# Auth Domain (existente — scaffolding issue #137)
samples/ShopDemo/Auth/Domain/
├── ShopDemo.Auth.Domain.csproj           # (existente — adicionar ref Security)
├── GlobalUsings.cs                       # (existente)
├── Repositories/
│   └── IUserRepository.cs               # Extends IRepository<User>
└── Services/
    ├── IAuthenticationService.cs         # Interface do domain service
    └── AuthenticationService.cs          # Orquestra User + Security

# Testes Unitários (existentes — scaffolding issue #137)
tests/UnitTests/ShopDemo/Auth/Domain.Entities/
├── ShopDemo.UnitTests.Auth.Domain.Entities.csproj  # (existente)
└── Users/
    ├── UserTests.cs                     # Testes da entidade User
    ├── UserMetadataTests.cs             # Testes do metadata
    ├── UserStatusTests.cs               # Testes das transições de estado
    └── Inputs/
        └── InputObjectTests.cs          # Testes dos Input Objects

tests/UnitTests/ShopDemo/Auth/Domain/
├── ShopDemo.UnitTests.Auth.Domain.csproj  # (existente)
├── Repositories/
│   └── IUserRepositoryTests.cs          # Testes do contrato
└── Services/
    └── AuthenticationServiceTests.cs    # Testes do domain service

tests/UnitTests/BuildingBlocks/Security/
├── Bedrock.UnitTests.BuildingBlocks.Security.csproj  # NOVO
└── Passwords/
    ├── PasswordHasherTests.cs           # Testes de hash/verify
    ├── PasswordPolicyTests.cs           # Testes de política de senha
    └── PepperConfigurationTests.cs      # Testes de rotação de pepper

# Testes de Mutação
tests/MutationTests/ShopDemo/Auth/Domain.Entities/
└── stryker-config.json                  # NOVO

tests/MutationTests/ShopDemo/Auth/Domain/
└── stryker-config.json                  # NOVO

tests/MutationTests/BuildingBlocks/Security/
└── stryker-config.json                  # NOVO
```

**Structure Decision**: Segue exatamente a estrutura existente do Bedrock: BuildingBlocks para componentes reutilizáveis do framework, samples/ShopDemo para implementações concretas, testes em relação 1:1. O novo `Bedrock.BuildingBlocks.Security` é justificado como cross-cutting concern de segurança reutilizável (não específico ao Auth).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Novo BuildingBlock (Security) | Encapsula Argon2id + pepper como infraestrutura reutilizável cross-service | Colocar no Domain.Entities acoplaria criptografia ao domínio; colocar no Domain limitaria reuso |
| PasswordHashResult (tipo adicional) | Carrega byte[] + versão do pepper para suportar rotação transparente | Retornar apenas byte[] impediria saber qual versão do pepper foi usada para re-hash |
