using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Connections.Models;

public class PostgreSqlConnectionOptionsTests : TestBase
{
    public PostgreSqlConnectionOptionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ConnectionString_ShouldBeNullByDefault()
    {
        // Arrange & Act
        LogArrange("Creating new PostgreSqlConnectionOptions");
        PostgreSqlConnectionOptions options = new();

        // Assert
        LogAssert("Verifying ConnectionString is null by default");
        options.ConnectionString.ShouldBeNull();
    }

    [Fact]
    public void WithConnectionString_ShouldSetConnectionString()
    {
        // Arrange
        LogArrange("Creating new PostgreSqlConnectionOptions");
        PostgreSqlConnectionOptions options = new();
        const string connectionString = "Host=localhost;Database=test;Username=user;Password=pass";

        // Act
        LogAct("Setting connection string");
        options.WithConnectionString(connectionString);

        // Assert
        LogAssert("Verifying ConnectionString is set");
        options.ConnectionString.ShouldBe(connectionString);
    }

    [Fact]
    public void WithConnectionString_ShouldReturnSameInstance()
    {
        // Arrange
        LogArrange("Creating new PostgreSqlConnectionOptions");
        PostgreSqlConnectionOptions options = new();

        // Act
        LogAct("Setting connection string and checking return value");
        PostgreSqlConnectionOptions result = options.WithConnectionString("test");

        // Assert
        LogAssert("Verifying same instance is returned for fluent API");
        result.ShouldBeSameAs(options);
    }

    [Fact]
    public void WithConnectionString_ShouldAllowOverwriting()
    {
        // Arrange
        LogArrange("Creating PostgreSqlConnectionOptions with initial connection string");
        PostgreSqlConnectionOptions options = new();
        options.WithConnectionString("initial");

        // Act
        LogAct("Overwriting connection string");
        options.WithConnectionString("updated");

        // Assert
        LogAssert("Verifying ConnectionString is updated");
        options.ConnectionString.ShouldBe("updated");
    }

    [Fact]
    public void WithConnectionString_ShouldAcceptEmptyString()
    {
        // Arrange
        LogArrange("Creating new PostgreSqlConnectionOptions");
        PostgreSqlConnectionOptions options = new();

        // Act
        LogAct("Setting empty connection string");
        options.WithConnectionString(string.Empty);

        // Assert
        LogAssert("Verifying empty string is accepted");
        options.ConnectionString.ShouldBe(string.Empty);
    }
}
