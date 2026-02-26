using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IServiceClientClaimRepository : IRepository<ServiceClientClaim>
{
    Task<IReadOnlyList<ServiceClientClaim>> GetByServiceClientIdAsync(
        ExecutionContext executionContext,
        Id serviceClientId,
        CancellationToken cancellationToken);

    Task<bool> DeleteByServiceClientIdAsync(
        ExecutionContext executionContext,
        Id serviceClientId,
        CancellationToken cancellationToken);
}
