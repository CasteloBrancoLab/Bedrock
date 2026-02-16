using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;
using Npgsql;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Integration tests for PostgreSQL connection and basic operations.
/// </summary>
[Collection("PostgresRepository")]
[Feature("PostgreSQL Connection", "Testes básicos de conexão e autenticação para banco de dados PostgreSQL")]
public class PostgresConnectionIntegrationTests : IntegrationTestBase
{
    private readonly PostgresRepositoryFixture _fixture;

    public PostgresConnectionIntegrationTests(
        PostgresRepositoryFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    [Scenario("Deve conectar ao banco de dados com usuário admin")]
    public async Task Should_connect_to_database_with_admin_user()
    {
        // Arrange
        LogArrange("Obtendo string de conexão do admin");
        var env = UseEnvironment(_fixture.Environments["repository"]);
        var connectionString = env.Postgres["main"].GetConnectionString("testdb");
        LogDatabaseConnection("testdb", "postgres");

        // Act
        LogAct("Abrindo conexão e executando query");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        var result = await command.ExecuteScalarAsync();

        // Assert
        LogAssert("Verificando conexão e execução da query");
        result.ShouldBe(1);
        LogInfo("Successfully connected and executed query");
    }

    [Fact]
    [Scenario("Deve conectar ao banco de dados com usuário app_user")]
    public async Task Should_connect_to_database_with_app_user()
    {
        // Arrange
        LogArrange("Obtendo string de conexão do app_user");
        var env = UseEnvironment(_fixture.Environments["repository"]);
        var connectionString = env.Postgres["main"].GetConnectionString("testdb", user: "app_user");
        LogDatabaseConnection("testdb", "app_user");

        // Act
        LogAct("Abrindo conexão e executando query");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        var result = await command.ExecuteScalarAsync();

        // Assert
        LogAssert("Verificando conexão e execução da query");
        result.ShouldBe(1);
        LogInfo("Successfully connected as app_user");
    }

    [Fact]
    [Scenario("Deve inserir e selecionar dados com usuário app_user")]
    public async Task Should_insert_and_select_with_app_user()
    {
        // Arrange
        LogArrange("Preparando dados de teste");
        var env = UseEnvironment(_fixture.Environments["repository"]);
        var connectionString = env.Postgres["main"].GetConnectionString("testdb", user: "app_user");
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        const string entityName = "Test Entity";

        LogDatabaseConnection("testdb", "app_user");

        // Act
        LogAct("Inserindo e selecionando entidade");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Insert
        await using var insertCommand = new NpgsqlCommand(
            """
            INSERT INTO test_entities (id, tenant_code, name, created_by, created_at,
                created_correlation_id, created_execution_origin, created_business_operation_code)
            VALUES (@id, @tenantCode, @name, @createdBy, @createdAt,
                @createdCorrelationId, @createdExecutionOrigin, @createdBusinessOperationCode)
            """,
            connection);

        insertCommand.Parameters.AddWithValue("id", entityId);
        insertCommand.Parameters.AddWithValue("tenantCode", tenantCode);
        insertCommand.Parameters.AddWithValue("name", entityName);
        insertCommand.Parameters.AddWithValue("createdBy", "test_user");
        insertCommand.Parameters.AddWithValue("createdAt", DateTimeOffset.UtcNow);
        insertCommand.Parameters.AddWithValue("createdCorrelationId", Guid.NewGuid());
        insertCommand.Parameters.AddWithValue("createdExecutionOrigin", "IntegrationTests");
        insertCommand.Parameters.AddWithValue("createdBusinessOperationCode", "TEST_OPERATION");

        await insertCommand.ExecuteNonQueryAsync();
        LogSql("INSERT executed successfully");

        // Select
        await using var selectCommand = new NpgsqlCommand(
            "SELECT name FROM test_entities WHERE id = @id AND tenant_code = @tenantCode",
            connection);

        selectCommand.Parameters.AddWithValue("id", entityId);
        selectCommand.Parameters.AddWithValue("tenantCode", tenantCode);

        var selectedName = await selectCommand.ExecuteScalarAsync();
        LogSql("SELECT executed successfully");

        // Assert
        LogAssert("Verificando que a entidade foi persistida corretamente");
        selectedName.ShouldBe(entityName);
        LogInfo($"Entity '{entityName}' persisted and retrieved successfully");
    }

    [Fact]
    [Scenario("Deve selecionar dados com usuário readonly_user")]
    public async Task Should_select_with_readonly_user()
    {
        // Arrange
        LogArrange("Configurando teste com admin e lendo com usuário readonly");
        var env = UseEnvironment(_fixture.Environments["repository"]);

        // First, insert with admin
        var adminConnectionString = env.Postgres["main"].GetConnectionString("testdb");
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        const string entityName = "Readonly Test Entity";

        await using (var adminConnection = new NpgsqlConnection(adminConnectionString))
        {
            await adminConnection.OpenAsync();

            await using var insertCommand = new NpgsqlCommand(
                """
                INSERT INTO test_entities (id, tenant_code, name, created_by, created_at,
                    created_correlation_id, created_execution_origin, created_business_operation_code)
                VALUES (@id, @tenantCode, @name, @createdBy, @createdAt,
                    @createdCorrelationId, @createdExecutionOrigin, @createdBusinessOperationCode)
                """,
                adminConnection);

            insertCommand.Parameters.AddWithValue("id", entityId);
            insertCommand.Parameters.AddWithValue("tenantCode", tenantCode);
            insertCommand.Parameters.AddWithValue("name", entityName);
            insertCommand.Parameters.AddWithValue("createdBy", "test_user");
            insertCommand.Parameters.AddWithValue("createdAt", DateTimeOffset.UtcNow);
            insertCommand.Parameters.AddWithValue("createdCorrelationId", Guid.NewGuid());
            insertCommand.Parameters.AddWithValue("createdExecutionOrigin", "IntegrationTests");
            insertCommand.Parameters.AddWithValue("createdBusinessOperationCode", "TEST_OPERATION");

            await insertCommand.ExecuteNonQueryAsync();
        }

        LogInfo("Entity inserted with admin user");

        // Act
        LogAct("Selecionando com usuário readonly");
        var readonlyConnectionString = env.Postgres["main"].GetConnectionString("testdb", user: "readonly_user");
        LogDatabaseConnection("testdb", "readonly_user");

        await using var connection = new NpgsqlConnection(readonlyConnectionString);
        await connection.OpenAsync();

        await using var selectCommand = new NpgsqlCommand(
            "SELECT name FROM test_entities WHERE id = @id",
            connection);

        selectCommand.Parameters.AddWithValue("id", entityId);

        var selectedName = await selectCommand.ExecuteScalarAsync();

        // Assert
        LogAssert("Verificando que o usuário readonly consegue selecionar");
        selectedName.ShouldBe(entityName);
        LogInfo("Readonly user successfully retrieved entity");
    }

    [Fact]
    [Scenario("Deve falhar ao inserir dados com usuário readonly_user")]
    public async Task Should_fail_insert_with_readonly_user()
    {
        // Arrange
        LogArrange("Tentando inserção com usuário readonly (deve falhar)");
        var env = UseEnvironment(_fixture.Environments["repository"]);
        var connectionString = env.Postgres["main"].GetConnectionString("testdb", user: "readonly_user");
        LogDatabaseConnection("testdb", "readonly_user");

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var insertCommand = new NpgsqlCommand(
            """
            INSERT INTO test_entities (id, tenant_code, name, created_by, created_at)
            VALUES (@id, @tenantCode, @name, @createdBy, @createdAt)
            """,
            connection);

        insertCommand.Parameters.AddWithValue("id", Guid.NewGuid());
        insertCommand.Parameters.AddWithValue("tenantCode", Guid.NewGuid());
        insertCommand.Parameters.AddWithValue("name", "Should Fail");
        insertCommand.Parameters.AddWithValue("createdBy", "test_user");
        insertCommand.Parameters.AddWithValue("createdAt", DateTimeOffset.UtcNow);

        // Act & Assert
        LogAct("Executando INSERT (esperando permissão negada)");
        var exception = await Should.ThrowAsync<PostgresException>(
            insertCommand.ExecuteNonQueryAsync);

        LogAssert("Verificando que a permissão foi negada");
        exception.SqlState.ShouldBe("42501"); // insufficient_privilege
        LogInfo($"Permission correctly denied: {exception.MessageText}");
    }
}
