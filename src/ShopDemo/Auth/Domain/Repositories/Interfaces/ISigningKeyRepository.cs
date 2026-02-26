using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.SigningKeys;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface ISigningKeyRepository : IRepository<SigningKey>
{
    Task<SigningKey?> GetActiveAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);

    Task<SigningKey?> GetByKidAsync(
        ExecutionContext executionContext,
        Kid kid,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SigningKey>> GetAllValidAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        SigningKey aggregateRoot,
        CancellationToken cancellationToken);
}
