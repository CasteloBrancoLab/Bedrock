using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IDenyListEntryPostgreSqlRepository
    : IPostgreSqlRepository<DenyListEntry>
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
