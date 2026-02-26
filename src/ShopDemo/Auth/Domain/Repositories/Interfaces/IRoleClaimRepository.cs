using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.RoleClaims;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IRoleClaimRepository : IRepository<RoleClaim>
{
    Task<IReadOnlyList<RoleClaim>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Id roleId,
        CancellationToken cancellationToken);

    Task<RoleClaim?> GetByRoleIdAndClaimIdAsync(
        ExecutionContext executionContext,
        Id roleId,
        Id claimId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        RoleClaim roleClaim,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        RoleClaim roleClaim,
        CancellationToken cancellationToken);
}
