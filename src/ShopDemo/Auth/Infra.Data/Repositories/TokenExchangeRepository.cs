using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.Repositories;

public sealed class TokenExchangeRepository
    : RepositoryBase<TokenExchange>,
    ITokenExchangeRepository
{
    private readonly ITokenExchangePostgreSqlRepository _postgreSqlRepository;

    public TokenExchangeRepository(
        ILogger<TokenExchangeRepository> logger,
        ITokenExchangePostgreSqlRepository postgreSqlRepository
    ) : base(logger)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlRepository);
        _postgreSqlRepository = postgreSqlRepository;
    }

    public async Task<IReadOnlyList<TokenExchange>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByUserIdAsync(
                executionContext,
                userId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting token exchanges by user ID.");
            return [];
        }
    }

    public async Task<TokenExchange?> GetByIssuedTokenJtiAsync(
        ExecutionContext executionContext,
        string issuedTokenJti,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByIssuedTokenJtiAsync(
                executionContext,
                issuedTokenJti,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting token exchange by issued token JTI.");
            return null;
        }
    }

    protected override Task<bool> ExistsInternalAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.ExistsAsync(
            executionContext,
            id,
            cancellationToken);
    }

    // Stryker disable all : Metodo stub sem implementacao - mutantes equivalentes (yield break sem corpo)
    [ExcludeFromCodeCoverage(Justification = "Metodo stub sem implementacao - async iterator gera branch inalcancavel no state machine")]
    protected override async IAsyncEnumerable<TokenExchange> GetAllInternalAsync(
        PaginationInfo paginationInfo,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
    // Stryker restore all

    protected override Task<TokenExchange?> GetByIdInternalAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);
    }

    // Stryker disable all : Metodo stub sem implementacao - mutantes equivalentes (yield break sem corpo)
    [ExcludeFromCodeCoverage(Justification = "Metodo stub sem implementacao - async iterator gera branch inalcancavel no state machine")]
    protected override async IAsyncEnumerable<TokenExchange> GetModifiedSinceInternalAsync(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
    // Stryker restore all

    protected override Task<bool> RegisterNewInternalAsync(
        ExecutionContext executionContext,
        TokenExchange aggregateRoot,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.RegisterNewAsync(
            executionContext,
            aggregateRoot,
            cancellationToken);
    }
}
