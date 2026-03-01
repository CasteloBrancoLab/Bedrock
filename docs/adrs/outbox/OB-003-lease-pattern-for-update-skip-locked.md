# OB-003: Lease Pattern com FOR UPDATE SKIP LOCKED

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fila de despacho num armazem com varios operadores. Se dois
operadores pegarem na mesma encomenda ao mesmo tempo, um deles trabalha
em vao — ou pior, a encomenda e processada duas vezes. A solucao e cada
operador "reservar" uma encomenda por tempo limitado: pega nela, tem 5
minutos para despachar. Se nao despachar a tempo, a encomenda volta a
fila para outro operador. Enquanto esta reservada, nenhum outro operador
a toca.

### O Problema Tecnico

O processamento do outbox precisa de:

1. **Paralelismo**: Multiplos workers processam entries simultaneamente
   para throughput.
2. **Sem duplicacao**: Cada entry e processado exactamente uma vez (ou
   retentado se falhar).
3. **Resiliencia**: Se um worker crashar, as entries que reservou devem
   voltar a fila.

As abordagens comuns sao:

- **Lock distribuido (Redis/ZooKeeper)**: Infraestrutura adicional,
  single point of failure, complexidade operacional.
- **Optimistic concurrency (version column)**: Requer retry loops no
  worker quando ha conflito — desperdia trabalho.
- **`SELECT ... FOR UPDATE` (pessimistic lock)**: Bloqueia rows —
  workers ficam em espera se tentarem as mesmas rows.

Nenhuma destas escala bem com multiplos workers sem overhead.

## A Decisao

Usamos o **lease pattern** implementado com `FOR UPDATE SKIP LOCKED`
do PostgreSQL. Dois campos no `OutboxEntry` controlam o lease:

```csharp
public bool IsProcessing { get; init; }              // lease ativo?
public DateTimeOffset? ProcessingExpiration { get; init; } // quando expira?
```

O `ClaimNextBatchAsync` usa uma CTE atomica:

```sql
WITH candidates AS (
    SELECT id FROM {schema}.{table}
    WHERE
        -- Branch 1: entries pendentes ou falhadas (para retry)
        (status IN (1, 4) AND is_processing = FALSE)
        OR
        -- Branch 2: leases expirados (worker crashou)
        (status = 2 AND processing_expiration < NOW())
    ORDER BY created_at
    LIMIT @batchSize
    FOR UPDATE SKIP LOCKED    -- atomico: skip rows ja reservadas
)
UPDATE {schema}.{table} SET
    status = 2,               -- Processing
    is_processing = TRUE,
    processing_expiration = NOW() + @leaseDuration
FROM candidates
WHERE {schema}.{table}.id = candidates.id
RETURNING ...;
```

**Regras fundamentais:**

1. **`FOR UPDATE SKIP LOCKED`**: Cada worker salta rows ja reservadas
   por outro — zero espera, zero conflito.
2. **Lease com expiracao**: `ProcessingExpiration` garante que entries
   de workers crashados voltam a fila automaticamente.
3. **Atomicidade**: CTE + UPDATE numa unica query — sem race condition
   entre SELECT e UPDATE.
4. **Sem infraestrutura adicional**: Apenas PostgreSQL — sem Redis,
   ZooKeeper ou distributed lock managers.
5. **Branch duplo**: A CTE cobre entries novas/falhadas E leases
   expirados na mesma query.

### Ciclo de Vida do Lease

```
Pending/Failed → ClaimNextBatch → Processing (lease ativo)
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                  ▼
               MarkAsSent       MarkAsFailed      Lease expira
               (status=3)      (retry_count++)   (worker crash)
                    │                 │                  │
                    ▼                 ▼                  ▼
                  Sent          Failed/Dead*        Disponivel
                                                   novamente**
```

\* Se `retry_count >= MaxRetries`, transicao para `Dead` (OB-005).
\** Branch 2 da CTE reclama entries com lease expirado.

## Consequencias

### Beneficios

- Escalabilidade horizontal: N workers processam em paralelo sem
  coordenacao.
- Zero duplicacao: Cada entry e claimed por exactamente um worker por
  vez.
- Resiliencia a crashes: Leases expirados sao automaticamente
  reclamados.
- Sem infraestrutura adicional: PostgreSQL nativo, sem dependencias
  externas.

### Trade-offs (Com Perspectiva)

- **Lease duration deve ser calibrado**: Muito curto e o worker nao
  termina a tempo; muito longo e entries de workers crashados ficam
  bloqueadas. Na pratica, 30s-2min e suficiente para a maioria dos
  cenarios.
- **Polling**: Workers precisam fazer poll periodico. O custo de um
  `SELECT` com partial index e negligivel (OB-010).
- **`SKIP LOCKED` e PostgreSQL-especifico**: Nao e portavel para todos
  os RDBMS. Como o Bedrock usa PostgreSQL como storage primario, este
  e um trade-off aceite.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Lease Pattern** (Gray & Cheriton, 1989): Conceder acesso exclusivo
  temporario a um recurso. Se o detentor falhar, o lease expira e o
  recurso volta a ficar disponivel.
- **Competing Consumers** (Hohpe & Woolf, 2003): Multiplos consumidores
  processam mensagens da mesma fila em paralelo. O `SKIP LOCKED`
  implementa este padrao a nivel de base de dados.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `OutboxEntry` com `IsProcessing` e `ProcessingExpiration` |
| Bedrock.BuildingBlocks.Outbox.PostgreSql | Implementa `ClaimNextBatchAsync` com CTE + `FOR UPDATE SKIP LOCKED` |

## Referencias no Codigo

- Repositorio base: `src/BuildingBlocks/Outbox.PostgreSql/OutboxPostgreSqlRepositoryBase.cs`
- Entry model: `src/BuildingBlocks/Outbox/Models/OutboxEntry.cs`
- ADR relacionada: [OB-005 — Dead-Lettering Automatico](./OB-005-dead-lettering-automatico-maxretries.md)
- ADR relacionada: [OB-010 — Indices Parciais Estrategicos](./OB-010-indices-parciais-estrategicos.md)
