using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IClaimDependencyPostgreSqlRepository
    : IPostgreSqlRepository<ClaimDependency>
{
    Task<IReadOnlyList<ClaimDependency>> GetByClaimIdAsync(
        ExecutionContext executionContext,
        Id claimId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ClaimDependency>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        ClaimDependency claimDependency,
        CancellationToken cancellationToken);
}
