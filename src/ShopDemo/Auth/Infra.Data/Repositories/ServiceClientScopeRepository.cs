using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.Repositories;

public sealed class ServiceClientScopeRepository
    : RepositoryBase<ServiceClientScope>,
    IServiceClientScopeRepository
{
    private readonly IServiceClientScopePostgreSqlRepository _postgreSqlRepository;

    public ServiceClientScopeRepository(
        ILogger<ServiceClientScopeRepository> logger,
        IServiceClientScopePostgreSqlRepository postgreSqlRepository
    ) : base(logger)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlRepository);
        _postgreSqlRepository = postgreSqlRepository;
    }

    public async Task<IReadOnlyList<ServiceClientScope>> GetByServiceClientIdAsync(
        ExecutionContext executionContext,
        Id serviceClientId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByServiceClientIdAsync(
                executionContext,
                serviceClientId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting service client scopes by service client ID.");
            return [];
        }
    }

    public async Task<bool> DeleteByServiceClientIdAsync(
        ExecutionContext executionContext,
        Id serviceClientId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.DeleteByServiceClientIdAsync(
                executionContext,
                serviceClientId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while deleting service client scopes by service client ID.");
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
    protected override async IAsyncEnumerable<ServiceClientScope> GetAllInternalAsync(
        PaginationInfo paginationInfo,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
    // Stryker restore all

    protected override Task<ServiceClientScope?> GetByIdInternalAsync(
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
    protected override async IAsyncEnumerable<ServiceClientScope> GetModifiedSinceInternalAsync(
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
        ServiceClientScope aggregateRoot,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.RegisterNewAsync(
            executionContext,
            aggregateRoot,
            cancellationToken);
    }
}
