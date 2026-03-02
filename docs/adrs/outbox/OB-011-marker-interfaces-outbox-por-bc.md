# OB-011: Marker Interfaces para Outbox por BC

## Status

Aceita

## Contexto

### O Problema Tecnico

O outbox define interfaces genericas no building block:
`IOutboxRepository`, `IOutboxWriter<TPayload>`. Num monorepo com
multiplos BCs, cada BC precisa da sua propria instancia de outbox
(tabela, configuracao, UoW). Se todos os BCs registrarem no DI
com as mesmas interfaces genericas:

```csharp
// Auth BC
services.AddScoped<IOutboxRepository, AuthOutboxRepository>();
// Catalog BC (futuro)
services.AddScoped<IOutboxRepository, CatalogOutboxRepository>();
// Qual IOutboxRepository o container resolve? O ultimo registrado.
```

O problema e identico ao descrito em IN-006 para conexoes — a solucao
tambem e identica: **marker interfaces**.

## A Decisao

Cada BC declara marker interfaces vazias para o repositorio e o writer
do outbox, estendendo as interfaces genericas do building block:

**Marker para repositorio** (em `Infra.Data.PostgreSql`):

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/Outbox/Interfaces/IAuthOutboxRepository.cs
using Bedrock.BuildingBlocks.Outbox.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Outbox.Interfaces;

public interface IAuthOutboxRepository : IOutboxRepository
{
    // Corpo vazio — marker interface
}
```

**Marker para writer** (em `Infra.CrossCutting.Messages`):

```csharp
// ShopDemo.Auth.Infra.CrossCutting.Messages/Outbox/Interfaces/IAuthOutboxWriter.cs
using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Outbox.Interfaces;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;

public interface IAuthOutboxWriter : IOutboxWriter<MessageBase>
{
    // Corpo vazio — marker interface
}
```

**Regras fundamentais:**

1. **Uma marker por BC por conceito**: `IAuthOutboxRepository` para o
   repositorio, `IAuthOutboxWriter` para o writer.
2. **Corpo vazio**: Sem membros adicionais — apenas distingue o BC
   no container DI.
3. **Nomenclatura**: `I{BoundedContext}Outbox{Conceito}` — ex:
   `IAuthOutboxRepository`, `ICatalogOutboxWriter`.
4. **Placement estrategico**:
   - `IAuthOutboxRepository` em `Infra.Data.PostgreSql` — so e
     consumido pelo writer do mesmo projeto.
   - `IAuthOutboxWriter` em `Infra.CrossCutting.Messages` — consumido
     pela camada Application (que nao referencia `Infra.Data.*`).

### Cadeia de heranca

```
IOutboxRepository (BuildingBlocks.Outbox)         ← interface generica
  └── IAuthOutboxRepository (Auth.Infra.Data)     ← marker do BC

IOutboxWriter<MessageBase> (BuildingBlocks.Outbox) ← interface generica
  └── IAuthOutboxWriter (Auth.CrossCutting.Messages) ← marker do BC
```

**Registro DI:**

```csharp
// Cada BC registra com sua marker — zero ambiguidade
services.TryAddScoped<IAuthOutboxRepository, AuthOutboxRepository>();
services.TryAddScoped<IAuthOutboxWriter, AuthOutboxWriter>();
```

### Placement: por que IAuthOutboxWriter nao vive em Infra.Data?

A camada Application precisa de injectar `IAuthOutboxWriter` nos use
cases. Application nao pode referenciar `Infra.Data.PostgreSql`
(violaria Clean Architecture — dependencia de dentro para fora).
`Infra.CrossCutting.Messages` e o projecto "ponte" ja referenciado
por Application, tornando-o o local natural para a marker do writer.

O repositorio, por outro lado, e consumido apenas dentro de
`Infra.Data.PostgreSql` (pelo writer concreto), logo vive la.

## Consequencias

### Beneficios

- Resolucao DI sem ambiguidade entre multiplos BCs.
- Erros de wiring detectados em tempo de compilacao.
- Principio consistente com IN-006 (conexoes) — code agents aplicam
  o mesmo padrao.
- Use cases dependem de `IAuthOutboxWriter` — desacoplados da
  tecnologia de persistencia.

### Trade-offs (Com Perspectiva)

- **Duas interfaces "vazias" por BC**: ~4 linhas cada. O custo e
  trivial comparado com o beneficio de type safety.
- **Decisao de placement**: Requer entender a regra de dependencia
  entre camadas. Documentada aqui para referencia futura.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Marker Interface** (Bloch, Effective Java): Interfaces vazias que
  classificam tipos sem adicionar comportamento.
- **Separated Interface** (Fowler, POEAA): Interface definida no
  projecto consumidor, separada da implementacao.

### O Que o Clean Architecture Diz

> "Source code dependencies must point only inward."

A marker do writer vive em `Infra.CrossCutting.Messages` (camada
externa) e a implementacao em `Infra.Data.PostgreSql` (camada ainda
mais externa). Application depende apenas da marker — nunca da
implementacao.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `IOutboxRepository` e `IOutboxWriter<T>` — interfaces base |

## Referencias no Codigo

- Marker repositorio: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Outbox/Interfaces/IAuthOutboxRepository.cs`
- Marker writer: `src/ShopDemo/Auth/Infra.CrossCutting.Messages/Outbox/Interfaces/IAuthOutboxWriter.cs`
- DI: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Bootstrapper.cs`
- ADR base: [IN-006 — Marker Interface para Conexao](../infrastructure/IN-006-conexao-marker-interface-herda-iconnection.md)
- ADR relacionada: [OB-008 — Composicao sobre Heranca](./OB-008-composicao-sobre-heranca-outboxwriter.md)
