using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;

namespace ShopDemo.Auth.Domain.Resolvers.Interfaces;

public interface IClaimResolver
{
    Task<IReadOnlyDictionary<string, ClaimValue>> ResolveUserClaimsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);
}
