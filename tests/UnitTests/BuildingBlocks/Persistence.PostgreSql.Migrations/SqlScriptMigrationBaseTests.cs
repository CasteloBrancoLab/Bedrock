using Bedrock.BuildingBlocks.Testing;
using Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations;

public class SqlScriptMigrationBaseTests : TestBase
{
    public SqlScriptMigrationBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Constructor_WithValidAttributeAndExistingScripts_ShouldSucceed()
    {
        // Arrange
        LogArrange("Preparing to instantiate migration with valid attribute and scripts");

        // Act
        LogAct("Creating V202602140001_CreateTestTable instance");
        var migration = new V202602140001_CreateTestTable();

        // Assert
        LogAssert("Verifying migration was created successfully");
        migration.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithoutSqlScriptAttribute_ShouldThrowInvalidOperationException()
    {
        // Arrange
        LogArrange("Preparing to instantiate migration without SqlScript attribute");

        // Act
        LogAct("Creating MigrationWithoutAttribute instance");
        var action = () => new MigrationWithoutAttribute();

        // Assert
        LogAssert("Verifying InvalidOperationException is thrown");
        var exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("SqlScriptAttribute");
    }

    [Fact]
    public void Constructor_WithMissingEmbeddedScript_ShouldThrowInvalidOperationException()
    {
        // Arrange
        LogArrange("Preparing to instantiate migration referencing non-existent script");

        // Act
        LogAct("Creating MigrationWithMissingScript instance");
        var action = () => new MigrationWithMissingScript();

        // Assert
        LogAssert("Verifying InvalidOperationException is thrown for missing script");
        var exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("not found");
    }

    [Fact]
    public void Constructor_WithIrreversibleMigration_ShouldSucceed()
    {
        // Arrange
        LogArrange("Preparing to instantiate irreversible migration (no DOWN script)");

        // Act
        LogAct("Creating V202602140002_IrreversibleMigration instance");
        var migration = new V202602140002_IrreversibleMigration();

        // Assert
        LogAssert("Verifying migration was created successfully");
        migration.ShouldNotBeNull();
    }

    [Fact]
    public void Down_WithNullDownScript_ShouldThrowInvalidOperationException()
    {
        // Arrange
        LogArrange("Creating irreversible migration instance");
        var migration = new V202602140002_IrreversibleMigration();

        // Act
        LogAct("Calling Down() on irreversible migration");
        var action = () => migration.Down();

        // Assert
        LogAssert("Verifying InvalidOperationException is thrown for missing DOWN script");
        var exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("does not have a DOWN script");
    }
}
