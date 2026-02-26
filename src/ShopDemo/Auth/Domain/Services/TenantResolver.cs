using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class TenantResolver : ITenantResolver
{
    private readonly ITenantRepository _tenantRepository;

    public TenantResolver(
        ITenantRepository tenantRepository
    )
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
    }

    public async Task<Tenant?> ResolveByDomainAsync(
        ExecutionContext executionContext,
        string domain,
        CancellationToken cancellationToken)
    {
        Tenant? tenant = await _tenantRepository.GetByDomainAsync(
            executionContext,
            domain,
            cancellationToken);

        if (tenant is null)
            return null;

        if (tenant.Status != TenantStatus.Active)
            return null;

        return tenant;
    }
}
