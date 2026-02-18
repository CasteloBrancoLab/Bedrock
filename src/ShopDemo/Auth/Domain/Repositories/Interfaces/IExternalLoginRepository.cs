using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IExternalLoginRepository : IRepository<ExternalLogin>
{
    Task<ExternalLogin?> GetByProviderAndProviderUserIdAsync(
        ExecutionContext executionContext,
        LoginProvider provider,
        string providerUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ExternalLogin>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<bool> DeleteByUserIdAndProviderAsync(
        ExecutionContext executionContext,
        Id userId,
        LoginProvider provider,
        CancellationToken cancellationToken);
}
