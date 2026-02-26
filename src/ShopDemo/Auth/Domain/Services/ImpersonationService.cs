using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Resolvers.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class ImpersonationService : IImpersonationService
{
    private const string CanImpersonateClaimName = "can_impersonate";
    private const string IsImpersonatableClaimName = "is_impersonatable";
    private const int DefaultSessionDurationMinutes = 30;

    private readonly IImpersonationSessionRepository _impersonationSessionRepository;
    private readonly IClaimResolver _claimResolver;

    public ImpersonationService(
        IImpersonationSessionRepository impersonationSessionRepository,
        IClaimResolver claimResolver)
    {
        ArgumentNullException.ThrowIfNull(impersonationSessionRepository);
        ArgumentNullException.ThrowIfNull(claimResolver);

        _impersonationSessionRepository = impersonationSessionRepository;
        _claimResolver = claimResolver;
    }

    public async Task<ImpersonationSession?> ValidateAndCreateAsync(
        ExecutionContext executionContext,
        Id operatorUserId,
        Id targetUserId,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, ClaimValue> operatorClaims = await _claimResolver.ResolveUserClaimsAsync(
            executionContext, operatorUserId, cancellationToken);

        if (!HasGrantedClaim(operatorClaims, CanImpersonateClaimName))
        {
            executionContext.AddErrorMessage(code: "ImpersonationSession.OperatorNotAuthorized");
            return null;
        }

        IReadOnlyDictionary<string, ClaimValue> targetClaims = await _claimResolver.ResolveUserClaimsAsync(
            executionContext, targetUserId, cancellationToken);

        if (IsExplicitlyDenied(targetClaims, IsImpersonatableClaimName))
        {
            executionContext.AddErrorMessage(code: "ImpersonationSession.TargetNotImpersonatable");
            return null;
        }

        ImpersonationSession? activeOperatorSession = await _impersonationSessionRepository.GetActiveByTargetUserIdAsync(
            executionContext, operatorUserId, cancellationToken);

        if (activeOperatorSession is not null)
        {
            executionContext.AddErrorMessage(code: "ImpersonationSession.ChainImpersonationNotAllowed");
            return null;
        }

        var input = new RegisterNewImpersonationSessionInput(
            operatorUserId,
            targetUserId,
            executionContext.Timestamp.AddMinutes(DefaultSessionDurationMinutes));

        ImpersonationSession? session = ImpersonationSession.RegisterNew(executionContext, input);
        if (session is null)
            return null;

        bool registered = await _impersonationSessionRepository.RegisterNewAsync(
            executionContext, session, cancellationToken);

        return registered ? session : null;
    }

    public async Task<ImpersonationSession?> EndSessionAsync(
        ExecutionContext executionContext,
        Id sessionId,
        CancellationToken cancellationToken)
    {
        ImpersonationSession? session = await _impersonationSessionRepository.GetByIdAsync(
            executionContext, sessionId, cancellationToken);

        if (session is null)
            return null;

        ImpersonationSession? ended = session.End(executionContext, new EndImpersonationSessionInput());
        if (ended is null)
            return null;

        bool updated = await _impersonationSessionRepository.UpdateAsync(
            executionContext, ended, cancellationToken);

        return updated ? ended : null;
    }

    private static bool HasGrantedClaim(
        IReadOnlyDictionary<string, ClaimValue> claims,
        string claimName)
    {
        return claims.TryGetValue(claimName, out ClaimValue value) && value.IsGranted;
    }

    private static bool IsExplicitlyDenied(
        IReadOnlyDictionary<string, ClaimValue> claims,
        string claimName)
    {
        return claims.TryGetValue(claimName, out ClaimValue value) && value.IsDenied;
    }
}
