using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IPasswordResetTokenPostgreSqlRepository
    : IPostgreSqlRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetByTokenHashAsync(
        ExecutionContext executionContext,
        string tokenHash,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        PasswordResetToken aggregateRoot,
        CancellationToken cancellationToken);

    Task<int> RevokeAllByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken);
}
