using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Events;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface ICascadeRevocationService
{
    Task<UserDeactivatedEvent?> RevokeAllUserTokensAsync(
        ExecutionContext executionContext,
        Id userId,
        string? reason,
        CancellationToken cancellationToken);

    Task<UserPermissionsChangedEvent?> RecalculateApiTokenPermissionsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);
}
