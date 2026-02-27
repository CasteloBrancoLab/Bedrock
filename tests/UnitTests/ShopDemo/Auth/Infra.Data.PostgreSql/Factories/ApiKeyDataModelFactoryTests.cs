using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ApiKeyDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ApiKeyDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapServiceClientIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ApiKey entity with known ServiceClientId");
        var expectedServiceClientId = Guid.NewGuid();
        var entity = CreateTestEntity(
            serviceClientId: expectedServiceClientId,
            keyPrefix: "pfx",
            keyHash: "hash123",
            status: ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ServiceClientId mapping");
        dataModel.ServiceClientId.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Create_ShouldMapKeyPrefixCorrectly()
    {
        // Arrange
        LogArrange("Creating ApiKey entity with known KeyPrefix");
        string expectedKeyPrefix = Faker.Random.String2(8);
        var entity = CreateTestEntity(
            serviceClientId: Guid.NewGuid(),
            keyPrefix: expectedKeyPrefix,
            keyHash: "hash123",
            status: ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying KeyPrefix mapping");
        dataModel.KeyPrefix.ShouldBe(expectedKeyPrefix);
    }

    [Fact]
    public void Create_ShouldMapKeyHashCorrectly()
    {
        // Arrange
        LogArrange("Creating ApiKey entity with known KeyHash");
        string expectedKeyHash = Faker.Random.String2(64);
        var entity = CreateTestEntity(
            serviceClientId: Guid.NewGuid(),
            keyPrefix: "pfx",
            keyHash: expectedKeyHash,
            status: ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying KeyHash mapping");
        dataModel.KeyHash.ShouldBe(expectedKeyHash);
    }

    [Theory]
    [InlineData(ApiKeyStatus.Active, 1)]
    [InlineData(ApiKeyStatus.Revoked, 2)]
    public void Create_ShouldMapStatusAsShortCorrectly(ApiKeyStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating ApiKey entity with status {status}");
        var entity = CreateTestEntity(
            serviceClientId: Guid.NewGuid(),
            keyPrefix: "pfx",
            keyHash: "hash123",
            status: status);

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ApiKey entity with known ExpiresAt");
        DateTimeOffset? expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        var entity = CreateTestEntity(
            serviceClientId: Guid.NewGuid(),
            keyPrefix: "pfx",
            keyHash: "hash123",
            status: ApiKeyStatus.Active,
            expiresAt: expectedExpiresAt);

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapNullExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ApiKey entity with null ExpiresAt");
        var entity = CreateTestEntity(
            serviceClientId: Guid.NewGuid(),
            keyPrefix: "pfx",
            keyHash: "hash123",
            status: ApiKeyStatus.Active,
            expiresAt: null);

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapLastUsedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ApiKey entity with known LastUsedAt");
        DateTimeOffset? expectedLastUsedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var entity = CreateTestEntity(
            serviceClientId: Guid.NewGuid(),
            keyPrefix: "pfx",
            keyHash: "hash123",
            status: ApiKeyStatus.Active,
            lastUsedAt: expectedLastUsedAt);

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying LastUsedAt mapping");
        dataModel.LastUsedAt.ShouldBe(expectedLastUsedAt);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ApiKey entity with known RevokedAt");
        DateTimeOffset? expectedRevokedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = CreateTestEntity(
            serviceClientId: Guid.NewGuid(),
            keyPrefix: "pfx",
            keyHash: "hash123",
            status: ApiKeyStatus.Revoked,
            revokedAt: expectedRevokedAt);

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ApiKey entity with specific EntityInfo values");
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

        var entity = ApiKey.CreateFromExistingInfo(
            new CreateFromExistingInfoApiKeyInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "pfx",
                "hash123",
                ApiKeyStatus.Active,
                null,
                null,
                null));

        // Act
        LogAct("Creating ApiKeyDataModel from ApiKey entity");
        var dataModel = ApiKeyDataModelFactory.Create(entity);

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

    private static ApiKey CreateTestEntity(
        Guid serviceClientId,
        string keyPrefix,
        string keyHash,
        ApiKeyStatus status,
        DateTimeOffset? expiresAt = null,
        DateTimeOffset? lastUsedAt = null,
        DateTimeOffset? revokedAt = null)
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

        return ApiKey.CreateFromExistingInfo(
            new CreateFromExistingInfoApiKeyInput(
                entityInfo,
                Id.CreateFromExistingInfo(serviceClientId),
                keyPrefix,
                keyHash,
                status,
                expiresAt,
                lastUsedAt,
                revokedAt));
    }

    #endregion
}
