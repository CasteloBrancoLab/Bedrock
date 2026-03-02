using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Services.Outputs;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface ICascadeRevocationService
{
    Task<UserDeactivationOutput?> RevokeAllUserTokensAsync(
        ExecutionContext executionContext,
        Id userId,
        string? reason,
        CancellationToken cancellationToken);

    Task<PermissionsRecalculationOutput?> RecalculateApiTokenPermissionsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);
}
