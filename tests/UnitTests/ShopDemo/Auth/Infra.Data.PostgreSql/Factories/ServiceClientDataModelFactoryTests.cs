using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ServiceClientDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapClientIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClient entity with known clientId");
        string expectedClientId = Faker.Random.AlphaNumeric(20);
        var entity = CreateTestEntity(expectedClientId, [1, 2, 3], "Test Client", ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClientDataModel from ServiceClient entity");
        var dataModel = ServiceClientDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ClientId mapping");
        dataModel.ClientId.ShouldBe(expectedClientId);
    }

    [Fact]
    public void Create_ShouldMapClientSecretHashCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClient entity with known client secret hash");
        byte[] expectedHash = [10, 20, 30, 40, 50];
        var entity = CreateTestEntity("client-id", expectedHash, "Test Client", ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClientDataModel from ServiceClient entity");
        var dataModel = ServiceClientDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ClientSecretHash mapping");
        dataModel.ClientSecretHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Create_ShouldMapNameCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClient entity with known name");
        string expectedName = Faker.Company.CompanyName();
        var entity = CreateTestEntity("client-id", [1, 2, 3], expectedName, ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClientDataModel from ServiceClient entity");
        var dataModel = ServiceClientDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Name mapping");
        dataModel.Name.ShouldBe(expectedName);
    }

    [Theory]
    [InlineData(ServiceClientStatus.Active, 1)]
    [InlineData(ServiceClientStatus.Revoked, 2)]
    public void Create_ShouldMapStatusAsShortCorrectly(ServiceClientStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating ServiceClient entity with status {status}");
        var entity = CreateTestEntity("client-id", [1, 2, 3], "Test Client", status);

        // Act
        LogAct("Creating ServiceClientDataModel from ServiceClient entity");
        var dataModel = ServiceClientDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapCreatedByUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClient entity with known createdByUserId");
        var expectedCreatedByUserId = Guid.NewGuid();
        var entity = CreateTestEntityWithCreatedByUserId(expectedCreatedByUserId);

        // Act
        LogAct("Creating ServiceClientDataModel from ServiceClient entity");
        var dataModel = ServiceClientDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying CreatedByUserId mapping");
        dataModel.CreatedByUserId.ShouldBe(expectedCreatedByUserId);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClient entity with known expiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        var entity = CreateTestEntityWithExpiry(expectedExpiresAt, null);

        // Act
        LogAct("Creating ServiceClientDataModel from ServiceClient entity");
        var dataModel = ServiceClientDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClient entity with known revokedAt");
        var expectedRevokedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = CreateTestEntityWithExpiry(null, expectedRevokedAt);

        // Act
        LogAct("Creating ServiceClientDataModel from ServiceClient entity");
        var dataModel = ServiceClientDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ServiceClient entity with specific EntityInfo values");
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
        LogAct("Creating ServiceClientDataModel from ServiceClient entity");
        var dataModel = ServiceClientDataModelFactory.Create(entity);

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

    private static ServiceClient CreateTestEntityWithExpiry(DateTimeOffset? expiresAt, DateTimeOffset? revokedAt)
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
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                expiresAt,
                revokedAt));
    }

    #endregion
}
