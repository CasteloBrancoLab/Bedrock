using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IDenyListEntryDataModelRepository
    : IPostgreSqlDataModelRepository<DenyListEntryDataModel>
{
    Task<bool> ExistsByTypeAndValueAsync(
        ExecutionContext executionContext,
        short type,
        string value,
        CancellationToken cancellationToken);

    Task<DenyListEntryDataModel?> GetByTypeAndValueAsync(
        ExecutionContext executionContext,
        short type,
        string value,
        CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken);

    Task<bool> DeleteByTypeAndValueAsync(
        ExecutionContext executionContext,
        short type,
        string value,
        CancellationToken cancellationToken);
}
