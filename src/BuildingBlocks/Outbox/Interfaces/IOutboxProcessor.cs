namespace Bedrock.BuildingBlocks.Outbox.Interfaces;

/// <summary>
/// Processa entries da outbox em lote.
/// Implementado por workers (background services) que leem da outbox
/// e entregam ao destino (broker, webhook endpoint, etc.).
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Reclama e processa o proximo lote de entries.
    /// </summary>
    /// <param name="batchSize">Quantidade maxima de entries a processar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Numero de entries processadas com sucesso.</returns>
    Task<int> ProcessBatchAsync(int batchSize, CancellationToken cancellationToken);
}
