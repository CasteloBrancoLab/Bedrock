using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IRefreshTokenPostgreSqlRepository
    : IPostgreSqlRepository<RefreshToken>
{
    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<RefreshToken?> GetByTokenHashAsync(
        ExecutionContext executionContext,
        TokenHash tokenHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> GetActiveByFamilyIdAsync(
        ExecutionContext executionContext,
        TokenFamily familyId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        RefreshToken aggregateRoot,
        CancellationToken cancellationToken);
}
