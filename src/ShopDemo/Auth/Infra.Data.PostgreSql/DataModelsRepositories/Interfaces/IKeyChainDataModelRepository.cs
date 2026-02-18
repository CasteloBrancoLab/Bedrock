using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IKeyChainDataModelRepository
    : IPostgreSqlDataModelRepository<KeyChainDataModel>
{
    Task<IReadOnlyList<KeyChainDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);

    Task<KeyChainDataModel?> GetByUserIdAndKeyIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        string keyId,
        CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken);
}
