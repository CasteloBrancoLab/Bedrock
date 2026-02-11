# Infrastructure ADRs

Decisoes arquiteturais sobre infraestrutura, camadas de persistencia, cross-cutting concerns e organizacao de projetos em bounded contexts.

## ADRs

| ADR | Titulo | Status |
|-----|--------|--------|
| [IN-001](./IN-001-camadas-canonicas-bounded-context.md) | Camadas Canonicas de um Bounded Context | Aceita |
| [IN-002](./IN-002-domain-entities-projeto-separado.md) | Entidades de Dominio Vivem em Projeto Separado | Aceita |
| [IN-003](./IN-003-domain-projeto-separado.md) | Domain É um Projeto Separado de Domain.Entities | Aceita |
| [IN-004](./IN-004-modelo-dados-detalhe-implementacao.md) | Modelo de Dados É Detalhe de Implementacao | Aceita |
| [IN-005](./IN-005-infra-data-facade-persistencia.md) | Infra.Data Atua como Facade de Persistencia | Aceita |

## Escopo

- **IN-***: Regras que definem a estrutura de projetos de infraestrutura, camadas de dados, persistencia e cross-cutting concerns.

## Navegacao

- [Voltar para ADRs](../README.md)
- [Code Style ADRs](../code-style/README.md)
- [Domain Entities ADRs](../domain-entities/README.md)
