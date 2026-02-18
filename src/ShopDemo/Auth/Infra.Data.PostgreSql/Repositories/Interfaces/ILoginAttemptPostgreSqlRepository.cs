using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface ILoginAttemptPostgreSqlRepository
    : IPostgreSqlRepository<LoginAttempt>
{
    Task<IReadOnlyList<LoginAttempt>> GetRecentByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        DateTimeOffset since,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LoginAttempt>> GetRecentByIpAddressAsync(
        ExecutionContext executionContext,
        string ipAddress,
        DateTimeOffset since,
        CancellationToken cancellationToken);
}
