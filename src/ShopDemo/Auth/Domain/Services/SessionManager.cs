using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class SessionManager : ISessionManager
{
    private readonly ISessionRepository _sessionRepository;

    public SessionManager(
        ISessionRepository sessionRepository)
    {
        ArgumentNullException.ThrowIfNull(sessionRepository);

        _sessionRepository = sessionRepository;
    }

    public async Task<Session?> CreateSessionAsync(
        ExecutionContext executionContext,
        Id userId,
        Id refreshTokenId,
        string? deviceInfo,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset expiresAt,
        int maxActiveSessions,
        SessionLimitStrategy strategy,
        CancellationToken cancellationToken)
    {
        int activeCount = await _sessionRepository.CountActiveByUserIdAsync(
            executionContext, userId, cancellationToken);

        if (activeCount >= maxActiveSessions)
        {
            if (strategy == SessionLimitStrategy.RejectNew)
            {
                executionContext.AddErrorMessage(code: "Session.MaxActiveSessionsReached");
                return null;
            }

            if (strategy == SessionLimitStrategy.RevokeOldest)
            {
                bool revoked = await RevokeOldestSessionAsync(
                    executionContext, userId, cancellationToken);

                if (!revoked)
                    return null;
            }
        }

        var input = new RegisterNewSessionInput(
            userId,
            refreshTokenId,
            deviceInfo,
            ipAddress,
            userAgent,
            expiresAt);

        Session? session = Session.RegisterNew(executionContext, input);
        if (session is null)
            return null;

        bool registered = await _sessionRepository.RegisterNewAsync(
            executionContext, session, cancellationToken);

        return registered ? session : null;
    }

    public async Task<Session?> RevokeSessionAsync(
        ExecutionContext executionContext,
        Id sessionId,
        CancellationToken cancellationToken)
    {
        Session? session = await _sessionRepository.GetByIdAsync(
            executionContext, sessionId, cancellationToken);

        if (session is null)
            return null;

        Session? revoked = session.Revoke(executionContext, new RevokeSessionInput());
        if (revoked is null)
            return null;

        bool updated = await _sessionRepository.UpdateAsync(
            executionContext, revoked, cancellationToken);

        return updated ? revoked : null;
    }

    public async Task<int> RevokeAllSessionsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Session> activeSessions = await _sessionRepository.GetActiveByUserIdAsync(
            executionContext, userId, cancellationToken);

        int revokedCount = 0;

        foreach (Session session in activeSessions)
        {
            Session? revoked = session.Revoke(executionContext, new RevokeSessionInput());
            if (revoked is null)
                continue;

            bool updated = await _sessionRepository.UpdateAsync(
                executionContext, revoked, cancellationToken);

            if (updated)
                revokedCount++;
        }

        return revokedCount;
    }

    public async Task<Session?> UpdateActivityAsync(
        ExecutionContext executionContext,
        Id sessionId,
        CancellationToken cancellationToken)
    {
        Session? session = await _sessionRepository.GetByIdAsync(
            executionContext, sessionId, cancellationToken);

        if (session is null)
            return null;

        Session? updated = session.UpdateActivity(executionContext, new UpdateSessionActivityInput());
        if (updated is null)
            return null;

        bool persisted = await _sessionRepository.UpdateAsync(
            executionContext, updated, cancellationToken);

        return persisted ? updated : null;
    }

    private async Task<bool> RevokeOldestSessionAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Session> activeSessions = await _sessionRepository.GetActiveByUserIdAsync(
            executionContext, userId, cancellationToken);

        if (activeSessions.Count == 0)
            return false;

        Session oldest = activeSessions[0];
        foreach (Session session in activeSessions)
        {
            if (session.EntityInfo.EntityChangeInfo.CreatedAt < oldest.EntityInfo.EntityChangeInfo.CreatedAt)
                oldest = session;
        }

        Session? revoked = oldest.Revoke(executionContext, new RevokeSessionInput());
        if (revoked is null)
            return false;

        return await _sessionRepository.UpdateAsync(
            executionContext, revoked, cancellationToken);
    }
}
