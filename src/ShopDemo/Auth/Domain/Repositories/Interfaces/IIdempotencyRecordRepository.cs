using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IIdempotencyRecordRepository : IRepository<IdempotencyRecord>
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
