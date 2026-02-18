using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Validators.Interfaces;

namespace ShopDemo.Auth.Domain.Validators;

public sealed class ClaimDependencyValidator : IClaimDependencyValidator
{
    private const string DependencyNotGrantedMessageCode = "ClaimDependencyValidator.DependencyNotGranted";

    private readonly IClaimDependencyRepository _claimDependencyRepository;
    private readonly IRoleClaimRepository _roleClaimRepository;

    public ClaimDependencyValidator(
        IClaimDependencyRepository claimDependencyRepository,
        IRoleClaimRepository roleClaimRepository
    )
    {
        _claimDependencyRepository = claimDependencyRepository ?? throw new ArgumentNullException(nameof(claimDependencyRepository));
        _roleClaimRepository = roleClaimRepository ?? throw new ArgumentNullException(nameof(roleClaimRepository));
    }

    public async Task<bool> ValidateClaimDependenciesAsync(
        ExecutionContext executionContext,
        Id roleId,
        Id claimId,
        ClaimValue value,
        CancellationToken cancellationToken)
    {
        if (!value.IsGranted)
            return true;

        IReadOnlyList<ClaimDependency> dependencies = await _claimDependencyRepository.GetByClaimIdAsync(
            executionContext,
            claimId,
            cancellationToken);

        bool allDependenciesMet = true;

        foreach (ClaimDependency dependency in dependencies)
        {
            RoleClaim? roleClaim = await _roleClaimRepository.GetByRoleIdAndClaimIdAsync(
                executionContext,
                roleId,
                dependency.DependsOnClaimId,
                cancellationToken);

            if (roleClaim is null || !roleClaim.Value.IsGranted)
            {
                executionContext.AddErrorMessage(code: DependencyNotGrantedMessageCode);
                allDependenciesMet = false;
            }
        }

        return allDependenciesMet;
    }
}
