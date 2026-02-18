using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IApiTokenPermissionValidator
{
    Task<bool> ValidatePermissionCeilingAsync(
        ExecutionContext executionContext,
        Id creatorUserId,
        IReadOnlyDictionary<Id, ClaimValue> requestedClaims,
        CancellationToken cancellationToken);
}
