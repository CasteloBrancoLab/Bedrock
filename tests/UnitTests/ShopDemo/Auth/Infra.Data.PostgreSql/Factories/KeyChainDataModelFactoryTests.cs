using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class KeyChainDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public KeyChainDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating KeyChain entity with known UserId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "kid-001", "pubkey", "secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChainDataModel from KeyChain entity");
        var dataModel = KeyChainDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapKeyIdCorrectly()
    {
        // Arrange
        LogArrange("Creating KeyChain entity with known KeyId");
        string expectedKeyId = "kid-" + Faker.Random.AlphaNumeric(8);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedKeyId, "pubkey", "secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChainDataModel from KeyChain entity");
        var dataModel = KeyChainDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying KeyId mapping");
        dataModel.KeyId.ShouldBe(expectedKeyId);
    }

    [Fact]
    public void Create_ShouldMapPublicKeyCorrectly()
    {
        // Arrange
        LogArrange("Creating KeyChain entity with known PublicKey");
        string expectedPublicKey = Faker.Random.AlphaNumeric(64);
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", expectedPublicKey, "secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChainDataModel from KeyChain entity");
        var dataModel = KeyChainDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying PublicKey mapping");
        dataModel.PublicKey.ShouldBe(expectedPublicKey);
    }

    [Fact]
    public void Create_ShouldMapEncryptedSharedSecretCorrectly()
    {
        // Arrange
        LogArrange("Creating KeyChain entity with known EncryptedSharedSecret");
        string expectedSecret = Faker.Random.AlphaNumeric(128);
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", "pubkey", expectedSecret, KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChainDataModel from KeyChain entity");
        var dataModel = KeyChainDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying EncryptedSharedSecret mapping");
        dataModel.EncryptedSharedSecret.ShouldBe(expectedSecret);
    }

    [Theory]
    [InlineData(KeyChainStatus.Active, 1)]
    [InlineData(KeyChainStatus.DecryptOnly, 2)]
    public void Create_ShouldMapStatusAsShortCorrectly(KeyChainStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating KeyChain entity with status {status}");
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", "pubkey", "secret", status, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChainDataModel from KeyChain entity");
        var dataModel = KeyChainDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating KeyChain entity with known ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        var entity = CreateTestEntity(Guid.NewGuid(), "kid-001", "pubkey", "secret", KeyChainStatus.Active, expectedExpiresAt);

        // Act
        LogAct("Creating KeyChainDataModel from KeyChain entity");
        var dataModel = KeyChainDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating KeyChain entity with specific EntityInfo values");
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
        LogAct("Creating KeyChainDataModel from KeyChain entity");
        var dataModel = KeyChainDataModelFactory.Create(entity);

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
