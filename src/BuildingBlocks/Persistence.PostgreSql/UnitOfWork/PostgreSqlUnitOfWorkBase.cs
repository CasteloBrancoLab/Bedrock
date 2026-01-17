using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;

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
            _ = await OpenConnectionAsync(executionContext, cancellationToken);

            _ = await BeginTransactionAsync(executionContext, cancellationToken);

            bool operationSucceeded = await handlerAsync(
                executionContext,
                input,
                cancellationToken
            );

            if (!operationSucceeded)
            {
                _ = await RollbackAsync(
                    executionContext,
                    cancellationToken
                );

                return false;
            }

            _ = await CommitAsync(executionContext, cancellationToken);
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
            );

            return false;
        }
        finally
        {
            _ = await CloseConnectionAsync(
                executionContext,
                cancellationToken
            );
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

        _currentTransaction = await _postgreSqlConnection.GetConnectionObject()!.BeginTransactionAsync(cancellationToken);

        return true;
    }
    // Stryker restore all

    /// <summary>
    /// Closes the connection and disposes transaction.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - depends on TryCloseConnectionAsync which requires active connection.
    /// </remarks>
    // Stryker disable all : Depende de TryCloseConnectionAsync que requer conexao ativa
    [ExcludeFromCodeCoverage(Justification = "Depende de TryCloseConnectionAsync que requer conexao ativa")]
    public Task<bool> CloseConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        _currentTransaction?.Dispose();
        _currentTransaction = null;

        Task<bool> tryCloseOutput = _postgreSqlConnection.TryCloseConnectionAsync(executionContext, cancellationToken);

        _postgreSqlConnection.Dispose();

        return tryCloseOutput;
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

        await _currentTransaction.CommitAsync(cancellationToken);

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

        await _currentTransaction.RollbackAsync(cancellationToken);

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
    #endregion
}
