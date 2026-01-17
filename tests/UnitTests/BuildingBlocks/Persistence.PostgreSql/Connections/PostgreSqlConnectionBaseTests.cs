using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Connections;

public class PostgreSqlConnectionBaseTests : TestBase
{
    private readonly ExecutionContext _executionContext;

    public PostgreSqlConnectionBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Mock<TimeProvider> timeProviderMock = new();
        timeProviderMock.Setup(tp => tp.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        _executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            executionUser: "test-user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST",
            minimumMessageType: MessageType.Information,
            timeProvider: timeProviderMock.Object
        );
    }

    [Fact]
    public void IsOpen_WhenNotConnected_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating new connection instance");
        using TestablePostgreSqlConnection connection = new("invalid_connection_string");

        // Act
        LogAct("Checking if connection is open");
        bool isOpen = connection.IsOpen();

        // Assert
        LogAssert("Verifying connection is not open");
        isOpen.ShouldBeFalse();
    }

    [Fact]
    public void GetConnectionObject_WhenNotConnected_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating new connection instance");
        using TestablePostgreSqlConnection connection = new("invalid_connection_string");

        // Act
        LogAct("Getting connection object");
        var connectionObject = connection.GetConnectionObject();

        // Assert
        LogAssert("Verifying connection object is null");
        connectionObject.ShouldBeNull();
    }

    [Fact]
    public async Task TryCloseConnectionAsync_WhenNotConnected_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating new connection instance");
        using TestablePostgreSqlConnection connection = new("invalid_connection_string");

        // Act
        LogAct("Closing connection that was never opened");
        bool result = await connection.TryCloseConnectionAsync(_executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying close returns true even when not connected");
        result.ShouldBeTrue();
    }

    [Fact]
    public void Dispose_WhenNotConnected_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Creating new connection instance");
        TestablePostgreSqlConnection connection = new("invalid_connection_string");

        // Act & Assert
        LogAct("Disposing connection");
        Should.NotThrow(() => connection.Dispose());
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Creating new connection instance");
        TestablePostgreSqlConnection connection = new("invalid_connection_string");

        // Act & Assert
        LogAct("Disposing connection multiple times");
        Should.NotThrow(() =>
        {
            connection.Dispose();
            connection.Dispose();
        });
    }

    [Fact]
    public async Task DisposeAsync_WhenNotConnected_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Creating new connection instance");
        TestablePostgreSqlConnection connection = new("invalid_connection_string");

        // Act & Assert
        LogAct("Disposing connection asynchronously");
        await Should.NotThrowAsync(async () => await connection.DisposeAsync());
    }

    [Fact]
    public async Task DisposeAsync_WhenCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Creating new connection instance");
        TestablePostgreSqlConnection connection = new("invalid_connection_string");

        // Act & Assert
        LogAct("Disposing connection asynchronously multiple times");
        await Should.NotThrowAsync(async () =>
        {
            await connection.DisposeAsync();
            await connection.DisposeAsync();
        });
    }

    [Fact]
    public async Task TryOpenConnectionAsync_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        LogArrange("Creating connection with invalid connection string");
        using TestablePostgreSqlConnection connection = new("Host=invalid;Database=invalid");

        // Act & Assert
        LogAct("Attempting to open connection with invalid string");
        await Should.ThrowAsync<Exception>(async () =>
            await connection.TryOpenConnectionAsync(_executionContext, CancellationToken.None));
    }

    [Fact]
    public async Task TryOpenConnectionAsync_WithCancellation_ShouldRespectCancellation()
    {
        // Arrange
        LogArrange("Creating connection and cancelled token");
        using TestablePostgreSqlConnection connection = new("Host=localhost;Database=test");
        CancellationTokenSource cts = new();
        cts.Cancel();

        // Act & Assert
        LogAct("Attempting to open connection with cancelled token");
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await connection.TryOpenConnectionAsync(_executionContext, cts.Token));
    }
}

/// <summary>
/// Testable implementation of PostgreSqlConnectionBase
/// </summary>
internal sealed class TestablePostgreSqlConnection : PostgreSqlConnectionBase
{
    private readonly string _connectionString;

    public TestablePostgreSqlConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureInternal(PostgreSqlConnectionOptions options)
    {
        options.WithConnectionString(_connectionString);
    }
}
