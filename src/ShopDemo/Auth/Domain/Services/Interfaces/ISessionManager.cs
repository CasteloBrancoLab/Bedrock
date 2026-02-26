using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface ISessionManager
{
    Task<Session?> CreateSessionAsync(
        ExecutionContext executionContext,
        Id userId,
        Id refreshTokenId,
        string? deviceInfo,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset expiresAt,
        int maxActiveSessions,
        SessionLimitStrategy strategy,
        CancellationToken cancellationToken);

    Task<Session?> RevokeSessionAsync(
        ExecutionContext executionContext,
        Id sessionId,
        CancellationToken cancellationToken);

    Task<int> RevokeAllSessionsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<Session?> UpdateActivityAsync(
        ExecutionContext executionContext,
        Id sessionId,
        CancellationToken cancellationToken);
}
