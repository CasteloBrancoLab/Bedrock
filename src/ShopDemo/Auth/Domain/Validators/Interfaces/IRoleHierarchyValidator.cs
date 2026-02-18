using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Validators.Interfaces;

public interface IRoleHierarchyValidator
{
    Task<bool> ValidateNoCircularDependencyAsync(
        ExecutionContext executionContext,
        Id roleId,
        Id parentRoleId,
        CancellationToken cancellationToken);
}
