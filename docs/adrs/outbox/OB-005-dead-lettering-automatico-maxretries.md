# OB-005: Dead-Lettering Automatico por MaxRetries

## Status

Aceita

## Contexto

### O Problema (Analogia)

Uma carta devolvida pelo correio e reenviada automaticamente. Mas apos 3
devolucoes, nao faz sentido continuar a tentar — a carta vai para o
"gabinete de cartas mortas" para investigacao manual. Se nao houvesse
limite, a carta ficaria em loop infinito entre remetente e destinatario.

### O Problema Tecnico

Quando o processamento de um outbox entry falha (ex: message broker
indisponivel, erro de rede), o entry deve ser retentado. Mas retentativas
infinitas criam problemas:

1. **Poison messages**: Entries com payload invalido nunca serao
   processadas com sucesso — retentativas infinitas consomem recursos.
2. **Acumulacao**: Entries falhadas permanentes bloqueiam o
   processamento de entries validas (se houver limite de batch).
3. **Mascaramento de problemas**: Retentativas silenciosas escondem
   erros sistematicos que requerem intervencao.

## A Decisao

O repositorio PostgreSQL faz transicao automatica para o status `Dead`
quando `retry_count >= MaxRetries`:

```csharp
// Em MarkAsFailedAsync:
UPDATE {schema}.{table} SET
    status = CASE
        WHEN retry_count + 1 >= @maxRetries THEN 5  -- Dead
        ELSE 4                                        -- Failed (retentavel)
    END,
    retry_count = retry_count + 1,
    is_processing = FALSE,
    processing_expiration = NULL,
    processed_at = CASE
        WHEN retry_count + 1 >= @maxRetries THEN NOW()
        ELSE NULL
    END
WHERE id = @id;
```

E tambem no `ClaimNextBatchAsync`, para entries com lease expirado:

```sql
-- Branch 2 da CTE: leases expirados
CASE
    WHEN retry_count + 1 >= @maxRetries THEN 5  -- Dead
    ELSE 2                                       -- Processing (re-claim)
END
```

**Estados do ciclo de vida:**

```
Pending(1) → Processing(2) → Sent(3)        ← sucesso
                  │
                  ▼
             Failed(4) ──→ Processing(2)     ← retry (se retry_count < MaxRetries)
                  │
                  ▼
              Dead(5)                         ← MaxRetries excedido
```

**Regras fundamentais:**

1. **MaxRetries configuravel**: Default 5, configuravel via
   `OutboxPostgreSqlOptions.WithMaxRetries(byte)`.
2. **Transicao automatica**: Nao requer logica no worker — o SQL
   do repositorio faz a transicao.
3. **Sem auto-purge**: Entries `Dead` permanecem na tabela para
   investigacao manual ou replay futuro.
4. **`processed_at` preenchido**: Entries `Dead` recebem timestamp
   para indicar quando foram "finalizadas" (mesmo sem sucesso).

## Consequencias

### Beneficios

- Poison messages nao bloqueiam o processamento indefinidamente.
- Retentativas sao limitadas por design — sem surpresas em producao.
- Entries `Dead` sao preservadas para diagnostico e potencial replay.
- Logica de dead-lettering centralizada no repositorio — workers nao
  precisam implementar.

### Trade-offs (Com Perspectiva)

- **Entries `Dead` acumulam**: Sem purge automatico, a tabela cresce.
  Na pratica, um job periodico de limpeza (fora do escopo do outbox)
  pode purgar entries `Dead` mais antigas que N dias.
- **MaxRetries fixo por BC**: Todos os tipos de mensagem de um BC
  partilham o mesmo limite. Se necessario, um BC pode estender o
  repositorio e personalizar — mas na pratica 5 retries cobre a
  grande maioria dos cenarios transientes.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `OutboxEntryStatus.Dead` (valor 5) |
| Bedrock.BuildingBlocks.Outbox.PostgreSql | Implementa transicao automatica em `MarkAsFailedAsync` e `ClaimNextBatchAsync` |

## Referencias no Codigo

- Status enum: `src/BuildingBlocks/Outbox/Models/OutboxEntryStatus.cs`
- Repositorio: `src/BuildingBlocks/Outbox.PostgreSql/OutboxPostgreSqlRepositoryBase.cs`
- Opcoes: `src/BuildingBlocks/Outbox.PostgreSql/OutboxPostgreSqlOptions.cs`
- ADR relacionada: [OB-003 — Lease Pattern](./OB-003-lease-pattern-for-update-skip-locked.md)
