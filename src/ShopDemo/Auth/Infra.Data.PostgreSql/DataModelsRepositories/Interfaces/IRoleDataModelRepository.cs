using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IRoleDataModelRepository
    : IPostgreSqlDataModelRepository<RoleDataModel>
{
    Task<RoleDataModel?> GetByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken);
}
