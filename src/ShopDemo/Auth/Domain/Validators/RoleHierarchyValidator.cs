using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Validators.Interfaces;

namespace ShopDemo.Auth.Domain.Validators;

public sealed class RoleHierarchyValidator : IRoleHierarchyValidator
{
    private readonly IRoleHierarchyRepository _roleHierarchyRepository;

    public RoleHierarchyValidator(
        IRoleHierarchyRepository roleHierarchyRepository
    )
    {
        _roleHierarchyRepository = roleHierarchyRepository ?? throw new ArgumentNullException(nameof(roleHierarchyRepository));
    }

    public async Task<bool> ValidateNoCircularDependencyAsync(
        ExecutionContext executionContext,
        Id roleId,
        Id parentRoleId,
        CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid>();
        return !await IsReachableAsync(executionContext, parentRoleId, roleId, visited, cancellationToken);
    }

    private async Task<bool> IsReachableAsync(
        ExecutionContext executionContext,
        Id currentRoleId,
        Id targetRoleId,
        HashSet<Guid> visited,
        CancellationToken cancellationToken)
    {
        if (currentRoleId == targetRoleId)
            return true;

        if (!visited.Add(currentRoleId.Value))
            return false;

        IReadOnlyList<RoleHierarchy> parentRelations = await _roleHierarchyRepository.GetByRoleIdAsync(
            executionContext,
            currentRoleId,
            cancellationToken);

        foreach (RoleHierarchy relation in parentRelations)
        {
            if (await IsReachableAsync(executionContext, relation.ParentRoleId, targetRoleId, visited, cancellationToken))
                return true;
        }

        return false;
    }
}
