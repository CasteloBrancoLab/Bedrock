namespace Bedrock.BuildingBlocks.Outbox.Interfaces;

/// <summary>
/// Grava payloads na outbox transacional.
/// O writer serializa o payload e cria um <see cref="Models.OutboxEntry"/>
/// na mesma transacao que persiste a entidade de dominio.
/// </summary>
/// <typeparam name="TPayload">Tipo do payload (ex: MessageBase para messages).</typeparam>
public interface IOutboxWriter<in TPayload>
{
    /// <summary>
    /// Serializa o payload e enfileira na outbox.
    /// A entry sera persistida junto com a transacao corrente (Unit of Work).
    /// </summary>
    /// <param name="payload">Payload a ser enfileirado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task EnqueueAsync(TPayload payload, CancellationToken cancellationToken);
}
