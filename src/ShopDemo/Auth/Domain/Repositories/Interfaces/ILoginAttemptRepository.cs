using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface ILoginAttemptRepository : IRepository<LoginAttempt>
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
