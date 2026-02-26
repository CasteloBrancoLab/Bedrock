using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IServiceClientScopeDataModelRepository
    : IPostgreSqlDataModelRepository<ServiceClientScopeDataModel>
{
    Task<IReadOnlyList<ServiceClientScopeDataModel>> GetByServiceClientIdAsync(
        ExecutionContext executionContext,
        Guid serviceClientId,
        CancellationToken cancellationToken);

    Task<bool> DeleteByServiceClientIdAsync(
        ExecutionContext executionContext,
        Guid serviceClientId,
        CancellationToken cancellationToken);
}
