<div align="center">

# 🪨 Bedrock

**A fundação que sua arquitetura .NET merece.**

Um framework de building blocks para Domain-Driven Design, Clean Architecture e desenvolvimento assistido por IA no .NET 10.

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=CasteloBrancoLab_Bedrock&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=CasteloBrancoLab_Bedrock)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=CasteloBrancoLab_Bedrock&metric=coverage)](https://sonarcloud.io/summary/new_code?id=CasteloBrancoLab_Bedrock)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=CasteloBrancoLab_Bedrock&metric=bugs)](https://sonarcloud.io/summary/new_code?id=CasteloBrancoLab_Bedrock)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=CasteloBrancoLab_Bedrock&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=CasteloBrancoLab_Bedrock)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-latest-239120?style=flat&logo=csharp)
![Mutation Score](https://img.shields.io/badge/mutation%20score-100%25-brightgreen?style=flat)
![License](https://img.shields.io/badge/license-Apache%202.0-blue?style=flat)

[Comece Aqui](#-comece-aqui) · [Building Blocks](#-building-blocks) · [Decisoes Arquiteturais](#-decisoes-arquiteturais) · [Documentacao](docs/)

</div>

---

## Por Que Este Repositorio Existe

Um **showcase educacional** para mentoria em arquitetura de software, demonstrando:

- Como estruturar um projeto .NET seguindo Clean Architecture
- Padroes de Domain-Driven Design aplicados na pratica
- Desenvolvimento assistido por IA com guardrails e supervisao humana
- Documentacao de decisoes arquiteturais (ADRs) para cada escolha de design

```
Humano define O QUE e POR QUE  →  IA implementa o COMO  →  Guardrails garantem qualidade
```

> **Para Mentorados**: Este nao e apenas um framework para copiar. E um material de estudo vivo, onde cada decisao esta documentada e justificada.

> **⚠️ Em desenvolvimento ativo**: O conteudo abaixo representa o estado atual do projeto e evoluira com o tempo.

---

### 🧬 Laboratorio de Vibe Coding

Este projeto e tambem um **laboratorio de vibe coding para projetos do mundo real**. Um arquiteto experiente orienta code agents (Claude Code) para criar **100% do projeto** — codigo, testes, documentacao, ADRs, pipelines — tudo gerado por IA sob supervisao e guardrails rigorosos.

| | Arquiteto (Humano) | Code Agent (IA) |
|---|---------------------|-----------------|
| **Papel** | Orienta, supervisiona, decide | Implementa 100% do projeto |
| **Responsabilidade** | O QUE e POR QUE | COMO — codigo, testes, ADRs, docs, pipelines |
| **Qualidade** | Define guardrails e quality gates | Executa e valida (100% cobertura + 100% mutacao) |

O objetivo e demonstrar que **vibe coding com guardrails rigorosos** produz codigo de qualidade real — validado por testes de mutacao, analise estatica e revisao arquitetural.

---

## Destaques

| | Feature | Detalhes |
|---|---------|---------|
| **🆔** | **IDs UUIDv7** | Ordenados por tempo, thread-safe, 67M+ IDs/ms por thread |
| **🔒** | **Dominio Imutavel** | Estado invalido nunca existe em memoria — factory methods + entidades sealed |
| **🌐** | **Multi-Tenancy** | Isolamento de tenant nativo no nivel da entidade |
| **📋** | **Trilha de Auditoria Completa** | CreatedBy, ChangedBy, CorrelationId, Origin — automatico em cada mutacao |
| **⏱️** | **Locking Otimista** | `RegistryVersion` — 40ns por geracao, resistente a clock drift |
| **📄** | **Paginacao e Filtros** | `PaginationInfo` + `SortInfo` + `FilterInfo` type-safe com 12 operadores |
| **✅** | **Validacao** | Codigos de erro cacheados, integrados ao `ExecutionContext` |
| **🧪** | **100% Mutation Kill Rate** | Stryker.NET garante que cada linha de logica e testada de verdade |
| **📊** | **Observabilidade** | Correlation IDs + tracing distribuido via `ExecutionContext` |
| **📦** | **4 Formatos de Serializacao** | JSON · Protobuf · Avro · Parquet — todos com memory pooling |

---

## 🏗️ Building Blocks

```
Bedrock.BuildingBlocks
├── Core                    # Id, ExecutionContext, Validacao, Paginacao, Value Objects
│   ├── Ids/                  UUIDv7 com contador monotonico
│   ├── ExecutionContexts/    Correlacao, tenant, mensagens, excecoes
│   ├── Paginations/          PaginationInfo, SortInfo, FilterInfo
│   ├── ValueObjects/         BirthDate, EmailAddress, PhoneNumber
│   ├── TimeProviders/        CustomTimeProvider testavel
│   └── Validations/          ValidationUtils com cache
│
├── Domain                  # IRepository, IAggregateRoot
│   └── Entities/             EntityBase, EntityInfo, EntityChangeInfo
│
├── Data                    # RepositoryBase com Template Method pattern
├── Persistence             # PostgreSQL + UnitOfWork + DistributedLock
├── Serialization           # JSON, Protobuf, Avro, Parquet + JSON Schema
├── Observability           # Extensoes de logging + tracing distribuido
└── Testing                 # TestBase, ServiceCollectionFixture
```

---

## 📐 Decisoes Arquiteturais

Cada escolha de design esta documentada em **58 ADRs**. Estas sao as fundamentais:

| ADR | Decisao | Motivacao |
|-----|---------|-----------|
| [DE-001](docs/adrs/domain-entities/DE-001-entidades-devem-ser-sealed.md) | Entidades devem ser `sealed` | Previne hierarquias de heranca acidentais |
| [DE-002](docs/adrs/domain-entities/DE-002-construtores-privados-com-factory-methods.md) | Construtores privados + factory methods | Garante invariantes na criacao |
| [DE-004](docs/adrs/domain-entities/DE-004-estado-invalido-nunca-existe-na-memoria.md) | Estado invalido nunca existe em memoria | Validacao fail-fast, sem objetos parciais |
| [DE-028](docs/adrs/domain-entities/DE-028-executioncontext-explicito.md) | `ExecutionContext` explicito | Rastreabilidade completa em cada operacao |
| [DE-029](docs/adrs/domain-entities/DE-029-timeprovider-encapsulado-no-executioncontext.md) | `TimeProvider` encapsulado | Testabilidade total, zero `DateTime.Now` |

> **Lista completa**: [docs/adrs/domain-entities/](docs/adrs/domain-entities/README.md)

---

## 🧪 Quality Gates

Bedrock aplica **tolerancia zero** em qualidade atraves de multiplas camadas automatizadas:

```
Codigo Fonte
    │
    ▼
┌─────────────────┐   ┌──────────────────┐   ┌─────────────────┐
│  Testes Unit.    │──▶│  Testes Mutacao   │──▶│  SonarCloud     │
│  xUnit + Shouldly│   │  Stryker.NET      │   │  Quality Gate   │
│  100% cobertura  │   │  100% kill rate   │   │  0 bugs/vulns   │
└─────────────────┘   └──────────────────┘   └─────────────────┘
    │                                               │
    ▼                                               ▼
┌─────────────────┐                         ┌─────────────────┐
│  Testes Arquit.  │                         │  Pipeline CI    │
│  (Roslyn)        │                         │  8 jobs         │
└─────────────────┘                         └─────────────────┘
```

| Camada | Ferramenta | Threshold |
|--------|------------|-----------|
| Testes Unitarios | xUnit + Shouldly + Moq + Bogus | 100% cobertura de linhas |
| Testes de Mutacao | Stryker.NET | 100% kill rate |
| Qualidade de Codigo | SonarCloud | Quality Gate aprovado |
| Arquitetura | Roslyn CodeAnalysis | Regras estruturais aplicadas |
| Performance | BenchmarkDotNet | Deteccao de regressao |
| Integracao | Testcontainers + PostgreSQL | Verificacao ponta a ponta |

---

## 🚀 Comece Aqui

### Pre-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git + Bash (WSL/Git Bash no Windows)

### Clone e Execute

```bash
git clone https://github.com/CasteloBrancoLab/Bedrock.git
cd Bedrock

# Pipeline completa: build → test → mutate → report
./scripts/pipeline.sh

# Ou passos individuais
./scripts/build.sh          # Compilar
./scripts/test.sh           # Testes unitarios + cobertura
./scripts/mutate.sh         # Testes de mutacao
```

### Opcional: Integracao com SonarCloud

Crie um arquivo `.env` na raiz do projeto:

```env
SONAR_TOKEN=<seu-token-do-sonarcloud>
```

> Sem o `SONAR_TOKEN`, a pipeline funciona normalmente — a analise do SonarCloud e simplesmente ignorada.

---

## 🛠️ Stack Tecnica

| Categoria | Tecnologias |
|-----------|-------------|
| **Runtime** | .NET 10.0, C# latest |
| **Persistencia** | PostgreSQL, Npgsql, ObjectPool |
| **Serializacao** | System.Text.Json, protobuf-net, Apache Avro, Apache Arrow (Parquet) |
| **Testes** | xUnit, Shouldly, Moq, Bogus, Stryker.NET, Testcontainers, BenchmarkDotNet |
| **Qualidade** | SonarCloud, Coverlet, Roslyn CodeAnalysis |
| **CI/CD** | GitHub Actions (pipeline de 8 jobs), cache de NuGet |
| **IA** | Claude Code com guardrails via CLAUDE.md |

---

## 📁 Estrutura do Projeto

```
Bedrock/
├── src/BuildingBlocks/          # Codigo fonte do framework
│   ├── Core/                      Abstracoes fundamentais
│   ├── Domain/                    Building blocks de dominio
│   ├── Data/                      Implementacoes base de repositorio
│   ├── Persistence/               PostgreSQL + abstracoes
│   ├── Serialization/             JSON, Protobuf, Avro, Parquet
│   ├── Observability/             Logging + tracing
│   └── Testing/                   Infraestrutura de testes
│
├── src/Samples/ShopDemo/        # Implementacao de referencia (e-commerce)
│   ├── Customers.Domain/
│   ├── Orders.Domain/
│   └── Products.Domain/
│
├── tests/
│   ├── UnitTests/                 Mapeamento 1:1 com src/
│   ├── MutationTests/             Configs do Stryker por projeto
│   ├── ArchitectureTests/         Testes estruturais via Roslyn
│   ├── IntegrationTests/          Testcontainers
│   └── PerformanceTests/          BenchmarkDotNet
│
├── docs/
│   ├── adrs/                      58 Architecture Decision Records
│   ├── building-blocks/           Documentacao dos componentes
│   ├── code-styles/               Convencoes de codigo
│   └── workflows/                 Processos de desenvolvimento
│
└── scripts/                     # Automacao de pipeline local
```

---

## 📖 Documentacao

| Topico | Descricao |
|--------|-----------|
| [Building Blocks](docs/building-blocks/README.md) | Documentacao dos componentes com exemplos |
| [ADRs](docs/adrs/domain-entities/README.md) | 58 decisoes arquiteturais com justificativa |
| [Code Styles](docs/code-styles/README.md) | Convencoes de nomenclatura e padroes |
| [Workflows](docs/workflows/README.md) | Processos de desenvolvimento e pipelines |

---

## 🤖 Desenvolvimento Assistido por IA

Bedrock valida um modelo de **vibe coding** com separacao clara de responsabilidades:

| | Humano | IA (Claude Code) |
|---|--------|-------------------|
| **Papel** | Arquiteto | Implementador |
| **Define** | O Que e Por Que | Como |
| **Exemplos** | Requisitos, ADRs, direcao | Codigo, testes, refatoracoes |

A IA opera sob guardrails rigorosos definidos no [`CLAUDE.md`](CLAUDE.md), deve passar por todos os quality gates e seguir cada ADR.

---

## Licenca

[Apache License 2.0](LICENSE)

---

<div align="center">

**Construido com disciplina. Testado com rigor. Documentado com intencao.**

</div>
