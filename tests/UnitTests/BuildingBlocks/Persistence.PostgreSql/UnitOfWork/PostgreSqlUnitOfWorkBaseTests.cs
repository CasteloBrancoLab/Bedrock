using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;

public class PostgreSqlUnitOfWorkBaseTests : TestBase
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IPostgreSqlConnection> _connectionMock;
    private readonly ExecutionContext _executionContext;

    public PostgreSqlUnitOfWorkBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger>();
        _connectionMock = new Mock<IPostgreSqlConnection>();

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
    public void Name_ShouldReturnConfiguredName()
    {
        // Arrange
        LogArrange("Creating unit of work with specific name");
        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "TestUoW", _connectionMock.Object);

        // Act
        LogAct("Getting name");
        string name = unitOfWork.Name;

        // Assert
        LogAssert("Verifying name is set correctly");
        name.ShouldBe("TestUoW");
    }

    [Fact]
    public void GetCurrentTransaction_WhenNoTransaction_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating unit of work without transaction");
        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Getting current transaction");
        NpgsqlTransaction? transaction = unitOfWork.GetCurrentTransaction();

        // Assert
        LogAssert("Verifying transaction is null");
        transaction.ShouldBeNull();
    }

    [Fact]
    public void GetCurrentConnection_ShouldDelegateToConnection()
    {
        // Arrange
        LogArrange("Setting up connection mock");
        _connectionMock.Setup(c => c.GetConnectionObject()).Returns((NpgsqlConnection?)null);

        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Getting current connection");
        NpgsqlConnection? connection = unitOfWork.GetCurrentConnection();

        // Assert
        LogAssert("Verifying connection is returned from mock");
        connection.ShouldBeNull();
        _connectionMock.Verify(c => c.GetConnectionObject(), Times.Once);
    }

    [Fact]
    public void CreateNpgsqlCommand_ShouldCreateCommandWithText()
    {
        // Arrange
        LogArrange("Setting up unit of work");
        _connectionMock.Setup(c => c.GetConnectionObject()).Returns((NpgsqlConnection?)null);

        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Creating NpgsqlCommand");
        using NpgsqlCommand command = unitOfWork.CreateNpgsqlCommand("SELECT 1");

        // Assert
        LogAssert("Verifying command text is set");
        command.CommandText.ShouldBe("SELECT 1");
    }

    [Fact]
    public async Task OpenConnectionAsync_ShouldDelegateToConnection()
    {
        // Arrange
        LogArrange("Setting up connection mock");
        _connectionMock
            .Setup(c => c.TryOpenConnectionAsync(It.IsAny<ExecutionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Opening connection");
        bool result = await unitOfWork.OpenConnectionAsync(_executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying connection open was called");
        result.ShouldBeTrue();
        _connectionMock.Verify(c => c.TryOpenConnectionAsync(_executionContext, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CloseConnectionAsync_ShouldDelegateToConnection()
    {
        // Arrange
        LogArrange("Setting up connection mock");
        _connectionMock
            .Setup(c => c.TryCloseConnectionAsync(It.IsAny<ExecutionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Closing connection");
        bool result = await unitOfWork.CloseConnectionAsync(_executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying connection close was called");
        result.ShouldBeTrue();
        _connectionMock.Verify(c => c.TryCloseConnectionAsync(_executionContext, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CommitAsync_WhenNoTransaction_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating unit of work without transaction");
        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Committing without transaction");
        bool result = await unitOfWork.CommitAsync(_executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying commit returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task RollbackAsync_WhenNoTransaction_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating unit of work without transaction");
        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Rolling back without transaction");
        bool result = await unitOfWork.RollbackAsync(_executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying rollback returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void Dispose_ShouldDisposeConnectionAndTransaction()
    {
        // Arrange
        LogArrange("Creating unit of work");
        TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Disposing unit of work");
        unitOfWork.Dispose();

        // Assert
        LogAssert("Verifying connection is disposed");
        _connectionMock.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Creating unit of work");
        TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act & Assert
        LogAct("Disposing unit of work multiple times");
        Should.NotThrow(() =>
        {
            unitOfWork.Dispose();
            unitOfWork.Dispose();
        });
    }

    [Fact]
    public void Logger_ShouldBeAccessibleInDerivedClass()
    {
        // Arrange
        LogArrange("Creating unit of work");
        using TestableUnitOfWork unitOfWork = new(_loggerMock.Object, "Test", _connectionMock.Object);

        // Act
        LogAct("Getting logger from derived class");
        ILogger logger = unitOfWork.GetLogger();

        // Assert
        LogAssert("Verifying logger is accessible");
        logger.ShouldBeSameAs(_loggerMock.Object);
    }
}

/// <summary>
/// Testable implementation of PostgreSqlUnitOfWorkBase
/// </summary>
internal sealed class TestableUnitOfWork : PostgreSqlUnitOfWorkBase
{
    public TestableUnitOfWork(ILogger logger, string name, IPostgreSqlConnection connection)
        : base(logger, name, connection)
    {
    }

    /// <summary>
    /// Exposes the protected Logger property for testing
    /// </summary>
    public ILogger GetLogger() => Logger;
}
