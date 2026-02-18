using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.SigningKeys;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface ISigningKeyPostgreSqlRepository
    : IPostgreSqlRepository<SigningKey>
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
