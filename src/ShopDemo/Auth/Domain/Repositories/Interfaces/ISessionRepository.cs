using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Sessions;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface ISessionRepository : IRepository<Session>
{
    Task<IReadOnlyList<Session>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Session>> GetActiveByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<int> CountActiveByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        Session aggregateRoot,
        CancellationToken cancellationToken);
}
