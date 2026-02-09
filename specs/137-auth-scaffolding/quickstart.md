# Quickstart: Auth - Estrutura dos Projetos (Scaffolding)

**Date**: 2026-02-08
**Feature**: [spec.md](spec.md)

## Pré-requisitos

- .NET 10.0 SDK instalado
- Git com branch `137-auth-scaffolding` checked out
- Acesso ao repositório Bedrock

## Validação Rápida

Após a implementação do scaffolding, execute os seguintes comandos para validar:

### 1. Verificar se os projetos foram adicionados à solution

```bash
dotnet sln Bedrock.sln list | grep -i "Auth"
```

Resultado esperado: 10 projetos (5 src + 5 testes).

### 2. Compilar a solution

```bash
dotnet build Bedrock.sln
```

Resultado esperado: build bem-sucedido sem erros.

### 3. Executar testes

```bash
dotnet test Bedrock.sln
```

Resultado esperado: execução sem erros (zero testes do Auth, pois a estrutura está vazia).

### 4. Pipeline completa

```bash
./scripts/pipeline.sh
```

Resultado esperado: pipeline passa completamente.

### 5. Verificar estrutura de diretórios

```bash
find samples/ShopDemo/Auth -name "*.csproj" | sort
find tests/UnitTests/ShopDemo/Auth -name "*.csproj" | sort
find tests/MutationTests/ShopDemo/Auth -name "*.json" | sort
```

Resultado esperado:
- 5 arquivos `.csproj` em `samples/ShopDemo/Auth/`
- 5 arquivos `.csproj` em `tests/UnitTests/ShopDemo/Auth/`
- 5 arquivos `stryker-config.json` em `tests/MutationTests/ShopDemo/Auth/`

### 6. Verificar registro no teste de arquitetura

```bash
grep -n "ShopDemo.Auth" tests/ArchitectureTests/Templates/Domain.Entities/Fixtures/DomainEntitiesArchFixture.cs
```

Resultado esperado: linha com `Path.Combine(rootDir, "samples", "ShopDemo", "Auth", "Domain.Entities", ...)`.
