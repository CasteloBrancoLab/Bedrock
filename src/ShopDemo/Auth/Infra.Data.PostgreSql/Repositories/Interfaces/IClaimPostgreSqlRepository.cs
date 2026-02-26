using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Claims;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IClaimPostgreSqlRepository
    : IPostgreSqlRepository<Claim>
{
    Task<Claim?> GetByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Claim>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
