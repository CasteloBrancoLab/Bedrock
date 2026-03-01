using Bedrock.BuildingBlocks.Outbox.PostgreSql;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Outbox.PostgreSql;

public class OutboxPostgreSqlOptionsTests : TestBase
{
    public OutboxPostgreSqlOptionsTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void DefaultValues_ShouldHaveExpectedDefaults()
    {
        // Arrange
        LogArrange("Creating OutboxPostgreSqlOptions with defaults");

        // Act
        LogAct("Constructing with default values");
        var options = new OutboxPostgreSqlOptions();

        // Assert
        LogAssert("Verifying default values");
        options.Schema.ShouldBe("public");
        options.TableName.ShouldBe("outbox");
        options.MaxRetries.ShouldBe((byte)5);
    }

    [Fact]
    public void WithSchema_ShouldSetSchema()
    {
        // Arrange
        LogArrange("Creating options");
        var options = new OutboxPostgreSqlOptions();

        // Act
        LogAct("Setting schema via fluent API");
        var result = options.WithSchema("auth");

        // Assert
        LogAssert("Verifying schema and fluent return");
        options.Schema.ShouldBe("auth");
        result.ShouldBeSameAs(options);
    }

    [Fact]
    public void WithTableName_ShouldSetTableName()
    {
        // Arrange
        LogArrange("Creating options");
        var options = new OutboxPostgreSqlOptions();

        // Act
        LogAct("Setting table name via fluent API");
        var result = options.WithTableName("auth_outbox");

        // Assert
        LogAssert("Verifying table name and fluent return");
        options.TableName.ShouldBe("auth_outbox");
        result.ShouldBeSameAs(options);
    }

    [Fact]
    public void WithMaxRetries_ShouldSetMaxRetries()
    {
        // Arrange
        LogArrange("Creating options");
        var options = new OutboxPostgreSqlOptions();

        // Act
        LogAct("Setting max retries via fluent API");
        var result = options.WithMaxRetries(10);

        // Assert
        LogAssert("Verifying max retries and fluent return");
        options.MaxRetries.ShouldBe((byte)10);
        result.ShouldBeSameAs(options);
    }

    [Fact]
    public void FluentChaining_ShouldSetAllValues()
    {
        // Arrange
        LogArrange("Creating options with fluent chaining");

        // Act
        LogAct("Chaining all With* methods");
        var options = new OutboxPostgreSqlOptions()
            .WithSchema("payments")
            .WithTableName("payment_outbox")
            .WithMaxRetries(3);

        // Assert
        LogAssert("Verifying all values set via chaining");
        options.Schema.ShouldBe("payments");
        options.TableName.ShouldBe("payment_outbox");
        options.MaxRetries.ShouldBe((byte)3);
    }
}
