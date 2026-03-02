# OB-006: Repositorio Outbox em Tres Camadas

## Status

Aceita

## Contexto

### O Problema (Analogia)

Um contrato de franchising tem tres niveis: o contrato-quadro da marca
(regras universais), o contrato regional (adaptacoes ao mercado local)
e o contrato da loja (detalhes especificos). Se cada loja definisse
tudo do zero, haveria inconsistencia e duplicacao. Se todas usassem o
mesmo contrato generico, nao haveria espaco para necessidades locais.

### O Problema Tecnico

O outbox precisa de:

1. **Contrato uniforme**: Todos os BCs usam a mesma interface para
   persistir e ler entries (`IOutboxRepository`).
2. **Implementacao PostgreSQL reutilizavel**: SQL, parametros, lease
   pattern — comuns a todos os BCs que usam PostgreSQL.
3. **Personalizacao por BC**: Nome da tabela, schema, MaxRetries —
   diferem por bounded context.

Uma unica classe concreta global nao permite personalizacao. Uma
interface por BC sem classe base comum implica duplicacao de SQL.

## A Decisao

O repositorio segue uma arquitectura em tres camadas:

```
IOutboxRepository (Outbox — interface)
    │
    ▼
OutboxPostgreSqlRepositoryBase (Outbox.PostgreSql — classe abstrata)
    │
    ▼
AuthOutboxRepository (ShopDemo.Auth — classe concreta sealed)
```

**Camada 1 — Interface (Outbox):**

```csharp
public interface IOutboxRepository : IOutboxReader
{
    Task AddAsync(OutboxEntry entry, CancellationToken cancellationToken);
}
```

Contrato puro. Nao conhece PostgreSQL.

**Camada 2 — Base abstrata (Outbox.PostgreSql):**

```csharp
public abstract class OutboxPostgreSqlRepositoryBase : IOutboxRepository
{
    protected abstract void ConfigureInternal(OutboxPostgreSqlOptions options);

    // Implementa AddAsync, ClaimNextBatchAsync, MarkAsSentAsync,
    // MarkAsFailedAsync com SQL parametrizado.
}
```

Template method: o BC fornece configuracao, a base fornece
implementacao.

**Camada 3 — Concreta sealed (BC-specific):**

```csharp
public sealed class AuthOutboxRepository
    : OutboxPostgreSqlRepositoryBase, IAuthOutboxRepository
{
    public AuthOutboxRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override void ConfigureInternal(OutboxPostgreSqlOptions options)
    {
        options.WithTableName("auth_outbox");
    }
}
```

Apenas configuracao — zero duplicacao de SQL.

**Regras fundamentais:**

1. **Interface no building block core**: `IOutboxRepository` nao depende
   de tecnologia.
2. **Base abstrata por tecnologia**: `OutboxPostgreSqlRepositoryBase`
   encapsula todo o SQL e o template method.
3. **Sealed class por BC**: A classe concreta e `sealed` — nao ha mais
   heranca apos o BC.
4. **Marker interface do BC**: Implementa tambem `IAuthOutboxRepository`
   para isolamento DI (OB-011).
5. **UnitOfWork do BC**: Recebe `IAuthPostgreSqlUnitOfWork` (marker),
   nao o generico.

## Consequencias

### Beneficios

- Zero duplicacao de SQL entre BCs — toda a logica complexa vive na base.
- Novos BCs integram outbox com ~10 linhas de codigo (classe + override).
- Troca de tecnologia (ex: MySQL) requer apenas nova base abstrata —
  os BCs nao mudam.
- Type safety via generics e markers em cada camada.

### Trade-offs (Com Perspectiva)

- **Tres niveis de indireccao**: Pode parecer over-engineering para um
  unico BC. Mas assim que o segundo BC integra outbox, o investimento
  na base abstrata ja se paga.
- **Template method limita flexibilidade**: Se um BC precisar de SQL
  radicalmente diferente, a base nao serve. Na pratica, a personalizacao
  por tabela/schema/maxRetries cobre todos os cenarios previstos.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Template Method** (GoF): A base define o algoritmo; a subclasse
  fornece detalhes via `ConfigureInternal`.
- **Layer Supertype** (Fowler, POEAA): Tipo base que encapsula
  comportamento comum a todas as instancias de uma camada.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `IOutboxRepository` e `IOutboxReader` |
| Bedrock.BuildingBlocks.Outbox.PostgreSql | Implementa `OutboxPostgreSqlRepositoryBase` |

## Referencias no Codigo

- Interface: `src/BuildingBlocks/Outbox/Interfaces/IOutboxRepository.cs`
- Base abstrata: `src/BuildingBlocks/Outbox.PostgreSql/OutboxPostgreSqlRepositoryBase.cs`
- Concreta: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Outbox/AuthOutboxRepository.cs`
- ADR relacionada: [OB-004 — Lazy SQL Initialization](./OB-004-lazy-sql-initialization.md)
- ADR relacionada: [OB-011 — Marker Interfaces por BC](./OB-011-marker-interfaces-outbox-por-bc.md)
