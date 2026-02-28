using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Domain.Entities.UserRoles;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class ClaimResolver : IClaimResolver
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleClaimRepository _roleClaimRepository;
    private readonly IRoleHierarchyRepository _roleHierarchyRepository;
    private readonly IClaimRepository _claimRepository;

    public ClaimResolver(
        IUserRoleRepository userRoleRepository,
        IRoleClaimRepository roleClaimRepository,
        IRoleHierarchyRepository roleHierarchyRepository,
        IClaimRepository claimRepository
    )
    {
        _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
        _roleClaimRepository = roleClaimRepository ?? throw new ArgumentNullException(nameof(roleClaimRepository));
        _roleHierarchyRepository = roleHierarchyRepository ?? throw new ArgumentNullException(nameof(roleHierarchyRepository));
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
    }

    public async Task<IReadOnlyDictionary<string, ClaimValue>> ResolveUserClaimsAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Claim> allClaims = await _claimRepository.GetAllAsync(
            executionContext,
            cancellationToken);

        var claimNameById = allClaims.ToDictionary(
            static c => c.EntityInfo.Id.Value,
            static c => c.Name);

        IReadOnlyList<UserRole> userRoles = await _userRoleRepository.GetByUserIdAsync(
            executionContext,
            userId,
            cancellationToken);

        IReadOnlyList<RoleHierarchy> allHierarchies = await _roleHierarchyRepository.GetAllAsync(
            executionContext,
            cancellationToken);

        var hierarchyByRoleId = allHierarchies
            .GroupBy(static h => h.RoleId.Value)
            .ToDictionary(
                static g => g.Key,
                static g => g.Select(static h => h.ParentRoleId).ToList());

        var resolvedUserClaims = new Dictionary<string, ClaimValue>();

        foreach (UserRole userRole in userRoles)
        {
            Dictionary<Guid, ClaimValue> roleResolved = await ResolveRoleClaimsAsync(
                executionContext,
                userRole.RoleId,
                hierarchyByRoleId,
                cancellationToken);

            foreach (var (claimId, claimValue) in roleResolved)
            {
                if (!claimNameById.TryGetValue(claimId, out string? claimName))
                    continue;

                if (resolvedUserClaims.TryGetValue(claimName, out ClaimValue existing))
                {
                    resolvedUserClaims[claimName] = Min(existing, claimValue);
                }
                else
                {
                    resolvedUserClaims[claimName] = claimValue;
                }
            }
        }

        foreach (var claim in allClaims)
        {
            if (!resolvedUserClaims.ContainsKey(claim.Name))
            {
                resolvedUserClaims[claim.Name] = ClaimValue.Denied;
            }
        }

        return resolvedUserClaims;
    }

    private async Task<Dictionary<Guid, ClaimValue>> ResolveRoleClaimsAsync(
        ExecutionContext executionContext,
        Id roleId,
        Dictionary<Guid, List<Id>> hierarchyByRoleId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RoleClaim> roleClaims = await _roleClaimRepository.GetByRoleIdAsync(
            executionContext,
            roleId,
            cancellationToken);

        var resolved = new Dictionary<Guid, ClaimValue>();

        foreach (RoleClaim roleClaim in roleClaims)
        {
            resolved[roleClaim.ClaimId.Value] = roleClaim.Value;
        }

        if (!hierarchyByRoleId.TryGetValue(roleId.Value, out List<Id>? parentRoleIds) || parentRoleIds.Count == 0)
        {
            foreach (var (claimId, value) in resolved)
            {
                if (value.IsInherited)
                {
                    resolved[claimId] = ClaimValue.Denied;
                }
            }

            return resolved;
        }

        var parentResults = new List<Dictionary<Guid, ClaimValue>>();

        foreach (Id parentRoleId in parentRoleIds)
        {
            Dictionary<Guid, ClaimValue> parentResolved = await ResolveRoleClaimsAsync(
                executionContext,
                parentRoleId,
                hierarchyByRoleId,
                cancellationToken);

            parentResults.Add(parentResolved);
        }

        var allClaimIds = new HashSet<Guid>(resolved.Keys);
        foreach (var parentResult in parentResults)
        {
            foreach (var claimId in parentResult.Keys)
            {
                allClaimIds.Add(claimId);
            }
        }

        var finalResolved = new Dictionary<Guid, ClaimValue>();

        foreach (Guid claimId in allClaimIds)
        {
            if (resolved.TryGetValue(claimId, out ClaimValue directValue) && !directValue.IsInherited)
            {
                finalResolved[claimId] = directValue;
            }
            else
            {
                ClaimValue? inheritedValue = null;

                foreach (var parentResult in parentResults)
                {
                    if (parentResult.TryGetValue(claimId, out ClaimValue parentValue))
                    {
                        inheritedValue = inheritedValue.HasValue
                            ? Min(inheritedValue.Value, parentValue)
                            : parentValue;
                    }
                }

                finalResolved[claimId] = inheritedValue ?? ClaimValue.Denied;
            }
        }

        return finalResolved;
    }

    private static ClaimValue Min(ClaimValue a, ClaimValue b)
    {
        return a.Value <= b.Value ? a : b;
    }
}
