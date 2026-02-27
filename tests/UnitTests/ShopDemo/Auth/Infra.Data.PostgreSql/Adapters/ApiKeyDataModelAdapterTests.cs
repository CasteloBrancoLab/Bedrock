using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ApiKeyDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ApiKeyDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateServiceClientIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel and ApiKey with different ServiceClientIds");
        var dataModel = CreateTestDataModel();
        var expectedServiceClientId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedServiceClientId, "pfx", "hash123", ApiKeyStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        ApiKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ServiceClientId was updated");
        dataModel.ServiceClientId.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Adapt_ShouldUpdateKeyPrefixFromEntity()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel and ApiKey with different KeyPrefixes");
        var dataModel = CreateTestDataModel();
        string expectedKeyPrefix = Faker.Random.String2(8);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedKeyPrefix, "hash123", ApiKeyStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        ApiKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying KeyPrefix was updated");
        dataModel.KeyPrefix.ShouldBe(expectedKeyPrefix);
    }

    [Fact]
    public void Adapt_ShouldUpdateKeyHashFromEntity()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel and ApiKey with different KeyHashes");
        var dataModel = CreateTestDataModel();
        string expectedKeyHash = Faker.Random.String2(64);
        var entity = CreateTestEntity(Guid.NewGuid(), "pfx", expectedKeyHash, ApiKeyStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        ApiKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying KeyHash was updated");
        dataModel.KeyHash.ShouldBe(expectedKeyHash);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel and ApiKey with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)ApiKeyStatus.Active;
        var entity = CreateTestEntity(Guid.NewGuid(), "pfx", "hash123", ApiKeyStatus.Revoked);

        // Act
        LogAct("Adapting data model from entity");
        ApiKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)ApiKeyStatus.Revoked);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel and ApiKey with different ExpiresAt values");
        var dataModel = CreateTestDataModel();
        DateTimeOffset? expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(60);
        var entity = CreateTestEntity(Guid.NewGuid(), "pfx", "hash123", ApiKeyStatus.Active, expiresAt: expectedExpiresAt);

        // Act
        LogAct("Adapting data model from entity");
        ApiKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateLastUsedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel and ApiKey with different LastUsedAt values");
        var dataModel = CreateTestDataModel();
        DateTimeOffset? expectedLastUsedAt = DateTimeOffset.UtcNow.AddHours(-3);
        var entity = CreateTestEntity(Guid.NewGuid(), "pfx", "hash123", ApiKeyStatus.Active, lastUsedAt: expectedLastUsedAt);

        // Act
        LogAct("Adapting data model from entity");
        ApiKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying LastUsedAt was updated");
        dataModel.LastUsedAt.ShouldBe(expectedLastUsedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateRevokedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel and ApiKey with different RevokedAt values");
        var dataModel = CreateTestDataModel();
        DateTimeOffset? expectedRevokedAt = DateTimeOffset.UtcNow.AddDays(-2);
        var entity = CreateTestEntity(Guid.NewGuid(), "pfx", "hash123", ApiKeyStatus.Revoked, revokedAt: expectedRevokedAt);

        // Act
        LogAct("Adapting data model from entity");
        ApiKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying RevokedAt was updated");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel and ApiKey with different EntityInfo values");
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
        LogAct("Adapting data model from entity");
        ApiKeyDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ApiKeyDataModel and ApiKey");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "pfx", "hash123", ApiKeyStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        var result = ApiKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ApiKeyDataModel CreateTestDataModel()
    {
        return new ApiKeyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            ServiceClientId = Guid.NewGuid(),
            KeyPrefix = "initial-pfx",
            KeyHash = "initial-hash",
            Status = (short)ApiKeyStatus.Active
        };
    }

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
