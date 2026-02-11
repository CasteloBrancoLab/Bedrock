using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Configuration;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Persistence.Connections;

public class AuthPostgreSqlConnectionTests : TestBase
{
    public AuthPostgreSqlConnectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock IConfiguration");
        var configurationMock = new Mock<IConfiguration>();

        // Act
        LogAct("Instantiating AuthPostgreSqlConnection");
        var connection = new ShopDemo.Auth.Infra.Persistence.Connections.AuthPostgreSqlConnection(
            configurationMock.Object);

        // Assert
        LogAssert("Verifying instance was created successfully");
        connection.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInheritFromPostgreSqlConnectionBase()
    {
        // Arrange
        LogArrange("Creating mock IConfiguration");
        var configurationMock = new Mock<IConfiguration>();

        // Act
        LogAct("Instantiating AuthPostgreSqlConnection");
        var connection = new ShopDemo.Auth.Infra.Persistence.Connections.AuthPostgreSqlConnection(
            configurationMock.Object);

        // Assert
        LogAssert("Verifying inheritance from PostgreSqlConnectionBase");
        connection.ShouldBeAssignableTo<PostgreSqlConnectionBase>();
    }

    [Fact]
    public void Constructor_ConnectionShouldNotBeOpenInitially()
    {
        // Arrange
        LogArrange("Creating mock IConfiguration");
        var configurationMock = new Mock<IConfiguration>();

        // Act
        LogAct("Instantiating AuthPostgreSqlConnection and checking state");
        var connection = new ShopDemo.Auth.Infra.Persistence.Connections.AuthPostgreSqlConnection(
            configurationMock.Object);

        // Assert
        LogAssert("Verifying connection is not open initially");
        connection.IsOpen().ShouldBeFalse();
    }

    [Fact]
    public void Constructor_ShouldImplementIAuthPostgreSqlConnection()
    {
        // Arrange
        LogArrange("Creating mock IConfiguration");
        var configurationMock = new Mock<IConfiguration>();

        // Act
        LogAct("Instantiating AuthPostgreSqlConnection");
        var connection = new ShopDemo.Auth.Infra.Persistence.Connections.AuthPostgreSqlConnection(
            configurationMock.Object);

        // Assert
        LogAssert("Verifying implementation of IAuthPostgreSqlConnection");
        connection.ShouldBeAssignableTo<ShopDemo.Auth.Infra.Persistence.Connections.Interfaces.IAuthPostgreSqlConnection>();
    }
}
