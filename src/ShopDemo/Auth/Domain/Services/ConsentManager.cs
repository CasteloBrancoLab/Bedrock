using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.UserConsents;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;
using ShopDemo.Auth.Domain.Entities.UserConsents.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class ConsentManager : IConsentManager
{
    private readonly IConsentTermRepository _consentTermRepository;
    private readonly IUserConsentRepository _userConsentRepository;

    public ConsentManager(
        IConsentTermRepository consentTermRepository,
        IUserConsentRepository userConsentRepository)
    {
        ArgumentNullException.ThrowIfNull(consentTermRepository);
        ArgumentNullException.ThrowIfNull(userConsentRepository);

        _consentTermRepository = consentTermRepository;
        _userConsentRepository = userConsentRepository;
    }

    public async Task<IReadOnlyList<ConsentTerm>> CheckPendingConsentsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        var pendingConsents = new List<ConsentTerm>();

        ConsentTermType[] consentTypes =
        [
            ConsentTermType.TermsOfUse,
            ConsentTermType.PrivacyPolicy,
            ConsentTermType.Marketing
        ];

        IReadOnlyList<UserConsent> userConsents = await _userConsentRepository.GetByUserIdAsync(
            executionContext,
            userId,
            cancellationToken);

        foreach (ConsentTermType consentType in consentTypes)
        {
            ConsentTerm? latestTerm = await _consentTermRepository.GetLatestByTypeAsync(
                executionContext,
                consentType,
                cancellationToken);

            if (latestTerm is null)
                continue;

            bool hasActiveConsent = userConsents.Any(uc =>
                uc.ConsentTermId == latestTerm.EntityInfo.Id
                && uc.Status == UserConsentStatus.Active);

            if (!hasActiveConsent)
                pendingConsents.Add(latestTerm);
        }

        return pendingConsents;
    }

    public async Task<UserConsent?> RecordConsentAsync(
        ExecutionContext executionContext,
        Id userId,
        Id consentTermId,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        bool consentTermExists = await _consentTermRepository.ExistsAsync(
            executionContext,
            consentTermId,
            cancellationToken);

        if (!consentTermExists)
        {
            executionContext.AddErrorMessage(code: "ConsentManager.ConsentTermNotFound");
            return null;
        }

        UserConsent? existingConsent = await _userConsentRepository.GetActiveByUserIdAndConsentTermIdAsync(
            executionContext,
            userId,
            consentTermId,
            cancellationToken);

        if (existingConsent is not null)
        {
            executionContext.AddErrorMessage(code: "ConsentManager.ConsentAlreadyActive");
            return null;
        }

        UserConsent? userConsent = UserConsent.RegisterNew(
            executionContext,
            new RegisterNewUserConsentInput(
                userId,
                consentTermId,
                ipAddress));

        if (userConsent is null)
            return null;

        bool registered = await _userConsentRepository.RegisterNewAsync(
            executionContext,
            userConsent,
            cancellationToken);

        if (!registered)
            return null;

        return userConsent;
    }

    public async Task<UserConsent?> RevokeConsentAsync(
        ExecutionContext executionContext,
        Id userId,
        Id consentTermId,
        CancellationToken cancellationToken)
    {
        UserConsent? existingConsent = await _userConsentRepository.GetActiveByUserIdAndConsentTermIdAsync(
            executionContext,
            userId,
            consentTermId,
            cancellationToken);

        if (existingConsent is null)
        {
            executionContext.AddErrorMessage(code: "ConsentManager.ActiveConsentNotFound");
            return null;
        }

        UserConsent? revokedConsent = existingConsent.Revoke(
            executionContext,
            new RevokeUserConsentInput());

        if (revokedConsent is null)
            return null;

        bool updated = await _userConsentRepository.UpdateAsync(
            executionContext,
            revokedConsent,
            cancellationToken);

        if (!updated)
            return null;

        return revokedConsent;
    }
}
