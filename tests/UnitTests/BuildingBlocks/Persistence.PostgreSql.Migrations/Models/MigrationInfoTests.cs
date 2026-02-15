using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.Models;

public class MigrationInfoTests : TestBase
{
    public MigrationInfoTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_WithValidParameters_ShouldSetProperties()
    {
        // Arrange
        LogArrange("Defining migration info parameters");
        const long version = 202602141200;
        const string description = "create_users_table";
        var appliedOn = new DateTimeOffset(2026, 2, 14, 12, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Creating MigrationInfo");
        var info = MigrationInfo.Create(version, description, appliedOn);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        info.Version.ShouldBe(version);
        info.Description.ShouldBe(description);
        info.AppliedOn.ShouldBe(appliedOn);
    }

    [Fact]
    public void Create_WithNullAppliedOn_ShouldDefaultToNull()
    {
        // Arrange
        LogArrange("Defining migration info without applied date");
        const long version = 202602141200;
        const string description = "create_users_table";

        // Act
        LogAct("Creating MigrationInfo without appliedOn");
        var info = MigrationInfo.Create(version, description);

        // Assert
        LogAssert("Verifying AppliedOn is null");
        info.AppliedOn.ShouldBeNull();
    }

    [Fact]
    public void Create_WithZeroVersion_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Preparing zero version");

        // Act
        LogAct("Creating MigrationInfo with version 0");
        Action action = () => MigrationInfo.Create(0, "description");

        // Assert
        LogAssert("Verifying ArgumentOutOfRangeException is thrown");
        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNegativeVersion_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Preparing negative version");

        // Act
        LogAct("Creating MigrationInfo with negative version");
        Action action = () => MigrationInfo.Create(-1, "description");

        // Assert
        LogAssert("Verifying ArgumentOutOfRangeException is thrown");
        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNullDescription_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing null description");

        // Act
        LogAct("Creating MigrationInfo with null description");
        Action action = () => MigrationInfo.Create(1, null!);

        // Assert
        LogAssert("Verifying ArgumentException is thrown");
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing empty description");

        // Act
        LogAct("Creating MigrationInfo with empty description");
        Action action = () => MigrationInfo.Create(1, "");

        // Assert
        LogAssert("Verifying ArgumentException is thrown");
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceDescription_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing whitespace-only description");

        // Act
        LogAct("Creating MigrationInfo with whitespace description");
        Action action = () => MigrationInfo.Create(1, "   ");

        // Assert
        LogAssert("Verifying ArgumentException is thrown");
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        LogArrange("Creating two MigrationInfo with same values");
        var appliedOn = new DateTimeOffset(2026, 2, 14, 12, 0, 0, TimeSpan.Zero);
        var info1 = MigrationInfo.Create(202602141200, "create_users_table", appliedOn);
        var info2 = MigrationInfo.Create(202602141200, "create_users_table", appliedOn);

        // Act
        LogAct("Comparing MigrationInfo instances for equality");
        var areEqual = info1.Equals(info2);

        // Assert
        LogAssert("Verifying they are equal");
        areEqual.ShouldBeTrue();
        (info1 == info2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_WithDifferentVersions_ShouldNotBeEqual()
    {
        // Arrange
        LogArrange("Creating two MigrationInfo with different versions");
        var info1 = MigrationInfo.Create(202602141200, "create_users_table");
        var info2 = MigrationInfo.Create(202602141300, "create_users_table");

        // Act
        LogAct("Comparing MigrationInfo instances");
        var areEqual = info1.Equals(info2);

        // Assert
        LogAssert("Verifying they are not equal");
        areEqual.ShouldBeFalse();
        (info1 != info2).ShouldBeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldBeConsistent()
    {
        // Arrange
        LogArrange("Creating two MigrationInfo with same values");
        var appliedOn = new DateTimeOffset(2026, 2, 14, 12, 0, 0, TimeSpan.Zero);
        var info1 = MigrationInfo.Create(202602141200, "create_users_table", appliedOn);
        var info2 = MigrationInfo.Create(202602141200, "create_users_table", appliedOn);

        // Act
        LogAct("Getting hash codes");
        var hash1 = info1.GetHashCode();
        var hash2 = info2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal");
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void DefaultValue_ShouldHaveDefaultProperties()
    {
        // Arrange
        LogArrange("Creating default MigrationInfo");

        // Act
        LogAct("Using default keyword");
        var info = default(MigrationInfo);

        // Assert
        LogAssert("Verifying default values");
        info.Version.ShouldBe(0);
        info.Description.ShouldBeNull();
        info.AppliedOn.ShouldBeNull();
    }
}
