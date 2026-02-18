using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IServiceClientDataModelRepository
    : IPostgreSqlDataModelRepository<ServiceClientDataModel>
{
    Task<ServiceClientDataModel?> GetByClientIdAsync(
        ExecutionContext executionContext,
        string clientId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceClientDataModel>> GetByCreatorUserIdAsync(
        ExecutionContext executionContext,
        Guid createdByUserId,
        CancellationToken cancellationToken);
}
