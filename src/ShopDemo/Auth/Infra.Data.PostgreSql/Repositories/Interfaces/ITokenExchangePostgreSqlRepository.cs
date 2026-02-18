using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface ITokenExchangePostgreSqlRepository
    : IPostgreSqlRepository<TokenExchange>
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
