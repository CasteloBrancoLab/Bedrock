using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ApiKeys;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IApiKeyRepository : IRepository<ApiKey>
{
    Task<ApiKey?> GetByKeyHashAsync(
        ExecutionContext executionContext,
        string keyHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ApiKey>> GetByServiceClientIdAsync(
        ExecutionContext executionContext,
        Id serviceClientId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        ApiKey aggregateRoot,
        CancellationToken cancellationToken);
}
