using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ClaimDependencyDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ClaimDependencyDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateClaimIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModel and ClaimDependency with different ClaimIds");
        var dataModel = CreateTestDataModel();
        var expectedClaimId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedClaimId, Guid.NewGuid());

        // Act
        LogAct("Adapting data model from entity");
        ClaimDependencyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ClaimId was updated");
        dataModel.ClaimId.ShouldBe(expectedClaimId);
    }

    [Fact]
    public void Adapt_ShouldUpdateDependsOnClaimIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModel and ClaimDependency with different DependsOnClaimIds");
        var dataModel = CreateTestDataModel();
        var expectedDependsOnClaimId = Guid.NewGuid();
        var entity = CreateTestEntity(Guid.NewGuid(), expectedDependsOnClaimId);

        // Act
        LogAct("Adapting data model from entity");
        ClaimDependencyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying DependsOnClaimId was updated");
        dataModel.DependsOnClaimId.ShouldBe(expectedDependsOnClaimId);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModel and ClaimDependency with different EntityInfo values");
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

        var entity = ClaimDependency.CreateFromExistingInfo(
            new CreateFromExistingInfoClaimDependencyInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid())));

        // Act
        LogAct("Adapting data model from entity");
        ClaimDependencyDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ClaimDependencyDataModel and ClaimDependency");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid());

        // Act
        LogAct("Adapting data model from entity");
        var result = ClaimDependencyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ClaimDependencyDataModel CreateTestDataModel()
    {
        return new ClaimDependencyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            ClaimId = Guid.NewGuid(),
            DependsOnClaimId = Guid.NewGuid()
        };
    }

    private static ClaimDependency CreateTestEntity(Guid claimId, Guid dependsOnClaimId)
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

        return ClaimDependency.CreateFromExistingInfo(
            new CreateFromExistingInfoClaimDependencyInput(
                entityInfo,
                Id.CreateFromExistingInfo(claimId),
                Id.CreateFromExistingInfo(dependsOnClaimId)));
    }

    #endregion
}
