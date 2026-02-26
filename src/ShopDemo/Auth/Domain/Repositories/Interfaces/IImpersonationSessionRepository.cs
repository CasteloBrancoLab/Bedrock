using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IImpersonationSessionRepository : IRepository<ImpersonationSession>
{
    Task<ImpersonationSession?> GetActiveByOperatorUserIdAsync(
        ExecutionContext executionContext,
        Id operatorUserId,
        CancellationToken cancellationToken);

    Task<ImpersonationSession?> GetActiveByTargetUserIdAsync(
        ExecutionContext executionContext,
        Id targetUserId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        ImpersonationSession aggregateRoot,
        CancellationToken cancellationToken);
}
