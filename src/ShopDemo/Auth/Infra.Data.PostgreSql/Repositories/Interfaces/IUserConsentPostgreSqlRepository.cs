using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.UserConsents;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IUserConsentPostgreSqlRepository
    : IPostgreSqlRepository<UserConsent>
{
    Task<IReadOnlyList<UserConsent>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<UserConsent?> GetActiveByUserIdAndConsentTermIdAsync(
        ExecutionContext executionContext,
        Id userId,
        Id consentTermId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        UserConsent aggregateRoot,
        CancellationToken cancellationToken);
}
