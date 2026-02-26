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
using NpgsqlTypes;
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

    // ── RefreshToken Repository Factories ──

    public RefreshTokenDataModelRepository CreateRefreshTokenDataModelRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
    {
        var logger = GetService<ILoggerFactory>().CreateLogger<RefreshTokenDataModelRepository>();
        var mapper = GetService<IDataModelMapper<RefreshTokenDataModel>>();
        return new RefreshTokenDataModelRepository(logger, unitOfWork, mapper);
    }

    public RefreshTokenPostgreSqlRepository CreateRefreshTokenPostgreSqlRepository(IRefreshTokenDataModelRepository dataModelRepository)
    {
        return new RefreshTokenPostgreSqlRepository(dataModelRepository);
    }

    // ── Session Repository Factories ──

    public SessionDataModelRepository CreateSessionDataModelRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
    {
        var logger = GetService<ILoggerFactory>().CreateLogger<SessionDataModelRepository>();
        var mapper = GetService<IDataModelMapper<SessionDataModel>>();
        return new SessionDataModelRepository(logger, unitOfWork, mapper);
    }

    public SessionPostgreSqlRepository CreateSessionPostgreSqlRepository(ISessionDataModelRepository dataModelRepository)
    {
        return new SessionPostgreSqlRepository(dataModelRepository);
    }

    // ── TokenExchange Repository Factories ──

    public TokenExchangeDataModelRepository CreateTokenExchangeDataModelRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
    {
        var logger = GetService<ILoggerFactory>().CreateLogger<TokenExchangeDataModelRepository>();
        var mapper = GetService<IDataModelMapper<TokenExchangeDataModel>>();
        return new TokenExchangeDataModelRepository(logger, unitOfWork, mapper);
    }

    public TokenExchangePostgreSqlRepository CreateTokenExchangePostgreSqlRepository(ITokenExchangeDataModelRepository dataModelRepository)
    {
        return new TokenExchangePostgreSqlRepository(dataModelRepository);
    }

    // ── RefreshToken Test Data Factory ──

    public RefreshTokenDataModel CreateTestRefreshTokenDataModel(
        Guid? id = null,
        Guid? tenantCode = null,
        Guid? userId = null,
        byte[]? tokenHash = null,
        Guid? familyId = null,
        DateTimeOffset? expiresAt = null,
        short status = 1,
        DateTimeOffset? revokedAt = null,
        Guid? replacedByTokenId = null,
        long entityVersion = 1)
    {
        return new RefreshTokenDataModel
        {
            Id = id ?? Guid.NewGuid(),
            TenantCode = tenantCode ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            TokenHash = tokenHash ?? GenerateTestPasswordHash(),
            FamilyId = familyId ?? Guid.NewGuid(),
            ExpiresAt = TruncateToMicroseconds(expiresAt ?? DateTimeOffset.UtcNow.AddHours(1)),
            Status = status,
            RevokedAt = revokedAt is not null ? TruncateToMicroseconds(revokedAt.Value) : null,
            ReplacedByTokenId = replacedByTokenId,
            CreatedBy = "integration_test_user",
            CreatedAt = TruncateToMicroseconds(DateTimeOffset.UtcNow),
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "IntegrationTests",
            CreatedBusinessOperationCode = "TEST_OPERATION",
            EntityVersion = entityVersion
        };
    }

    // ── Session Test Data Factory ──

    public SessionDataModel CreateTestSessionDataModel(
        Guid? id = null,
        Guid? tenantCode = null,
        Guid? userId = null,
        Guid? refreshTokenId = null,
        string? deviceInfo = "TestDevice",
        string? ipAddress = "127.0.0.1",
        string? userAgent = "TestAgent/1.0",
        DateTimeOffset? expiresAt = null,
        short status = 1,
        DateTimeOffset? lastActivityAt = null,
        DateTimeOffset? revokedAt = null,
        long entityVersion = 1)
    {
        return new SessionDataModel
        {
            Id = id ?? Guid.NewGuid(),
            TenantCode = tenantCode ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            RefreshTokenId = refreshTokenId ?? Guid.NewGuid(),
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = TruncateToMicroseconds(expiresAt ?? DateTimeOffset.UtcNow.AddHours(1)),
            Status = status,
            LastActivityAt = TruncateToMicroseconds(lastActivityAt ?? DateTimeOffset.UtcNow),
            RevokedAt = revokedAt is not null ? TruncateToMicroseconds(revokedAt.Value) : null,
            CreatedBy = "integration_test_user",
            CreatedAt = TruncateToMicroseconds(DateTimeOffset.UtcNow),
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "IntegrationTests",
            CreatedBusinessOperationCode = "TEST_OPERATION",
            EntityVersion = entityVersion
        };
    }

    // ── TokenExchange Test Data Factory ──

    public TokenExchangeDataModel CreateTestTokenExchangeDataModel(
        Guid? id = null,
        Guid? tenantCode = null,
        Guid? userId = null,
        string? subjectTokenJti = null,
        string? requestedAudience = null,
        string? issuedTokenJti = null,
        DateTimeOffset? issuedAt = null,
        DateTimeOffset? expiresAt = null,
        long entityVersion = 1)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        return new TokenExchangeDataModel
        {
            Id = id ?? Guid.NewGuid(),
            TenantCode = tenantCode ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            SubjectTokenJti = subjectTokenJti ?? $"subj_{uniqueId}",
            RequestedAudience = requestedAudience ?? $"aud_{uniqueId}",
            IssuedTokenJti = issuedTokenJti ?? $"iss_{uniqueId}",
            IssuedAt = TruncateToMicroseconds(issuedAt ?? DateTimeOffset.UtcNow),
            ExpiresAt = TruncateToMicroseconds(expiresAt ?? DateTimeOffset.UtcNow.AddHours(1)),
            CreatedBy = "integration_test_user",
            CreatedAt = TruncateToMicroseconds(DateTimeOffset.UtcNow),
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "IntegrationTests",
            CreatedBusinessOperationCode = "TEST_OPERATION",
            EntityVersion = entityVersion
        };
    }

    // ── RefreshToken Direct SQL Helpers ──

    public async Task InsertRefreshTokenDirectlyAsync(RefreshTokenDataModel token)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO auth_refresh_tokens (id, tenant_code, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code,
                entity_version, user_id, token_hash, family_id, expires_at, status, revoked_at, replaced_by_token_id)
            VALUES (@id, @tenantCode, @createdBy, @createdAt,
                @createdCorrelationId, @createdExecutionOrigin, @createdBusinessOperationCode,
                @lastChangedBy, @lastChangedAt, @lastChangedExecutionOrigin,
                @lastChangedCorrelationId, @lastChangedBusinessOperationCode,
                @entityVersion, @userId, @tokenHash, @familyId, @expiresAt, @status, @revokedAt, @replacedByTokenId)
            """,
            connection);

        AddBaseParameters(command, token);
        command.Parameters.AddWithValue("userId", token.UserId);
        command.Parameters.AddWithValue("tokenHash", token.TokenHash);
        command.Parameters.AddWithValue("familyId", token.FamilyId);
        command.Parameters.AddWithValue("expiresAt", token.ExpiresAt);
        command.Parameters.AddWithValue("status", token.Status);
        command.Parameters.AddWithValue("revokedAt", (object?)token.RevokedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("replacedByTokenId", (object?)token.ReplacedByTokenId ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<RefreshTokenDataModel?> GetRefreshTokenDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT id, tenant_code, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code,
                entity_version, user_id, token_hash, family_id, expires_at, status, revoked_at, replaced_by_token_id
            FROM auth_refresh_tokens
            WHERE id = @id AND tenant_code = @tenantCode
            """,
            connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        var model = new RefreshTokenDataModel();
        ReadBaseFields(reader, model);
        model.UserId = reader.GetGuid(13);
        model.TokenHash = (byte[])reader[14];
        model.FamilyId = reader.GetGuid(15);
        model.ExpiresAt = new DateTimeOffset(reader.GetDateTime(16), TimeSpan.Zero);
        model.Status = reader.GetInt16(17);
        model.RevokedAt = reader.IsDBNull(18) ? null : new DateTimeOffset(reader.GetDateTime(18), TimeSpan.Zero);
        model.ReplacedByTokenId = reader.IsDBNull(19) ? null : reader.GetGuid(19);
        return model;
    }

    public async Task DeleteRefreshTokenDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM auth_refresh_tokens WHERE id = @id AND tenant_code = @tenantCode",
            connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateRefreshTokenEntityVersionDirectlyAsync(Guid id, Guid tenantCode, long newVersion)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "UPDATE auth_refresh_tokens SET entity_version = @newVersion WHERE id = @id AND tenant_code = @tenantCode",
            connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        command.Parameters.AddWithValue("newVersion", newVersion);
        await command.ExecuteNonQueryAsync();
    }

    // ── Session Direct SQL Helpers ──

    public async Task InsertSessionDirectlyAsync(SessionDataModel session)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO auth_sessions (id, tenant_code, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code,
                entity_version, user_id, refresh_token_id, device_info, ip_address, user_agent,
                expires_at, status, last_activity_at, revoked_at)
            VALUES (@id, @tenantCode, @createdBy, @createdAt,
                @createdCorrelationId, @createdExecutionOrigin, @createdBusinessOperationCode,
                @lastChangedBy, @lastChangedAt, @lastChangedExecutionOrigin,
                @lastChangedCorrelationId, @lastChangedBusinessOperationCode,
                @entityVersion, @userId, @refreshTokenId, @deviceInfo, @ipAddress, @userAgent,
                @expiresAt, @status, @lastActivityAt, @revokedAt)
            """,
            connection);

        AddBaseParameters(command, session);
        command.Parameters.AddWithValue("userId", session.UserId);
        command.Parameters.AddWithValue("refreshTokenId", session.RefreshTokenId);
        command.Parameters.AddWithValue("deviceInfo", (object?)session.DeviceInfo ?? DBNull.Value);
        command.Parameters.AddWithValue("ipAddress", (object?)session.IpAddress ?? DBNull.Value);
        command.Parameters.AddWithValue("userAgent", (object?)session.UserAgent ?? DBNull.Value);
        command.Parameters.AddWithValue("expiresAt", session.ExpiresAt);
        command.Parameters.AddWithValue("status", session.Status);
        command.Parameters.AddWithValue("lastActivityAt", session.LastActivityAt);
        command.Parameters.AddWithValue("revokedAt", (object?)session.RevokedAt ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<SessionDataModel?> GetSessionDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT id, tenant_code, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code,
                entity_version, user_id, refresh_token_id, device_info, ip_address, user_agent,
                expires_at, status, last_activity_at, revoked_at
            FROM auth_sessions
            WHERE id = @id AND tenant_code = @tenantCode
            """,
            connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        var model = new SessionDataModel();
        ReadBaseFields(reader, model);
        model.UserId = reader.GetGuid(13);
        model.RefreshTokenId = reader.GetGuid(14);
        model.DeviceInfo = reader.IsDBNull(15) ? null : reader.GetString(15);
        model.IpAddress = reader.IsDBNull(16) ? null : reader.GetString(16);
        model.UserAgent = reader.IsDBNull(17) ? null : reader.GetString(17);
        model.ExpiresAt = new DateTimeOffset(reader.GetDateTime(18), TimeSpan.Zero);
        model.Status = reader.GetInt16(19);
        model.LastActivityAt = new DateTimeOffset(reader.GetDateTime(20), TimeSpan.Zero);
        model.RevokedAt = reader.IsDBNull(21) ? null : new DateTimeOffset(reader.GetDateTime(21), TimeSpan.Zero);
        return model;
    }

    public async Task DeleteSessionDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM auth_sessions WHERE id = @id AND tenant_code = @tenantCode",
            connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateSessionEntityVersionDirectlyAsync(Guid id, Guid tenantCode, long newVersion)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "UPDATE auth_sessions SET entity_version = @newVersion WHERE id = @id AND tenant_code = @tenantCode",
            connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        command.Parameters.AddWithValue("newVersion", newVersion);
        await command.ExecuteNonQueryAsync();
    }

    // ── TokenExchange Direct SQL Helpers ──

    public async Task InsertTokenExchangeDirectlyAsync(TokenExchangeDataModel exchange)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO auth_token_exchanges (id, tenant_code, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code,
                entity_version, user_id, subject_token_jti, requested_audience,
                issued_token_jti, issued_at, expires_at)
            VALUES (@id, @tenantCode, @createdBy, @createdAt,
                @createdCorrelationId, @createdExecutionOrigin, @createdBusinessOperationCode,
                @lastChangedBy, @lastChangedAt, @lastChangedExecutionOrigin,
                @lastChangedCorrelationId, @lastChangedBusinessOperationCode,
                @entityVersion, @userId, @subjectTokenJti, @requestedAudience,
                @issuedTokenJti, @issuedAt, @expiresAt)
            """,
            connection);

        AddBaseParameters(command, exchange);
        command.Parameters.AddWithValue("userId", exchange.UserId);
        command.Parameters.AddWithValue("subjectTokenJti", exchange.SubjectTokenJti);
        command.Parameters.AddWithValue("requestedAudience", exchange.RequestedAudience);
        command.Parameters.AddWithValue("issuedTokenJti", exchange.IssuedTokenJti);
        command.Parameters.AddWithValue("issuedAt", exchange.IssuedAt);
        command.Parameters.AddWithValue("expiresAt", exchange.ExpiresAt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<TokenExchangeDataModel?> GetTokenExchangeDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT id, tenant_code, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code,
                last_changed_by, last_changed_at, last_changed_execution_origin,
                last_changed_correlation_id, last_changed_business_operation_code,
                entity_version, user_id, subject_token_jti, requested_audience,
                issued_token_jti, issued_at, expires_at
            FROM auth_token_exchanges
            WHERE id = @id AND tenant_code = @tenantCode
            """,
            connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        var model = new TokenExchangeDataModel();
        ReadBaseFields(reader, model);
        model.UserId = reader.GetGuid(13);
        model.SubjectTokenJti = reader.GetString(14);
        model.RequestedAudience = reader.GetString(15);
        model.IssuedTokenJti = reader.GetString(16);
        model.IssuedAt = new DateTimeOffset(reader.GetDateTime(17), TimeSpan.Zero);
        model.ExpiresAt = new DateTimeOffset(reader.GetDateTime(18), TimeSpan.Zero);
        return model;
    }

    public async Task DeleteTokenExchangeDirectlyAsync(Guid id, Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM auth_token_exchanges WHERE id = @id AND tenant_code = @tenantCode",
            connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantCode", tenantCode);
        await command.ExecuteNonQueryAsync();
    }

    // ── Shared Helpers ──

    private static void AddBaseParameters(NpgsqlCommand command, Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels.DataModelBase model)
    {
        command.Parameters.AddWithValue("id", model.Id);
        command.Parameters.AddWithValue("tenantCode", model.TenantCode);
        command.Parameters.AddWithValue("createdBy", model.CreatedBy);
        command.Parameters.AddWithValue("createdAt", model.CreatedAt);
        command.Parameters.AddWithValue("createdCorrelationId", model.CreatedCorrelationId);
        command.Parameters.AddWithValue("createdExecutionOrigin", model.CreatedExecutionOrigin);
        command.Parameters.AddWithValue("createdBusinessOperationCode", model.CreatedBusinessOperationCode);
        command.Parameters.AddWithValue("lastChangedBy", (object?)model.LastChangedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedAt", (object?)model.LastChangedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedExecutionOrigin", (object?)model.LastChangedExecutionOrigin ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedCorrelationId", (object?)model.LastChangedCorrelationId ?? DBNull.Value);
        command.Parameters.AddWithValue("lastChangedBusinessOperationCode", (object?)model.LastChangedBusinessOperationCode ?? DBNull.Value);
        command.Parameters.AddWithValue("entityVersion", model.EntityVersion);
    }

    private static void ReadBaseFields(NpgsqlDataReader reader, Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels.DataModelBase model)
    {
        model.Id = reader.GetGuid(0);
        model.TenantCode = reader.GetGuid(1);
        model.CreatedBy = reader.GetString(2);
        model.CreatedAt = new DateTimeOffset(reader.GetDateTime(3), TimeSpan.Zero);
        model.CreatedCorrelationId = reader.GetGuid(4);
        model.CreatedExecutionOrigin = reader.GetString(5);
        model.CreatedBusinessOperationCode = reader.GetString(6);
        model.LastChangedBy = reader.IsDBNull(7) ? null : reader.GetString(7);
        model.LastChangedAt = reader.IsDBNull(8) ? null : new DateTimeOffset(reader.GetDateTime(8), TimeSpan.Zero);
        model.LastChangedExecutionOrigin = reader.IsDBNull(9) ? null : reader.GetString(9);
        model.LastChangedCorrelationId = reader.IsDBNull(10) ? null : reader.GetGuid(10);
        model.LastChangedBusinessOperationCode = reader.IsDBNull(11) ? null : reader.GetString(11);
        model.EntityVersion = reader.GetInt64(12);
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IDataModelMapper<UserDataModel>, UserDataModelMapper>();
        services.AddSingleton<IDataModelMapper<RefreshTokenDataModel>, RefreshTokenDataModelMapper>();
        services.AddSingleton<IDataModelMapper<SessionDataModel>, SessionDataModelMapper>();
        services.AddSingleton<IDataModelMapper<TokenExchangeDataModel>, TokenExchangeDataModelMapper>();
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
    /// Truncates a DateTimeOffset to microsecond precision to match PostgreSQL timestamptz storage.
    /// PostgreSQL stores timestamps with microsecond precision (6 decimal digits),
    /// while .NET DateTimeOffset has tick precision (100ns = 7 decimal digits).
    /// </summary>
    private static DateTimeOffset TruncateToMicroseconds(DateTimeOffset dto) =>
        new(dto.Ticks - (dto.Ticks % 10), dto.Offset);

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
