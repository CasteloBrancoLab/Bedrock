using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IClaimDependencyRepository : IRepository<ClaimDependency>
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
