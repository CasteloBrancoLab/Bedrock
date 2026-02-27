using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class KeyChainFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public KeyChainFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel with specific UserId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedUserId, "kid-001", "pubkey", "secret", (short)KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapKeyIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel with specific KeyId");
        string expectedKeyId = "kid-" + Faker.Random.AlphaNumeric(8);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedKeyId, "pubkey", "secret", (short)KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying KeyId mapping");
        entity.KeyId.Value.ShouldBe(expectedKeyId);
    }

    [Fact]
    public void Create_ShouldMapPublicKeyFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel with specific PublicKey");
        string expectedPublicKey = Faker.Random.AlphaNumeric(64);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "kid-001", expectedPublicKey, "secret", (short)KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying PublicKey mapping");
        entity.PublicKey.ShouldBe(expectedPublicKey);
    }

    [Fact]
    public void Create_ShouldMapEncryptedSharedSecretFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel with specific EncryptedSharedSecret");
        string expectedSecret = Faker.Random.AlphaNumeric(128);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "kid-001", "pubkey", expectedSecret, (short)KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EncryptedSharedSecret mapping");
        entity.EncryptedSharedSecret.ShouldBe(expectedSecret);
    }

    [Theory]
    [InlineData((short)1, KeyChainStatus.Active)]
    [InlineData((short)2, KeyChainStatus.DecryptOnly)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, KeyChainStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating KeyChainDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "kid-001", "pubkey", "secret", statusValue, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel with specific ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "kid-001", "pubkey", "secret", (short)KeyChainStatus.Active, expectedExpiresAt);

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel with specific base fields");
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-5);
        long expectedVersion = Faker.Random.Long(1);
        string? expectedLastChangedBy = Faker.Person.FullName;
        var expectedLastChangedAt = DateTimeOffset.UtcNow;
        var expectedLastChangedCorrelationId = Guid.NewGuid();
        string expectedLastChangedExecutionOrigin = "TestOrigin";
        string expectedLastChangedBusinessOperationCode = "TEST_OP";

        var dataModel = new KeyChainDataModel
        {
            Id = expectedId,
            TenantCode = expectedTenantCode,
            CreatedBy = expectedCreatedBy,
            CreatedAt = expectedCreatedAt,
            LastChangedBy = expectedLastChangedBy,
            LastChangedAt = expectedLastChangedAt,
            LastChangedCorrelationId = expectedLastChangedCorrelationId,
            LastChangedExecutionOrigin = expectedLastChangedExecutionOrigin,
            LastChangedBusinessOperationCode = expectedLastChangedBusinessOperationCode,
            EntityVersion = expectedVersion,
            UserId = Guid.NewGuid(),
            KeyId = "kid-001",
            PublicKey = "pubkey",
            EncryptedSharedSecret = "secret",
            Status = (short)KeyChainStatus.Active,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        };

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EntityInfo fields");
        entity.EntityInfo.Id.Value.ShouldBe(expectedId);
        entity.EntityInfo.TenantInfo.Code.ShouldBe(expectedTenantCode);
        entity.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe(expectedCreatedBy);
        entity.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(expectedCreatedAt);
        entity.EntityInfo.EntityVersion.Value.ShouldBe(expectedVersion);
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBe(expectedLastChangedBy);
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(expectedLastChangedAt);
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(expectedLastChangedCorrelationId);
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe(expectedLastChangedExecutionOrigin);
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe(expectedLastChangedBusinessOperationCode);
    }

    [Fact]
    public void Create_WithNullLastChangedFields_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel with null last-changed fields");
        var dataModel = new KeyChainDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            KeyId = "kid-001",
            PublicKey = "pubkey",
            EncryptedSharedSecret = "secret",
            Status = (short)KeyChainStatus.Active,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        };

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel with nulls");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying nullable fields are null");
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapCreatedCorrelationIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel to verify CreatedCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "kid-001", "pubkey", "secret", (short)KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel to verify CreatedExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "kid-001", "pubkey", "secret", (short)KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModel to verify CreatedBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "kid-001", "pubkey", "secret", (short)KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        LogAct("Creating KeyChain from KeyChainDataModel");
        var entity = KeyChainFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static KeyChainDataModel CreateTestDataModel(
        Guid userId,
        string keyId,
        string publicKey,
        string encryptedSharedSecret,
        short status,
        DateTimeOffset expiresAt)
    {
        return new KeyChainDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_KEYCHAIN",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = userId,
            KeyId = keyId,
            PublicKey = publicKey,
            EncryptedSharedSecret = encryptedSharedSecret,
            Status = status,
            ExpiresAt = expiresAt
        };
    }

    #endregion
}
