using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Integration tests for PostgreSqlConnectionBase connection lifecycle management.
/// </summary>
[Collection("PostgresRepository")]
[Feature("Connection Lifecycle", "Gerenciamento do ciclo de vida da conexão PostgreSQL")]
public class ConnectionLifecycleIntegrationTests : IntegrationTestBase
{
    private readonly PostgresRepositoryFixture _fixture;

    public ConnectionLifecycleIntegrationTests(
        PostgresRepositoryFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TryOpenConnectionAsync_Should_OpenConnection()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating connection");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();

        // Act
        LogAct("Opening connection");
        var result = await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying connection is open");
        result.ShouldBeTrue();
        connection.IsOpen().ShouldBeTrue();
        connection.GetConnectionObject().ShouldNotBeNull();
        LogInfo("Connection opened successfully");
    }

    [Fact]
    public async Task TryOpenConnectionAsync_Should_BeIdempotent()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and opening connection");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);
        var firstConnectionObject = connection.GetConnectionObject();

        // Act
        LogAct("Calling TryOpenConnectionAsync again");
        var result = await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying second call returns true and connection is still open");
        result.ShouldBeTrue();
        connection.IsOpen().ShouldBeTrue();
        // Note: The double-check locking pattern returns early if already open
        LogInfo("TryOpenConnectionAsync is idempotent");
    }

    [Fact]
    public async Task TryCloseConnectionAsync_Should_CloseConnection()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and opening connection");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Closing connection");
        var result = await connection.TryCloseConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying connection is closed");
        result.ShouldBeTrue();
        connection.IsOpen().ShouldBeFalse();
        connection.GetConnectionObject().ShouldBeNull();
        LogInfo("Connection closed successfully");
    }

    [Fact]
    public async Task TryCloseConnectionAsync_Should_BeIdempotent()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating, opening, and closing connection");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);
        await connection.TryCloseConnectionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Calling TryCloseConnectionAsync again on already closed connection");
        var result = await connection.TryCloseConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying second close returns true without error");
        result.ShouldBeTrue();
        connection.IsOpen().ShouldBeFalse();
        LogInfo("TryCloseConnectionAsync is idempotent");
    }

    [Fact]
    public void IsOpen_Should_ReturnFalse_Initially()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating connection without opening");
        using var connection = _fixture.CreateAppUserConnection();

        // Act
        LogAct("Checking IsOpen");
        var result = connection.IsOpen();

        // Assert
        LogAssert("Verifying IsOpen returns false");
        result.ShouldBeFalse();
        LogInfo("IsOpen correctly returns false before opening");
    }

    [Fact]
    public void GetConnectionObject_Should_ReturnNull_BeforeOpen()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating connection without opening");
        using var connection = _fixture.CreateAppUserConnection();

        // Act
        LogAct("Getting connection object");
        var result = connection.GetConnectionObject();

        // Assert
        LogAssert("Verifying GetConnectionObject returns null");
        result.ShouldBeNull();
        LogInfo("GetConnectionObject correctly returns null before opening");
    }

    [Fact]
    public async Task GetConnectionObject_Should_ReturnConnection_AfterOpen()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and opening connection");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Getting connection object");
        var result = connection.GetConnectionObject();

        // Assert
        LogAssert("Verifying GetConnectionObject returns NpgsqlConnection");
        result.ShouldNotBeNull();
        result.State.ShouldBe(System.Data.ConnectionState.Open);
        LogInfo("GetConnectionObject correctly returns open NpgsqlConnection");
    }

    [Fact]
    public async Task DisposeAsync_Should_CloseConnection()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and opening connection");
        var executionContext = _fixture.CreateExecutionContext();
        var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);
        connection.IsOpen().ShouldBeTrue();

        // Act
        LogAct("Disposing connection");
        await connection.DisposeAsync();

        // Assert
        LogAssert("Verifying connection is closed after dispose");
        connection.IsOpen().ShouldBeFalse();
        connection.GetConnectionObject().ShouldBeNull();
        LogInfo("DisposeAsync closed connection correctly");
    }

    [Fact]
    public async Task Dispose_Should_CloseConnection()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and opening connection");
        var executionContext = _fixture.CreateExecutionContext();
        var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);
        connection.IsOpen().ShouldBeTrue();

        // Act
        LogAct("Disposing connection synchronously");
        connection.Dispose();

        // Assert
        LogAssert("Verifying connection is closed after dispose");
        connection.IsOpen().ShouldBeFalse();
        LogInfo("Dispose closed connection correctly");
    }

    [Fact]
    public async Task Connection_Should_ExecuteQueries_WhenOpen()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and opening connection");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Executing query through connection");
        using var npgsqlConnection = connection.GetConnectionObject();
        await using var command = npgsqlConnection!.CreateCommand();
        command.CommandText = "SELECT 1 + 1";
        var result = await command.ExecuteScalarAsync();

        // Assert
        LogAssert("Verifying query executed successfully");
        result.ShouldBe(2);
        LogInfo("Query executed successfully through open connection");
    }

    [Fact]
    public async Task IsOpen_Should_ReflectConnectionState()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating connection");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();

        // Assert initial state
        connection.IsOpen().ShouldBeFalse();

        // Act - Open
        LogAct("Opening connection");
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Assert open state
        LogAssert("Verifying IsOpen reflects open state");
        connection.IsOpen().ShouldBeTrue();

        // Act - Close
        LogAct("Closing connection");
        await connection.TryCloseConnectionAsync(executionContext, CancellationToken.None);

        // Assert closed state
        LogAssert("Verifying IsOpen reflects closed state");
        connection.IsOpen().ShouldBeFalse();
        LogInfo("IsOpen correctly reflects connection state transitions");
    }
}
