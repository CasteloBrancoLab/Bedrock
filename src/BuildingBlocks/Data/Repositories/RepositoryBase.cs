using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Repositories;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Data.Repositories;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: RepositoryBase - Template Method Pattern para Repositórios
═══════════════════════════════════════════════════════════════════════════════

RepositoryBase implementa o Template Method Pattern para fornecer comportamento
consistente de tratamento de erros enquanto permite implementações específicas
de acesso a dados.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Métodos Públicos Encapsulam Tratamento de Erros
───────────────────────────────────────────────────────────────────────────────

Os métodos públicos são "template methods" que:
✅ Chamam os métodos abstratos internos
✅ Capturam exceções e registram com contexto de tracing
✅ Retornam valores padrão em caso de falha (graceful degradation)

Isso garante que:
- Exceções nunca propagam para camadas superiores
- Todas as falhas são registradas com informações de rastreamento
- Sistema permanece estável mesmo com falhas de persistência

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Métodos Abstratos Internos Para Implementação Concreta
───────────────────────────────────────────────────────────────────────────────

Cada método público tem um correspondente *InternalAsync protegido e abstrato:
- GetAllAsync → GetAllInternalAsync
- GetByIdAsync → GetByIdInternalAsync
- ExistsAsync → ExistsInternalAsync
- RegisterNewAsync → RegisterNewInternalAsync
- GetModifiedSinceAsync → GetModifiedSinceInternalAsync

Implementações concretas (MongoRepository, PostgresRepository, etc.)
DEVEM implementar APENAS os métodos internos, nunca os públicos.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Tratamento de Erros com Degradação Graciosa
───────────────────────────────────────────────────────────────────────────────

Em caso de exceção, os métodos retornam valores padrão seguros:
- Task<TAggregateRoot?> → null (agregado não encontrado)
- Task<bool> → false (operação falhou)
- IAsyncEnumerable<T> → AsyncEnumerable.Empty<T>() (lista vazia)

IMPORTANTE: Este comportamento oculta erros dos chamadores.
Considere se isso é desejado para seu caso de uso.
Alternativa: propagar exceções após registrar.

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Abstract base class for repository implementations that provides consistent error handling
/// and distributed tracing integration using the Template Method pattern.
/// </summary>
/// <remarks>
/// <para>
/// This class implements <see cref="IRepository{TAggregateRoot}"/> and provides a uniform
/// error handling strategy across all repository operations. Each public method wraps
/// the corresponding abstract internal method in a try-catch block that logs exceptions
/// with distributed tracing context and returns a safe default value.
/// </para>
/// <para>
/// Concrete implementations should override only the protected abstract internal methods
/// (e.g., <see cref="GetByIdInternalAsync"/>) and focus purely on data access logic.
/// Error handling is automatically provided by the base class.
/// </para>
/// </remarks>
/// <typeparam name="TAggregateRoot">
/// The type of the aggregate root. Must implement <see cref="IAggregateRoot"/>.
/// </typeparam>
public abstract class RepositoryBase<TAggregateRoot>
    : IRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
    /// <summary>
    /// Gets the logger instance for this repository.
    /// </summary>
    /// <remarks>
    /// Use this logger in derived classes for additional logging needs.
    /// For exception logging, prefer using the built-in error handling
    /// provided by the public methods.
    /// </remarks>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TAggregateRoot}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for recording operations and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    protected RepositoryBase(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TAggregateRoot> GetAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        CancellationToken cancellationToken)
    {
        try
        {
            return GetAllInternalAsync(paginationInfo, cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting all aggregate roots.");
            return AsyncEnumerable.Empty<TAggregateRoot>();
        }
    }

    /// <inheritdoc />
    public Task<TAggregateRoot?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        try
        {
            return GetByIdInternalAsync(executionContext, id, cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting aggregate root by ID.");
            return Task.FromResult<TAggregateRoot?>(default);
        }
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        try
        {
            return ExistsInternalAsync(executionContext, id, cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while checking existence of aggregate root by ID.");
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public Task<bool> RegisterNewAsync(
        ExecutionContext executionContext,
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken)
    {
        try
        {
            return RegisterNewInternalAsync(executionContext, aggregateRoot, cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while registering new aggregate root.");
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TAggregateRoot> GetModifiedSinceAsync(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        try
        {
            return GetModifiedSinceInternalAsync(executionContext, timeProvider, since, cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while getting modified aggregate roots since specified time.");
            return AsyncEnumerable.Empty<TAggregateRoot>();
        }
    }

    /// <summary>
    /// When overridden in a derived class, retrieves all aggregate roots with pagination.
    /// </summary>
    /// <param name="paginationInfo">The pagination parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of aggregate roots.</returns>
    protected abstract IAsyncEnumerable<TAggregateRoot> GetAllInternalAsync(
        PaginationInfo paginationInfo,
        CancellationToken cancellationToken);

    /// <summary>
    /// When overridden in a derived class, retrieves an aggregate root by its ID.
    /// </summary>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The aggregate root if found; otherwise, null.</returns>
    protected abstract Task<TAggregateRoot?> GetByIdInternalAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken);

    /// <summary>
    /// When overridden in a derived class, checks if an aggregate root exists.
    /// </summary>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="id">The unique identifier to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the aggregate root exists; otherwise, false.</returns>
    protected abstract Task<bool> ExistsInternalAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken);

    /// <summary>
    /// When overridden in a derived class, registers a new aggregate root.
    /// </summary>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="aggregateRoot">The aggregate root to register.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the registration was successful; otherwise, false.</returns>
    protected abstract Task<bool> RegisterNewInternalAsync(
        ExecutionContext executionContext,
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken);

    /// <summary>
    /// When overridden in a derived class, retrieves aggregate roots modified since a timestamp.
    /// </summary>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="timeProvider">The time provider for consistent time operations.</param>
    /// <param name="since">The timestamp to compare against.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of modified aggregate roots.</returns>
    protected abstract IAsyncEnumerable<TAggregateRoot> GetModifiedSinceInternalAsync(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        CancellationToken cancellationToken);
}
