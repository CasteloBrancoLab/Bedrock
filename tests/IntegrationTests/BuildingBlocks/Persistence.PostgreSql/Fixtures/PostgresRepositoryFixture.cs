using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Integration.Environments;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;

/// <summary>
/// Fixture for PostgreSQL repository integration tests.
/// Provides a configured PostgreSQL container with test databases and users.
/// </summary>
public class PostgresRepositoryFixture : ServiceCollectionFixture
{
    /// <summary>
    /// Gets the connection string for the test database with admin access.
    /// </summary>
    public string GetAdminConnectionString()
    {
        return Environments["repository"]
            .Postgres["main"]
            .GetConnectionString("testdb");
    }

    /// <summary>
    /// Gets the connection string for the test database with app user access.
    /// </summary>
    public string GetAppUserConnectionString()
    {
        return Environments["repository"]
            .Postgres["main"]
            .GetConnectionString("testdb", user: "app_user");
    }

    /// <summary>
    /// Gets the connection string for the test database with readonly user access.
    /// </summary>
    public string GetReadonlyUserConnectionString()
    {
        return Environments["repository"]
            .Postgres["main"]
            .GetConnectionString("testdb", user: "readonly_user");
    }

    /// <summary>
    /// Creates an ExecutionContext for integration tests.
    /// </summary>
    /// <param name="tenantCode">Optional tenant code. If not provided, a new GUID is generated.</param>
    /// <returns>A new ExecutionContext configured for testing.</returns>
    public Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext CreateExecutionContext(Guid? tenantCode = null)
    {
        return Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(tenantCode ?? Guid.NewGuid(), "IntegrationTestTenant"),
            executionUser: "integration_test_user",
            executionOrigin: "IntegrationTests",
            businessOperationCode: "TEST_OPERATION",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System
        );
    }

    /// <summary>
    /// Creates a UnitOfWork with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <returns>A new TestPostgreSqlUnitOfWork instance.</returns>
    public TestPostgreSqlUnitOfWork CreateUnitOfWork(string connectionString)
    {
        var connection = new TestPostgreSqlConnection(connectionString);
        var logger = GetService<ILoggerFactory>().CreateLogger<TestPostgreSqlUnitOfWork>();
        return new TestPostgreSqlUnitOfWork(logger, connection);
    }

    /// <summary>
    /// Creates a UnitOfWork with app user credentials.
    /// </summary>
    /// <returns>A new TestPostgreSqlUnitOfWork instance.</returns>
    public TestPostgreSqlUnitOfWork CreateAppUserUnitOfWork()
    {
        return CreateUnitOfWork(GetAppUserConnectionString());
    }

    /// <summary>
    /// Creates a TestEntityRepository with the specified unit of work.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for the repository.</param>
    /// <returns>A new TestEntityRepository instance.</returns>
    public TestEntityRepository CreateRepository(TestPostgreSqlUnitOfWork unitOfWork)
    {
        var logger = GetService<ILoggerFactory>().CreateLogger<TestEntityRepository>();
        var mapper = GetService<IDataModelMapper<TestEntityDataModel>>();
        return new TestEntityRepository(logger, unitOfWork, mapper);
    }

    /// <summary>
    /// Creates a TestPostgreSqlConnection with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <returns>A new TestPostgreSqlConnection instance.</returns>
    public TestPostgreSqlConnection CreateConnection(string connectionString)
    {
        return new TestPostgreSqlConnection(connectionString);
    }

    /// <summary>
    /// Creates a TestPostgreSqlConnection with app user credentials.
    /// </summary>
    /// <returns>A new TestPostgreSqlConnection instance.</returns>
    public TestPostgreSqlConnection CreateAppUserConnection()
    {
        return CreateConnection(GetAppUserConnectionString());
    }

    /// <summary>
    /// Creates a TestEntityDataModel with test data.
    /// </summary>
    /// <param name="id">Optional entity ID. If not provided, a new GUID is generated.</param>
    /// <param name="tenantCode">Optional tenant code. If not provided, a new GUID is generated.</param>
    /// <param name="name">Optional entity name. If not provided, a unique name is generated.</param>
    /// <param name="entityVersion">The entity version for optimistic concurrency. Defaults to 1.</param>
    /// <returns>A new TestEntityDataModel instance.</returns>
    public TestEntityDataModel CreateTestEntity(
        Guid? id = null,
        Guid? tenantCode = null,
        string? name = null,
        long entityVersion = 1)
    {
        return new TestEntityDataModel
        {
            Id = id ?? Guid.NewGuid(),
            TenantCode = tenantCode ?? Guid.NewGuid(),
            Name = name ?? $"TestEntity_{Guid.NewGuid():N}",
            CreatedBy = "integration_test_user",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = entityVersion
        };
    }

    /// <summary>
    /// Cleans up test data for a specific tenant.
    /// </summary>
    /// <param name="tenantCode">The tenant code to clean up.</param>
    public async Task CleanupTestDataAsync(Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM test_entities WHERE tenant_code = @tenantCode",
            connection);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Inserts a test entity directly using raw SQL (bypassing repository).
    /// Useful for setting up test data.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    public async Task InsertTestEntityDirectlyAsync(TestEntityDataModel entity)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO test_entities (id, tenant_code, name, created_by, created_at,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code, entity_version)
            VALUES (@id, @tenantCode, @name, @createdBy, @createdAt,
                @lastChangedBy, @lastChangedAt, @lastChangedExecutionOrigin,
                @lastChangedCorrelationId, @lastChangedBusinessOperationCode, @entityVersion)
            """,
            connection);

        command.Parameters.AddWithValue("id", entity.Id);
        command.Parameters.AddWithValue("tenantCode", entity.TenantCode);
        command.Parameters.AddWithValue("name", entity.Name);
        command.Parameters.AddWithValue("createdBy", entity.CreatedBy);
        command.Parameters.AddWithValue("createdAt", entity.CreatedAt);
        command.Parameters.AddWithValue("lastChangedBy", (object?)entity.LastChangedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedAt", (object?)entity.LastChangedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedExecutionOrigin", (object?)entity.LastChangedExecutionOrigin ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedCorrelationId", (object?)entity.LastChangedCorrelationId ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedBusinessOperationCode", (object?)entity.LastChangedBusinessOperationCode ?? DBNull.Value);
        command.Parameters.AddWithValue("entityVersion", entity.EntityVersion);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Gets a test entity directly using raw SQL (bypassing repository).
    /// Useful for verifying test results.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="tenantCode">The tenant code.</param>
    /// <returns>The entity if found, null otherwise.</returns>
    public async Task<TestEntityDataModel?> GetTestEntityDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT id, tenant_code, name, created_by, created_at,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code, entity_version
            FROM test_entities
            WHERE id = @id AND tenant_code = @tenantCode
            """,
            connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new TestEntityDataModel
        {
            Id = reader.GetGuid(0),
            TenantCode = reader.GetGuid(1),
            Name = reader.GetString(2),
            CreatedBy = reader.GetString(3),
            CreatedAt = new DateTimeOffset(reader.GetDateTime(4), TimeSpan.Zero),
            LastChangedBy = reader.IsDBNull(5) ? null : reader.GetString(5),
            LastChangedAt = reader.IsDBNull(6) ? null : new DateTimeOffset(reader.GetDateTime(6), TimeSpan.Zero),
            LastChangedExecutionOrigin = reader.IsDBNull(7) ? null : reader.GetString(7),
            LastChangedCorrelationId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
            LastChangedBusinessOperationCode = reader.IsDBNull(9) ? null : reader.GetString(9),
            EntityVersion = reader.GetInt64(10)
        };
    }

    /// <summary>
    /// Updates a test entity version directly using raw SQL (bypassing repository).
    /// Useful for testing optimistic concurrency.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="tenantCode">The tenant code.</param>
    /// <param name="newVersion">The new version number.</param>
    public async Task UpdateEntityVersionDirectlyAsync(Guid id, Guid tenantCode, long newVersion)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "UPDATE test_entities SET entity_version = @newVersion WHERE id = @id AND tenant_code = @tenantCode",
            connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        command.Parameters.AddWithValue("newVersion", newVersion);

        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc />
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IDataModelMapper<TestEntityDataModel>, TestEntityDataModelMapper>();
    }

    /// <inheritdoc />
    protected override void ConfigureEnvironments(IEnvironmentRegistry environments)
    {
        environments.Register("repository", env => env
            .WithPostgres("main", pg => pg
                .WithImage("postgres:17")
                .WithDatabase("testdb", db => db
                    .WithSeedSql("""
                        CREATE TABLE IF NOT EXISTS test_entities (
                            id UUID PRIMARY KEY,
                            tenant_code UUID NOT NULL,
                            name TEXT NOT NULL,
                            created_by TEXT NOT NULL,
                            created_at TIMESTAMPTZ NOT NULL,
                            last_changed_by TEXT,
                            last_changed_at TIMESTAMPTZ,
                            last_changed_execution_origin TEXT,
                            last_changed_correlation_id UUID,
                            last_changed_business_operation_code TEXT,
                            entity_version BIGINT NOT NULL DEFAULT 1
                        );

                        CREATE INDEX IF NOT EXISTS idx_test_entities_tenant_id
                            ON test_entities(tenant_code, id);
                        """))
                .WithUser("app_user", "app_password", user => user
                    .WithSchemaPermission("public", PostgresSchemaPermission.Usage)
                    .OnDatabase("testdb", db => db
                        .OnAllTables(PostgresTablePermission.ReadWrite)
                        .OnAllSequences(PostgresSequencePermission.All)))
                .WithUser("readonly_user", "readonly_password", user => user
                    .WithSchemaPermission("public", PostgresSchemaPermission.Usage)
                    .OnDatabase("testdb", db => db
                        .OnAllTables(PostgresTablePermission.ReadOnly)))
                .WithResourceLimits(memory: "256m", cpu: 0.5)));
    }
}
