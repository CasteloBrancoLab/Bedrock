using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class TokenExchangeService : ITokenExchangeService
{
    private static readonly TimeSpan DefaultExchangedTokenDuration = TimeSpan.FromMinutes(5);

    private static readonly HashSet<string> AllowedAudiences = new(StringComparer.Ordinal)
    {
        "internal-services",
        "public-api"
    };

    private readonly ITokenExchangeRepository _tokenExchangeRepository;
    private readonly IDenyListService _denyListService;

    public TokenExchangeService(
        ITokenExchangeRepository tokenExchangeRepository,
        IDenyListService denyListService)
    {
        ArgumentNullException.ThrowIfNull(tokenExchangeRepository);
        ArgumentNullException.ThrowIfNull(denyListService);

        _tokenExchangeRepository = tokenExchangeRepository;
        _denyListService = denyListService;
    }

    public async Task<TokenExchange?> ExchangeTokenAsync(
        ExecutionContext executionContext,
        Id userId,
        string subjectTokenJti,
        string requestedAudience,
        bool isImpersonationToken,
        CancellationToken cancellationToken)
    {
        if (isImpersonationToken)
        {
            executionContext.AddErrorMessage(code: "TokenExchange.ImpersonationTokenNotAllowed");
            return null;
        }

        if (!AllowedAudiences.Contains(requestedAudience))
        {
            executionContext.AddErrorMessage(code: "TokenExchange.AudienceNotAllowed");
            return null;
        }

        bool isUserDenied = await _denyListService.IsUserRevokedAsync(
            executionContext,
            userId.Value.ToString(),
            cancellationToken);

        if (isUserDenied)
        {
            executionContext.AddErrorMessage(code: "TokenExchange.UserDenied");
            return null;
        }

        string issuedTokenJti = Guid.NewGuid().ToString();
        DateTimeOffset expiresAt = executionContext.Timestamp.Add(DefaultExchangedTokenDuration);

        TokenExchange? tokenExchange = TokenExchange.RegisterNew(
            executionContext,
            new RegisterNewTokenExchangeInput(
                userId,
                subjectTokenJti,
                requestedAudience,
                issuedTokenJti,
                expiresAt));

        if (tokenExchange is null)
            return null;

        bool registered = await _tokenExchangeRepository.RegisterNewAsync(
            executionContext,
            tokenExchange,
            cancellationToken);

        if (!registered)
            return null;

        return tokenExchange;
    }
}
