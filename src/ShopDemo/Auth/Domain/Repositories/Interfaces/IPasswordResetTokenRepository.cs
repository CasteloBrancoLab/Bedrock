using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
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
