using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ServiceClientClaimDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientClaimDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateServiceClientIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel and ServiceClientClaim with different serviceClientIds");
        var dataModel = CreateTestDataModel();
        var expectedServiceClientId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedServiceClientId, Guid.NewGuid(), (short)1);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ServiceClientId was updated");
        dataModel.ServiceClientId.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Adapt_ShouldUpdateClaimIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel and ServiceClientClaim with different claimIds");
        var dataModel = CreateTestDataModel();
        var expectedClaimId = Guid.NewGuid();
        var entity = CreateTestEntity(Guid.NewGuid(), expectedClaimId, (short)1);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ClaimId was updated");
        dataModel.ClaimId.ShouldBe(expectedClaimId);
    }

    [Fact]
    public void Adapt_ShouldUpdateValueFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel and ServiceClientClaim with different values");
        var dataModel = CreateTestDataModel();
        dataModel.Value = (short)1;
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), (short)-1);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Value was updated");
        dataModel.Value.ShouldBe((short)-1);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel and ServiceClientClaim with different EntityInfo values");
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

        var entity = ServiceClientClaim.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientClaimInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                ClaimValue.CreateFromExistingInfo(1)));

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientClaimDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ServiceClientClaimDataModel and ServiceClientClaim");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), (short)1);

        // Act
        LogAct("Adapting data model from entity");
        var result = ServiceClientClaimDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ServiceClientClaimDataModel CreateTestDataModel()
    {
        return new ServiceClientClaimDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            ServiceClientId = Guid.NewGuid(),
            ClaimId = Guid.NewGuid(),
            Value = (short)1
        };
    }

    private static ServiceClientClaim CreateTestEntity(
        Guid serviceClientId,
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

        return ServiceClientClaim.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientClaimInput(
                entityInfo,
                Id.CreateFromExistingInfo(serviceClientId),
                Id.CreateFromExistingInfo(claimId),
                ClaimValue.CreateFromExistingInfo(value)));
    }

    #endregion
}
