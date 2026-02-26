using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IConsentTermDataModelRepository
    : IPostgreSqlDataModelRepository<ConsentTermDataModel>
{
    Task<ConsentTermDataModel?> GetLatestByTypeAsync(
        ExecutionContext executionContext,
        short type,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConsentTermDataModel>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
