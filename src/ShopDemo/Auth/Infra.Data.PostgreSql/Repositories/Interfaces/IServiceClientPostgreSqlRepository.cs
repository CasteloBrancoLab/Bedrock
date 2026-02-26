using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ServiceClients;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IServiceClientPostgreSqlRepository
    : IPostgreSqlRepository<ServiceClient>
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
