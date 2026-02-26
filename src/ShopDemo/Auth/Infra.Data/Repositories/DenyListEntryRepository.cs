using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.Repositories;

public sealed class DenyListEntryRepository
    : RepositoryBase<DenyListEntry>,
    IDenyListRepository
{
    private readonly IDenyListEntryPostgreSqlRepository _postgreSqlRepository;

    public DenyListEntryRepository(
        ILogger<DenyListEntryRepository> logger,
        IDenyListEntryPostgreSqlRepository postgreSqlRepository
    ) : base(logger)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlRepository);
        _postgreSqlRepository = postgreSqlRepository;
    }

    public async Task<bool> ExistsByTypeAndValueAsync(
        ExecutionContext executionContext,
        DenyListEntryType type,
        string value,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.ExistsByTypeAndValueAsync(
                executionContext,
                type,
                value,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while checking deny list entry existence by type and value.");
            return false;
        }
    }

    public async Task<DenyListEntry?> GetByTypeAndValueAsync(
        ExecutionContext executionContext,
        DenyListEntryType type,
        string value,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByTypeAndValueAsync(
                executionContext,
                type,
                value,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting deny list entry by type and value.");
            return null;
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
                "An error occurred while deleting expired deny list entries.");
            return 0;
        }
    }

    public async Task<bool> DeleteByTypeAndValueAsync(
        ExecutionContext executionContext,
        DenyListEntryType type,
        string value,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.DeleteByTypeAndValueAsync(
                executionContext,
                type,
                value,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while deleting deny list entry by type and value.");
            return false;
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
    protected override async IAsyncEnumerable<DenyListEntry> GetAllInternalAsync(
        PaginationInfo paginationInfo,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
    // Stryker restore all

    protected override Task<DenyListEntry?> GetByIdInternalAsync(
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
    protected override async IAsyncEnumerable<DenyListEntry> GetModifiedSinceInternalAsync(
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
        DenyListEntry aggregateRoot,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.RegisterNewAsync(
            executionContext,
            aggregateRoot,
            cancellationToken);
    }
}
