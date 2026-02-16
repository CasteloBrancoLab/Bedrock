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
[Feature("UserDataModel BinaryImporter", "MapBinaryImporter com NpgsqlBinaryImporter real")]
public class UserDataModelMapperBinaryImporterIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public UserDataModelMapperBinaryImporterIntegrationTests(
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
        LogArrange("Criando usuario e mapper para bulk insert");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        user.LastChangedBy = "bulk_modifier";
        user.LastChangedAt = DateTimeOffset.UtcNow;
        user.LastChangedExecutionOrigin = "BulkImport";
        user.LastChangedCorrelationId = Guid.NewGuid();
        user.LastChangedBusinessOperationCode = "BULK_OP";

        var mapper = new UserDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com NpgsqlConnection real");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_users (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "username, email, password_hash, status) FROM STDIN (FORMAT BINARY)"))
        {
            await importer.StartRowAsync();
            mapper.MapBinaryImporter(importer, user);
            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que o registro foi inserido via bulk");
        var persisted = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(user.Id);
        persisted.Username.ShouldBe(user.Username);
        persisted.Email.ShouldBe(user.Email);
        persisted.PasswordHash.ShouldBe(user.PasswordHash);
        persisted.Status.ShouldBe(user.Status);
        persisted.LastChangedBy.ShouldBe("bulk_modifier");
    }

    [Fact]
    public async Task MapBinaryImporter_Should_HandleNullableFieldsAsNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com campos nullable nulos para bulk insert");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);

        var mapper = new UserDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com campos nullable nulos");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_users (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "username, email, password_hash, status) FROM STDIN (FORMAT BINARY)"))
        {
            await importer.StartRowAsync();
            mapper.MapBinaryImporter(importer, user);
            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que campos nullable foram persistidos como null");
        var persisted = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
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
        LogArrange("Criando multiplos usuarios para bulk insert");
        var tenantCode = Guid.NewGuid();
        var users = Enumerable.Range(0, 5)
            .Select(i => _fixture.CreateTestUserDataModel(
                tenantCode: tenantCode,
                username: $"bulk_user_{i}",
                email: $"bulk_{i}@example.com"))
            .ToList();

        var mapper = new UserDataModelMapper();

        // Act
        LogAct("Executando BinaryImport com multiplos registros");
        await using var connection = new NpgsqlConnection(_fixture.GetAdminConnectionString());
        await connection.OpenAsync();

        await using (var importer = await connection.BeginBinaryImportAsync(
            "COPY auth_users (id, tenant_code, created_by, created_at, " +
            "created_correlation_id, created_execution_origin, created_business_operation_code, " +
            "last_changed_by, last_changed_at, last_changed_execution_origin, " +
            "last_changed_correlation_id, last_changed_business_operation_code, entity_version, " +
            "username, email, password_hash, status) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var user in users)
            {
                await importer.StartRowAsync();
                mapper.MapBinaryImporter(importer, user);
            }

            await importer.CompleteAsync();
        }

        // Assert
        LogAssert("Verificando que todos os registros foram inseridos");
        foreach (var user in users)
        {
            var persisted = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
            persisted.ShouldNotBeNull();
            persisted.Username.ShouldBe(user.Username);
            persisted.Email.ShouldBe(user.Email);
        }
    }
}
