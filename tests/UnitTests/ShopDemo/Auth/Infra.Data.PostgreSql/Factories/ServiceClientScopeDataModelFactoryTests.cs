using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ServiceClientScopeDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientScopeDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapServiceClientIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClientScope entity with known serviceClientId");
        var expectedServiceClientId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedServiceClientId, "openid");

        // Act
        LogAct("Creating ServiceClientScopeDataModel from ServiceClientScope entity");
        var dataModel = ServiceClientScopeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ServiceClientId mapping");
        dataModel.ServiceClientId.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Create_ShouldMapScopeCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClientScope entity with known scope");
        string expectedScope = "profile";
        var entity = CreateTestEntity(Guid.NewGuid(), expectedScope);

        // Act
        LogAct("Creating ServiceClientScopeDataModel from ServiceClientScope entity");
        var dataModel = ServiceClientScopeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Scope mapping");
        dataModel.Scope.ShouldBe(expectedScope);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ServiceClientScope entity with specific EntityInfo values");
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

        var entity = ServiceClientScope.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientScopeInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "openid"));

        // Act
        LogAct("Creating ServiceClientScopeDataModel from ServiceClientScope entity");
        var dataModel = ServiceClientScopeDataModelFactory.Create(entity);

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
