using System.Reflection;
using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Integration.Environments;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Connections;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;

/// <summary>
/// Fixture for Auth PostgreSQL integration tests.
/// Uses the Auth migrations project to seed the database schema.
/// </summary>
public class AuthPostgreSqlFixture : ServiceCollectionFixture
{
    public string GetAdminConnectionString()
    {
        return Environments["auth-repository"]
            .Postgres["main"]
            .GetConnectionString("testdb");
    }

    public string GetAppUserConnectionString()
    {
        return Environments["auth-repository"]
            .Postgres["main"]
            .GetConnectionString("testdb", user: "app_user");
    }

    public string GetReadonlyUserConnectionString()
    {
        return Environments["auth-repository"]
            .Postgres["main"]
            .GetConnectionString("testdb", user: "readonly_user");
    }

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

    public AuthPostgreSqlUnitOfWork CreateUnitOfWork(string connectionString)
    {
        var connection = new TestAuthPostgreSqlConnection(connectionString);
        var logger = GetService<ILoggerFactory>().CreateLogger<AuthPostgreSqlUnitOfWork>();
        return new AuthPostgreSqlUnitOfWork(logger, connection);
    }

    public AuthPostgreSqlUnitOfWork CreateAppUserUnitOfWork()
    {
        return CreateUnitOfWork(GetAppUserConnectionString());
    }

    public UserDataModelRepository CreateDataModelRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
    {
        var logger = GetService<ILoggerFactory>().CreateLogger<UserDataModelRepository>();
        var mapper = GetService<IDataModelMapper<UserDataModel>>();
        return new UserDataModelRepository(logger, unitOfWork, mapper);
    }

    public UserPostgreSqlRepository CreatePostgreSqlRepository(IUserDataModelRepository dataModelRepository)
    {
        return new UserPostgreSqlRepository(dataModelRepository);
    }

    public TestAuthPostgreSqlConnection CreateConnection(string connectionString)
    {
        return new TestAuthPostgreSqlConnection(connectionString);
    }

    public UserDataModel CreateTestUserDataModel(
        Guid? id = null,
        Guid? tenantCode = null,
        string? username = null,
        string? email = null,
        byte[]? passwordHash = null,
        short status = 1,
        long entityVersion = 1)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        return new UserDataModel
        {
            Id = id ?? Guid.NewGuid(),
            TenantCode = tenantCode ?? Guid.NewGuid(),
            Username = username ?? $"testuser_{uniqueId}",
            Email = email ?? $"test_{uniqueId}@example.com",
            PasswordHash = passwordHash ?? GenerateTestPasswordHash(),
            Status = status,
            CreatedBy = "integration_test_user",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "IntegrationTests",
            CreatedBusinessOperationCode = "TEST_OPERATION",
            EntityVersion = entityVersion
        };
    }

    public async Task InsertUserDirectlyAsync(UserDataModel user)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO auth_users (id, tenant_code, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code,
                entity_version, username, email, password_hash, status)
            VALUES (@id, @tenantCode, @createdBy, @createdAt,
                @createdCorrelationId, @createdExecutionOrigin, @createdBusinessOperationCode,
                @lastChangedBy, @lastChangedAt, @lastChangedExecutionOrigin,
                @lastChangedCorrelationId, @lastChangedBusinessOperationCode,
                @entityVersion, @username, @email, @passwordHash, @status)
            """,
            connection);

        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("tenantCode", user.TenantCode);
        command.Parameters.AddWithValue("createdBy", user.CreatedBy);
        command.Parameters.AddWithValue("createdAt", user.CreatedAt);
        command.Parameters.AddWithValue("createdCorrelationId", user.CreatedCorrelationId);
        command.Parameters.AddWithValue("createdExecutionOrigin", user.CreatedExecutionOrigin);
        command.Parameters.AddWithValue("createdBusinessOperationCode", user.CreatedBusinessOperationCode);
        command.Parameters.AddWithValue("lastChangedBy", (object?)user.LastChangedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedAt", (object?)user.LastChangedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedExecutionOrigin", (object?)user.LastChangedExecutionOrigin ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedCorrelationId", (object?)user.LastChangedCorrelationId ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedBusinessOperationCode", (object?)user.LastChangedBusinessOperationCode ?? DBNull.Value);
        command.Parameters.AddWithValue("entityVersion", user.EntityVersion);
        command.Parameters.AddWithValue("username", user.Username);
        command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("passwordHash", user.PasswordHash);
        command.Parameters.AddWithValue("status", user.Status);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<UserDataModel?> GetUserDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT id, tenant_code, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code,
                entity_version, username, email, password_hash, status
            FROM auth_users
            WHERE id = @id AND tenant_code = @tenantCode
            """,
            connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new UserDataModel
        {
            Id = reader.GetGuid(0),
            TenantCode = reader.GetGuid(1),
            CreatedBy = reader.GetString(2),
            CreatedAt = new DateTimeOffset(reader.GetDateTime(3), TimeSpan.Zero),
            CreatedCorrelationId = reader.GetGuid(4),
            CreatedExecutionOrigin = reader.GetString(5),
            CreatedBusinessOperationCode = reader.GetString(6),
            LastChangedBy = reader.IsDBNull(7) ? null : reader.GetString(7),
            LastChangedAt = reader.IsDBNull(8) ? null : new DateTimeOffset(reader.GetDateTime(8), TimeSpan.Zero),
            LastChangedExecutionOrigin = reader.IsDBNull(9) ? null : reader.GetString(9),
            LastChangedCorrelationId = reader.IsDBNull(10) ? null : reader.GetGuid(10),
            LastChangedBusinessOperationCode = reader.IsDBNull(11) ? null : reader.GetString(11),
            EntityVersion = reader.GetInt64(12),
            Username = reader.GetString(13),
            Email = reader.GetString(14),
            PasswordHash = (byte[])reader[15],
            Status = reader.GetInt16(16)
        };
    }

    public async Task CleanupTestDataAsync(Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM auth_users WHERE tenant_code = @tenantCode",
            connection);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteUserDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM auth_users WHERE id = @id AND tenant_code = @tenantCode",
            connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateEntityVersionDirectlyAsync(Guid id, Guid tenantCode, long newVersion)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "UPDATE auth_users SET entity_version = @newVersion WHERE id = @id AND tenant_code = @tenantCode",
            connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        command.Parameters.AddWithValue("newVersion", newVersion);

        await command.ExecuteNonQueryAsync();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IDataModelMapper<UserDataModel>, UserDataModelMapper>();
    }

    protected override void ConfigureEnvironments(IEnvironmentRegistry environments)
    {
        string seedSql = ReadMigrationSql();

        environments.Register("auth-repository", env => env
            .WithPostgres("main", pg => pg
                .WithImage("postgres:17")
                .WithDatabase("testdb", db => db
                    .WithSeedSql(seedSql))
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

    private static byte[] GenerateTestPasswordHash()
    {
        var hash = new byte[64];
        Random.Shared.NextBytes(hash);
        return hash;
    }

    /// <summary>
    /// Reads the migration UP SQL from the Auth migrations assembly embedded resources.
    /// This ensures the test database schema is always in sync with the actual migrations.
    /// </summary>
    private static string ReadMigrationSql()
    {
        Assembly migrationAssembly = typeof(ShopDemo.Auth.Infra.Data.PostgreSql.Migrations.AuthMigrationManager).Assembly;

        // Collect all UP migration scripts in version order
        var upScripts = migrationAssembly
            .GetManifestResourceNames()
            .Where(static name => name.Contains(".Scripts.Up.", StringComparison.Ordinal))
            .OrderBy(static name => name)
            .ToList();

        if (upScripts.Count == 0)
            throw new InvalidOperationException("No migration UP scripts found in the Auth migrations assembly.");

        var sqlParts = new List<string>(upScripts.Count);

        foreach (string resourceName in upScripts)
        {
            using Stream? stream = migrationAssembly.GetManifestResourceStream(resourceName);
            if (stream is null)
                throw new InvalidOperationException($"Could not read embedded resource: {resourceName}");

            using var reader = new StreamReader(stream);
            sqlParts.Add(reader.ReadToEnd());
        }

        return string.Join("\n\n", sqlParts);
    }
}
