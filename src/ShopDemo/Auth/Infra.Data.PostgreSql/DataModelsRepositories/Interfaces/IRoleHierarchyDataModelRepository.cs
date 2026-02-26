using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IRoleHierarchyDataModelRepository
    : IPostgreSqlDataModelRepository<RoleHierarchyDataModel>
{
    Task<IReadOnlyList<RoleHierarchyDataModel>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Guid roleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RoleHierarchyDataModel>> GetByParentRoleIdAsync(
        ExecutionContext executionContext,
        Guid parentRoleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RoleHierarchyDataModel>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
