using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface IConsentTermRepository : IRepository<ConsentTerm>
{
    Task<ConsentTerm?> GetLatestByTypeAsync(
        ExecutionContext executionContext,
        ConsentTermType type,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConsentTerm>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
