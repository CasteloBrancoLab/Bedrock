using System.Reflection;
using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Outbox.Interfaces;
using Bedrock.BuildingBlocks.Outbox.Messages;
using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Serialization.Abstractions.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Integration.Environments;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser;
using ShopDemo.Auth.Application.UseCases.RegisterUser;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using ShopDemo.Auth.Infra.Data.PostgreSql.Outbox;
using ShopDemo.Auth.Infra.Data.PostgreSql.Outbox.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using ShopDemo.IntegrationTests.Auth.Application.Connections;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace ShopDemo.IntegrationTests.Auth.Application.Fixtures;

public class AuthApplicationFixture : ServiceCollectionFixture
{
    private static readonly byte[] PepperV1 = [
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
        0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
        0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
    ];

    public PepperConfiguration StandardPepperConfig { get; } = new(
        activePepperVersion: 1,
        peppers: new Dictionary<int, byte[]> { { 1, PepperV1 } }
    );

    public string GetAdminConnectionString()
    {
        return Environments["auth-application"]
            .Postgres["main"]
            .GetConnectionString("testdb");
    }

    public string GetAppUserConnectionString()
    {
        return Environments["auth-application"]
            .Postgres["main"]
            .GetConnectionString("testdb", user: "app_user");
    }

    public ExecutionContext CreateExecutionContext(Guid? tenantCode = null)
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(tenantCode ?? Guid.NewGuid(), "IntegrationTestTenant"),
            executionUser: "integration_test_user",
            executionOrigin: "IntegrationTests",
            businessOperationCode: "TEST_OPERATION",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System
        );
    }

    public AuthPostgreSqlUnitOfWork CreateAppUserUnitOfWork()
    {
        var connection = new TestAuthPostgreSqlConnection(GetAppUserConnectionString());
        var logger = GetService<ILoggerFactory>().CreateLogger<AuthPostgreSqlUnitOfWork>();
        return new AuthPostgreSqlUnitOfWork(logger, connection);
    }

    public UserRepository CreateUserRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
    {
        var dataModelLogger = GetService<ILoggerFactory>().CreateLogger<UserDataModelRepository>();
        var mapper = GetService<IDataModelMapper<UserDataModel>>();
        var dataModelRepo = new UserDataModelRepository(dataModelLogger, unitOfWork, mapper);
        var postgreSqlRepo = new UserPostgreSqlRepository(dataModelRepo);
        var repoLogger = GetService<ILoggerFactory>().CreateLogger<UserRepository>();
        return new UserRepository(repoLogger, postgreSqlRepo);
    }

    public AuthenticationService CreateAuthenticationService(UserRepository userRepository)
    {
        var passwordHasher = new PasswordHasher(StandardPepperConfig);
        return new AuthenticationService(passwordHasher, userRepository);
    }

    public AuthOutboxRepository CreateAuthOutboxRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
    {
        return new AuthOutboxRepository(unitOfWork);
    }

    public AuthOutboxWriter CreateAuthOutboxWriter(IAuthOutboxRepository outboxRepository)
    {
        var serializer = GetService<IStringSerializer>();
        var messageSerializer = new MessageOutboxSerializer(serializer);
        return new AuthOutboxWriter(outboxRepository, messageSerializer, TimeProvider.System);
    }

    public RegisterUserUseCase CreateRegisterUserUseCase(
        IAuthPostgreSqlUnitOfWork unitOfWork,
        AuthenticationService authenticationService,
        IAuthOutboxWriter outboxWriter)
    {
        var logger = GetService<ILoggerFactory>().CreateLogger<RegisterUserUseCase>();
        return new RegisterUserUseCase(
            logger, unitOfWork, authenticationService, outboxWriter, TimeProvider.System);
    }

    public AuthenticateUserUseCase CreateAuthenticateUserUseCase(
        AuthenticationService authenticationService)
    {
        var logger = GetService<ILoggerFactory>().CreateLogger<AuthenticateUserUseCase>();
        return new AuthenticateUserUseCase(logger, authenticationService);
    }

    public async Task<List<OutboxEntry>> GetOutboxEntriesDirectlyAsync(Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT id, tenant_code, correlation_id, payload_type, content_type,
                payload, created_at, status, processed_at, retry_count, is_processing, processing_expiration
            FROM auth_outbox
            WHERE tenant_code = @tenantCode
            ORDER BY created_at
            """,
            connection);

        command.Parameters.AddWithValue("tenantCode", tenantCode);

        var entries = new List<OutboxEntry>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entries.Add(new OutboxEntry
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
            });
        }

        return entries;
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

    public async Task<int> CountUsersDirectlyAsync(Guid tenantCode)
    {
        await using var connection = new NpgsqlConnection(GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*)::int FROM auth_users WHERE tenant_code = @tenantCode",
            connection);

        command.Parameters.AddWithValue("tenantCode", tenantCode);
        return (int)(await command.ExecuteScalarAsync())!;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IDataModelMapper<UserDataModel>, UserDataModelMapper>();
        services.AddSingleton<IStringSerializer, AuthOutboxJsonSerializer>();
    }

    protected override void ConfigureEnvironments(IEnvironmentRegistry environments)
    {
        string seedSql = ReadMigrationSql();

        environments.Register("auth-application", env => env
            .WithPostgres("main", pg => pg
                .WithImage("postgres:17")
                .WithDatabase("testdb", db => db
                    .WithSeedSql(seedSql))
                .WithUser("app_user", "app_password", user => user
                    .WithSchemaPermission("public", PostgresSchemaPermission.Usage)
                    .OnDatabase("testdb", db => db
                        .OnAllTables(PostgresTablePermission.ReadWrite)
                        .OnAllSequences(PostgresSequencePermission.All)))
                .WithResourceLimits(memory: "256m", cpu: 0.5)));
    }

    private static string ReadMigrationSql()
    {
        Assembly migrationAssembly = typeof(ShopDemo.Auth.Infra.Data.PostgreSql.Migrations.AuthMigrationManager).Assembly;

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
