using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IPasswordHistoryPostgreSqlRepository
    : IPostgreSqlRepository<PasswordHistory>
{
    Task<IReadOnlyList<PasswordHistory>> GetLatestByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        int count,
        CancellationToken cancellationToken);
}
