using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IIdempotencyRecordPostgreSqlRepository
    : IPostgreSqlRepository<IdempotencyRecord>
{
    Task<IdempotencyRecord?> GetByKeyAsync(
        ExecutionContext executionContext,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        IdempotencyRecord aggregateRoot,
        CancellationToken cancellationToken);

    Task<int> RemoveExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
