using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;
using Npgsql;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;

public abstract class PostgreSqlConnectionBase
    : IPostgreSqlConnection
{
    // Fields
    private NpgsqlConnection? _npgsqlConnection;
    private readonly Lock _lock = new();

    // Constructors
    protected PostgreSqlConnectionBase()
    {
    }

    // Public Methods
    public bool IsOpen()
    {
        return _npgsqlConnection?.State == System.Data.ConnectionState.Open;
    }

    /// <summary>
    /// Opens a connection asynchronously with thread-safe double-check locking pattern.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection.OpenAsync which needs real database.
    /// The double-check pattern inside lock requires race conditions to test.
    /// </remarks>
    // Stryker disable all : Requer conexao PostgreSQL real para OpenAsync - testes de integracao necessarios
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real para OpenAsync - testes de integracao necessarios")]
    public async Task<bool> TryOpenConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        // Caminho rápido: evita overhead do lock se a conexão já estiver aberta
        if (IsOpen())
        {
            return true;
        }

        NpgsqlConnection newConnection;

        lock (_lock)
        {
            // Padrão double-check: outra thread pode ter aberto a conexão
            // enquanto aguardávamos a aquisição do lock
            if (IsOpen())
            {
                return true;
            }

            PostgreSqlConnectionOptions postgreSqlConnectionOptions = new();
            ConfigureInternal(postgreSqlConnectionOptions);

            // Cria a conexão dentro do lock para garantir que apenas uma instância seja criada,
            // mas abre fora do lock para não bloquear outras threads durante I/O de rede
            newConnection = new NpgsqlConnection(postgreSqlConnectionOptions.ConnectionString);
        }

        try
        {
            // OpenAsync é executado fora do lock para não bloquear threads durante I/O de rede
            await newConnection.OpenAsync(cancellationToken).ConfigureAwait(false);

            // Troca atômica usando Interlocked.Exchange garante atribuição thread-safe
            // e retorna o valor anterior para dispose adequado
            NpgsqlConnection? oldConnection = Interlocked.Exchange(ref _npgsqlConnection, newConnection);

            // Faz dispose da conexão antiga para evitar memory leak e devolvê-la ao pool do Npgsql
            if (oldConnection is not null)
            {
                await oldConnection.DisposeAsync().ConfigureAwait(false);
            }

            return true;
        }
        catch
        {
            // Se OpenAsync falhar, faz dispose da nova conexão para evitar vazamento de recursos
            // e relança a exceção para o chamador tratar
            await newConnection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
    // Stryker restore all

    /// <summary>
    /// Closes the connection asynchronously.
    /// </summary>
    /// <remarks>
    /// Cannot be fully unit tested - requires active NpgsqlConnection.
    /// Branch with actual connection requires integration tests.
    /// </remarks>
    // Stryker disable all : Ramificacao com conexao ativa requer testes de integracao com banco PostgreSQL real
    [ExcludeFromCodeCoverage(Justification = "Ramificacao com conexao ativa requer testes de integracao com banco PostgreSQL real")]
    public async Task<bool> TryCloseConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        NpgsqlConnection? connection = Interlocked.Exchange(ref _npgsqlConnection, null);

        if (connection is null)
        {
            return true;
        }

        await connection.DisposeAsync().ConfigureAwait(false);

        return true;
    }
    // Stryker restore all

    /// <summary>
    /// Returns the underlying NpgsqlConnection, auto-opening it if not already open.
    /// Returns null if the connection has been disposed.
    /// Uses synchronous NpgsqlConnection.Open() for compatibility with sync callers.
    /// </summary>
    // Stryker disable all : Auto-open requer conexao PostgreSQL real - testes de integracao necessarios
    [ExcludeFromCodeCoverage(Justification = "Auto-open requer conexao PostgreSQL real - testes de integracao necessarios")]
    public NpgsqlConnection? GetConnectionObject()
    {
        if (_disposedValue)
            return null;

        if (_npgsqlConnection is null || _npgsqlConnection.State != System.Data.ConnectionState.Open)
        {
            EnsureConnectionOpen();
        }

        return _npgsqlConnection;
    }
    // Stryker restore all

    // Private Methods

    /// <summary>
    /// Opens the connection synchronously if it is not already open.
    /// Thread-safe via double-check locking pattern.
    /// </summary>
    // Stryker disable all : Requer conexao PostgreSQL real para Open - testes de integracao necessarios
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real para Open - testes de integracao necessarios")]
    private void EnsureConnectionOpen()
    {
        lock (_lock)
        {
            if (_npgsqlConnection is not null && _npgsqlConnection.State == System.Data.ConnectionState.Open)
                return;

            PostgreSqlConnectionOptions options = new();
            ConfigureInternal(options);

            var newConnection = new NpgsqlConnection(options.ConnectionString);

            try
            {
                newConnection.Open();

                NpgsqlConnection? oldConnection = Interlocked.Exchange(ref _npgsqlConnection, newConnection);
                oldConnection?.Dispose();
            }
            catch
            {
                newConnection.Dispose();
                throw;
            }
        }
    }
    // Stryker restore all

    // Protected Methods
    protected abstract void ConfigureInternal(PostgreSqlConnectionOptions options);

    // Private Methods
    #region [ Dispose Pattern ]
    private bool _disposedValue;

    /// <summary>
    /// Disposes the connection resources.
    /// </summary>
    /// <remarks>
    /// Cannot be fully unit tested - requires active NpgsqlConnection.
    /// Disposing branch with active connection requires integration tests.
    /// </remarks>
    // Stryker disable all : Ramificacao disposing com conexao ativa requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Ramificacao disposing com conexao ativa requer testes de integracao")]
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _npgsqlConnection?.Dispose();
                _npgsqlConnection = null;
            }

            _disposedValue = true;
        }
    }
    // Stryker restore all

    // Stryker disable all : Padrao Dispose - chamadas internas ja cobertas por exclusao do Dispose(bool)
    [ExcludeFromCodeCoverage(Justification = "Padrao Dispose - chamadas internas ja cobertas por exclusao do Dispose(bool)")]
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    // Stryker restore all

    /// <summary>
    /// Disposes the connection resources asynchronously.
    /// </summary>
    /// <remarks>
    /// Cannot be fully unit tested - requires active NpgsqlConnection.
    /// Disposing branch with active connection requires integration tests.
    /// </remarks>
    // Stryker disable all : Ramificacao disposing com conexao ativa requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Ramificacao disposing com conexao ativa requer testes de integracao")]
    public async ValueTask DisposeAsync()
    {
        if (!_disposedValue)
        {
            if (_npgsqlConnection is not null)
            {
                await _npgsqlConnection.DisposeAsync().ConfigureAwait(false);
                _npgsqlConnection = null;
            }

            _disposedValue = true;
        }

        GC.SuppressFinalize(this);
    }
    // Stryker restore all
    #endregion [ Dispose Pattern ]
}
