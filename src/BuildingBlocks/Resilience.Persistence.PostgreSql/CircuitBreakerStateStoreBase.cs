using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces;
using Bedrock.BuildingBlocks.Resilience.Models;
using Bedrock.BuildingBlocks.Resilience.Persistence.PostgreSql.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Bedrock.BuildingBlocks.Resilience.Persistence.PostgreSql;

/// <summary>
/// Abstract base class for a PostgreSQL-backed distributed circuit breaker state store.
/// Uses the Bedrock <see cref="IPostgreSqlConnection"/> abstraction for connection management.
/// </summary>
/// <remarks>
/// <para>
/// Follows the same quality patterns as <c>DataModelRepositoryBase</c>:
/// all methods receive <see cref="ExecutionContext"/> for distributed tracing,
/// exceptions are caught, logged via <c>LogExceptionForDistributedTracing</c>,
/// and registered via <see cref="ExecutionContext.AddException"/>.
/// </para>
/// <para>
/// Subclasses override <see cref="ConfigureInternal"/> to provide schema and table name.
/// The connection is injected via constructor — the client implementation decides whether to use
/// a dedicated connection or share an existing one.
/// </para>
/// <para>
/// SQL statements are built once on first use and cached for the lifetime of the singleton.
/// Uses <see cref="IPostgreSqlConnection.GetConnectionObject()"/> which auto-opens the connection
/// via the base class's thread-safe <c>EnsureConnectionOpen</c> pattern.
/// </para>
/// <para>
/// Uses optimistic concurrency: <c>UPDATE ... WHERE updated_at &lt; @updatedAt</c> ensures that
/// only the most recent state change wins when multiple instances write concurrently.
/// </para>
/// <para>
/// The table and index are auto-created on first use via <c>CREATE TABLE IF NOT EXISTS</c>
/// and <c>CREATE INDEX IF NOT EXISTS</c>.
/// </para>
/// </remarks>
public abstract class CircuitBreakerStateStoreBase : ICircuitBreakerStateStore
{
    private readonly IPostgreSqlConnection _connection;
    private readonly ILogger _logger;
    private string _selectSql = null!;
    private string _upsertSql = null!;
    private string _createTableSql = null!;
    private string _createIndexSql = null!;
    private volatile bool _configured;
    private volatile bool _schemaInitialized;

    /// <summary>
    /// Initializes the state store with a PostgreSQL connection and logger.
    /// </summary>
    /// <param name="logger">The logger for distributed tracing.</param>
    /// <param name="connection">
    /// The PostgreSQL connection abstraction. The client implementation decides the connection lifecycle
    /// (dedicated singleton connection or shared).
    /// </param>
    protected CircuitBreakerStateStoreBase(ILogger logger, IPostgreSqlConnection connection)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(connection);

        _logger = logger;
        _connection = connection;
    }

    /// <summary>
    /// Configures the state store options. Called once on first use.
    /// </summary>
    /// <param name="options">The options to configure with schema and table name.</param>
    protected abstract void ConfigureInternal(CircuitBreakerStateStoreOptions options);

    // ================================
    // ICircuitBreakerStateStore
    // ================================

    /// <inheritdoc />
    // Stryker disable all : NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao")]
    public async Task<CircuitBreakerStateEntry?> GetStateAsync(
        ExecutionContext executionContext,
        string policyCode,
        CancellationToken cancellationToken)
    {
        try
        {
            EnsureConfigured();
            await EnsureSchemaInitializedAsync(executionContext, cancellationToken).ConfigureAwait(false);

            var connection = GetConnection();
            if (connection is null)
                return null;

            await using var command = CreateSelectCommand(connection, policyCode);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                return null;

            return MapStateEntry(reader);
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return null;
        }
    }
    // Stryker restore all

    /// <inheritdoc />
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> UpdateStateAsync(
        ExecutionContext executionContext,
        string policyCode,
        CircuitBreakerDistributedState state,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            EnsureConfigured();
            await EnsureSchemaInitializedAsync(executionContext, cancellationToken).ConfigureAwait(false);

            var connection = GetConnection();
            if (connection is null)
                return false;

            await using var command = CreateUpsertCommand(connection, policyCode, state, updatedAt);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return false;
        }
    }
    // Stryker restore all

    // ================================
    // Configuration
    // ================================

    private void EnsureConfigured()
    {
        if (_configured)
            return;

        var options = new CircuitBreakerStateStoreOptions();
        ConfigureInternal(options);

        var fullTableName = $"{options.Schema}.{options.TableName}";
        BuildSqlStatements(fullTableName);

        _configured = true;
    }

    private void BuildSqlStatements(string fullTableName)
    {
        _createTableSql = $"""
            CREATE TABLE IF NOT EXISTS {fullTableName} (
                policy_code VARCHAR(256) PRIMARY KEY,
                state SMALLINT NOT NULL,
                updated_at TIMESTAMPTZ NOT NULL
            )
            """;

        _createIndexSql = $"""
            CREATE INDEX IF NOT EXISTS ix_{fullTableName.Replace('.', '_')}_updated_at
            ON {fullTableName} (updated_at)
            """;

        _selectSql = $"""
            SELECT state, updated_at
            FROM {fullTableName}
            WHERE policy_code = @policyCode
            """;

        _upsertSql = $"""
            INSERT INTO {fullTableName} (policy_code, state, updated_at)
            VALUES (@policyCode, @state, @updatedAt)
            ON CONFLICT (policy_code) DO UPDATE
            SET state = EXCLUDED.state, updated_at = EXCLUDED.updated_at
            WHERE {fullTableName}.updated_at < EXCLUDED.updated_at
            """;
    }

    // ================================
    // Schema Initialization
    // ================================

    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    private async Task EnsureSchemaInitializedAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        if (_schemaInitialized)
            return;

        var connection = GetConnection();
        if (connection is null)
            return;

        try
        {
            await using var createTableCommand = new NpgsqlCommand(_createTableSql, connection);
            await createTableCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await using var createIndexCommand = new NpgsqlCommand(_createIndexSql, connection);
            await createIndexCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _schemaInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
        }
    }
    // Stryker restore all

    // ================================
    // Connection
    // ================================

    private NpgsqlConnection? GetConnection()
    {
        return _connection.GetConnectionObject();
    }

    // ================================
    // Command Helpers
    // ================================

    private NpgsqlCommand CreateSelectCommand(NpgsqlConnection connection, string policyCode)
    {
        var command = new NpgsqlCommand(_selectSql, connection);
        command.Parameters.AddWithValue("policyCode", NpgsqlDbType.Varchar, policyCode);
        return command;
    }

    private NpgsqlCommand CreateUpsertCommand(
        NpgsqlConnection connection,
        string policyCode,
        CircuitBreakerDistributedState state,
        DateTimeOffset updatedAt)
    {
        var command = new NpgsqlCommand(_upsertSql, connection);
        command.Parameters.AddWithValue("policyCode", NpgsqlDbType.Varchar, policyCode);
        command.Parameters.AddWithValue("state", NpgsqlDbType.Smallint, (short)state);
        command.Parameters.AddWithValue("updatedAt", NpgsqlDbType.TimestampTz, updatedAt);
        return command;
    }

    // ================================
    // Mapping
    // ================================

    private static CircuitBreakerStateEntry MapStateEntry(NpgsqlDataReader reader)
    {
        return new CircuitBreakerStateEntry(
            State: (CircuitBreakerDistributedState)reader.GetInt16(0),
            UpdatedAt: reader.GetFieldValue<DateTimeOffset>(1));
    }
}
