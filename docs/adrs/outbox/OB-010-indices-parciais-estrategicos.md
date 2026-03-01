# OB-010: Indices Parciais Estrategicos para Outbox

## Status

Aceita

## Contexto

### O Problema (Analogia)

Uma biblioteca com um milhao de livros pode ter um indice completo de
todos os livros por autor. Mas se 90% das consultas forem "livros
disponiveis para emprestimo", um indice so dos livros disponiveis e
mais pequeno, mais rapido e mais facil de manter. Indexar livros ja
emprestados ou em reparacao desperdiica espaco no indice sem beneficio.

### O Problema Tecnico

A tabela outbox tem um ciclo de vida claro: entries passam de
`Pending` → `Processing` → `Sent` (ou `Failed` → `Dead`). A query
mais critica — `ClaimNextBatchAsync` — precisa encontrar rapidamente:

1. **Branch 1**: Entries com `status IN (Pending, Failed)` e
   `is_processing = FALSE`.
2. **Branch 2**: Entries com `status = Processing` e lease expirado
   (`processing_expiration < NOW()`).

Entries com status `Sent` ou `Dead` **nunca sao consultadas pelo
processador** — sao historicas. Um indice B-tree completo sobre `status`
incluiria essas entries, desperdicando espaco e I/O de manutencao.

Um full table scan sem indice e O(n) onde n e o total de entries
(incluindo historicas). Com partial indexes, a busca e O(m) onde m
e apenas o subconjunto activo — ordens de magnitude menor em tabelas
com historico grande.

## A Decisao

Dois partial indexes cobrem os dois branches do `ClaimNextBatchAsync`:

```sql
-- Index 1: Branch 1 — entries pendentes ou falhadas (retentaveis)
CREATE INDEX IF NOT EXISTS idx_auth_outbox_status_created
    ON auth_outbox(status, created_at)
    WHERE status IN (1, 4);   -- 1=Pending, 4=Failed

-- Index 2: Branch 2 — leases expirados (worker crashou)
CREATE INDEX IF NOT EXISTS idx_auth_outbox_processing_expiration
    ON auth_outbox(processing_expiration, created_at)
    WHERE status = 2;         -- 2=Processing
```

**Regras fundamentais:**

1. **Partial indexes**: Clausula `WHERE` restringe o indice ao
   subconjunto relevante — entries historicas (`Sent`, `Dead`) nao
   ocupam espaco no indice.
2. **Colunas leading corretas**: Cada indice tem como leading column
   o campo usado na clausula `WHERE` da query correspondente.
3. **`created_at` como tiebreaker**: Garante ordem FIFO dentro de
   cada subset de status.
4. **PK cobre operacoes pontuais**: `MarkAsSentAsync` e
   `MarkAsFailedAsync` usam `WHERE id = @id` — cobertos pela PK.
5. **Sem indice em `Sent`/`Dead`**: Entries finalizadas nao sao
   consultadas pelo processador. Se necessario para analytics, criar
   indice separado — nao misturar com o hot path.

### Mapeamento query → indice

| Query | Clausula WHERE | Indice |
|-------|---------------|--------|
| ClaimNextBatch Branch 1 | `status IN (1,4) AND is_processing = FALSE` | `idx_*_status_created` |
| ClaimNextBatch Branch 2 | `status = 2 AND processing_expiration < NOW()` | `idx_*_processing_expiration` |
| MarkAsSent | `id = @id` | PK |
| MarkAsFailed | `id = @id` | PK |
| AddAsync (INSERT) | — | PK (auto) |

## Consequencias

### Beneficios

- Indice 1 contem apenas entries activas — tamanho proporcional ao
  backlog, nao ao historico total.
- Indice 2 contem apenas entries em processamento — tipicamente
  poucos entries (batchSize * N workers).
- Manutencao de indice (vacuum, reindex) proporcional ao subconjunto,
  nao a tabela inteira.
- `ClaimNextBatchAsync` e eficiente mesmo com milhoes de entries
  historicas na tabela.

### Trade-offs (Com Perspectiva)

- **Dois indices em vez de um**: Mais I/O de escrita (INSERT atualiza
  ambos os indices para entries novas). Na pratica, o custo e
  marginal — entries novas so entram no indice 1 (`status=1`).
- **Especificos ao SQL actual**: Se a query do `ClaimNextBatchAsync`
  mudar, os indices podem precisar de ajuste. A query e definida no
  building block (nao no BC), minimizando este risco.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox.PostgreSql | Define as queries que os indices servem |

## Referencias no Codigo

- Migration Auth: `src/ShopDemo/Auth/Infra.Data.PostgreSql.Migrations/Scripts/Up/V202603010001__create_auth_outbox_table.sql`
- Repositorio base: `src/BuildingBlocks/Outbox.PostgreSql/OutboxPostgreSqlRepositoryBase.cs`
- ADR relacionada: [OB-003 — Lease Pattern](./OB-003-lease-pattern-for-update-skip-locked.md)
