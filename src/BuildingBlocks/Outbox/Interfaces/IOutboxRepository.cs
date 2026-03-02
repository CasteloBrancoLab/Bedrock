using Bedrock.BuildingBlocks.Outbox.Models;

namespace Bedrock.BuildingBlocks.Outbox.Interfaces;

/// <summary>
/// Repositorio de persistencia para a outbox.
/// Implementado pela camada de infraestrutura (Infra.Data.PostgreSql).
/// Combina as operacoes de escrita (writer) e leitura (reader) em um unico contrato.
/// </summary>
public interface IOutboxRepository : IOutboxReader
{
    /// <summary>
    /// Persiste uma nova entry na outbox.
    /// Deve participar da transacao corrente (Unit of Work).
    /// </summary>
    Task AddAsync(OutboxEntry entry, CancellationToken cancellationToken);
}
