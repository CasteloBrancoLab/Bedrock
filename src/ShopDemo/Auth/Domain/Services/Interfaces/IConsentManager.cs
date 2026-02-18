using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.UserConsents;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IConsentManager
{
    Task<IReadOnlyList<ConsentTerm>> CheckPendingConsentsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<UserConsent?> RecordConsentAsync(
        ExecutionContext executionContext,
        Id userId,
        Id consentTermId,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<UserConsent?> RevokeConsentAsync(
        ExecutionContext executionContext,
        Id userId,
        Id consentTermId,
        CancellationToken cancellationToken);
}
