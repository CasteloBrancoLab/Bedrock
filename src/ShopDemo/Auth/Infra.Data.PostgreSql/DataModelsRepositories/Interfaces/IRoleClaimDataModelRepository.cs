using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IRoleClaimDataModelRepository
    : IPostgreSqlDataModelRepository<RoleClaimDataModel>
{
    Task<IReadOnlyList<RoleClaimDataModel>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Guid roleId,
        CancellationToken cancellationToken);

    Task<RoleClaimDataModel?> GetByRoleIdAndClaimIdAsync(
        ExecutionContext executionContext,
        Guid roleId,
        Guid claimId,
        CancellationToken cancellationToken);
}
