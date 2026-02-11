using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Persistence.DataModels;

namespace ShopDemo.Auth.Infra.Persistence.DataModelsRepositories.Interfaces;

public interface IUserDataModelRepository
    : IPostgreSqlDataModelRepository<UserDataModel>
{
    Task<UserDataModel?> GetByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken);

    Task<UserDataModel?> GetByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);
}
