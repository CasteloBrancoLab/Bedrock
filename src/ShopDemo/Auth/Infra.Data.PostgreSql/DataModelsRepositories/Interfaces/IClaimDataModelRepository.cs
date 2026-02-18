using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IClaimDataModelRepository
    : IPostgreSqlDataModelRepository<ClaimDataModel>
{
    Task<ClaimDataModel?> GetByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ClaimDataModel>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
