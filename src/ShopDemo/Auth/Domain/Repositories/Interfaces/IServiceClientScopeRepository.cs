using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IServiceClientScopeRepository : IRepository<ServiceClientScope>
{
    Task<IReadOnlyList<ServiceClientScope>> GetByServiceClientIdAsync(
        ExecutionContext executionContext,
        Id serviceClientId,
        CancellationToken cancellationToken);

    Task<bool> DeleteByServiceClientIdAsync(
        ExecutionContext executionContext,
        Id serviceClientId,
        CancellationToken cancellationToken);
}
