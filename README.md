# Bedrock

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=CasteloBrancoLab_Bedrock&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=CasteloBrancoLab_Bedrock)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=CasteloBrancoLab_Bedrock&metric=coverage)](https://sonarcloud.io/summary/new_code?id=CasteloBrancoLab_Bedrock)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=CasteloBrancoLab_Bedrock&metric=bugs)](https://sonarcloud.io/summary/new_code?id=CasteloBrancoLab_Bedrock)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=CasteloBrancoLab_Bedrock&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=CasteloBrancoLab_Bedrock)

Um framework de referencia para **mentoria em arquitetura de software**, demonstrando boas praticas de DDD, Clean Architecture e desenvolvimento assistido por IA.

---

## Por Que Este Repositorio Existe

Este repositorio e um **showcase educacional** que demonstra:

- Como estruturar um projeto .NET seguindo principios de Clean Architecture
- Padroes de Domain-Driven Design aplicados na pratica
- Desenvolvimento assistido por IA com guardrails e supervisao humana
- Documentacao de decisoes arquiteturais (ADRs) para cada escolha de design

> **Para Mentorados**: Este nao e apenas um framework para copiar. E um material de estudo vivo, onde cada decisao esta documentada e justificada.

---

## Por Onde Comecar

### 1. Entenda a Filosofia

| Conceito | O Que E | Por Que Importa |
|----------|---------|-----------------|
| **Building Blocks** | Componentes reutilizaveis | Evita reinventar a roda em cada projeto |
| **ADRs** | Decisoes arquiteturais documentadas | Entende o "porque" de cada escolha |
| **Guardrails** | Regras que a IA deve seguir | Garante consistencia mesmo com automacao |

### 2. Explore a Documentacao

| Topico | Link | Descricao |
|--------|------|-----------|
| **Building Blocks** | [docs/building-blocks/](docs/building-blocks/README.md) | Componentes fundamentais do framework |
| **ADRs - Domain Entities** | [docs/adrs/domain-entities/](docs/adrs/domain-entities/README.md) | 58 decisoes sobre modelagem de entidades |
| **Code Styles** | [docs/code-styles/](docs/code-styles/README.md) | Convencoes de codigo do projeto |
| **Workflows** | [docs/workflows/](docs/workflows/README.md) | Fluxos de trabalho e processos |

### 3. Navegue pelo Codigo

```
src/
└── BuildingBlocks/
    ├── Core/           # Abstraccoes fundamentais
    │   ├── Ids/        # Geracao de IDs tipo UUIDv7
    │   ├── ExecutionContexts/  # Contexto de execucao
    │   ├── TenantInfos/       # Multi-tenancy
    │   ├── TimeProviders/     # Abstracoes de tempo
    │   └── Validations/       # Utilitarios de validacao
    │
    ├── Domain/         # Building blocks de dominio
    │   └── Entities/   # EntityBase, AggregateRoot, etc.
    │
    ├── Data/           # Persistencia
    └── Testing/        # Base para testes
```

---

## Conceitos-Chave

### Building Blocks Principais

| Building Block | Proposito | Documentacao |
|----------------|-----------|--------------|
| **Id** | Geracao de IDs ordenados por tempo (UUIDv7) | [docs/building-blocks/core/ids/](docs/building-blocks/core/ids/id.md) |
| **ExecutionContext** | Contexto de execucao com tenant, usuario, mensagens | [docs/building-blocks/core/execution-contexts/](docs/building-blocks/core/execution-contexts/execution-context.md) |
| **TenantInfo** | Informacoes de tenant para multi-tenancy | [docs/building-blocks/core/tenant-infos/](docs/building-blocks/core/tenant-infos/tenant-info.md) |
| **ValidationUtils** | Validacoes padronizadas com codigos de erro | [docs/building-blocks/core/validations/](docs/building-blocks/core/validations/validation-utils.md) |
| **BirthDate** | Value object para datas de nascimento | [docs/building-blocks/core/birth-dates/](docs/building-blocks/core/birth-dates/birth-date.md) |
| **PaginationInfo** | Paginacao, ordenacao e filtros type-safe | [docs/building-blocks/core/paginations/](docs/building-blocks/core/paginations/pagination-info.md) |

### ADRs Fundamentais

Algumas decisoes arquiteturais essenciais para entender o design:

| ADR | Titulo | Por Que Ler |
|-----|--------|-------------|
| DE-001 | [Entidades devem ser sealed](docs/adrs/domain-entities/DE-001-entidades-devem-ser-sealed.md) | Previne heranca acidental |
| DE-002 | [Construtores privados com factory methods](docs/adrs/domain-entities/DE-002-construtores-privados-com-factory-methods.md) | Garante invariantes |
| DE-004 | [Estado invalido nunca existe na memoria](docs/adrs/domain-entities/DE-004-estado-invalido-nunca-existe-na-memoria.md) | Principio fundamental |
| DE-028 | [ExecutionContext explicito](docs/adrs/domain-entities/DE-028-executioncontext-explicito.md) | Rastreabilidade completa |
| DE-029 | [TimeProvider encapsulado](docs/adrs/domain-entities/DE-029-timeprovider-encapsulado-no-executioncontext.md) | Testabilidade total |

---

## Desenvolvimento Assistido por IA

Este projeto valida um modelo de **vibe coding** onde:

| Responsabilidade | Quem Faz | Exemplos |
|------------------|----------|----------|
| **O QUE e PORQUE** | Humano | Requisitos, decisoes arquiteturais, direcao |
| **COMO** | IA (Claude Code) | Implementacao, testes, refatoracoes |

### Guardrails Estabelecidos

A IA segue regras definidas em:

- [`CLAUDE.md`](CLAUDE.md) - Instrucoes especificas para o code agent
- ADRs - Decisoes que a IA deve respeitar
- Code Styles - Convencoes de nomenclatura e estrutura

---

## Stack Tecnica

- **.NET 10.0** - Framework base
- **C# (latest)** - Linguagem principal
- **xUnit + Shouldly + Moq + Bogus** - Stack de testes
- **Stryker.NET** - Testes de mutacao (100% kill rate)
- **SonarCloud** - Qualidade e cobertura

---

## Configuracao Local

### Claude Code

Este projeto utiliza [Claude Code](https://claude.com/claude-code). Para configurar:

1. Crie `.claude/settings.local.json`:

```json
{
  "permissions": {
    "allow": [
      "Bash",
      "Edit",
      "Write",
      "Read",
      "WebFetch",
      "WebSearch",
      "mcp__*"
    ]
  }
}
```

> **Nota**: Este arquivo nao e versionado (`.gitignore`).

### Executando a Pipeline Local

```bash
./scripts/pipeline.sh   # Pipeline completa
./scripts/test.sh       # Apenas testes
./scripts/mutate.sh     # Testes de mutacao
```

---

## Licenca

Veja [LICENSE](LICENSE) para detalhes.
