using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IPasswordHistoryDataModelRepository
    : IPostgreSqlDataModelRepository<PasswordHistoryDataModel>
{
    Task<IReadOnlyList<PasswordHistoryDataModel>> GetLatestByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        int count,
        CancellationToken cancellationToken);
}
