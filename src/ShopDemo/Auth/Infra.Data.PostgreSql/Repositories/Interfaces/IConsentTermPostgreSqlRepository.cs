using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface IConsentTermPostgreSqlRepository
    : IPostgreSqlRepository<ConsentTerm>
{
    Task<ConsentTerm?> GetLatestByTypeAsync(
        ExecutionContext executionContext,
        ConsentTermType type,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConsentTerm>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
