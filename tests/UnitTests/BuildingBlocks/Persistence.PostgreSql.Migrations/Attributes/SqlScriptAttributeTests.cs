using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;

public class SqlScriptAttributeTests : TestBase
{
    public SqlScriptAttributeTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Constructor_WithUpScriptOnly_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        LogArrange("Defining UP script resource name");
        const string upScript = "Up/V202602141200__create_users_table.sql";

        // Act
        LogAct("Creating SqlScriptAttribute with UP script only");
        var attribute = new SqlScriptAttribute(upScript);

        // Assert
        LogAssert("Verifying properties are set correctly");
        attribute.UpScriptResourceName.ShouldBe(upScript);
        attribute.DownScriptResourceName.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithUpAndDownScripts_ShouldSetBothProperties()
    {
        // Arrange
        LogArrange("Defining UP and DOWN script resource names");
        const string upScript = "Up/V202602141200__create_users_table.sql";
        const string downScript = "Down/V202602141200__create_users_table.sql";

        // Act
        LogAct("Creating SqlScriptAttribute with both scripts");
        var attribute = new SqlScriptAttribute(upScript, downScript);

        // Assert
        LogAssert("Verifying both properties are set");
        attribute.UpScriptResourceName.ShouldBe(upScript);
        attribute.DownScriptResourceName.ShouldBe(downScript);
    }

    [Fact]
    public void Constructor_WithNullUpScript_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing null UP script");

        // Act
        LogAct("Creating SqlScriptAttribute with null UP script");
        var action = () => new SqlScriptAttribute(null!);

        // Assert
        LogAssert("Verifying ArgumentException is thrown");
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyUpScript_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing empty UP script");

        // Act
        LogAct("Creating SqlScriptAttribute with empty UP script");
        var action = () => new SqlScriptAttribute("");

        // Assert
        LogAssert("Verifying ArgumentException is thrown");
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithWhitespaceUpScript_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing whitespace-only UP script");

        // Act
        LogAct("Creating SqlScriptAttribute with whitespace UP script");
        var action = () => new SqlScriptAttribute("   ");

        // Assert
        LogAssert("Verifying ArgumentException is thrown");
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNullDownScript_ShouldSetDownScriptToNull()
    {
        // Arrange
        LogArrange("Defining UP script with explicit null DOWN script");
        const string upScript = "Up/V202602141200__create_users_table.sql";

        // Act
        LogAct("Creating SqlScriptAttribute with explicit null DOWN script");
        var attribute = new SqlScriptAttribute(upScript, null);

        // Assert
        LogAssert("Verifying DOWN script is null");
        attribute.DownScriptResourceName.ShouldBeNull();
    }

    [Fact]
    public void AttributeUsage_ShouldTargetClassOnly()
    {
        // Arrange
        LogArrange("Getting AttributeUsage from SqlScriptAttribute type");

        // Act
        LogAct("Reading AttributeUsage attribute");
        var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
            typeof(SqlScriptAttribute), typeof(AttributeUsageAttribute))!;

        // Assert
        LogAssert("Verifying AttributeUsage targets Class only and is not inherited");
        usage.ValidOn.ShouldBe(AttributeTargets.Class);
        usage.Inherited.ShouldBeFalse();
        usage.AllowMultiple.ShouldBeFalse();
    }
}
