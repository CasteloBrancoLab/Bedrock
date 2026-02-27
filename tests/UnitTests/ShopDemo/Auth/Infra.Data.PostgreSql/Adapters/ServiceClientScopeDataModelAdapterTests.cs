using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ServiceClientScopeDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientScopeDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateServiceClientIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModel and ServiceClientScope with different serviceClientIds");
        var dataModel = CreateTestDataModel();
        var expectedServiceClientId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedServiceClientId, "openid");

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientScopeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ServiceClientId was updated");
        dataModel.ServiceClientId.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Adapt_ShouldUpdateScopeFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModel and ServiceClientScope with different scopes");
        var dataModel = CreateTestDataModel();
        string expectedScope = "email";
        var entity = CreateTestEntity(Guid.NewGuid(), expectedScope);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientScopeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Scope was updated");
        dataModel.Scope.ShouldBe(expectedScope);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModel and ServiceClientScope with different EntityInfo values");
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

        var entity = ServiceClientScope.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientScopeInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "openid"));

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientScopeDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ServiceClientScopeDataModel and ServiceClientScope");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "openid");

        // Act
        LogAct("Adapting data model from entity");
        var result = ServiceClientScopeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ServiceClientScopeDataModel CreateTestDataModel()
    {
        return new ServiceClientScopeDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            ServiceClientId = Guid.NewGuid(),
            Scope = "initial-scope"
        };
    }

    private static ServiceClientScope CreateTestEntity(Guid serviceClientId, string scope)
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

        return ServiceClientScope.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientScopeInput(
                entityInfo,
                Id.CreateFromExistingInfo(serviceClientId),
                scope));
    }

    #endregion
}
