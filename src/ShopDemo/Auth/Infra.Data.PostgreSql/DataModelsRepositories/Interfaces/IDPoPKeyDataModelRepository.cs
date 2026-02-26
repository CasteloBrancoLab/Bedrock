using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IDPoPKeyDataModelRepository
    : IPostgreSqlDataModelRepository<DPoPKeyDataModel>
{
    Task<DPoPKeyDataModel?> GetActiveByUserIdAndThumbprintAsync(
        ExecutionContext executionContext,
        Guid userId,
        string jwkThumbprint,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DPoPKeyDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);
}
