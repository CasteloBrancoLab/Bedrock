using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IUserRoleDataModelRepository
    : IPostgreSqlDataModelRepository<UserRoleDataModel>
{
    Task<IReadOnlyList<UserRoleDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UserRoleDataModel>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Guid roleId,
        CancellationToken cancellationToken);
}
