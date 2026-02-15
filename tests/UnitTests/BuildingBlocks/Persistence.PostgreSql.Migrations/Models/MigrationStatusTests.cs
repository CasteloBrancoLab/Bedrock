using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.Models;

public class MigrationStatusTests : TestBase
{
    public MigrationStatusTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_WithValidCollections_ShouldSetProperties()
    {
        // Arrange
        LogArrange("Defining applied and pending migration lists");
        var applied = new[]
        {
            MigrationInfo.Create(202602141200, "create_users_table", DateTimeOffset.UtcNow),
            MigrationInfo.Create(202602141300, "add_email_column", DateTimeOffset.UtcNow)
        };
        var pending = new[]
        {
            MigrationInfo.Create(202602141400, "create_roles_table")
        };

        // Act
        LogAct("Creating MigrationStatus");
        var status = MigrationStatus.Create(applied, pending);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        status.AppliedMigrations.Count.ShouldBe(2);
        status.PendingMigrations.Count.ShouldBe(1);
        status.LastAppliedVersion.ShouldBe(202602141300);
        status.HasPendingMigrations.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithEmptyAppliedList_ShouldHaveNullLastAppliedVersion()
    {
        // Arrange
        LogArrange("Defining empty applied list with pending migrations");
        IReadOnlyList<MigrationInfo> applied = Array.Empty<MigrationInfo>();
        var pending = new[]
        {
            MigrationInfo.Create(202602141200, "create_users_table")
        };

        // Act
        LogAct("Creating MigrationStatus with no applied migrations");
        var status = MigrationStatus.Create(applied, pending);

        // Assert
        LogAssert("Verifying LastAppliedVersion is null");
        status.LastAppliedVersion.ShouldBeNull();
        status.HasPendingMigrations.ShouldBeTrue();
        status.AppliedMigrations.Count.ShouldBe(0);
    }

    [Fact]
    public void Create_WithEmptyPendingList_ShouldHaveNoPendingMigrations()
    {
        // Arrange
        LogArrange("Defining applied migrations with empty pending list");
        var applied = new[]
        {
            MigrationInfo.Create(202602141200, "create_users_table", DateTimeOffset.UtcNow)
        };
        IReadOnlyList<MigrationInfo> pending = Array.Empty<MigrationInfo>();

        // Act
        LogAct("Creating MigrationStatus with no pending migrations");
        var status = MigrationStatus.Create(applied, pending);

        // Assert
        LogAssert("Verifying HasPendingMigrations is false");
        status.HasPendingMigrations.ShouldBeFalse();
        status.PendingMigrations.Count.ShouldBe(0);
    }

    [Fact]
    public void Create_WithBothEmptyLists_ShouldReturnEmptyStatus()
    {
        // Arrange
        LogArrange("Defining both empty lists");
        IReadOnlyList<MigrationInfo> applied = Array.Empty<MigrationInfo>();
        IReadOnlyList<MigrationInfo> pending = Array.Empty<MigrationInfo>();

        // Act
        LogAct("Creating MigrationStatus with no migrations at all");
        var status = MigrationStatus.Create(applied, pending);

        // Assert
        LogAssert("Verifying empty status");
        status.AppliedMigrations.Count.ShouldBe(0);
        status.PendingMigrations.Count.ShouldBe(0);
        status.LastAppliedVersion.ShouldBeNull();
        status.HasPendingMigrations.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithNullAppliedMigrations_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null applied migrations");

        // Act
        LogAct("Creating MigrationStatus with null applied migrations");
        var action = () => MigrationStatus.Create(null!, Array.Empty<MigrationInfo>());

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullPendingMigrations_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null pending migrations");

        // Act
        LogAct("Creating MigrationStatus with null pending migrations");
        var action = () => MigrationStatus.Create(Array.Empty<MigrationInfo>(), null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void LastAppliedVersion_ShouldReturnLastElementVersion()
    {
        // Arrange
        LogArrange("Defining applied migrations in ascending order");
        var applied = new[]
        {
            MigrationInfo.Create(202602141200, "first_migration", DateTimeOffset.UtcNow),
            MigrationInfo.Create(202602141300, "second_migration", DateTimeOffset.UtcNow),
            MigrationInfo.Create(202602141400, "third_migration", DateTimeOffset.UtcNow)
        };

        // Act
        LogAct("Creating MigrationStatus and checking last applied version");
        var status = MigrationStatus.Create(applied, Array.Empty<MigrationInfo>());

        // Assert
        LogAssert("Verifying last applied version is the highest");
        status.LastAppliedVersion.ShouldBe(202602141400);
    }

    [Fact]
    public void Collections_ShouldBeImmutable()
    {
        // Arrange
        LogArrange("Creating MigrationStatus with typed collections");
        var applied = new List<MigrationInfo>
        {
            MigrationInfo.Create(202602141200, "create_users_table", DateTimeOffset.UtcNow)
        };
        var pending = new List<MigrationInfo>
        {
            MigrationInfo.Create(202602141300, "add_email_column")
        };

        // Act
        LogAct("Creating MigrationStatus from mutable lists");
        var status = MigrationStatus.Create(applied, pending);

        // Assert
        LogAssert("Verifying collections are IReadOnlyList");
        status.AppliedMigrations.ShouldBeAssignableTo<IReadOnlyList<MigrationInfo>>();
        status.PendingMigrations.ShouldBeAssignableTo<IReadOnlyList<MigrationInfo>>();
    }
}
