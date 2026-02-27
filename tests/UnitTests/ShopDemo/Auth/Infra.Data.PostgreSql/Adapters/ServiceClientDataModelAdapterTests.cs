using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ServiceClientDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateClientIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel and ServiceClient with different clientIds");
        var dataModel = CreateTestDataModel();
        string expectedClientId = Faker.Random.AlphaNumeric(20);
        var entity = CreateTestEntity(expectedClientId, [10, 20, 30], "New Client", ServiceClientStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ClientId was updated");
        dataModel.ClientId.ShouldBe(expectedClientId);
    }

    [Fact]
    public void Adapt_ShouldUpdateClientSecretHashFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel and ServiceClient with different client secret hashes");
        var dataModel = CreateTestDataModel();
        byte[] expectedHash = [99, 88, 77, 66, 55];
        var entity = CreateTestEntity("client-id", expectedHash, "Test Client", ServiceClientStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ClientSecretHash was updated");
        dataModel.ClientSecretHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Adapt_ShouldUpdateNameFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel and ServiceClient with different names");
        var dataModel = CreateTestDataModel();
        string expectedName = Faker.Company.CompanyName();
        var entity = CreateTestEntity("client-id", [1, 2, 3], expectedName, ServiceClientStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Name was updated");
        dataModel.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel and ServiceClient with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)ServiceClientStatus.Active;
        var entity = CreateTestEntity("client-id", [1, 2, 3], "Test Client", ServiceClientStatus.Revoked);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)ServiceClientStatus.Revoked);
    }

    [Fact]
    public void Adapt_ShouldUpdateCreatedByUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel and ServiceClient with different createdByUserIds");
        var dataModel = CreateTestDataModel();
        var expectedCreatedByUserId = Guid.NewGuid();
        var entity = CreateTestEntityWithCreatedByUserId(expectedCreatedByUserId);

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying CreatedByUserId was updated");
        dataModel.CreatedByUserId.ShouldBe(expectedCreatedByUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel and ServiceClient with different EntityInfo values");
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

        var entity = ServiceClient.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientInput(
                entityInfo,
                "client-id",
                [1, 2, 3],
                "Test Client",
                ServiceClientStatus.Active,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                null,
                null));

        // Act
        LogAct("Adapting data model from entity");
        ServiceClientDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ServiceClientDataModel and ServiceClient");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity("client-id", [1, 2, 3], "Test Client", ServiceClientStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        var result = ServiceClientDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ServiceClientDataModel CreateTestDataModel()
    {
        return new ServiceClientDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            ClientId = "initial-client-id",
            ClientSecretHash = [1, 2, 3],
            Name = "Initial Client",
            Status = (short)ServiceClientStatus.Active,
            CreatedByUserId = Guid.NewGuid(),
            ExpiresAt = null,
            RevokedAt = null
        };
    }

    private static ServiceClient CreateTestEntity(
        string clientId,
        byte[] clientSecretHash,
        string name,
        ServiceClientStatus status)
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

        return ServiceClient.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientInput(
                entityInfo,
                clientId,
                clientSecretHash,
                name,
                status,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                null,
                null));
    }

    private static ServiceClient CreateTestEntityWithCreatedByUserId(Guid createdByUserId)
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

        return ServiceClient.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientInput(
                entityInfo,
                "client-id",
                [1, 2, 3],
                "Test Client",
                ServiceClientStatus.Active,
                Id.CreateFromExistingInfo(createdByUserId),
                null,
                null));
    }

    #endregion
}
