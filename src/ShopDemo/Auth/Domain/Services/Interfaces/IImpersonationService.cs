using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IImpersonationService
{
    Task<ImpersonationSession?> ValidateAndCreateAsync(
        ExecutionContext executionContext,
        Id operatorUserId,
        Id targetUserId,
        CancellationToken cancellationToken);

    Task<ImpersonationSession?> EndSessionAsync(
        ExecutionContext executionContext,
        Id sessionId,
        CancellationToken cancellationToken);
}
