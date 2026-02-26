using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface ITokenExchangeService
{
    Task<TokenExchange?> ExchangeTokenAsync(
        ExecutionContext executionContext,
        Id userId,
        string subjectTokenJti,
        string requestedAudience,
        bool isImpersonationToken,
        CancellationToken cancellationToken);
}
