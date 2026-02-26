using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IRecoveryCodeDataModelRepository
    : IPostgreSqlDataModelRepository<RecoveryCodeDataModel>
{
    Task<RecoveryCodeDataModel?> GetByUserIdAndCodeHashAsync(
        ExecutionContext executionContext,
        Guid userId,
        string codeHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RecoveryCodeDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);

    Task<int> DeleteAllByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);
}
