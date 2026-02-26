using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.Repositories;

public sealed class PasswordResetTokenRepository
    : RepositoryBase<PasswordResetToken>,
    IPasswordResetTokenRepository
{
    private readonly IPasswordResetTokenPostgreSqlRepository _postgreSqlRepository;

    public PasswordResetTokenRepository(
        ILogger<PasswordResetTokenRepository> logger,
        IPasswordResetTokenPostgreSqlRepository postgreSqlRepository
    ) : base(logger)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlRepository);
        _postgreSqlRepository = postgreSqlRepository;
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(
        ExecutionContext executionContext,
        string tokenHash,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByTokenHashAsync(
                executionContext,
                tokenHash,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting password reset token by token hash.");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        PasswordResetToken aggregateRoot,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.UpdateAsync(
                executionContext,
                aggregateRoot,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while updating password reset token.");
            return false;
        }
    }

    public async Task<int> RevokeAllByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.RevokeAllByUserIdAsync(
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
                "An error occurred while revoking all password reset tokens by user ID.");
            return 0;
        }
    }

    public async Task<int> DeleteExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.DeleteExpiredAsync(
                executionContext,
                referenceDate,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while deleting expired password reset tokens.");
            return 0;
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
    protected override async IAsyncEnumerable<PasswordResetToken> GetAllInternalAsync(
        PaginationInfo paginationInfo,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
    // Stryker restore all

    protected override Task<PasswordResetToken?> GetByIdInternalAsync(
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
    protected override async IAsyncEnumerable<PasswordResetToken> GetModifiedSinceInternalAsync(
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
        PasswordResetToken aggregateRoot,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.RegisterNewAsync(
            executionContext,
            aggregateRoot,
            cancellationToken);
    }
}
