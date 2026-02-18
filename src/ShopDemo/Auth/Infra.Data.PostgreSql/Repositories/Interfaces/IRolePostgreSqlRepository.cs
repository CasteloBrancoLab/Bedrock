using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Roles;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IRolePostgreSqlRepository
    : IPostgreSqlRepository<Role>
{
    Task<Role?> GetByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        Role aggregateRoot,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        Role aggregateRoot,
        CancellationToken cancellationToken);
}
