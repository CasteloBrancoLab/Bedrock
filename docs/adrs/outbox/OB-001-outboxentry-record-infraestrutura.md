# OB-001: OutboxEntry como Record de Infraestrutura

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine um servico postal. Quando voce envia uma carta, o recibo de
postagem nao e a carta — e um registro administrativo que rastreia
destino, peso, data e status de entrega. O recibo nao tem identidade
propria no sentido de "ser uma coisa no seu dominio"; ele existe para
garantir que a carta chega. Se o recibo fosse tratado como um pacote
igual aos outros, passaria pelas mesmas regras de inspeccao, seguro e
rastreamento — overhead desnecessario para algo que e pura operacao.

### O Problema Tecnico

O padrao Transactional Outbox requer um registro temporario que persiste
o payload de um evento ate ser processado e enviado ao destino (message
broker, outro BC, etc.). Este registro — `OutboxEntry` — tem campos como
`Id`, `Payload`, `Status`, `RetryCount` e `ProcessingExpiration`.

A tentacao e modelar `OutboxEntry` como uma entidade de dominio, herdando
de `EntityBase` com audit columns (`CreatedBy`, `LastChangedBy`,
`EntityVersion`, etc.). Mas o outbox entry:

1. **Nao tem significado de dominio** — nao participa em regras de negocio.
2. **E efemero** — criado, processado e potencialmente purgado.
3. **Nao precisa de audit trail** — ninguem pergunta "quem alterou o
   status do outbox entry?".
4. **Nao precisa de optimistic concurrency** — a concorrencia e resolvida
   pelo lease pattern (OB-003), nao por `EntityVersion`.

Se usar `EntityBase`, cada entry carrega 10 colunas base desnecessarias
(ver PG-001), aumenta o tamanho da tabela e complica o mapper sem
beneficio.

## A Decisao

`OutboxEntry` e um `sealed record` de infraestrutura com campos
explicitamente definidos — **nao herda de `EntityBase`** nem de
`DataModelBase`:

```csharp
namespace Bedrock.BuildingBlocks.Outbox.Models;

public sealed record OutboxEntry
{
    public required Guid Id { get; init; }
    public required Guid TenantCode { get; init; }
    public required Guid CorrelationId { get; init; }
    public required string PayloadType { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Payload { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required OutboxEntryStatus Status { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
    public byte RetryCount { get; init; }
    public bool IsProcessing { get; init; }
    public DateTimeOffset? ProcessingExpiration { get; init; }
}
```

**Regras fundamentais:**

1. **Sealed record, nao class nem entity**: Imutabilidade por design.
   Campos sao `init`-only.
2. **Sem heranca de EntityBase**: Nao participa do ciclo de vida de
   entidades de dominio.
3. **Sem DataModelBase**: Nao usa o sistema de mappers/binary import
   das entidades — SQL e construido diretamente no repositorio.
4. **Campos proprios de lifecycle**: `Status`, `RetryCount`,
   `IsProcessing`, `ProcessingExpiration` sao especificos do outbox
   e nao existem no modelo de entidades.

## Consequencias

### Beneficios

- Tabela outbox com apenas 12 colunas (vs 22+ se herdasse EntityBase).
- SQL de INSERT/UPDATE simples e direto, sem overhead de audit columns.
- Separacao clara: entidades de dominio vs registros de infraestrutura.
- Code agents nao confundem outbox entries com domain entities.

### Trade-offs (Com Perspectiva)

- **Sem audit trail**: Nao ha `CreatedBy`/`LastChangedBy`. Na pratica,
  o outbox entry e criado pelo sistema (use case), nao por um humano.
  O `CorrelationId` e `TenantCode` ja fornecem rastreabilidade.
- **Sem EntityVersion**: Concorrencia e gerida pelo lease pattern
  (OB-003), nao por optimistic locking. Trade-off inexistente.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `OutboxEntry` e `OutboxEntryStatus` |
| Bedrock.BuildingBlocks.Outbox.PostgreSql | Consome `OutboxEntry` no repositorio — SQL direto, sem mapper |

## Referencias no Codigo

- Definicao: `src/BuildingBlocks/Outbox/Models/OutboxEntry.cs`
- Enum de status: `src/BuildingBlocks/Outbox/Models/OutboxEntryStatus.cs`
- ADR relacionada: [OB-003 — Lease Pattern](./OB-003-lease-pattern-for-update-skip-locked.md)
