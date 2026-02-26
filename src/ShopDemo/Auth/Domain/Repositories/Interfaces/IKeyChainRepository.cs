using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.KeyChains;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IKeyChainRepository : IRepository<KeyChain>
{
    Task<IReadOnlyList<KeyChain>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<KeyChain?> GetByUserIdAndKeyIdAsync(
        ExecutionContext executionContext,
        Id userId,
        KeyId keyId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        KeyChain aggregateRoot,
        CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken);
}
