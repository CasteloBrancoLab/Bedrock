# IN-010: DataModel Deve Herdar DataModelBase

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN010_DataModelInheritsDataModelBaseRule**, que verifica:

- Classes no namespace `*.DataModels` de projetos `*.Infra.Data.{Tech}`
  devem herdar, direta ou indiretamente, de
  `Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels.DataModelBase`.
- DataModels nao devem conter metodos com logica de negocio (apenas
  propriedades com `get`/`set`).

## Contexto

### O Problema (Analogia)

Imagine uma rede de farmacias. Cada medicamento vendido precisa ter na
embalagem: numero de lote, data de fabricacao, data de validade, nome
do laboratorio e codigo de barras. Sao campos obrigatorios por lei. Alem
disso, cada medicamento tem seus proprios campos (dosagem, principio
ativo, forma farmaceutica). Se uma farmacia vender um medicamento sem
lote ou sem validade, e uma violacao regulatoria. O formulario padrao
garante que nenhum campo obrigatorio seja esquecido.

### O Problema Tecnico

Toda entidade persistida precisa de metadados de infraestrutura que nao
sao regras de negocio, mas sao essenciais para o funcionamento correto
do sistema:

- **Identificacao**: `Id` (Guid), `TenantCode` (multi-tenancy).
- **Auditoria**: `CreatedBy`, `CreatedAt`, `CreatedCorrelationId`,
  `CreatedExecutionOrigin`, `CreatedBusinessOperationCode`.
- **Rastreio de alteracoes**: `LastChangedBy`, `LastChangedAt`,
  `LastChangedCorrelationId`, `LastChangedExecutionOrigin`,
  `LastChangedBusinessOperationCode`.
- **Concorrencia otimista**: `EntityVersion`.


Se cada DataModel reimplementar esses campos manualmente, a
inconsistencia e inevitavel: um esquece `TenantCode`, outro usa `int`
em vez de `Guid` para `Id`, outro nao tem `EntityVersion`. A
`DataModelBase` centraliza esses campos, garantindo uniformidade.

## Como Normalmente E Feito

### Abordagem Tradicional

A maioria dos projetos define DTOs de persistencia avulsos, cada um com
seus proprios campos de metadados:

```csharp
public class UserRecord
{
    public int Id { get; set; }              // int, nao Guid
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }  // DateTime, nao DateTimeOffset
    // Sem TenantCode, sem auditoria, sem versao
}

public class OrderRecord
{
    public Guid OrderId { get; set; }        // "OrderId" em vez de "Id"
    public Guid TenantId { get; set; }       // "TenantId" em vez de "TenantCode"
    public string CreatedByUser { get; set; } // "CreatedByUser" em vez de "CreatedBy"
}
```

### Por Que Nao Funciona Bem

- **Inconsistencia de nomes**: Cada DTO inventa sua nomenclatura para
  campos comuns (`Id` vs. `OrderId`, `TenantCode` vs. `TenantId`).
- **Inconsistencia de tipos**: `int` vs. `Guid` para IDs, `DateTime`
  vs. `DateTimeOffset` para timestamps.
- **Campos obrigatorios esquecidos**: Sem base class, nao ha garantia de
  que `TenantCode`, `EntityVersion` ou campos de auditoria existam.
- **Factories e Adapters nao funcionam**: `DataModelBaseFactory` e
  `DataModelBaseAdapter` esperam `DataModelBase` — DTOs avulsos nao
  sao compativeis.

## A Decisao

### Nossa Abordagem

Todo DataModel em `Infra.Data.{Tech}` deve herdar de `DataModelBase`:

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/DataModels/UserDataModel.cs
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class UserDataModel : DataModelBase
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public byte[] PasswordHash { get; set; } = null!;
    public short Status { get; set; }
}
```

**Campos herdados de `DataModelBase`:**

| Campo | Tipo | Proposito |
|-------|------|-----------|
| `Id` | `Guid` | Identificacao unica |
| `TenantCode` | `Guid` | Multi-tenancy |
| `CreatedBy` | `string` | Quem criou |
| `CreatedAt` | `DateTimeOffset` | Quando criou |
| `CreatedCorrelationId` | `Guid` | Rastreio distribuido da criacao |
| `CreatedExecutionOrigin` | `string` | Origem da execucao (API, batch, etc.) |
| `CreatedBusinessOperationCode` | `string` | Codigo da operacao de negocio |
| `LastChangedBy` | `string?` | Quem alterou por ultimo |
| `LastChangedAt` | `DateTimeOffset?` | Quando alterou por ultimo |
| `LastChangedCorrelationId` | `Guid?` | Rastreio distribuido da alteracao |
| `LastChangedExecutionOrigin` | `string?` | Origem da execucao da alteracao |
| `LastChangedBusinessOperationCode` | `string?` | Codigo da operacao de alteracao |
| `EntityVersion` | `long` | Concorrencia otimista |

**Regras fundamentais:**

1. **Heranca obrigatoria**: Todo DataModel herda de `DataModelBase`.
2. **Apenas propriedades**: DataModels sao DTOs de persistencia — so
   `get`/`set`, sem metodos de negocio.
3. **Namespace canonico**: Reside em `*.DataModels`.
4. **Nomenclatura**: `{Entity}DataModel` (ex: `UserDataModel`).
5. **Tipos primitivos**: Properties do DataModel usam tipos primitivos
   (string, byte[], short) — nao value objects de dominio.

### Por Que Funciona Melhor

- **Uniformidade**: Todos os DataModels tem exatamente os mesmos campos
  de metadados, com os mesmos nomes e tipos.
- **Compatibilidade com framework**: `DataModelBaseFactory`,
  `DataModelBaseAdapter`, `DataModelMapperBase` e
  `DataModelRepositoryBase` operam sobre `DataModelBase` —
  interoperabilidade garantida.
- **Multi-tenancy automatica**: Todo DataModel tem `TenantCode` — e
  impossivel criar um registro sem tenant.
- **Auditoria completa**: Quem criou, quando, de onde, com qual
  correlationId — para toda entidade persistida.

## Consequencias

### Beneficios

- Todos os DataModels sao compativeis com Factories, Adapters, Mappers
  e Repositories do framework.
- Campos de auditoria e multi-tenancy nunca sao esquecidos.
- Concorrencia otimista disponivel para todos os registros.
- Code agents geram DataModels corretos com apenas 2 informacoes: nome
  da entidade e propriedades especificas.

### Trade-offs (Com Perspectiva)

- **Campos "extras"**: Todo DataModel carrega 13 campos herdados. Em
  tabelas com poucos campos proprios, parece desproporcional. Na pratica,
  esses campos sao essenciais para operacao em producao (auditoria,
  multi-tenancy, concorrencia) e o custo de armazenamento e negligivel.
- **Sem heranca multipla**: C# nao suporta heranca multipla de classes.
  Se um DataModel precisar de outra base, nao e possivel. Na pratica,
  DataModels nunca precisam de outra base — sao DTOs simples.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Layer Supertype** (Fowler, POEAA): Uma classe base que concentra
  comportamento comum a todos os objetos de uma camada. `DataModelBase`
  e o Layer Supertype da camada de persistencia.
- **Data Transfer Object** (Fowler, POEAA): DataModels sao DTOs que
  transferem dados entre a aplicacao e o banco de dados.

### O Que o DDD Diz

> "Persistence is an infrastructure concern, not a domain concern."
>
> *Persistencia e uma preocupacao de infraestrutura, nao de dominio.*

Evans (2003). DataModels sao artefatos de infraestrutura — vivem
exclusivamente na camada tecnologica, com tipos primitivos, sem
value objects de dominio.

### O Que o Clean Architecture Diz

> "The database is a detail."
>
> *O banco de dados e um detalhe.*

Robert C. Martin (2017). DataModels materializam esse detalhe — sao a
representacao concreta de como o banco armazena dados, completamente
isolados do dominio.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Quais campos o DataModelBase fornece e por que cada um e necessario?"
2. "Qual a diferenca entre um DataModel e uma entidade de dominio?"
3. "Como o EntityVersion implementa concorrencia otimista?"

### Leitura Recomendada

- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 18 — Layer Supertype
- Eric Evans, *Domain-Driven Design* (2003), Cap. 6 — Repositories
- Robert C. Martin, *Clean Architecture* (2017), Cap. 30 — The Database
  Is a Detail

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelBase` com todos os campos de metadados |
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelBaseFactory` e `DataModelBaseAdapter` que operam sobre `DataModelBase` |

## Referencias no Codigo

- DataModel de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/DataModels/UserDataModel.cs`
- Base class: `src/BuildingBlocks/Persistence.PostgreSql/DataModels/DataModelBase.cs`
- ADR relacionada: [IN-004 — Modelo de Dados E Detalhe de Implementacao](./IN-004-modelo-dados-detalhe-implementacao.md)
