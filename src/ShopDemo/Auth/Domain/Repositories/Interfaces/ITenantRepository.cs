using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Tenants;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
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
