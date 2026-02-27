using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class SigningKeyFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public SigningKeyFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapKidFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel with specific kid");
        string expectedKid = Guid.NewGuid().ToString();
        var dataModel = CreateTestDataModel(kid: expectedKid);

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Kid mapping");
        entity.Kid.Value.ShouldBe(expectedKid);
    }

    [Fact]
    public void Create_ShouldMapAlgorithmFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel with specific algorithm");
        string expectedAlgorithm = "ES256";
        var dataModel = CreateTestDataModel(algorithm: expectedAlgorithm);

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Algorithm mapping");
        entity.Algorithm.ShouldBe(expectedAlgorithm);
    }

    [Fact]
    public void Create_ShouldMapPublicKeyFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel with specific public key");
        string expectedPublicKey = "-----BEGIN PUBLIC KEY-----\nMIIBIjANBg==\n-----END PUBLIC KEY-----";
        var dataModel = CreateTestDataModel(publicKey: expectedPublicKey);

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying PublicKey mapping");
        entity.PublicKey.ShouldBe(expectedPublicKey);
    }

    [Fact]
    public void Create_ShouldMapEncryptedPrivateKeyFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel with specific encrypted private key");
        string expectedEncryptedPrivateKey = "encrypted-private-key-data";
        var dataModel = CreateTestDataModel(encryptedPrivateKey: expectedEncryptedPrivateKey);

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EncryptedPrivateKey mapping");
        entity.EncryptedPrivateKey.ShouldBe(expectedEncryptedPrivateKey);
    }

    [Theory]
    [InlineData((short)1, SigningKeyStatus.Active)]
    [InlineData((short)2, SigningKeyStatus.Rotated)]
    [InlineData((short)3, SigningKeyStatus.Revoked)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, SigningKeyStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating SigningKeyDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel(status: statusValue);

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapRotatedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel with specific rotatedAt");
        var expectedRotatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var dataModel = CreateTestDataModel();
        dataModel.RotatedAt = expectedRotatedAt;

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RotatedAt mapping");
        entity.RotatedAt.ShouldBe(expectedRotatedAt);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel with specific expiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(90);
        var dataModel = CreateTestDataModel();
        dataModel.ExpiresAt = expectedExpiresAt;

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel with specific base fields");
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

        var dataModel = new SigningKeyDataModel
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
            Kid = Guid.NewGuid().ToString(),
            Algorithm = "RS256",
            PublicKey = "public-key",
            EncryptedPrivateKey = "encrypted-private-key",
            Status = (short)SigningKeyStatus.Active,
            RotatedAt = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(90)
        };

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

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
        LogArrange("Creating SigningKeyDataModel with null last-changed fields");
        var dataModel = new SigningKeyDataModel
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
            Kid = Guid.NewGuid().ToString(),
            Algorithm = "RS256",
            PublicKey = "public-key",
            EncryptedPrivateKey = "encrypted-private-key",
            Status = (short)SigningKeyStatus.Active,
            RotatedAt = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(90)
        };

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel with nulls");
        var entity = SigningKeyFactory.Create(dataModel);

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
        LogArrange("Creating SigningKeyDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating SigningKey from SigningKeyDataModel");
        var entity = SigningKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static SigningKeyDataModel CreateTestDataModel(
        string? kid = null,
        string? algorithm = null,
        string? publicKey = null,
        string? encryptedPrivateKey = null,
        short? status = null)
    {
        return new SigningKeyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_SIGNING_KEY",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Kid = kid ?? Guid.NewGuid().ToString(),
            Algorithm = algorithm ?? "RS256",
            PublicKey = publicKey ?? "public-key",
            EncryptedPrivateKey = encryptedPrivateKey ?? "encrypted-private-key",
            Status = status ?? (short)SigningKeyStatus.Active,
            RotatedAt = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(90)
        };
    }

    #endregion
}
