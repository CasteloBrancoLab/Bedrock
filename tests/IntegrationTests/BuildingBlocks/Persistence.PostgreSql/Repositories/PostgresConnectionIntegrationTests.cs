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
    public async Task Should_connect_to_database_with_admin_user()
    {
        // Arrange
        LogArrange("Getting admin connection string");
        var connectionString = _fixture.GetAdminConnectionString();
        LogDatabaseConnection("testdb", "postgres");

        // Act
        LogAct("Opening connection and executing query");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        var result = await command.ExecuteScalarAsync();

        // Assert
        LogAssert("Verifying connection and query execution");
        result.ShouldBe(1);
        LogInfo("Successfully connected and executed query");
    }

    [Fact]
    public async Task Should_connect_to_database_with_app_user()
    {
        // Arrange
        LogArrange("Getting app user connection string");
        var connectionString = _fixture.GetAppUserConnectionString();
        LogDatabaseConnection("testdb", "app_user");

        // Act
        LogAct("Opening connection and executing query");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        var result = await command.ExecuteScalarAsync();

        // Assert
        LogAssert("Verifying connection and query execution");
        result.ShouldBe(1);
        LogInfo("Successfully connected as app_user");
    }

    [Fact]
    public async Task Should_insert_and_select_with_app_user()
    {
        // Arrange
        LogArrange("Preparing test data");
        var connectionString = _fixture.GetAppUserConnectionString();
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        const string entityName = "Test Entity";

        LogDatabaseConnection("testdb", "app_user");

        // Act
        LogAct("Inserting and selecting entity");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Insert
        await using var insertCommand = new NpgsqlCommand(
            """
            INSERT INTO test_entities (id, tenant_code, name, created_by, created_at)
            VALUES (@id, @tenantCode, @name, @createdBy, @createdAt)
            """,
            connection);

        insertCommand.Parameters.AddWithValue("id", entityId);
        insertCommand.Parameters.AddWithValue("tenantCode", tenantCode);
        insertCommand.Parameters.AddWithValue("name", entityName);
        insertCommand.Parameters.AddWithValue("createdBy", "test_user");
        insertCommand.Parameters.AddWithValue("createdAt", DateTimeOffset.UtcNow);

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
        LogAssert("Verifying entity was persisted correctly");
        selectedName.ShouldBe(entityName);
        LogInfo($"Entity '{entityName}' persisted and retrieved successfully");
    }

    [Fact]
    public async Task Should_select_with_readonly_user()
    {
        // Arrange
        LogArrange("Setting up test with admin, then reading with readonly user");

        // First, insert with admin
        var adminConnectionString = _fixture.GetAdminConnectionString();
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        const string entityName = "Readonly Test Entity";

        await using (var adminConnection = new NpgsqlConnection(adminConnectionString))
        {
            await adminConnection.OpenAsync();

            await using var insertCommand = new NpgsqlCommand(
                """
                INSERT INTO test_entities (id, tenant_code, name, created_by, created_at)
                VALUES (@id, @tenantCode, @name, @createdBy, @createdAt)
                """,
                adminConnection);

            insertCommand.Parameters.AddWithValue("id", entityId);
            insertCommand.Parameters.AddWithValue("tenantCode", tenantCode);
            insertCommand.Parameters.AddWithValue("name", entityName);
            insertCommand.Parameters.AddWithValue("createdBy", "test_user");
            insertCommand.Parameters.AddWithValue("createdAt", DateTimeOffset.UtcNow);

            await insertCommand.ExecuteNonQueryAsync();
        }

        LogInfo("Entity inserted with admin user");

        // Act
        LogAct("Selecting with readonly user");
        var readonlyConnectionString = _fixture.GetReadonlyUserConnectionString();
        LogDatabaseConnection("testdb", "readonly_user");

        await using var connection = new NpgsqlConnection(readonlyConnectionString);
        await connection.OpenAsync();

        await using var selectCommand = new NpgsqlCommand(
            "SELECT name FROM test_entities WHERE id = @id",
            connection);

        selectCommand.Parameters.AddWithValue("id", entityId);

        var selectedName = await selectCommand.ExecuteScalarAsync();

        // Assert
        LogAssert("Verifying readonly user can select");
        selectedName.ShouldBe(entityName);
        LogInfo("Readonly user successfully retrieved entity");
    }

    [Fact]
    public async Task Should_fail_insert_with_readonly_user()
    {
        // Arrange
        LogArrange("Attempting insert with readonly user (should fail)");
        var connectionString = _fixture.GetReadonlyUserConnectionString();
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
        LogAct("Executing INSERT (expecting permission denied)");
        var exception = await Should.ThrowAsync<PostgresException>(
            insertCommand.ExecuteNonQueryAsync);

        LogAssert("Verifying permission was denied");
        exception.SqlState.ShouldBe("42501"); // insufficient_privilege
        LogInfo($"Permission correctly denied: {exception.MessageText}");
    }
}
