using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Infra.Persistence.Connections.Interfaces;
using ShopDemo.Auth.Infra.Persistence.UnitOfWork;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Persistence.UnitOfWork;

public class AuthPostgreSqlUnitOfWorkTests : TestBase
{
    public AuthPostgreSqlUnitOfWorkTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock logger and connection");
        var loggerMock = new Mock<ILogger<AuthPostgreSqlUnitOfWork>>();
        var connectionMock = new Mock<IAuthPostgreSqlConnection>();

        // Act
        LogAct("Instantiating AuthPostgreSqlUnitOfWork");
        var unitOfWork = new AuthPostgreSqlUnitOfWork(
            loggerMock.Object,
            connectionMock.Object);

        // Assert
        LogAssert("Verifying instance was created successfully");
        unitOfWork.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInheritFromPostgreSqlUnitOfWorkBase()
    {
        // Arrange
        LogArrange("Creating mock dependencies");
        var loggerMock = new Mock<ILogger<AuthPostgreSqlUnitOfWork>>();
        var connectionMock = new Mock<IAuthPostgreSqlConnection>();

        // Act
        LogAct("Instantiating AuthPostgreSqlUnitOfWork");
        var unitOfWork = new AuthPostgreSqlUnitOfWork(
            loggerMock.Object,
            connectionMock.Object);

        // Assert
        LogAssert("Verifying inheritance from PostgreSqlUnitOfWorkBase");
        unitOfWork.ShouldBeAssignableTo<PostgreSqlUnitOfWorkBase>();
    }

    [Fact]
    public void Constructor_ShouldSetNameProperty()
    {
        // Arrange
        LogArrange("Creating mock dependencies");
        var loggerMock = new Mock<ILogger<AuthPostgreSqlUnitOfWork>>();
        var connectionMock = new Mock<IAuthPostgreSqlConnection>();

        // Act
        LogAct("Instantiating AuthPostgreSqlUnitOfWork");
        var unitOfWork = new AuthPostgreSqlUnitOfWork(
            loggerMock.Object,
            connectionMock.Object);

        // Assert
        LogAssert("Verifying Name property is set");
        unitOfWork.Name.ShouldBe("AuthPostgreSqlUnitOfWork");
    }

    [Fact]
    public void GetCurrentTransaction_ShouldReturnNullInitially()
    {
        // Arrange
        LogArrange("Creating AuthPostgreSqlUnitOfWork");
        var loggerMock = new Mock<ILogger<AuthPostgreSqlUnitOfWork>>();
        var connectionMock = new Mock<IAuthPostgreSqlConnection>();
        var unitOfWork = new AuthPostgreSqlUnitOfWork(
            loggerMock.Object,
            connectionMock.Object);

        // Act
        LogAct("Getting current transaction");
        var transaction = unitOfWork.GetCurrentTransaction();

        // Assert
        LogAssert("Verifying no transaction exists initially");
        transaction.ShouldBeNull();
    }
}
