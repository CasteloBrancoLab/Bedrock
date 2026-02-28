using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class ApiTokenPermissionValidator : IApiTokenPermissionValidator
{
    private readonly IClaimResolver _claimResolver;
    private readonly IClaimRepository _claimRepository;

    public ApiTokenPermissionValidator(
        IClaimResolver claimResolver,
        IClaimRepository claimRepository
    )
    {
        _claimResolver = claimResolver ?? throw new ArgumentNullException(nameof(claimResolver));
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
    }

    public async Task<bool> ValidatePermissionCeilingAsync(
        ExecutionContext executionContext,
        Id creatorUserId,
        IReadOnlyDictionary<Id, ClaimValue> requestedClaims,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, ClaimValue> creatorClaims = await _claimResolver.ResolveUserClaimsAsync(
            executionContext,
            creatorUserId,
            cancellationToken);

        IReadOnlyList<Claim> allClaims = await _claimRepository.GetAllAsync(
            executionContext,
            cancellationToken);

        var claimIdToName = new Dictionary<Id, string>();
        foreach (Claim claim in allClaims)
        {
            claimIdToName[claim.EntityInfo.Id] = claim.Name;
        }

        foreach (var (claimId, requestedValue) in requestedClaims)
        {
            if (!claimIdToName.TryGetValue(claimId, out string? claimName))
            {
                executionContext.AddErrorMessage(
                    code: "ApiTokenPermissionValidator.ClaimNotFound");
                return false;
            }

            if (!creatorClaims.TryGetValue(claimName, out ClaimValue creatorValue))
            {
                executionContext.AddErrorMessage(
                    code: "ApiTokenPermissionValidator.CreatorLacksPermission");
                return false;
            }

            if (requestedValue.Value > creatorValue.Value)
            {
                executionContext.AddErrorMessage(
                    code: "ApiTokenPermissionValidator.ExceedsCreatorPermission");
                return false;
            }
        }

        return true;
    }
}
