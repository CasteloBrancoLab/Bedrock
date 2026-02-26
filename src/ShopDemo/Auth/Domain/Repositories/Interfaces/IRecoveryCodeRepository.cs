using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IRecoveryCodeRepository : IRepository<RecoveryCode>
{
    Task<RecoveryCode?> GetByUserIdAndCodeHashAsync(
        ExecutionContext executionContext,
        Id userId,
        string codeHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RecoveryCode>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        RecoveryCode aggregateRoot,
        CancellationToken cancellationToken);

    Task<int> RevokeAllByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);
}
