# Implementation Plan: Configuration BuildingBlock

**Branch**: `001-configuration-building-block` | **Date**: 2026-02-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-configuration-building-block/spec.md`

## Summary

BuildingBlock de infraestrutura que fornece `ConfigurationManagerBase` — uma classe base abstrata que encapsula `IConfiguration` e estende seu comportamento com um pipeline de handlers (padrao mediator). Handlers customizados recebem chave + valor e podem transformar, substituir ou repassar. O caminho de configuracao e derivado automaticamente do nome da classe + propriedade (sem strings manuais). Registro via fluent API type-safe com IoC. Sem handlers concretos (ex: KeyVault) — apenas infraestrutura base.

**Abordagem tecnica**: Projeto unico `Bedrock.BuildingBlocks.Configuration` seguindo os padroes existentes (Template Method, readonly structs, zero-allocation). IConfiguration recebido via construtor (DI). Pipeline de handlers como cadeia ordenada com escopo por chave/classe/propriedade. Expressoes type-safe compiladas e cacheadas para derivacao de caminho.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: Microsoft.Extensions.Configuration.Abstractions, Microsoft.Extensions.DependencyInjection.Abstractions, Bedrock.BuildingBlocks.Core
**Storage**: N/A (in-memory — leitura de IConfiguration)
**Testing**: xUnit + Shouldly + Moq + Bogus + Stryker.NET (100/100/100)
**Target Platform**: .NET library (cross-platform)
**Project Type**: Single project (BuildingBlock library)
**Performance Goals**: <1ms por chave para operacoes Get em memoria (CS-003 da spec)
**Constraints**: Zero dependencia de handlers concretos; apenas infraestrutura base. Sem ExecutionContext (configuracao e acessada fora do contexto de request).
**Scale/Scope**: 1 projeto src + 3 projetos de teste (unit, mutation, architecture)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gates

| Principio | Status | Notas |
|-----------|--------|-------|
| I. Qualidade Inegociavel | PASS | 100% coverage + 100% mutation planejados. xUnit, Shouldly, Moq, Bogus, Stryker.NET. |
| II. Simplicidade Deliberada | PASS | Projeto unico, sem camadas extras. Pipeline e fluent API sao o requisito core, nao over-engineering. |
| III. Observabilidade Nativa | PASS (parcial) | ILogger para logging estruturado no pipeline. Sem distributed tracing (ExecutionContext indisponivel no contexto de configuracao). Ver Complexity Tracking. |
| IV. Explicito sobre Implicito | PASS (parcial) | Sem ExecutionContext como primeiro parametro — configuracao e acessada em startup/DI, nao per-request. Toda configuracao de handler explicita via fluent API. Ver Complexity Tracking. |
| V. Automacao como Garantia | PASS | Pipeline local cobre este BuildingBlock. Scripts existentes suportam novo projeto. |
| BB-I. Performance | PASS | ConfigurationPath e HandlerScope como readonly struct. Expressoes compiladas e cacheadas. Zero allocation no hot path de Get. |
| BB-II. Imutabilidade | PASS | Models sao readonly structs. LoadStrategy e enum. Valores de configuracao sao read-mostly. |
| BB-IV. Modularidade por Contrato | PASS | Proprio .csproj. Relacao 1:1 com testes. Interfaces em Interfaces/ quando aplicavel. Dependencia unidirecional Configuration <- Core. |
| BB-VII. Arquitetura Verificada | PASS | Projeto de architecture tests incluido. Regras CS (CodeStyle) e IN (Infrastructure) aplicaveis. |
| BB-VIII. Template Method | PASS | ConfigurationManagerBase usa Initialize/ConfigureInternal conforme padrao existente (AvroSerializerBase, JsonSerializerBase). |
| BB-IX. Disciplina de Testes | PASS | TestBase, AAA, Shouldly, Moq, Bogus. Nomenclatura padrao. |
| BB-XI. Templates como Lei | N/A | Nao e um BuildingBlock de Domain Entities. Padroes gerais (sealed, etc.) serao seguidos. |

### Post-Design Re-check

| Principio | Status | Notas |
|-----------|--------|-------|
| III. Observabilidade | PASS | ILogger injetado no ConfigurationManagerBase. Logs em pipeline execution (handler start/end, erros). |
| IV. Explicito | PASS | Toda configuracao de handler declarativa via fluent API. Sem magic strings (path derivado automaticamente). |
| BB-I. Performance | PASS | Paths cacheados em ConcurrentDictionary. Expression trees compilados uma vez. HandlerScope.Matches() usa string comparison otimizada. |

## Project Structure

### Documentation (this feature)

```text
specs/001-configuration-building-block/
├── spec.md              # Especificacao da feature
├── plan.md              # Este arquivo
├── research.md          # Phase 0: decisoes de design
├── data-model.md        # Phase 1: modelo de dados
├── quickstart.md        # Phase 1: guia rapido de uso
├── contracts/           # Phase 1: API publica
│   └── public-api.md
├── checklists/
│   └── requirements.md  # Checklist de qualidade da spec
└── tasks.md             # Phase 2: tarefas de implementacao (/speckit.tasks)
```

### Source Code (repository root)

```text
src/BuildingBlocks/Configuration/
├── Bedrock.BuildingBlocks.Configuration.csproj
├── GlobalUsings.cs
├── ConfigurationManagerBase.cs
├── ConfigurationPath.cs
├── Handlers/
│   ├── ConfigurationHandlerBase.cs
│   ├── HandlerScope.cs
│   └── Enums/
│       └── LoadStrategy.cs
├── Pipeline/
│   └── ConfigurationPipeline.cs
└── Registration/
    ├── ConfigurationHandlerBuilder.cs
    ├── ConfigurationOptions.cs
    └── ServiceCollectionExtensions.cs

tests/UnitTests/BuildingBlocks/Configuration/
├── Bedrock.UnitTests.BuildingBlocks.Configuration.csproj
├── ConfigurationManagerBaseTests.cs
├── ConfigurationPathTests.cs
├── Handlers/
│   ├── ConfigurationHandlerBaseTests.cs
│   └── HandlerScopeTests.cs
├── Pipeline/
│   └── ConfigurationPipelineTests.cs
└── Registration/
    ├── ConfigurationHandlerBuilderTests.cs
    ├── ConfigurationOptionsTests.cs
    └── ServiceCollectionExtensionsTests.cs

tests/MutationTests/BuildingBlocks/Configuration/
└── stryker-config.json

tests/ArchitectureTests/BuildingBlocks/Configuration/
├── Bedrock.ArchitectureTests.BuildingBlocks.Configuration.csproj
├── Fixtures/
│   └── ArchFixture.cs
├── CodeStyleRuleTests.cs
└── InfrastructureRuleTests.cs
```

**Structure Decision**: Projeto unico seguindo o padrao existente de BuildingBlocks. Estrutura interna por feature domain (Handlers/, Pipeline/, Registration/) conforme convencao Core e Persistence.PostgreSql. Relacao 1:1 com projetos de teste. Nenhuma camada adicional necessaria — este BuildingBlock e infraestrutura base pura.

## Complexity Tracking

> **Justificativas para desvios parciais da constituicao:**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| III. Sem distributed tracing (ExecutionContext) | Configuracao e acessada em startup e DI resolution, onde ExecutionContext nao existe. ILogger padrao fornece observabilidade suficiente. | Exigir ExecutionContext em Get/Set tornaria a API impraticavel para o caso de uso primario (leitura de config em construtores e factories). |
| IV. Sem ExecutionContext como primeiro parametro | Mesmo motivo acima. O padrao de acesso a configuracao e fundamentalmente diferente de operacoes de dominio/infraestrutura. | Alternativa seria criar um ExecutionContext de "sistema" para configuracao, mas isso adicionaria complexidade sem beneficio real — a leitura de config nao pertence a um request/tenant especifico. |
