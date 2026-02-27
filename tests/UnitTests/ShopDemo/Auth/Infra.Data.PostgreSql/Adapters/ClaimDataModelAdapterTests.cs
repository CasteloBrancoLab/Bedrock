using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.Claims.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ClaimDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ClaimDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateNameFromEntity()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel and Claim with different Names");
        var dataModel = CreateTestDataModel();
        string expectedName = Faker.Random.Word();
        var entity = CreateTestEntity(expectedName, "A description");

        // Act
        LogAct("Adapting data model from entity");
        ClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Name was updated");
        dataModel.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void Adapt_ShouldUpdateDescriptionFromEntity()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel and Claim with different Descriptions");
        var dataModel = CreateTestDataModel();
        string expectedDescription = Faker.Lorem.Sentence();
        var entity = CreateTestEntity("claim-name", expectedDescription);

        // Act
        LogAct("Adapting data model from entity");
        ClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Description was updated");
        dataModel.Description.ShouldBe(expectedDescription);
    }

    [Fact]
    public void Adapt_ShouldUpdateNullDescriptionFromEntity()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel and Claim with null Description");
        var dataModel = CreateTestDataModel();
        dataModel.Description = "old description";
        var entity = CreateTestEntity("claim-name", null);

        // Act
        LogAct("Adapting data model from entity");
        ClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Description was updated to null");
        dataModel.Description.ShouldBeNull();
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel and Claim with different EntityInfo values");
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

        var entity = Claim.CreateFromExistingInfo(
            new CreateFromExistingInfoClaimInput(entityInfo, "claim-name", null));

        // Act
        LogAct("Adapting data model from entity");
        ClaimDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ClaimDataModel and Claim");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity("claim-name", null);

        // Act
        LogAct("Adapting data model from entity");
        var result = ClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ClaimDataModel CreateTestDataModel()
    {
        return new ClaimDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            Name = "initial-name",
            Description = "initial-description"
        };
    }

    private static Claim CreateTestEntity(string name, string? description)
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

        return Claim.CreateFromExistingInfo(
            new CreateFromExistingInfoClaimInput(entityInfo, name, description));
    }

    #endregion
}
