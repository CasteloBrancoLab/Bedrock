using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ApiKeys;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IApiKeyPostgreSqlRepository
    : IPostgreSqlRepository<ApiKey>
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
