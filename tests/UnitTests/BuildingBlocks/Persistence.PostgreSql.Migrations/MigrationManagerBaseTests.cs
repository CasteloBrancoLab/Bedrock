using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Bedrock.BuildingBlocks.Testing;
using Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using MessageType = Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums.MessageType;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations;

public class MigrationManagerBaseTests : TestBase
{
    private readonly Mock<ILogger> _loggerMock;

    public MigrationManagerBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger>();
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null logger");

        // Act
        LogAct("Creating TestMigrationManager with null logger");
        var action = () => new TestMigrationManager(null!, "Host=localhost");

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldSucceed()
    {
        // Arrange
        LogArrange("Preparing valid logger");

        // Act
        LogAct("Creating TestMigrationManager");
        var manager = new TestMigrationManager(_loggerMock.Object, "Host=localhost");

        // Assert
        LogAssert("Verifying manager was created");
        manager.ShouldNotBeNull();
    }

    [Fact]
    public async Task MigrateUpAsync_WithNullExecutionContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Creating manager with valid logger");
        var manager = new TestMigrationManager(_loggerMock.Object, "Host=localhost");

        // Act
        LogAct("Calling MigrateUpAsync with null execution context");
        var action = () => manager.MigrateUpAsync(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        await action.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void CreateServiceProvider_ShouldReturnValidProvider()
    {
        // Arrange
        LogArrange("Creating manager with valid configuration");
        var manager = new TestMigrationManager(
            _loggerMock.Object,
            "Host=localhost;Database=test",
            "public");

        // Act
        LogAct("Creating FluentMigrator service provider");
        using var serviceProvider = manager.CreateServiceProvider();

        // Assert
        LogAssert("Verifying service provider is valid");
        serviceProvider.ShouldNotBeNull();
    }

    [Fact]
    public async Task MigrateDownAsync_WithNullExecutionContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Creating manager with valid logger");
        var manager = new TestMigrationManager(_loggerMock.Object, "Host=localhost");

        // Act
        LogAct("Calling MigrateDownAsync with null execution context");
        var action = () => manager.MigrateDownAsync(null!, 0);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        await action.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetStatusAsync_WithNullExecutionContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Creating manager with valid logger");
        var manager = new TestMigrationManager(_loggerMock.Object, "Host=localhost");

        // Act
        LogAct("Calling GetStatusAsync with null execution context");
        var action = () => manager.GetStatusAsync(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        await action.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MigrateUpAsync_WithValidExecutionContext_ShouldDelegateToInternalMethod()
    {
        // Arrange
        LogArrange("Creating manager with valid logger and invalid connection (no PostgreSQL)");
        var manager = new TestMigrationManager(_loggerMock.Object, "Host=localhost;Port=1;Timeout=1");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling MigrateUpAsync with valid execution context (expect failure due to no DB)");
        var action = () => manager.MigrateUpAsync(executionContext);

        // Assert
        LogAssert("Verifying delegation occurred and exception propagated from infrastructure");
        await action.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task MigrateDownAsync_WithValidExecutionContext_ShouldDelegateToInternalMethod()
    {
        // Arrange
        LogArrange("Creating manager with valid logger and invalid connection (no PostgreSQL)");
        var manager = new TestMigrationManager(_loggerMock.Object, "Host=localhost;Port=1;Timeout=1");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling MigrateDownAsync with valid execution context (expect failure due to no DB)");
        var action = () => manager.MigrateDownAsync(executionContext, 0);

        // Assert
        LogAssert("Verifying delegation occurred and exception propagated from infrastructure");
        await action.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetStatusAsync_WithValidExecutionContext_ShouldDelegateToInternalMethod()
    {
        // Arrange
        LogArrange("Creating manager with valid logger and invalid connection (no PostgreSQL)");
        var manager = new TestMigrationManager(_loggerMock.Object, "Host=localhost;Port=1;Timeout=1");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling GetStatusAsync with valid execution context (expect failure due to no DB)");
        var action = () => manager.GetStatusAsync(executionContext);

        // Assert
        LogAssert("Verifying delegation occurred and exception propagated from infrastructure");
        await action.ShouldThrowAsync<Exception>();
    }

    private static ExecutionContext CreateTestExecutionContext()
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: Bedrock.BuildingBlocks.Core.TenantInfos.TenantInfo.Create(Guid.Empty),
            executionUser: "test-user",
            executionOrigin: "unit-test",
            businessOperationCode: "TEST_MIGRATION",
            minimumMessageType: MessageType.Information,
            timeProvider: TimeProvider.System);
    }
}
