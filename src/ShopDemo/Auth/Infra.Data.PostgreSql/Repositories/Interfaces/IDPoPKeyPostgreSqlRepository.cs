using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IDPoPKeyPostgreSqlRepository
    : IPostgreSqlRepository<DPoPKey>
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
