using Bedrock.BuildingBlocks.Outbox.Models;

namespace Bedrock.BuildingBlocks.Outbox.Interfaces;

/// <summary>
/// Le entries da outbox para processamento por workers.
/// Implementa o lease pattern: ClaimNextBatchAsync marca entries como
/// "em processamento" com TTL, evitando processamento duplicado.
/// </summary>
public interface IOutboxReader
{
    /// <summary>
    /// Reclama atomicamente o proximo lote de entries pendentes.
    /// Marca as entries com IsProcessing=true e define o TTL via leaseDuration.
    /// A implementacao pode usar FOR UPDATE SKIP LOCKED (PostgreSQL) ou
    /// equivalente para garantir exclusividade.
    /// </summary>
    /// <param name="batchSize">Quantidade maxima de entries a reclamar.</param>
    /// <param name="leaseDuration">Duracao do lease. Apos expirar, outro worker pode reclamar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de entries reclamadas (pode ser vazia).</returns>
    Task<IReadOnlyList<OutboxEntry>> ClaimNextBatchAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marca uma entry como enviada com sucesso.
    /// </summary>
    Task MarkAsSentAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Marca uma entry como falha. Incrementa RetryCount.
    /// Se exceder o limite de retentativas, transiciona para Dead.
    /// </summary>
    Task MarkAsFailedAsync(Guid id, CancellationToken cancellationToken);
}
