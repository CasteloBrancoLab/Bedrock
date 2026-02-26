using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.MfaSetups;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IMfaSetupRepository : IRepository<MfaSetup>
{
    Task<MfaSetup?> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        MfaSetup aggregateRoot,
        CancellationToken cancellationToken);

    Task<bool> DeleteByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);
}
