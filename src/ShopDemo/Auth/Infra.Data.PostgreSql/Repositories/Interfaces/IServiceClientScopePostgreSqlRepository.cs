using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IServiceClientScopePostgreSqlRepository
    : IPostgreSqlRepository<ServiceClientScope>
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
