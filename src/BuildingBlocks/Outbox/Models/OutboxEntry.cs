namespace Bedrock.BuildingBlocks.Outbox.Models;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: OutboxEntry - Registro Generico de Entrega Garantida
═══════════════════════════════════════════════════════════════════════════════

OutboxEntry representa um item na outbox transacional. NAO e uma entidade
de dominio (nao herda EntityBase) — e infraestrutura de entrega.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Payload como byte[] — Agnositco de Formato
───────────────────────────────────────────────────────────────────────────────

O payload e gravado como byte[] (bytea no PostgreSQL). O formato real
(JSON, Avro, Protobuf) e indicado pelo ContentType. Isso permite:
- Trocar de formato sem alterar a tabela
- Gravar e ler sem conversao intermediaria

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Lease Pattern para Concorrencia
───────────────────────────────────────────────────────────────────────────────

IsProcessing + ProcessingExpiration implementam o lease pattern:
- Worker faz UPDATE atomico marcando IsProcessing=true com TTL
- Se worker morrer, lease expira e outro worker retoma
- Nao requer lock externo (Redis, etc.)

A implementacao PostgreSQL adiciona FOR UPDATE SKIP LOCKED como otimizacao.

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Registro generico na outbox transacional. Armazena um payload serializado
/// com metadados de roteamento e controle de lease para processamento concorrente.
/// </summary>
public sealed record OutboxEntry
{
    /// <summary>
    /// Identificador unico da entry (UUIDv7 monotonic).
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Codigo do tenant para roteamento multi-tenant.
    /// </summary>
    public required Guid TenantCode { get; init; }

    /// <summary>
    /// Identificador de correlacao para distributed tracing.
    /// </summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>
    /// Discriminador do tipo de payload (ex: SchemaName de uma message).
    /// Usado pelo processor para saber como deserializar e rotear.
    /// </summary>
    public required string PayloadType { get; init; }

    /// <summary>
    /// Tipo de conteudo do payload serializado (ex: "application/json", "application/avro").
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Dados serializados do payload (bytea no PostgreSQL).
    /// </summary>
    public required byte[] Payload { get; init; }

    /// <summary>
    /// Momento da criacao da entry.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Status do ciclo de vida.
    /// </summary>
    public required OutboxEntryStatus Status { get; init; }

    /// <summary>
    /// Momento em que a entry foi processada com sucesso. Null se ainda nao processada.
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; init; }

    /// <summary>
    /// Numero de tentativas de processamento.
    /// </summary>
    public byte RetryCount { get; init; }

    /// <summary>
    /// Flag indicando se a entry esta sendo processada por um worker (lease ativo).
    /// </summary>
    public bool IsProcessing { get; init; }

    /// <summary>
    /// Data/hora UTC de expiracao do lease de processamento.
    /// Apos este momento, outro worker pode reclamar a entry.
    /// </summary>
    public DateTimeOffset? ProcessingExpiration { get; init; }
}
