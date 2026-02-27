using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class RoleHierarchyFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public RoleHierarchyFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapRoleIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel with specific RoleId");
        var expectedRoleId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedRoleId, Guid.NewGuid());

        // Act
        LogAct("Creating RoleHierarchy from RoleHierarchyDataModel");
        var entity = RoleHierarchyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RoleId mapping");
        entity.RoleId.Value.ShouldBe(expectedRoleId);
    }

    [Fact]
    public void Create_ShouldMapParentRoleIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel with specific ParentRoleId");
        var expectedParentRoleId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedParentRoleId);

        // Act
        LogAct("Creating RoleHierarchy from RoleHierarchyDataModel");
        var entity = RoleHierarchyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ParentRoleId mapping");
        entity.ParentRoleId.Value.ShouldBe(expectedParentRoleId);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel with specific base fields");
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-5);
        long expectedVersion = Faker.Random.Long(1);
        string? expectedLastChangedBy = Faker.Person.FullName;
        var expectedLastChangedAt = DateTimeOffset.UtcNow;
        var expectedLastChangedCorrelationId = Guid.NewGuid();
        string expectedLastChangedExecutionOrigin = "TestOrigin";
        string expectedLastChangedBusinessOperationCode = "TEST_OP";

        var dataModel = new RoleHierarchyDataModel
        {
            Id = expectedId,
            TenantCode = expectedTenantCode,
            CreatedBy = expectedCreatedBy,
            CreatedAt = expectedCreatedAt,
            LastChangedBy = expectedLastChangedBy,
            LastChangedAt = expectedLastChangedAt,
            LastChangedCorrelationId = expectedLastChangedCorrelationId,
            LastChangedExecutionOrigin = expectedLastChangedExecutionOrigin,
            LastChangedBusinessOperationCode = expectedLastChangedBusinessOperationCode,
            EntityVersion = expectedVersion,
            RoleId = Guid.NewGuid(),
            ParentRoleId = Guid.NewGuid()
        };

        // Act
        LogAct("Creating RoleHierarchy from RoleHierarchyDataModel");
        var entity = RoleHierarchyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EntityInfo fields");
        entity.EntityInfo.Id.Value.ShouldBe(expectedId);
        entity.EntityInfo.TenantInfo.Code.ShouldBe(expectedTenantCode);
        entity.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe(expectedCreatedBy);
        entity.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(expectedCreatedAt);
        entity.EntityInfo.EntityVersion.Value.ShouldBe(expectedVersion);
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBe(expectedLastChangedBy);
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(expectedLastChangedAt);
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(expectedLastChangedCorrelationId);
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe(expectedLastChangedExecutionOrigin);
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe(expectedLastChangedBusinessOperationCode);
    }

    [Fact]
    public void Create_WithNullLastChangedFields_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel with null last-changed fields");
        var dataModel = new RoleHierarchyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            RoleId = Guid.NewGuid(),
            ParentRoleId = Guid.NewGuid()
        };

        // Act
        LogAct("Creating RoleHierarchy from RoleHierarchyDataModel with nulls");
        var entity = RoleHierarchyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying nullable fields are null");
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapCreatedCorrelationIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel to verify CreatedCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid());

        // Act
        LogAct("Creating RoleHierarchy from RoleHierarchyDataModel");
        var entity = RoleHierarchyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel to verify CreatedExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid());

        // Act
        LogAct("Creating RoleHierarchy from RoleHierarchyDataModel");
        var entity = RoleHierarchyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel to verify CreatedBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid());

        // Act
        LogAct("Creating RoleHierarchy from RoleHierarchyDataModel");
        var entity = RoleHierarchyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static RoleHierarchyDataModel CreateTestDataModel(
        Guid roleId,
        Guid parentRoleId)
    {
        return new RoleHierarchyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_ROLE_HIERARCHY",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            RoleId = roleId,
            ParentRoleId = parentRoleId
        };
    }

    #endregion
}
