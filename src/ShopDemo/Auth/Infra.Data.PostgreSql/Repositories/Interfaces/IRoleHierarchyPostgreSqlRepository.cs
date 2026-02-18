using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IRoleHierarchyPostgreSqlRepository
    : IPostgreSqlRepository<RoleHierarchy>
{
    Task<IReadOnlyList<RoleHierarchy>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Id roleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RoleHierarchy>> GetByParentRoleIdAsync(
        ExecutionContext executionContext,
        Id parentRoleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RoleHierarchy>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        RoleHierarchy roleHierarchy,
        CancellationToken cancellationToken);
}
