using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IDenyListRepository : IRepository<DenyListEntry>
{
    Task<bool> ExistsByTypeAndValueAsync(
        ExecutionContext executionContext,
        DenyListEntryType type,
        string value,
        CancellationToken cancellationToken);

    Task<DenyListEntry?> GetByTypeAndValueAsync(
        ExecutionContext executionContext,
        DenyListEntryType type,
        string value,
        CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken);

    Task<bool> DeleteByTypeAndValueAsync(
        ExecutionContext executionContext,
        DenyListEntryType type,
        string value,
        CancellationToken cancellationToken);
}
