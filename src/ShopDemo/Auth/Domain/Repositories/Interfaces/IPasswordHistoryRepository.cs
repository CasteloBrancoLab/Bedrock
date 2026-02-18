using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IPasswordHistoryRepository : IRepository<PasswordHistory>
{
    Task<IReadOnlyList<PasswordHistory>> GetLatestByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        int count,
        CancellationToken cancellationToken);
}
