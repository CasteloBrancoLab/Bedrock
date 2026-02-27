using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class RoleHierarchyDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public RoleHierarchyDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateRoleIdFromEntity()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel and RoleHierarchy with different RoleIds");
        var dataModel = CreateTestDataModel();
        var expectedRoleId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedRoleId, Guid.NewGuid());

        // Act
        LogAct("Adapting data model from entity");
        RoleHierarchyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying RoleId was updated");
        dataModel.RoleId.ShouldBe(expectedRoleId);
    }

    [Fact]
    public void Adapt_ShouldUpdateParentRoleIdFromEntity()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel and RoleHierarchy with different ParentRoleIds");
        var dataModel = CreateTestDataModel();
        var expectedParentRoleId = Guid.NewGuid();
        var entity = CreateTestEntity(Guid.NewGuid(), expectedParentRoleId);

        // Act
        LogAct("Adapting data model from entity");
        RoleHierarchyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ParentRoleId was updated");
        dataModel.ParentRoleId.ShouldBe(expectedParentRoleId);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel and RoleHierarchy with different EntityInfo values");
        var dataModel = CreateTestDataModel();
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-2);
        long expectedVersion = Faker.Random.Long(1);

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(expectedId),
            tenantInfo: TenantInfo.Create(expectedTenantCode),
            createdAt: expectedCreatedAt,
            createdBy: expectedCreatedBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(expectedVersion));

        var entity = RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid())));

        // Act
        LogAct("Adapting data model from entity");
        RoleHierarchyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying base fields were updated from EntityInfo");
        dataModel.Id.ShouldBe(expectedId);
        dataModel.TenantCode.ShouldBe(expectedTenantCode);
        dataModel.CreatedBy.ShouldBe(expectedCreatedBy);
        dataModel.CreatedAt.ShouldBe(expectedCreatedAt);
        dataModel.EntityVersion.ShouldBe(expectedVersion);
    }

    [Fact]
    public void Adapt_ShouldReturnTheSameDataModelInstance()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModel and RoleHierarchy");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid());

        // Act
        LogAct("Adapting data model from entity");
        var result = RoleHierarchyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static RoleHierarchyDataModel CreateTestDataModel()
    {
        return new RoleHierarchyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            RoleId = Guid.NewGuid(),
            ParentRoleId = Guid.NewGuid()
        };
    }

    private static RoleHierarchy CreateTestEntity(
        Guid roleId,
        Guid parentRoleId)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(
                entityInfo,
                Id.CreateFromExistingInfo(roleId),
                Id.CreateFromExistingInfo(parentRoleId)));
    }

    #endregion
}
