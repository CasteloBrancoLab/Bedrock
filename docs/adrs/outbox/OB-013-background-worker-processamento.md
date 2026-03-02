# OB-013: Estrategia de Background Worker para Processamento

## Status

Aceita

## Contexto

### O Problema (Analogia)

Um armazem de encomendas precisa de operadores que periodicamente vao
a zona de despacho, pegam num lote de encomendas e processam-nas.
Sem operadores, as encomendas acumulam-se indefinidamente. O armazem
pode ter operadores internos (funcionarios permanentes) ou externos
(servico contratado) — o importante e que alguem processe as
encomendas regularmente.

### O Problema Tecnico

O outbox e write-only por design: use cases publicam entries via
`IOutboxWriter`, mas ninguem os consome automaticamente. E necessario
um **processador** que periodicamente:

1. Reclame um batch de entries (`ClaimNextBatchAsync`).
2. Envie cada entry ao destino (message broker, HTTP, etc.).
3. Marque como `Sent` ou `Failed` (`MarkAsSentAsync`/`MarkAsFailedAsync`).

Sem um processador, a tabela outbox cresce indefinidamente e os eventos
nunca chegam ao destino. A questao e: **onde e como implementar este
processador?**

## A Decisao

A arquitectura separa o **contrato** do processador da **implementacao
de hosting**:

### Contrato (Building Block)

```csharp
// Bedrock.BuildingBlocks.Outbox/Interfaces/IOutboxProcessor.cs
public interface IOutboxProcessor
{
    Task<int> ProcessBatchAsync(int batchSize, CancellationToken cancellationToken);
}

// Bedrock.BuildingBlocks.Outbox.Messages/Interfaces/IMessageOutboxProcessor.cs
public interface IMessageOutboxProcessor : IOutboxProcessor
{
    // Marker interface — especializa para mensagens
}
```

`IOutboxProcessor` define o contrato: processar um batch e retornar
quantos entries foram processados. `IMessageOutboxProcessor` e a
marker para mensagens (analogia com OB-011).

### Implementacao (Responsabilidade do Host)

O building block **nao fornece** um `BackgroundService` ou `IHostedService`.
A razao e que a estrategia de hosting varia:

| Estrategia | Quando usar |
|------------|-------------|
| `BackgroundService` (timer loop) | Aplicacao ASP.NET Core monolitica |
| Azure Functions (timer trigger) | Serverless / event-driven |
| Kubernetes CronJob | Microservicos containerizados |
| Worker Service dedicado | Alto throughput, isolamento |
| Trigger por CDC (Debezium) | Change Data Capture — sem polling |

O host da aplicacao escolhe a estrategia e invoca `IOutboxProcessor`:

```csharp
// Exemplo: BackgroundService com timer
public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var processor = scope.ServiceProvider
                .GetRequiredService<IMessageOutboxProcessor>();

            var processed = await processor.ProcessBatchAsync(
                batchSize: 10, cancellationToken: ct);

            // Se processou entries, continua imediatamente;
            // senao, espera antes do proximo poll.
            if (processed == 0)
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
```

**Regras fundamentais:**

1. **Building block define contrato, nao hosting**: `IOutboxProcessor`
   e a interface; o host implementa o loop.
2. **Scoped por iteracao**: Cada iteracao do worker cria um novo scope
   DI — repositorio, UoW e conexao sao frescos.
3. **Adaptive polling**: Se o batch retornar 0 entries, o worker
   espera (backoff); se retornar entries, processa imediatamente o
   proximo batch — maximiza throughput sob carga.
4. **Multiplos workers**: O lease pattern (OB-003) garante que N
   workers podem correr em paralelo sem duplicacao.
5. **Marker interface por BC**: `IMessageOutboxProcessor` especializa
   para mensagens — permite DI type-safe quando multiplos tipos de
   outbox coexistem.

### Fluxo do processador

```
Worker loop
    │
    ▼
IMessageOutboxProcessor.ProcessBatchAsync(batchSize)
    │
    ├── IOutboxReader.ClaimNextBatchAsync(batchSize, leaseDuration)
    │       └── SQL: FOR UPDATE SKIP LOCKED (OB-003)
    │
    ├── Para cada entry reclamado:
    │   ├── IOutboxDeserializer.Deserialize(payload, payloadType, contentType)
    │   │       └── Duas fases (OB-007)
    │   ├── Enviar ao destino (broker, HTTP, etc.)
    │   └── MarkAsSent ou MarkAsFailed
    │
    └── Retorna count de entries processados
```

## Consequencias

### Beneficios

- Flexibilidade de hosting: cada equipa escolhe a estrategia adequada.
- Building block reutilizavel: o mesmo `IOutboxProcessor` funciona em
  qualquer host.
- Testabilidade: o processador e testavel sem host (unit test com
  mocks de repository e deserializer).
- Escalabilidade: multiplos workers em paralelo via lease pattern.

### Trade-offs (Com Perspectiva)

- **Sem implementacao "out-of-the-box"**: O host deve implementar o
  loop. Na pratica, sao ~20 linhas de `BackgroundService`. Futuros
  building blocks podem fornecer implementacoes para hosts comuns
  (ASP.NET Core, Azure Functions).
- **Polling**: Sem CDC ou notification, o worker faz poll periodico.
  O custo de um `SELECT` com partial index (OB-010) e negligivel.
  Para latencia sub-segundo, pode-se usar `LISTEN/NOTIFY` do
  PostgreSQL como optimizacao futura.
- **Delay configuration**: O intervalo de backoff (ex: 5s) e
  responsabilidade do host, nao do building block. Nao ha default
  centralizado.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Polling Consumer** (Hohpe & Woolf, 2003): Consumidor que
  periodicamente verifica por novas mensagens.
- **Competing Consumers** (Hohpe & Woolf, 2003): Multiplos consumers
  processam em paralelo — implementado via lease pattern (OB-003).
- **Strategy** (GoF): A estrategia de hosting e injectavel — o
  processador nao conhece o host.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `IOutboxProcessor` |
| Bedrock.BuildingBlocks.Outbox.Messages | Define `IMessageOutboxProcessor` (marker) |
| Bedrock.BuildingBlocks.Outbox.PostgreSql | Implementa `ClaimNextBatchAsync` usado pelo processador |

## Referencias no Codigo

- Interface base: `src/BuildingBlocks/Outbox/Interfaces/IOutboxProcessor.cs`
- Marker: `src/BuildingBlocks/Outbox.Messages/Interfaces/IMessageOutboxProcessor.cs`
- Reader: `src/BuildingBlocks/Outbox/Interfaces/IOutboxReader.cs`
- ADR relacionada: [OB-003 — Lease Pattern](./OB-003-lease-pattern-for-update-skip-locked.md)
- ADR relacionada: [OB-007 — Desserializacao em Duas Fases](./OB-007-desserializacao-duas-fases.md)
- ADR relacionada: [OB-010 — Indices Parciais](./OB-010-indices-parciais-estrategicos.md)
