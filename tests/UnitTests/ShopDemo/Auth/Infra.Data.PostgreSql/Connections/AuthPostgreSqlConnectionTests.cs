using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Infra.CrossCutting.Configuration;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Connections;

public class AuthPostgreSqlConnectionTests : TestBase
{
    public AuthPostgreSqlConnectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    private static AuthConfigurationManager CreateConfigurationManager()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var logger = new Mock<ILogger<AuthConfigurationManager>>().Object;
        return new AuthConfigurationManager(configuration, logger);
    }

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating AuthConfigurationManager");
        AuthConfigurationManager configurationManager = CreateConfigurationManager();

        // Act
        LogAct("Instantiating AuthPostgreSqlConnection");
        var connection = new ShopDemo.Auth.Infra.Data.PostgreSql.Connections.AuthPostgreSqlConnection(
            configurationManager);

        // Assert
        LogAssert("Verifying instance was created successfully");
        connection.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInheritFromPostgreSqlConnectionBase()
    {
        // Arrange
        LogArrange("Creating AuthConfigurationManager");
        AuthConfigurationManager configurationManager = CreateConfigurationManager();

        // Act
        LogAct("Instantiating AuthPostgreSqlConnection");
        var connection = new ShopDemo.Auth.Infra.Data.PostgreSql.Connections.AuthPostgreSqlConnection(
            configurationManager);

        // Assert
        LogAssert("Verifying inheritance from PostgreSqlConnectionBase");
        connection.ShouldBeAssignableTo<PostgreSqlConnectionBase>();
    }

    [Fact]
    public void Constructor_ConnectionShouldNotBeOpenInitially()
    {
        // Arrange
        LogArrange("Creating AuthConfigurationManager");
        AuthConfigurationManager configurationManager = CreateConfigurationManager();

        // Act
        LogAct("Instantiating AuthPostgreSqlConnection and checking state");
        var connection = new ShopDemo.Auth.Infra.Data.PostgreSql.Connections.AuthPostgreSqlConnection(
            configurationManager);

        // Assert
        LogAssert("Verifying connection is not open initially");
        connection.IsOpen().ShouldBeFalse();
    }

    [Fact]
    public void Constructor_ShouldImplementIAuthPostgreSqlConnection()
    {
        // Arrange
        LogArrange("Creating AuthConfigurationManager");
        AuthConfigurationManager configurationManager = CreateConfigurationManager();

        // Act
        LogAct("Instantiating AuthPostgreSqlConnection");
        var connection = new ShopDemo.Auth.Infra.Data.PostgreSql.Connections.AuthPostgreSqlConnection(
            configurationManager);

        // Assert
        LogAssert("Verifying implementation of IAuthPostgreSqlConnection");
        connection.ShouldBeAssignableTo<ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces.IAuthPostgreSqlConnection>();
    }
}
