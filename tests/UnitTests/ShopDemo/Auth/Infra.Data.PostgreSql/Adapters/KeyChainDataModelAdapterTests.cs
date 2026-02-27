using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class KeyChainDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public KeyChainDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel and KeyChain with different UserIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "kid-001", "pubkey", "secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Adapting data model from entity");
        KeyChainDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateKeyIdFromEntity()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel and KeyChain with different KeyIds");
        var dataModel = CreateTestDataModel();
        string expectedKeyId = "kid-" + Faker.Random.AlphaNumeric(8);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedKeyId, "pubkey", "secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Adapting data model from entity");
        KeyChainDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying KeyId was updated");
        dataModel.KeyId.ShouldBe(expectedKeyId);
    }

    [Fact]
    public void Adapt_ShouldUpdatePublicKeyFromEntity()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel and KeyChain with different PublicKeys");
        var dataModel = CreateTestDataModel();
        string expectedPublicKey = Faker.Random.AlphaNumeric(64);
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", expectedPublicKey, "secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Adapting data model from entity");
        KeyChainDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying PublicKey was updated");
        dataModel.PublicKey.ShouldBe(expectedPublicKey);
    }

    [Fact]
    public void Adapt_ShouldUpdateEncryptedSharedSecretFromEntity()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel and KeyChain with different EncryptedSharedSecrets");
        var dataModel = CreateTestDataModel();
        string expectedSecret = Faker.Random.AlphaNumeric(128);
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", "pubkey", expectedSecret, KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Adapting data model from entity");
        KeyChainDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying EncryptedSharedSecret was updated");
        dataModel.EncryptedSharedSecret.ShouldBe(expectedSecret);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel and KeyChain with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)KeyChainStatus.Active;
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", "pubkey", "secret", KeyChainStatus.DecryptOnly, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Adapting data model from entity");
        KeyChainDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)KeyChainStatus.DecryptOnly);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel and KeyChain with different ExpiresAt");
        var dataModel = CreateTestDataModel();
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(90);
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", "pubkey", "secret", KeyChainStatus.Active, expectedExpiresAt);

        // Act
        LogAct("Adapting data model from entity");
        KeyChainDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel and KeyChain with different EntityInfo values");
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

        var entity = KeyChain.CreateFromExistingInfo(
            new CreateFromExistingInfoKeyChainInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                KeyId.CreateFromExistingInfo("kid-001"),
                "pubkey",
                "secret",
                KeyChainStatus.Active,
                DateTimeOffset.UtcNow.AddDays(1)));

        // Act
        LogAct("Adapting data model from entity");
        KeyChainDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating KeyChainDataModel and KeyChain");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", "pubkey", "secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Adapting data model from entity");
        var result = KeyChainDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static KeyChainDataModel CreateTestDataModel()
    {
        return new KeyChainDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            KeyId = "initial-kid",
            PublicKey = "initial-pubkey",
            EncryptedSharedSecret = "initial-secret",
            Status = (short)KeyChainStatus.Active,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };
    }

    private static KeyChain CreateTestEntity(
        Guid userId,
        string keyId,
        string publicKey,
        string encryptedSharedSecret,
        KeyChainStatus status,
        DateTimeOffset expiresAt)
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

        return KeyChain.CreateFromExistingInfo(
            new CreateFromExistingInfoKeyChainInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                KeyId.CreateFromExistingInfo(keyId),
                publicKey,
                encryptedSharedSecret,
                status,
                expiresAt));
    }

    #endregion
}
