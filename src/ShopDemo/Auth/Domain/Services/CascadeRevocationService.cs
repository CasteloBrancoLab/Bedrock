using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims.Inputs;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;
using ShopDemo.Auth.Domain.Events;
using ShopDemo.Auth.Domain.Events.Models;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class CascadeRevocationService : ICascadeRevocationService
{
    private static readonly TimeSpan DenyListExpirationForDeactivation = TimeSpan.FromDays(3650);

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IServiceClientRepository _serviceClientRepository;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IServiceClientClaimRepository _serviceClientClaimRepository;
    private readonly IDenyListService _denyListService;
    private readonly IClaimResolver _claimResolver;
    private readonly IClaimRepository _claimRepository;

    public CascadeRevocationService(
        IRefreshTokenRepository refreshTokenRepository,
        IServiceClientRepository serviceClientRepository,
        IApiKeyRepository apiKeyRepository,
        IServiceClientClaimRepository serviceClientClaimRepository,
        IDenyListService denyListService,
        IClaimResolver claimResolver,
        IClaimRepository claimRepository)
    {
        ArgumentNullException.ThrowIfNull(refreshTokenRepository);
        ArgumentNullException.ThrowIfNull(serviceClientRepository);
        ArgumentNullException.ThrowIfNull(apiKeyRepository);
        ArgumentNullException.ThrowIfNull(serviceClientClaimRepository);
        ArgumentNullException.ThrowIfNull(denyListService);
        ArgumentNullException.ThrowIfNull(claimResolver);
        ArgumentNullException.ThrowIfNull(claimRepository);

        _refreshTokenRepository = refreshTokenRepository;
        _serviceClientRepository = serviceClientRepository;
        _apiKeyRepository = apiKeyRepository;
        _serviceClientClaimRepository = serviceClientClaimRepository;
        _denyListService = denyListService;
        _claimResolver = claimResolver;
        _claimRepository = claimRepository;
    }

    public async Task<UserDeactivatedEvent?> RevokeAllUserTokensAsync(
        ExecutionContext executionContext,
        Id userId,
        string? reason,
        CancellationToken cancellationToken)
    {
        int revokedRefreshTokenCount = await RevokeRefreshTokensAsync(
            executionContext, userId, cancellationToken);

        (int revokedServiceClientCount, int revokedApiKeyCount) = await RevokeServiceClientsAndApiKeysAsync(
            executionContext, userId, cancellationToken);

        await _denyListService.RevokeUserAsync(
            executionContext,
            userId.Value.ToString(),
            executionContext.Timestamp.Add(DenyListExpirationForDeactivation),
            reason,
            cancellationToken);

        return new UserDeactivatedEvent(
            userId,
            reason,
            revokedRefreshTokenCount,
            revokedServiceClientCount,
            revokedApiKeyCount);
    }

    public async Task<UserPermissionsChangedEvent?> RecalculateApiTokenPermissionsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, ClaimValue> userClaims = await _claimResolver.ResolveUserClaimsAsync(
            executionContext, userId, cancellationToken);

        IReadOnlyList<Claim> allClaims = await _claimRepository.GetAllAsync(
            executionContext, cancellationToken);

        Dictionary<Id, string> claimIdToName = [];
        foreach (Claim claim in allClaims)
            claimIdToName[claim.EntityInfo.Id] = claim.Name;

        IReadOnlyList<ServiceClient> serviceClients = await _serviceClientRepository.GetByCreatorUserIdAsync(
            executionContext, userId, cancellationToken);

        var allChangedClaims = new List<ChangedClaimInfo>();

        foreach (ServiceClient serviceClient in serviceClients)
        {
            if (serviceClient.Status != ServiceClientStatus.Active)
                continue;

            IReadOnlyList<ChangedClaimInfo> changedClaims = await RecalculateServiceClientClaimsAsync(
                executionContext, serviceClient, userClaims, claimIdToName, cancellationToken);

            allChangedClaims.AddRange(changedClaims);
        }

        if (allChangedClaims.Count == 0)
            return null;

        return new UserPermissionsChangedEvent(userId, allChangedClaims);
    }

    private async Task<int> RevokeRefreshTokensAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RefreshToken> refreshTokens = await _refreshTokenRepository.GetByUserIdAsync(
            executionContext, userId, cancellationToken);

        int revokedCount = 0;

        foreach (RefreshToken refreshToken in refreshTokens)
        {
            if (refreshToken.Status != RefreshTokenStatus.Active)
                continue;

            RefreshToken? revoked = refreshToken.Revoke(executionContext, new RevokeRefreshTokenInput());
            if (revoked is null)
                continue;

            bool updated = await _refreshTokenRepository.UpdateAsync(
                executionContext, revoked, cancellationToken);

            if (updated)
                revokedCount++;
        }

        return revokedCount;
    }

    private async Task<(int ServiceClientCount, int ApiKeyCount)> RevokeServiceClientsAndApiKeysAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceClient> serviceClients = await _serviceClientRepository.GetByCreatorUserIdAsync(
            executionContext, userId, cancellationToken);

        int revokedServiceClientCount = 0;
        int revokedApiKeyCount = 0;

        foreach (ServiceClient serviceClient in serviceClients)
        {
            if (serviceClient.Status != ServiceClientStatus.Active)
                continue;

            ServiceClient? revokedClient = serviceClient.Revoke(executionContext, new RevokeServiceClientInput());
            if (revokedClient is null)
                continue;

            bool updated = await _serviceClientRepository.UpdateAsync(
                executionContext, revokedClient, cancellationToken);

            if (!updated)
                continue;

            revokedServiceClientCount++;

            revokedApiKeyCount += await RevokeApiKeysForServiceClientAsync(
                executionContext, serviceClient.EntityInfo.Id, cancellationToken);

            await _serviceClientClaimRepository.DeleteByServiceClientIdAsync(
                executionContext, serviceClient.EntityInfo.Id, cancellationToken);
        }

        return (revokedServiceClientCount, revokedApiKeyCount);
    }

    private async Task<int> RevokeApiKeysForServiceClientAsync(
        ExecutionContext executionContext,
        Id serviceClientId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ApiKey> apiKeys = await _apiKeyRepository.GetByServiceClientIdAsync(
            executionContext, serviceClientId, cancellationToken);

        int revokedCount = 0;

        foreach (ApiKey apiKey in apiKeys)
        {
            if (apiKey.Status != ApiKeyStatus.Active)
                continue;

            ApiKey? revokedKey = apiKey.Revoke(executionContext, new RevokeApiKeyInput());
            if (revokedKey is null)
                continue;

            bool keyUpdated = await _apiKeyRepository.UpdateAsync(
                executionContext, revokedKey, cancellationToken);

            if (keyUpdated)
                revokedCount++;
        }

        return revokedCount;
    }

    private async Task<IReadOnlyList<ChangedClaimInfo>> RecalculateServiceClientClaimsAsync(
        ExecutionContext executionContext,
        ServiceClient serviceClient,
        IReadOnlyDictionary<string, ClaimValue> userClaims,
        Dictionary<Id, string> claimIdToName,
        CancellationToken cancellationToken)
    {
        Id serviceClientId = serviceClient.EntityInfo.Id;

        IReadOnlyList<ServiceClientClaim> existingClaims = await _serviceClientClaimRepository.GetByServiceClientIdAsync(
            executionContext, serviceClientId, cancellationToken);

        var changedClaims = new List<ChangedClaimInfo>();
        var needsUpdate = false;

        foreach (ServiceClientClaim existingClaim in existingClaims)
        {
            ClaimValue oldValue = existingClaim.Value;
            ClaimValue newValue = CalculateCeilingValue(existingClaim.ClaimId, oldValue, userClaims, claimIdToName);

            if (oldValue.Value != newValue.Value)
            {
                changedClaims.Add(new ChangedClaimInfo(existingClaim.ClaimId, oldValue, newValue));
                needsUpdate = true;
            }
        }

        if (!needsUpdate)
            return changedClaims;

        await _serviceClientClaimRepository.DeleteByServiceClientIdAsync(
            executionContext, serviceClientId, cancellationToken);

        foreach (ServiceClientClaim existingClaim in existingClaims)
        {
            ClaimValue newValue = CalculateCeilingValue(existingClaim.ClaimId, existingClaim.Value, userClaims, claimIdToName);

            var input = new RegisterNewServiceClientClaimInput(
                serviceClientId,
                existingClaim.ClaimId,
                newValue);

            ServiceClientClaim? newClaim = ServiceClientClaim.RegisterNew(executionContext, input);
            if (newClaim is not null)
            {
                await _serviceClientClaimRepository.RegisterNewAsync(
                    executionContext, newClaim, cancellationToken);
            }
        }

        return changedClaims;
    }

    private static ClaimValue CalculateCeilingValue(
        Id claimId,
        ClaimValue tokenClaimValue,
        IReadOnlyDictionary<string, ClaimValue> userClaims,
        Dictionary<Id, string> claimIdToName)
    {
        if (!claimIdToName.TryGetValue(claimId, out string? claimName))
            return ClaimValue.Denied;

        if (!userClaims.TryGetValue(claimName, out ClaimValue userClaimValue))
            return ClaimValue.Denied;

        short minValue = Math.Min(tokenClaimValue.Value, userClaimValue.Value);
        return ClaimValue.CreateNew(minValue);
    }
}
