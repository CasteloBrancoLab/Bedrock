# OB-012: Telemetria e Monitorizacao do Outbox

## Status

Aceita

## Contexto

### O Problema (Analogia)

Um servico postal sem rastreamento e uma caixa negra: cartas entram,
podem ou nao chegar, e ninguem sabe onde estao. Com rastreamento
(telemetria), cada passo e visivel — recepcao, triagem, transporte,
entrega. Se uma carta atrasar, o ponto de congestionamento e
identificavel.

### O Problema Tecnico

O outbox processa entries de forma assincrona — entre a publicacao
(use case) e o envio efectivo (worker) existe uma lacuna temporal
invisivel sem instrumentacao. Problemas comuns:

1. **Backlog crescente**: Entries acumulam-se mais rapido do que sao
   processadas — sem metricas, so se descobre quando o sistema atrasa.
2. **Falhas silenciosas**: Entries falham repetidamente mas nao ha
   alerta — poison messages acumulam-se.
3. **Latencia de processamento**: O tempo entre `CreatedAt` e
   `ProcessedAt` cresce — sem traces, nao se sabe se o bottleneck e
   o broker, a rede ou o worker.
4. **Lease expirations**: Workers crasham e leases expiram — sem
   contadores, nao ha visibilidade do rate de falhas.

## A Decisao

O `OutboxPostgreSqlRepositoryBase` e instrumentado com OpenTelemetry
usando `ActivitySource` (traces) e `Meter` (metricas):

```csharp
public abstract class OutboxPostgreSqlRepositoryBase : IOutboxRepository
{
    private static readonly ActivitySource ActivitySource
        = new("Bedrock.BuildingBlocks.Outbox.PostgreSql");

    private static readonly Meter Meter
        = new("Bedrock.BuildingBlocks.Outbox.PostgreSql");

    // Contadores e histogramas definidos estaticamente
    private static readonly Counter<long> EntriesAdded = Meter.CreateCounter<long>(
        "outbox.entries.added", description: "Entries adicionados ao outbox");

    private static readonly Counter<long> EntriesClaimed = Meter.CreateCounter<long>(
        "outbox.entries.claimed", description: "Entries reclamados para processamento");

    private static readonly Counter<long> EntriesSent = Meter.CreateCounter<long>(
        "outbox.entries.sent", description: "Entries enviados com sucesso");

    private static readonly Counter<long> EntriesFailed = Meter.CreateCounter<long>(
        "outbox.entries.failed", description: "Entries que falharam");

    // Cada operacao cria um Activity span
    public async Task AddAsync(OutboxEntry entry, CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("outbox.add");
        activity?.SetTag("outbox.table", _options.TableName);
        activity?.SetTag("outbox.payload_type", entry.PayloadType);
        // ... SQL ...
        EntriesAdded.Add(1, new KeyValuePair<string, object?>(
            "outbox.table", _options.TableName));
    }
}
```

**Instrumentacao por operacao:**

| Operacao | Activity Span | Metricas |
|----------|--------------|----------|
| `AddAsync` | `outbox.add` | `outbox.entries.added` (+1) |
| `ClaimNextBatchAsync` | `outbox.claim` | `outbox.entries.claimed` (+N) |
| `MarkAsSentAsync` | `outbox.mark_sent` | `outbox.entries.sent` (+1) |
| `MarkAsFailedAsync` | `outbox.mark_failed` | `outbox.entries.failed` (+1) |

**Tags comuns:**

| Tag | Descricao | Exemplo |
|-----|-----------|---------|
| `outbox.table` | Nome da tabela | `auth_outbox` |
| `outbox.payload_type` | Tipo da mensagem | `UserRegisteredEvent` |
| `outbox.batch_size` | Tamanho do batch pedido | `10` |
| `outbox.claimed_count` | Entries efectivamente reclamados | `3` |

**Regras fundamentais:**

1. **ActivitySource e Meter estaticos**: Uma unica instancia por
   classe — sem overhead de criacao por request.
2. **Tags contextuais**: `outbox.table` distingue metricas entre BCs;
   `outbox.payload_type` distingue tipos de mensagem.
3. **Instrumentacao no building block, nao no BC**: O BC herda
   telemetria automaticamente ao usar o repositorio base.
4. **Zero-cost quando desactivado**: Se nenhum listener estiver
   subscrito, `StartActivity` retorna `null` e `Counter.Add` e no-op.

## Consequencias

### Beneficios

- Visibilidade end-to-end do ciclo de vida das entries.
- Alertas baseados em metricas: backlog > threshold, failed rate > X%.
- Dashboards operacionais (Grafana, Azure Monitor) sem instrumentacao
  custom por BC.
- Distributed tracing liga a entry ao use case que a criou (via
  `CorrelationId`).

### Trade-offs (Com Perspectiva)

- **Overhead de telemetria**: Minimal — ActivitySource e Meter sao
  optimizados para high-throughput. Quando desactivados, custo zero.
- **Nomes de metricas fixos**: Se um BC precisar de metricas custom,
  deve adicionar instrumentacao propria no writer do BC. As metricas
  base cobrem o caso comum.
- **Dependencia de OpenTelemetry SDK**: O building block referencia
  `System.Diagnostics` (built-in no .NET), nao o SDK do
  OpenTelemetry — zero dependencias externas.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox.PostgreSql | Instrumenta `ActivitySource` e `Meter` no repositorio base |
| Bedrock.BuildingBlocks.Observability | Pode definir convencoes de naming para spans e metricas |

## Referencias no Codigo

- Repositorio base: `src/BuildingBlocks/Outbox.PostgreSql/OutboxPostgreSqlRepositoryBase.cs`
- ADR relacionada: [CS-003 — Logging com Distributed Tracing](../code-style/CS-003-logging-sempre-com-distributed-tracing.md)
