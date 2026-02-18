using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IIdempotencyRecordDataModelRepository
    : IPostgreSqlDataModelRepository<IdempotencyRecordDataModel>
{
    Task<IdempotencyRecordDataModel?> GetByKeyAsync(
        ExecutionContext executionContext,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<int> RemoveExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
