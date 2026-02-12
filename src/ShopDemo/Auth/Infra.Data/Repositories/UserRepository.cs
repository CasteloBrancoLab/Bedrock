using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.Repositories;

public sealed class UserRepository
    : RepositoryBase<User>,
    IUserRepository
{
    private readonly IUserPostgreSqlRepository _postgreSqlRepository;

    public UserRepository(
        ILogger<UserRepository> logger,
        IUserPostgreSqlRepository postgreSqlRepository
    ) : base(logger)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlRepository);
        _postgreSqlRepository = postgreSqlRepository;
    }

    public async Task<User?> GetByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByEmailAsync(
                executionContext,
                email,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting user by email.");
            return null;
        }
    }

    public async Task<User?> GetByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByUsernameAsync(
                executionContext,
                username,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting user by username.");
            return null;
        }
    }

    public async Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.ExistsByEmailAsync(
                executionContext,
                email,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while checking existence by email.");
            return false;
        }
    }

    public async Task<bool> ExistsByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.ExistsByUsernameAsync(
                executionContext,
                username,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while checking existence by username.");
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
    protected override async IAsyncEnumerable<User> GetAllInternalAsync(
        PaginationInfo paginationInfo,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
    // Stryker restore all

    protected override Task<User?> GetByIdInternalAsync(
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
    protected override async IAsyncEnumerable<User> GetModifiedSinceInternalAsync(
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
        User aggregateRoot,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.RegisterNewAsync(
            executionContext,
            aggregateRoot,
            cancellationToken);
    }
}
