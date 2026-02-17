using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IRefreshTokenDataModelRepository
    : IPostgreSqlDataModelRepository<RefreshTokenDataModel>
{
    Task<IReadOnlyList<RefreshTokenDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);

    Task<RefreshTokenDataModel?> GetByTokenHashAsync(
        ExecutionContext executionContext,
        byte[] tokenHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshTokenDataModel>> GetActiveByFamilyIdAsync(
        ExecutionContext executionContext,
        Guid familyId,
        CancellationToken cancellationToken);
}
