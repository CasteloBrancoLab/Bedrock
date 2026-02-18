using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IRecoveryCodePostgreSqlRepository
    : IPostgreSqlRepository<RecoveryCode>
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
