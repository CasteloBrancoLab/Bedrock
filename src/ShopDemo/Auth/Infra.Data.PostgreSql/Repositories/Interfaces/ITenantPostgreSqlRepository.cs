using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Tenants;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface ITenantPostgreSqlRepository
    : IPostgreSqlRepository<Tenant>
{
    Task<Tenant?> GetByDomainAsync(
        ExecutionContext executionContext,
        string domain,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Tenant>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        Tenant aggregateRoot,
        CancellationToken cancellationToken);
}
