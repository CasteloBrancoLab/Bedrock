using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IDPoPKeyRepository : IRepository<DPoPKey>
{
    Task<DPoPKey?> GetActiveByUserIdAndThumbprintAsync(
        ExecutionContext executionContext,
        Id userId,
        JwkThumbprint jwkThumbprint,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DPoPKey>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        DPoPKey aggregateRoot,
        CancellationToken cancellationToken);
}
