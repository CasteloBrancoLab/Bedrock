using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IApiKeyDataModelRepository
    : IPostgreSqlDataModelRepository<ApiKeyDataModel>
{
    Task<ApiKeyDataModel?> GetByKeyHashAsync(
        ExecutionContext executionContext,
        string keyHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ApiKeyDataModel>> GetByServiceClientIdAsync(
        ExecutionContext executionContext,
        Guid serviceClientId,
        CancellationToken cancellationToken);
}
