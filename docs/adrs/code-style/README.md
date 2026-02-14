# Code Style ADRs

Decisões arquiteturais sobre organização de código, convenções de namespace e estrutura de diretórios aplicáveis a **todo o projeto Bedrock** (BuildingBlocks, ShopDemo, Templates).

## ADRs

| ADR | Titulo | Status |
|-----|--------|--------|
| [CS-001](./CS-001-interfaces-em-namespace-interfaces.md) | Interfaces em Namespace Interfaces | Aceita |
| [CS-002](./CS-002-lambdas-inline-devem-ser-static.md) | Lambdas Inline Devem Ser Static em Metodos do Projeto | Aceita |
| [CS-003](./CS-003-logging-sempre-com-distributed-tracing.md) | Logging Sempre com Distributed Tracing | Aceita |

## Diferença para Domain Entities (DE)

- **DE-***: Regras que se aplicam exclusivamente a entidades de dominio (classes que herdam de `EntityBase<T>`).
- **CS-***: Regras de code style que se aplicam a **qualquer tipo** em qualquer projeto do repositorio.

## Navegação

- [Voltar para ADRs](../README.md)
- [Domain Entities ADRs](../domain-entities/README.md)
