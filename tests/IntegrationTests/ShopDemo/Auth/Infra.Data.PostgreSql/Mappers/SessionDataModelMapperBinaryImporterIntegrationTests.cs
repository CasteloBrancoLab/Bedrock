using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Npgsql;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Mappers;

[Collection("AuthPostgreSql")]
[Feature("SessionDataModel BinaryImporter", "MapBinaryImporter com NpgsqlBinaryImporter real")]
public class SessionDataModelMapperBinaryImporterIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public SessionDataModelMapperBinaryImporterIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MapBinaryImporter_Should_BulkInsertWithAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao e mapper para bulk insert com todos os campos");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            status: 2,
            revokedAt: DateTimeOffset.UtcNow);
        session.LastChangedBy = "bulk_modifier";
        session.LastChangedAt = DateTimeOffset.UtcNow;
        session.LastChangedExecutionOrigin = "BulkImport";
        session.LastChangedCorrelationId = Guid.NewGuid();
        session.LastChangedBusinessOperationCode = "BULK_OP";

        var mapper = new SessionDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com NpgsqlConnection real");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_sessions (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, refresh_token_id, device_info, ip_address, user_agent, " +
            "expires_at, status, last_activity_at, revoked_at) FROM STDIN (FORMAT BINARY)"))
        {
            await importer.StartRowAsync();
            mapper.MapBinaryImporter(importer, session);
            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que o registro foi inserido via bulk com todos os campos");
        var persisted = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(session.Id);
        persisted.TenantCode.ShouldBe(session.TenantCode);
        persisted.UserId.ShouldBe(session.UserId);
        persisted.RefreshTokenId.ShouldBe(session.RefreshTokenId);
        persisted.DeviceInfo.ShouldBe(session.DeviceInfo);
        persisted.IpAddress.ShouldBe(session.IpAddress);
        persisted.UserAgent.ShouldBe(session.UserAgent);
        persisted.Status.ShouldBe(session.Status);
        persisted.RevokedAt.ShouldNotBeNull();
        persisted.LastChangedBy.ShouldBe("bulk_modifier");
        persisted.LastChangedAt.ShouldNotBeNull();
        persisted.LastChangedExecutionOrigin.ShouldBe("BulkImport");
        persisted.LastChangedCorrelationId.ShouldNotBeNull();
        persisted.LastChangedBusinessOperationCode.ShouldBe("BULK_OP");
    }

    [Fact]
    public async Task MapBinaryImporter_Should_HandleNullableFieldsAsNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao com campos nullable nulos para bulk insert");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            deviceInfo: null,
            ipAddress: null,
            userAgent: null,
            revokedAt: null);

        var mapper = new SessionDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com campos nullable nulos");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_sessions (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, refresh_token_id, device_info, ip_address, user_agent, " +
            "expires_at, status, last_activity_at, revoked_at) FROM STDIN (FORMAT BINARY)"))
        {
            await importer.StartRowAsync();
            mapper.MapBinaryImporter(importer, session);
            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que campos nullable foram persistidos como null");
        var persisted = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.DeviceInfo.ShouldBeNull();
        persisted.IpAddress.ShouldBeNull();
        persisted.UserAgent.ShouldBeNull();
        persisted.RevokedAt.ShouldBeNull();
        persisted.LastChangedBy.ShouldBeNull();
        persisted.LastChangedAt.ShouldBeNull();
        persisted.LastChangedExecutionOrigin.ShouldBeNull();
        persisted.LastChangedCorrelationId.ShouldBeNull();
        persisted.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public async Task MapBinaryImporter_Should_BulkInsertMultipleRecords()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando multiplas sessoes para bulk insert");
        var tenantCode = Guid.NewGuid();
        var sessions = Enumerable.Range(0, 5)
            .Select(_ => _fixture.CreateTestSessionDataModel(tenantCode: tenantCode))
            .ToList();

        var mapper = new SessionDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com multiplos registros");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_sessions (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, refresh_token_id, device_info, ip_address, user_agent, " +
            "expires_at, status, last_activity_at, revoked_at) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var session in sessions)
            {
                await importer.StartRowAsync();
                mapper.MapBinaryImporter(importer, session);
            }

            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que todos os registros foram inseridos");
        foreach (var session in sessions)
        {
            var persisted = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
            persisted.ShouldNotBeNull();
            persisted.UserId.ShouldBe(session.UserId);
            persisted.RefreshTokenId.ShouldBe(session.RefreshTokenId);
            persisted.DeviceInfo.ShouldBe(session.DeviceInfo);
        }
    }
}
