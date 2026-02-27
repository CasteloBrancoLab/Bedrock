using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Roles;
using ShopDemo.Auth.Domain.Entities.Roles.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class RoleDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public RoleDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapNameCorrectly()
    {
        // Arrange
        LogArrange("Creating Role entity with known Name");
        string expectedName = Faker.Lorem.Word();
        var entity = CreateTestEntity(expectedName, "A description");

        // Act
        LogAct("Creating RoleDataModel from Role entity");
        var dataModel = RoleDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Name mapping");
        dataModel.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void Create_ShouldMapDescriptionCorrectly()
    {
        // Arrange
        LogArrange("Creating Role entity with known Description");
        string expectedDescription = Faker.Lorem.Sentence();
        var entity = CreateTestEntity("AdminRole", expectedDescription);

        // Act
        LogAct("Creating RoleDataModel from Role entity");
        var dataModel = RoleDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Description mapping");
        dataModel.Description.ShouldBe(expectedDescription);
    }

    [Fact]
    public void Create_ShouldMapNullDescriptionCorrectly()
    {
        // Arrange
        LogArrange("Creating Role entity with null Description");
        var entity = CreateTestEntity("AdminRole", null);

        // Act
        LogAct("Creating RoleDataModel from Role entity");
        var dataModel = RoleDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null Description mapping");
        dataModel.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating Role entity with specific EntityInfo values");
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        string createdBy = Faker.Person.FullName;
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        long entityVersion = Faker.Random.Long(1);
        string? lastChangedBy = Faker.Person.FullName;
        var lastChangedAt = DateTimeOffset.UtcNow;
        var lastChangedCorrelationId = Guid.NewGuid();
        string lastChangedExecutionOrigin = "TestOrigin";
        string lastChangedBusinessOperationCode = "TEST_OP";

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId),
            tenantInfo: TenantInfo.Create(tenantCode),
            createdAt: createdAt,
            createdBy: createdBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_CREATE",
            lastChangedAt: lastChangedAt,
            lastChangedBy: lastChangedBy,
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: lastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: lastChangedBusinessOperationCode,
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion));

        var entity = Role.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleInput(
                entityInfo,
                "AdminRole",
                "A description"));

        // Act
        LogAct("Creating RoleDataModel from Role entity");
        var dataModel = RoleDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying base fields from EntityInfo");
        dataModel.Id.ShouldBe(entityId);
        dataModel.TenantCode.ShouldBe(tenantCode);
        dataModel.CreatedBy.ShouldBe(createdBy);
        dataModel.CreatedAt.ShouldBe(createdAt);
        dataModel.EntityVersion.ShouldBe(entityVersion);
        dataModel.LastChangedBy.ShouldBe(lastChangedBy);
        dataModel.LastChangedAt.ShouldBe(lastChangedAt);
        dataModel.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        dataModel.LastChangedExecutionOrigin.ShouldBe(lastChangedExecutionOrigin);
        dataModel.LastChangedBusinessOperationCode.ShouldBe(lastChangedBusinessOperationCode);
    }

    #region Helper Methods

    private static Role CreateTestEntity(
        string name,
        string? description)
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

        return Role.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleInput(
                entityInfo,
                name,
                description));
    }

    #endregion
}
