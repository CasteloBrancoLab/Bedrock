using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;

namespace ShopDemo.Auth.Domain.Validators.Interfaces;

public interface IClaimDependencyValidator
{
    Task<bool> ValidateClaimDependenciesAsync(
        ExecutionContext executionContext,
        Id roleId,
        Id claimId,
        ClaimValue value,
        CancellationToken cancellationToken);
}
