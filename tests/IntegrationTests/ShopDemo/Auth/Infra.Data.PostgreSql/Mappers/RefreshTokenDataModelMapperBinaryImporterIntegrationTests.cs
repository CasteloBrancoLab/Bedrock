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
[Feature("RefreshTokenDataModel BinaryImporter", "MapBinaryImporter com NpgsqlBinaryImporter real")]
public class RefreshTokenDataModelMapperBinaryImporterIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public RefreshTokenDataModelMapperBinaryImporterIntegrationTests(
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
        LogArrange("Criando refresh token e mapper para bulk insert com todos os campos");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            status: 3,
            revokedAt: DateTimeOffset.UtcNow,
            replacedByTokenId: Guid.NewGuid());
        token.LastChangedBy = "bulk_modifier";
        token.LastChangedAt = DateTimeOffset.UtcNow;
        token.LastChangedExecutionOrigin = "BulkImport";
        token.LastChangedCorrelationId = Guid.NewGuid();
        token.LastChangedBusinessOperationCode = "BULK_OP";

        var mapper = new RefreshTokenDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com NpgsqlConnection real");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_refresh_tokens (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, token_hash, family_id, expires_at, status, revoked_at, replaced_by_token_id) FROM STDIN (FORMAT BINARY)"))
        {
            await importer.StartRowAsync();
            mapper.MapBinaryImporter(importer, token);
            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que o registro foi inserido via bulk com todos os campos");
        var persisted = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(token.Id);
        persisted.TenantCode.ShouldBe(token.TenantCode);
        persisted.UserId.ShouldBe(token.UserId);
        persisted.TokenHash.ShouldBe(token.TokenHash);
        persisted.FamilyId.ShouldBe(token.FamilyId);
        persisted.Status.ShouldBe(token.Status);
        persisted.RevokedAt.ShouldNotBeNull();
        persisted.ReplacedByTokenId.ShouldBe(token.ReplacedByTokenId);
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
        LogArrange("Criando refresh token com campos nullable nulos para bulk insert");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            revokedAt: null,
            replacedByTokenId: null);

        var mapper = new RefreshTokenDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com campos nullable nulos");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_refresh_tokens (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, token_hash, family_id, expires_at, status, revoked_at, replaced_by_token_id) FROM STDIN (FORMAT BINARY)"))
        {
            await importer.StartRowAsync();
            mapper.MapBinaryImporter(importer, token);
            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que campos nullable foram persistidos como null");
        var persisted = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.RevokedAt.ShouldBeNull();
        persisted.ReplacedByTokenId.ShouldBeNull();
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
        LogArrange("Criando multiplos refresh tokens para bulk insert");
        var tenantCode = Guid.NewGuid();
        var tokens = Enumerable.Range(0, 5)
            .Select(_ => _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode))
            .ToList();

        var mapper = new RefreshTokenDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com multiplos registros");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_refresh_tokens (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "user_id, token_hash, family_id, expires_at, status, revoked_at, replaced_by_token_id) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var token in tokens)
            {
                await importer.StartRowAsync();
                mapper.MapBinaryImporter(importer, token);
            }

            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que todos os registros foram inseridos");
        foreach (var token in tokens)
        {
            var persisted = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
            persisted.ShouldNotBeNull();
            persisted.UserId.ShouldBe(token.UserId);
            persisted.FamilyId.ShouldBe(token.FamilyId);
            persisted.TokenHash.ShouldBe(token.TokenHash);
        }
    }
}
