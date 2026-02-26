using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ServiceClients;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IServiceClientRepository : IRepository<ServiceClient>
{
    Task<ServiceClient?> GetByClientIdAsync(
        ExecutionContext executionContext,
        string clientId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceClient>> GetByCreatorUserIdAsync(
        ExecutionContext executionContext,
        Id createdByUserId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        ServiceClient aggregateRoot,
        CancellationToken cancellationToken);
}
