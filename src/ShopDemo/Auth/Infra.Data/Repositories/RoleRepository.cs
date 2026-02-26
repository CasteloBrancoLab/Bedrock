using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Domain.Entities.Roles;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.Repositories;

public sealed class RoleRepository
    : RepositoryBase<Role>,
    IRoleRepository
{
    private readonly IRolePostgreSqlRepository _postgreSqlRepository;

    public RoleRepository(
        ILogger<RoleRepository> logger,
        IRolePostgreSqlRepository postgreSqlRepository
    ) : base(logger)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlRepository);
        _postgreSqlRepository = postgreSqlRepository;
    }

    public async Task<Role?> GetByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByNameAsync(
                executionContext,
                name,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting role by name.");
            return null;
        }
    }

    public async Task<bool> ExistsByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.ExistsByNameAsync(
                executionContext,
                name,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while checking existence by name.");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        Role aggregateRoot,
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
                "An error occurred while updating role.");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        Role aggregateRoot,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.DeleteAsync(
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
                "An error occurred while deleting role.");
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
    protected override async IAsyncEnumerable<Role> GetAllInternalAsync(
        PaginationInfo paginationInfo,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
    // Stryker restore all

    protected override Task<Role?> GetByIdInternalAsync(
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
    protected override async IAsyncEnumerable<Role> GetModifiedSinceInternalAsync(
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
        Role aggregateRoot,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.RegisterNewAsync(
            executionContext,
            aggregateRoot,
            cancellationToken);
    }
}
