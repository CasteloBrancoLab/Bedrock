using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.UserRoles;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IUserRolePostgreSqlRepository
    : IPostgreSqlRepository<UserRole>
{
    Task<IReadOnlyList<UserRole>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UserRole>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Id roleId,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        UserRole userRole,
        CancellationToken cancellationToken);
}
