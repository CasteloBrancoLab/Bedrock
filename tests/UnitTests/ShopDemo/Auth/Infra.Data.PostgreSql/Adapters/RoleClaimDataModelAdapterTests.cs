using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Domain.Entities.RoleClaims.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class RoleClaimDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public RoleClaimDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateRoleIdFromEntity()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModel and RoleClaim with different RoleIds");
        var dataModel = CreateTestDataModel();
        var expectedRoleId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedRoleId, Guid.NewGuid(), (short)1);

        // Act
        LogAct("Adapting data model from entity");
        RoleClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying RoleId was updated");
        dataModel.RoleId.ShouldBe(expectedRoleId);
    }

    [Fact]
    public void Adapt_ShouldUpdateClaimIdFromEntity()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModel and RoleClaim with different ClaimIds");
        var dataModel = CreateTestDataModel();
        var expectedClaimId = Guid.NewGuid();
        var entity = CreateTestEntity(Guid.NewGuid(), expectedClaimId, (short)1);

        // Act
        LogAct("Adapting data model from entity");
        RoleClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ClaimId was updated");
        dataModel.ClaimId.ShouldBe(expectedClaimId);
    }

    [Theory]
    [InlineData((short)1)]
    [InlineData((short)-1)]
    [InlineData((short)0)]
    public void Adapt_ShouldUpdateValueFromEntity(short expectedValue)
    {
        // Arrange
        LogArrange($"Creating RoleClaimDataModel and RoleClaim with Value={expectedValue}");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), expectedValue);

        // Act
        LogAct("Adapting data model from entity");
        RoleClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert($"Verifying Value was updated to {expectedValue}");
        dataModel.Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModel and RoleClaim with different EntityInfo values");
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

        var entity = RoleClaim.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleClaimInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                ClaimValue.CreateFromExistingInfo(1)));

        // Act
        LogAct("Adapting data model from entity");
        RoleClaimDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating RoleClaimDataModel and RoleClaim");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), (short)1);

        // Act
        LogAct("Adapting data model from entity");
        var result = RoleClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static RoleClaimDataModel CreateTestDataModel()
    {
        return new RoleClaimDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            RoleId = Guid.NewGuid(),
            ClaimId = Guid.NewGuid(),
            Value = 0
        };
    }

    private static RoleClaim CreateTestEntity(
        Guid roleId,
        Guid claimId,
        short value)
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

        return RoleClaim.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleClaimInput(
                entityInfo,
                Id.CreateFromExistingInfo(roleId),
                Id.CreateFromExistingInfo(claimId),
                ClaimValue.CreateFromExistingInfo(value)));
    }

    #endregion
}
