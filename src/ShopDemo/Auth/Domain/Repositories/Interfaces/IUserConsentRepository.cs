using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.UserConsents;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IUserConsentRepository : IRepository<UserConsent>
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
