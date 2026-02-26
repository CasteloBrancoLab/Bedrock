using ShopDemo.Auth.Domain.Entities.Tenants;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface ITenantResolver
{
    Task<Tenant?> ResolveByDomainAsync(
        ExecutionContext executionContext,
        string domain,
        CancellationToken cancellationToken);
}
