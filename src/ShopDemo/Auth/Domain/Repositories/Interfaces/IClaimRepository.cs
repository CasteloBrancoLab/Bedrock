using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Claims;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IClaimRepository : IRepository<Claim>
{
    Task<Claim?> GetByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Claim>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
