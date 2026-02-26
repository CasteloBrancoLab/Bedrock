using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IImpersonationSessionPostgreSqlRepository
    : IPostgreSqlRepository<ImpersonationSession>
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
