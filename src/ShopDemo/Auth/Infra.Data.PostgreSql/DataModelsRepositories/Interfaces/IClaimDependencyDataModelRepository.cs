using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IClaimDependencyDataModelRepository
    : IPostgreSqlDataModelRepository<ClaimDependencyDataModel>
{
    Task<IReadOnlyList<ClaimDependencyDataModel>> GetByClaimIdAsync(
        ExecutionContext executionContext,
        Guid claimId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ClaimDependencyDataModel>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
