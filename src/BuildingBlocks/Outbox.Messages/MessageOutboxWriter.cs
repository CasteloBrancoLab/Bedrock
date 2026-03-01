using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Outbox.Interfaces;
using Bedrock.BuildingBlocks.Outbox.Models;

namespace Bedrock.BuildingBlocks.Outbox.Messages;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: MessageOutboxWriter - Enfileira Messages na Outbox
═══════════════════════════════════════════════════════════════════════════════

Orquestra a gravacao de uma message na outbox transacional:
1. Serializa a message via IOutboxSerializer<MessageBase>
2. Extrai metadata (CorrelationId, TenantCode, SchemaName) do envelope
3. Cria o OutboxEntry com todos os campos preenchidos
4. Delega ao IOutboxRepository para persistir na transacao corrente

O Domain Service chama EnqueueAsync() e o entry participa da mesma
transacao que persiste a entidade (Unit of Work pattern).

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Implementa <see cref="IOutboxWriter{TPayload}"/> para <see cref="MessageBase"/>.
/// Extrai automaticamente metadata do envelope e delega a serializacao
/// ao <see cref="IOutboxSerializer{TPayload}"/>.
/// </summary>
public sealed class MessageOutboxWriter : IOutboxWriter<MessageBase>
{
    private readonly IOutboxRepository _repository;
    private readonly IOutboxSerializer<MessageBase> _serializer;
    private readonly TimeProvider _timeProvider;

    public MessageOutboxWriter(
        IOutboxRepository repository,
        IOutboxSerializer<MessageBase> serializer,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _serializer = serializer;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public Task EnqueueAsync(MessageBase payload, CancellationToken cancellationToken)
    {
        var metadata = payload.Metadata;

        var entry = new OutboxEntry
        {
            Id = Id.GenerateNewId(_timeProvider).Value,
            TenantCode = metadata.TenantCode,
            CorrelationId = metadata.CorrelationId,
            PayloadType = metadata.SchemaName,
            ContentType = _serializer.ContentType,
            Payload = _serializer.Serialize(payload),
            CreatedAt = _timeProvider.GetUtcNow(),
            Status = OutboxEntryStatus.Pending,
            ProcessedAt = null,
            RetryCount = 0,
            IsProcessing = false,
            ProcessingExpiration = null
        };

        return _repository.AddAsync(entry, cancellationToken);
    }
}
