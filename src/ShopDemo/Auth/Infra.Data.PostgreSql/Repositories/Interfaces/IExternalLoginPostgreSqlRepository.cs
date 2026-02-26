using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IExternalLoginPostgreSqlRepository
    : IPostgreSqlRepository<ExternalLogin>
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
