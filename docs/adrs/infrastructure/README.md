# Infrastructure ADRs

Decisoes arquiteturais sobre infraestrutura, camadas de persistencia, cross-cutting concerns e organizacao de projetos em bounded contexts.

## ADRs

### Organizacao de Camadas e Projetos

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [IN-001](./IN-001-camadas-canonicas-bounded-context.md) | Camadas Canonicas de um Bounded Context | Aceita | IN001 |
| [IN-002](./IN-002-domain-entities-projeto-separado.md) | Entidades de Dominio Vivem em Projeto Separado | Aceita | IN002 |
| [IN-003](./IN-003-domain-projeto-separado.md) | Domain E um Projeto Separado de Domain.Entities | Aceita | IN003 |
| [IN-004](./IN-004-modelo-dados-detalhe-implementacao.md) | Modelo de Dados E Detalhe de Implementacao | Aceita | — |
| [IN-005](./IN-005-infra-data-facade-persistencia.md) | Infra.Data Atua como Facade de Persistencia | Aceita | — |

### Conexao e UnitOfWork

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [IN-006](./IN-006-conexao-marker-interface-herda-iconnection.md) | Conexao do BC Deve Ter Marker Interface | Aceita | IN006 |
| [IN-007](./IN-007-unitofwork-marker-interface-herda-iunitofwork.md) | BC Deve Ter Marker Interface de UnitOfWork | Aceita | IN007 |
| [IN-008](./IN-008-conexao-implementacao-sealed-herda-base.md) | Implementacao de Conexao Deve Ser Sealed e Herdar Base | Aceita | IN008 |
| [IN-009](./IN-009-unitofwork-implementacao-sealed-herda-base.md) | Implementacao de UnitOfWork Deve Ser Sealed e Herdar Base | Aceita | IN009 |

### DataModels e Persistencia

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [IN-010](./IN-010-datamodel-herda-datamodelbase.md) | DataModel Deve Herdar DataModelBase | Aceita | IN010 |
| [IN-011](./IN-011-datamodel-repository-implementa-idatamodelrepository.md) | DataModelRepository Deve Implementar IDataModelRepository | Aceita | IN011 |
| [IN-012](./IN-012-repositorio-tech-implementa-irepository.md) | Repositorio Tecnologico Deve Implementar IRepository | Aceita | IN012 |

### Conversao DataModel e Entidade

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [IN-013](./IN-013-factories-bidirecionais-datamodel-entidade.md) | Factories Bidirecionais Para Conversao DataModel e Entidade | Aceita | IN013 |
| [IN-014](./IN-014-adapter-atualizacao-datamodel-existente.md) | Adapter Para Atualizacao de DataModel Existente | Aceita | IN014 |

### Estrutura e Delegacao

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [IN-015](./IN-015-estrutura-pastas-canonica-infra-data-tech.md) | Estrutura Canonica de Pastas em Infra.Data.{Tech} | Aceita | IN015 |
| [IN-016](./IN-016-repositorio-tech-agnostico-delega-para-tech.md) | Repositorio Tech-Agnostico Delega Para Tech | Aceita | IN016 |

### IoC e Composicao

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [IN-017](./IN-017-bootstrapper-por-camada-para-ioc.md) | Cada Camada Tem Seu Proprio Bootstrapper Para IoC | Aceita | IN017 |

### Organizacao no .sln

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [IN-018](./IN-018-solution-folders-canonicos-bounded-context.md) | Solution Folders Canonicos Para Bounded Context | Aceita | IN018 |

## Escopo

- **IN-***: Regras que definem a estrutura de projetos de infraestrutura, camadas de dados, persistencia e cross-cutting concerns.

## Navegacao

- [Voltar para ADRs](../README.md)
- [Code Style ADRs](../code-style/README.md)
- [Domain Entities ADRs](../domain-entities/README.md)
