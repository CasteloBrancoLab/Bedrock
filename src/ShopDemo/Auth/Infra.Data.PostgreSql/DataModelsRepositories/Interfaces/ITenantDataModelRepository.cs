using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface ITenantDataModelRepository
    : IPostgreSqlDataModelRepository<TenantDataModel>
{
    Task<TenantDataModel?> GetByDomainAsync(
        ExecutionContext executionContext,
        string domain,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantDataModel>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
