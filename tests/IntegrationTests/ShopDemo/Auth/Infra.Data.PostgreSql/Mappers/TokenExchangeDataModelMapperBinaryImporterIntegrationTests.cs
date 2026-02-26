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
[Feature("TokenExchangeDataModel BinaryImporter", "MapBinaryImporter com NpgsqlBinaryImporter real")]
public class TokenExchangeDataModelMapperBinaryImporterIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public TokenExchangeDataModelMapperBinaryImporterIntegrationTests(
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
        LogArrange("Criando token exchange e mapper para bulk insert com todos os campos");
        var tenantCode = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
        exchange.LastChangedBy = "bulk_modifier";
        exchange.LastChangedAt = DateTimeOffset.UtcNow;
        exchange.LastChangedExecutionOrigin = "BulkImport";
        exchange.LastChangedCorrelationId = Guid.NewGuid();
        exchange.LastChangedBusinessOperationCode = "BULK_OP";

        var mapper = new TokenExchangeDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com NpgsqlConnection real");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_token_exchanges (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, subject_token_jti, requested_audience, issued_token_jti, issued_at, expires_at) FROM STDIN (FORMAT BINARY)"))
        {
            await importer.StartRowAsync();
            mapper.MapBinaryImporter(importer, exchange);
            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que o registro foi inserido via bulk com todos os campos");
        var persisted = await _fixture.GetTokenExchangeDirectlyAsync(exchange.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(exchange.Id);
        persisted.TenantCode.ShouldBe(exchange.TenantCode);
        persisted.UserId.ShouldBe(exchange.UserId);
        persisted.SubjectTokenJti.ShouldBe(exchange.SubjectTokenJti);
        persisted.RequestedAudience.ShouldBe(exchange.RequestedAudience);
        persisted.IssuedTokenJti.ShouldBe(exchange.IssuedTokenJti);
        persisted.IssuedAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        persisted.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
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
        LogArrange("Criando token exchange com campos nullable nulos para bulk insert");
        var tenantCode = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);

        var mapper = new TokenExchangeDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com campos nullable nulos");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_token_exchanges (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, subject_token_jti, requested_audience, issued_token_jti, issued_at, expires_at) FROM STDIN (FORMAT BINARY)"))
        {
            await importer.StartRowAsync();
            mapper.MapBinaryImporter(importer, exchange);
            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que campos nullable foram persistidos como null");
        var persisted = await _fixture.GetTokenExchangeDirectlyAsync(exchange.Id, tenantCode);
        persisted.ShouldNotBeNull();
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
        LogArrange("Criando multiplos token exchanges para bulk insert");
        var tenantCode = Guid.NewGuid();
        var exchanges = Enumerable.Range(0, 5)
            .Select(_ => _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode))
            .ToList();

        var mapper = new TokenExchangeDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com multiplos registros");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_token_exchanges (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, subject_token_jti, requested_audience, issued_token_jti, issued_at, expires_at) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var exchange in exchanges)
            {
                await importer.StartRowAsync();
                mapper.MapBinaryImporter(importer, exchange);
            }

            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que todos os registros foram inseridos");
        foreach (var exchange in exchanges)
        {
            var persisted = await _fixture.GetTokenExchangeDirectlyAsync(exchange.Id, tenantCode);
            persisted.ShouldNotBeNull();
            persisted.UserId.ShouldBe(exchange.UserId);
            persisted.SubjectTokenJti.ShouldBe(exchange.SubjectTokenJti);
            persisted.IssuedTokenJti.ShouldBe(exchange.IssuedTokenJti);
        }
    }
}
