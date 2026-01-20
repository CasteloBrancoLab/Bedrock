using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;

/// <summary>
/// Base class for PostgreSQL unit of work implementations.
/// Manages database connections and transactions within a single scope.
/// </summary>
/// <remarks>
/// <para>
/// <b>Thread Safety:</b> This class is NOT thread-safe by design. Each async operation
/// should use its own UnitOfWork instance. Do not share instances across threads or
/// concurrent async operations, as the internal transaction state is not protected.
/// </para>
/// <para>
/// <b>Resource Management:</b> This class implements both IDisposable and IAsyncDisposable.
/// Prefer using <c>await using</c> in async contexts to ensure proper async cleanup.
/// </para>
/// <para>
/// <b>Usage Pattern:</b> Use the <see cref="ExecuteAsync{TInput}"/> method for transactional
/// operations. It handles opening connections, beginning transactions, committing/rolling back,
/// and closing connections automatically.
/// </para>
/// </remarks>
public abstract class PostgreSqlUnitOfWorkBase
    : IPostgreSqlUnitOfWork
{
    // Fields
    private readonly IPostgreSqlConnection _postgreSqlConnection;
    private NpgsqlTransaction? _currentTransaction;

    // Properties
    protected ILogger Logger { get; }
    public string Name { get; }

    // Constructors
    protected PostgreSqlUnitOfWorkBase(
        ILogger logger,
        string name,
        IPostgreSqlConnection postgreSqlConnection
    )
    {
        Logger = logger;
        Name = name;
        _postgreSqlConnection = postgreSqlConnection;
    }

    // Public Methods
    // Stryker disable once Block : Mutante equivalente - remover bloco retorna default (null) que e o esperado inicialmente
    public NpgsqlTransaction? GetCurrentTransaction()
    {
        return _currentTransaction;
    }

    public NpgsqlConnection? GetCurrentConnection()
    {
        return _postgreSqlConnection.GetConnectionObject();
    }

    public NpgsqlCommand CreateNpgsqlCommand(string commandText)
    {
        return new NpgsqlCommand(commandText, GetCurrentConnection(), GetCurrentTransaction());
    }

    /// <summary>
    /// Executes an operation within a transaction.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection and NpgsqlTransaction.
    /// The execution flow depends on database operations that cannot be mocked.
    /// </remarks>
    // Stryker disable all : Requer conexao PostgreSQL ativa e transacao - testes de integracao necessarios
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL ativa e transacao - testes de integracao necessarios")]
    public async Task<bool> ExecuteAsync<TInput>(
        ExecutionContext executionContext,
        TInput input,
        Func<ExecutionContext, TInput, CancellationToken, Task<bool>> handlerAsync,
        CancellationToken cancellationToken
    )
    {
        try
        {
            _ = await OpenConnectionAsync(executionContext, cancellationToken).ConfigureAwait(false);

            _ = await BeginTransactionAsync(executionContext, cancellationToken).ConfigureAwait(false);

            bool operationSucceeded = await handlerAsync(
                executionContext,
                input,
                cancellationToken
            ).ConfigureAwait(false);

            if (!operationSucceeded)
            {
                _ = await RollbackAsync(
                    executionContext,
                    cancellationToken
                ).ConfigureAwait(false);

                return false;
            }

            _ = await CommitAsync(executionContext, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex
            );

            executionContext.AddException(ex);

            _ = await RollbackAsync(
                executionContext,
                cancellationToken
            ).ConfigureAwait(false);

            return false;
        }
        finally
        {
            _ = await CloseConnectionAsync(
                executionContext,
                cancellationToken
            ).ConfigureAwait(false);
        }

        return true;
    }
    // Stryker restore all

    public Task<bool> OpenConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        return _postgreSqlConnection.TryOpenConnectionAsync(executionContext, cancellationToken);
    }
    /// <summary>
    /// Begins a transaction.
    /// </summary>
    /// <remarks>
    /// Cannot be fully unit tested - NpgsqlConnection.BeginTransactionAsync requires active connection.
    /// </remarks>
    // Stryker disable all : Requer conexao PostgreSQL ativa para BeginTransactionAsync
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL ativa para BeginTransactionAsync")]
    public async Task<bool> BeginTransactionAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        if (_currentTransaction is not null)
        {
            return true;
        }

        _currentTransaction = await _postgreSqlConnection.GetConnectionObject()!.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
    // Stryker restore all

    /// <summary>
    /// Closes the connection and disposes transaction asynchronously.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - depends on TryCloseConnectionAsync which requires active connection.
    /// Uses async disposal for NpgsqlTransaction to avoid blocking threads during I/O operations.
    /// Connection cleanup is delegated to TryCloseConnectionAsync which handles DisposeAsync internally.
    /// </remarks>
    // Stryker disable all : Depende de TryCloseConnectionAsync que requer conexao ativa
    [ExcludeFromCodeCoverage(Justification = "Depende de TryCloseConnectionAsync que requer conexao ativa")]
    public async Task<bool> CloseConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
            _currentTransaction = null;
        }

        return await _postgreSqlConnection.TryCloseConnectionAsync(executionContext, cancellationToken).ConfigureAwait(false);
    }
    // Stryker restore all

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <remarks>
    /// Cannot be fully unit tested - NpgsqlTransaction is sealed and cannot be mocked.
    /// Branch with active transaction requires integration tests.
    /// </remarks>
    // Stryker disable all : NpgsqlTransaction e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlTransaction e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> CommitAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        if (_currentTransaction is null)
        {
            return true;
        }

        await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
    // Stryker restore all

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <remarks>
    /// Cannot be fully unit tested - NpgsqlTransaction is sealed and cannot be mocked.
    /// Branch with active transaction requires integration tests.
    /// </remarks>
    // Stryker disable all : NpgsqlTransaction e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlTransaction e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> RollbackAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        if (_currentTransaction is null)
        {
            return true;
        }

        await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
    // Stryker restore all

    #region [ Dispose Pattern ]

    private bool _disposedValue;

    /// <summary>
    /// Disposes the unit of work resources.
    /// </summary>
    /// <remarks>
    /// Cannot be fully unit tested - disposing with active transaction requires integration tests.
    /// </remarks>
    // Stryker disable all : Dispose com transacao ativa requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Dispose com transacao ativa requer testes de integracao")]
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _postgreSqlConnection?.Dispose();
                _currentTransaction?.Dispose();
            }

            _disposedValue = true;
        }
    }
    // Stryker restore all

    // Stryker disable all : Padrao Dispose - chamadas internas ja cobertas por exclusao do Dispose(bool)
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    // Stryker restore all

    /// <summary>
    /// Disposes the unit of work resources asynchronously.
    /// </summary>
    /// <remarks>
    /// Preferred over synchronous Dispose in async contexts.
    /// Uses async disposal for both transaction and connection to avoid blocking threads.
    /// </remarks>
    // Stryker disable all : Padrao DisposeAsync - requer conexao e transacao ativas para teste completo
    [ExcludeFromCodeCoverage(Justification = "Padrao DisposeAsync - requer conexao e transacao ativas para teste completo")]
    public async ValueTask DisposeAsync()
    {
        if (!_disposedValue)
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync().ConfigureAwait(false);
                _currentTransaction = null;
            }

            await _postgreSqlConnection.DisposeAsync().ConfigureAwait(false);

            _disposedValue = true;
        }

        GC.SuppressFinalize(this);
    }
    // Stryker restore all
    #endregion
}
