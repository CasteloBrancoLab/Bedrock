using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;

namespace ShopDemo.Auth.Domain.Repositories.Interfaces;

public interface ITokenExchangeRepository : IRepository<TokenExchange>
{
    Task<IReadOnlyList<TokenExchange>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken);

    Task<TokenExchange?> GetByIssuedTokenJtiAsync(
        ExecutionContext executionContext,
        string issuedTokenJti,
        CancellationToken cancellationToken);
}
