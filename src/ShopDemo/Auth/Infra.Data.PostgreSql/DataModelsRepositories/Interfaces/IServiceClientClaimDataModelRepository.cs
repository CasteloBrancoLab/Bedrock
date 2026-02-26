using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IServiceClientClaimDataModelRepository
    : IPostgreSqlDataModelRepository<ServiceClientClaimDataModel>
{
    Task<IReadOnlyList<ServiceClientClaimDataModel>> GetByServiceClientIdAsync(
        ExecutionContext executionContext,
        Guid serviceClientId,
        CancellationToken cancellationToken);

    Task<bool> DeleteByServiceClientIdAsync(
        ExecutionContext executionContext,
        Guid serviceClientId,
        CancellationToken cancellationToken);
}
