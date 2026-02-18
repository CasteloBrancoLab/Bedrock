using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface ILoginAttemptDataModelRepository
    : IPostgreSqlDataModelRepository<LoginAttemptDataModel>
{
    Task<IReadOnlyList<LoginAttemptDataModel>> GetRecentByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        DateTimeOffset since,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LoginAttemptDataModel>> GetRecentByIpAddressAsync(
        ExecutionContext executionContext,
        string ipAddress,
        DateTimeOffset since,
        CancellationToken cancellationToken);
}
