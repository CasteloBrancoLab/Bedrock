using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Outbox.PostgreSql;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;
using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Integration.Environments;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.IntegrationTests.BuildingBlocks.Outbox.PostgreSql.Fixtures;

public class OutboxPostgreSqlFixture : ServiceCollectionFixture
{
    private const string SeedSql =
        """
        CREATE TABLE IF NOT EXISTS outbox (
            id UUID PRIMARY KEY,
            tenant_code UUID NOT NULL,
            correlation_id UUID NOT NULL,
            payload_type TEXT NOT NULL,
            content_type TEXT NOT NULL,
            payload BYTEA NOT NULL,
            created_at TIMESTAMPTZ NOT NULL,
            status SMALLINT NOT NULL,
            processed_at TIMESTAMPTZ,
            retry_count SMALLINT NOT NULL DEFAULT 0,
            is_processing BOOLEAN NOT NULL DEFAULT FALSE,
            processing_expiration TIMESTAMPTZ
        );
        CREATE INDEX IF NOT EXISTS idx_outbox_status_created
            ON outbox(status, created_at)
            WHERE status IN (1, 4);
        """;

    public string GetAdminConnectionString() =>
        Environments["outbox"].Postgres["main"].GetConnectionString("testdb");

    public string GetAppUserConnectionString() =>
        Environments["outbox"].Postgres["main"].GetConnectionString("testdb", user: "app_user");

    public ExecutionContext CreateExecutionContext(Guid? tenantCode = null)
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(tenantCode ?? Guid.NewGuid(), "IntegrationTestTenant"),
            executionUser: "integration_test_user",
            executionOrigin: "IntegrationTests",
            businessOperationCode: "TEST_OPERATION",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    public TestOutboxUnitOfWork CreateUnitOfWork(string connectionString)
    {
        var connection = new TestOutboxConnection(connectionString);
        var logger = GetService<ILoggerFactory>().CreateLogger<TestOutboxUnitOfWork>();
        return new TestOutboxUnitOfWork(logger, connection);
    }

    public TestOutboxUnitOfWork CreateAppUserUnitOfWork() =>
        CreateUnitOfWork(GetAppUserConnectionString());

    public TestOutboxRepository CreateRepository(TestOutboxUnitOfWork unitOfWork,
        Action<OutboxPostgreSqlOptions>? configure = null)
    {
        return new TestOutboxRepository(unitOfWork, configure);
    }

    public OutboxEntry CreateTestEntry(
        Guid? id = null,
        Guid? tenantCode = null,
        Guid? correlationId = null,
        OutboxEntryStatus status = OutboxEntryStatus.Pending,
        byte retryCount = 0,
        bool isProcessing = false,
        DateTimeOffset? processingExpiration = null)
    {
        return new OutboxEntry
        {
            Id = id ?? Guid.NewGuid(),
            TenantCode = tenantCode ?? Guid.NewGuid(),
            CorrelationId = correlationId ?? Guid.NewGuid(),
            PayloadType = "Bedrock.Tests.TestEvent, Bedrock.Tests",
            ContentType = "application/json",
            Payload = """{"value":"test"}"""u8.ToArray(),
            CreatedAt = DateTimeOffset.UtcNow,
            Status = status,
            ProcessedAt = null,
            RetryCount = retryCount,
            IsProcessing = isProcessing,
            ProcessingExpiration = processingExpiration
        };
    }

    public async Task InsertEntryDirectlyAsync(OutboxEntry entry)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO outbox (id, tenant_code, correlation_id, payload_type, content_type,
                payload, created_at, status, processed_at, retry_count, is_processing, processing_expiration)
            VALUES (@id, @tenant_code, @correlation_id, @payload_type, @content_type,
                @payload, @created_at, @status, @processed_at, @retry_count, @is_processing, @processing_expiration)
            """,
            connection);

        command.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, entry.Id);
        command.Parameters.AddWithValue("@tenant_code", NpgsqlDbType.Uuid, entry.TenantCode);
        command.Parameters.AddWithValue("@correlation_id", NpgsqlDbType.Uuid, entry.CorrelationId);
        command.Parameters.AddWithValue("@payload_type", NpgsqlDbType.Text, entry.PayloadType);
        command.Parameters.AddWithValue("@content_type", NpgsqlDbType.Text, entry.ContentType);
        command.Parameters.AddWithValue("@payload", NpgsqlDbType.Bytea, entry.Payload);
        command.Parameters.AddWithValue("@created_at", NpgsqlDbType.TimestampTz, entry.CreatedAt);
        command.Parameters.AddWithValue("@status", NpgsqlDbType.Smallint, (short)(byte)entry.Status);
        command.Parameters.AddWithValue("@processed_at", NpgsqlDbType.TimestampTz,
            (object?)entry.ProcessedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@retry_count", NpgsqlDbType.Smallint, (short)entry.RetryCount);
        command.Parameters.AddWithValue("@is_processing", NpgsqlDbType.Boolean, entry.IsProcessing);
        command.Parameters.AddWithValue("@processing_expiration", NpgsqlDbType.TimestampTz,
            (object?)entry.ProcessingExpiration ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<OutboxEntry?> GetEntryDirectlyAsync(Guid id)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT id, tenant_code, correlation_id, payload_type, content_type,
                payload, created_at, status, processed_at, retry_count, is_processing, processing_expiration
            FROM outbox WHERE id = @id
            """,
            connection);

        command.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, id);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new OutboxEntry
        {
            Id = reader.GetGuid(0),
            TenantCode = reader.GetGuid(1),
            CorrelationId = reader.GetGuid(2),
            PayloadType = reader.GetString(3),
            ContentType = reader.GetString(4),
            Payload = (byte[])reader.GetValue(5),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(6),
            Status = (OutboxEntryStatus)(byte)reader.GetInt16(7),
            ProcessedAt = reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8),
            RetryCount = (byte)reader.GetInt16(9),
            IsProcessing = reader.GetBoolean(10),
            ProcessingExpiration = reader.IsDBNull(11) ? null : reader.GetFieldValue<DateTimeOffset>(11)
        };
    }

    public async Task CleanupAsync()
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("DELETE FROM outbox", connection);
        await command.ExecuteNonQueryAsync();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
    }

    protected override void ConfigureEnvironments(IEnvironmentRegistry environments)
    {
        environments.Register("outbox", env => env
            .WithPostgres("main", pg => pg
                .WithImage("postgres:17")
                .WithDatabase("testdb", db => db
                    .WithSeedSql(SeedSql))
                .WithUser("app_user", "app_password", user => user
                    .WithSchemaPermission("public", PostgresSchemaPermission.Usage)
                    .OnDatabase("testdb", db => db
                        .OnAllTables(PostgresTablePermission.ReadWrite)
                        .OnAllSequences(PostgresSequencePermission.All)))
                .WithResourceLimits(memory: "256m", cpu: 0.5)));
    }
}

public class TestOutboxConnection : PostgreSqlConnectionBase
{
    private readonly string _connectionString;

    public TestOutboxConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureInternal(PostgreSqlConnectionOptions options)
    {
        options.WithConnectionString(_connectionString);
    }
}

public class TestOutboxUnitOfWork : PostgreSqlUnitOfWorkBase
{
    public TestOutboxUnitOfWork(
        ILogger<TestOutboxUnitOfWork> logger,
        TestOutboxConnection connection)
        : base(logger, "TestOutboxUnitOfWork", connection)
    {
    }
}

public class TestOutboxRepository : OutboxPostgreSqlRepositoryBase
{
    private readonly Action<OutboxPostgreSqlOptions>? _configure;

    public TestOutboxRepository(
        TestOutboxUnitOfWork unitOfWork,
        Action<OutboxPostgreSqlOptions>? configure = null)
        : base(unitOfWork)
    {
        _configure = configure;
    }

    protected override void ConfigureInternal(OutboxPostgreSqlOptions options)
    {
        _configure?.Invoke(options);
    }
}
