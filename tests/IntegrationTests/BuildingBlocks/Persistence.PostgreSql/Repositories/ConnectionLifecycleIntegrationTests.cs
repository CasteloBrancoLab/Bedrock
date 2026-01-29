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
        LogArrange("Criando conexão");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();

        // Act
        LogAct("Abrindo conexão");
        var result = await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a conexão está aberta");
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
        LogArrange("Criando e abrindo conexão");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);
        var firstConnectionObject = connection.GetConnectionObject();

        // Act
        LogAct("Chamando TryOpenConnectionAsync novamente");
        var result = await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a segunda chamada retorna true e a conexão permanece aberta");
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
        LogArrange("Criando e abrindo conexão");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Fechando conexão");
        var result = await connection.TryCloseConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a conexão está fechada");
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
        LogArrange("Criando, abrindo e fechando conexão");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);
        await connection.TryCloseConnectionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Chamando TryCloseConnectionAsync novamente em conexão já fechada");
        var result = await connection.TryCloseConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o segundo fechamento retorna true sem erro");
        result.ShouldBeTrue();
        connection.IsOpen().ShouldBeFalse();
        LogInfo("TryCloseConnectionAsync is idempotent");
    }

    [Fact]
    public void IsOpen_Should_ReturnFalse_Initially()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando conexão sem abrir");
        using var connection = _fixture.CreateAppUserConnection();

        // Act
        LogAct("Verificando IsOpen");
        var result = connection.IsOpen();

        // Assert
        LogAssert("Verificando que IsOpen retorna false");
        result.ShouldBeFalse();
        LogInfo("IsOpen correctly returns false before opening");
    }

    [Fact]
    public void GetConnectionObject_Should_ReturnNull_BeforeOpen()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando conexão sem abrir");
        using var connection = _fixture.CreateAppUserConnection();

        // Act
        LogAct("Obtendo objeto de conexão");
        var result = connection.GetConnectionObject();

        // Assert
        LogAssert("Verificando que GetConnectionObject retorna null");
        result.ShouldBeNull();
        LogInfo("GetConnectionObject correctly returns null before opening");
    }

    [Fact]
    public async Task GetConnectionObject_Should_ReturnConnection_AfterOpen()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando e abrindo conexão");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Obtendo objeto de conexão");
        var result = connection.GetConnectionObject();

        // Assert
        LogAssert("Verificando que GetConnectionObject retorna NpgsqlConnection");
        result.ShouldNotBeNull();
        result.State.ShouldBe(System.Data.ConnectionState.Open);
        LogInfo("GetConnectionObject correctly returns open NpgsqlConnection");
    }

    [Fact]
    public async Task DisposeAsync_Should_CloseConnection()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando e abrindo conexão");
        var executionContext = _fixture.CreateExecutionContext();
        var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);
        connection.IsOpen().ShouldBeTrue();

        // Act
        LogAct("Descartando conexão");
        await connection.DisposeAsync();

        // Assert
        LogAssert("Verificando que a conexão está fechada após dispose");
        connection.IsOpen().ShouldBeFalse();
        connection.GetConnectionObject().ShouldBeNull();
        LogInfo("DisposeAsync closed connection correctly");
    }

    [Fact]
    public async Task Dispose_Should_CloseConnection()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando e abrindo conexão");
        var executionContext = _fixture.CreateExecutionContext();
        var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);
        connection.IsOpen().ShouldBeTrue();

        // Act
        LogAct("Descartando conexão de forma síncrona");
        connection.Dispose();

        // Assert
        LogAssert("Verificando que a conexão está fechada após dispose");
        connection.IsOpen().ShouldBeFalse();
        LogInfo("Dispose closed connection correctly");
    }

    [Fact]
    public async Task Connection_Should_ExecuteQueries_WhenOpen()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando e abrindo conexão");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Executando query pela conexão");
        using var npgsqlConnection = connection.GetConnectionObject();
        await using var command = npgsqlConnection!.CreateCommand();
        command.CommandText = "SELECT 1 + 1";
        var result = await command.ExecuteScalarAsync();

        // Assert
        LogAssert("Verificando que a query foi executada com sucesso");
        result.ShouldBe(2);
        LogInfo("Query executed successfully through open connection");
    }

    [Fact]
    public async Task IsOpen_Should_ReflectConnectionState()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando conexão");
        var executionContext = _fixture.CreateExecutionContext();
        await using var connection = _fixture.CreateAppUserConnection();

        // Assert initial state
        connection.IsOpen().ShouldBeFalse();

        // Act - Open
        LogAct("Abrindo conexão");
        await connection.TryOpenConnectionAsync(executionContext, CancellationToken.None);

        // Assert open state
        LogAssert("Verificando que IsOpen reflete o estado aberto");
        connection.IsOpen().ShouldBeTrue();

        // Act - Close
        LogAct("Fechando conexão");
        await connection.TryCloseConnectionAsync(executionContext, CancellationToken.None);

        // Assert closed state
        LogAssert("Verificando que IsOpen reflete o estado fechado");
        connection.IsOpen().ShouldBeFalse();
        LogInfo("IsOpen correctly reflects connection state transitions");
    }
}
