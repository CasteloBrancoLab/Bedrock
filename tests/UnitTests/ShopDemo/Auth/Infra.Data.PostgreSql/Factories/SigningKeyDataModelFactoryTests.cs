using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class SigningKeyDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public SigningKeyDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapKidValueCorrectly()
    {
        // Arrange
        LogArrange("Creating SigningKey entity with known kid value");
        string expectedKid = Guid.NewGuid().ToString();
        var entity = CreateTestEntity(kid: expectedKid);

        // Act
        LogAct("Creating SigningKeyDataModel from SigningKey entity");
        var dataModel = SigningKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Kid.Value mapping");
        dataModel.Kid.ShouldBe(expectedKid);
    }

    [Fact]
    public void Create_ShouldMapAlgorithmCorrectly()
    {
        // Arrange
        LogArrange("Creating SigningKey entity with known algorithm");
        string expectedAlgorithm = "RS256";
        var entity = CreateTestEntity(algorithm: expectedAlgorithm);

        // Act
        LogAct("Creating SigningKeyDataModel from SigningKey entity");
        var dataModel = SigningKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Algorithm mapping");
        dataModel.Algorithm.ShouldBe(expectedAlgorithm);
    }

    [Fact]
    public void Create_ShouldMapPublicKeyCorrectly()
    {
        // Arrange
        LogArrange("Creating SigningKey entity with known public key");
        string expectedPublicKey = "-----BEGIN PUBLIC KEY-----\nMIIBIjANBg==\n-----END PUBLIC KEY-----";
        var entity = CreateTestEntity(publicKey: expectedPublicKey);

        // Act
        LogAct("Creating SigningKeyDataModel from SigningKey entity");
        var dataModel = SigningKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying PublicKey mapping");
        dataModel.PublicKey.ShouldBe(expectedPublicKey);
    }

    [Fact]
    public void Create_ShouldMapEncryptedPrivateKeyCorrectly()
    {
        // Arrange
        LogArrange("Creating SigningKey entity with known encrypted private key");
        string expectedEncryptedPrivateKey = "encrypted-private-key-data";
        var entity = CreateTestEntity(encryptedPrivateKey: expectedEncryptedPrivateKey);

        // Act
        LogAct("Creating SigningKeyDataModel from SigningKey entity");
        var dataModel = SigningKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying EncryptedPrivateKey mapping");
        dataModel.EncryptedPrivateKey.ShouldBe(expectedEncryptedPrivateKey);
    }

    [Theory]
    [InlineData(SigningKeyStatus.Active, 1)]
    [InlineData(SigningKeyStatus.Rotated, 2)]
    [InlineData(SigningKeyStatus.Revoked, 3)]
    public void Create_ShouldMapStatusAsShortCorrectly(SigningKeyStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating SigningKey entity with status {status}");
        var entity = CreateTestEntity(status: status);

        // Act
        LogAct("Creating SigningKeyDataModel from SigningKey entity");
        var dataModel = SigningKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapRotatedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating SigningKey entity with known rotatedAt");
        var expectedRotatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = CreateTestEntity(rotatedAt: expectedRotatedAt);

        // Act
        LogAct("Creating SigningKeyDataModel from SigningKey entity");
        var dataModel = SigningKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RotatedAt mapping");
        dataModel.RotatedAt.ShouldBe(expectedRotatedAt);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating SigningKey entity with known expiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(90);
        var entity = CreateTestEntity(expiresAt: expectedExpiresAt);

        // Act
        LogAct("Creating SigningKeyDataModel from SigningKey entity");
        var dataModel = SigningKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating SigningKey entity with specific EntityInfo values");
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

        var entity = SigningKey.CreateFromExistingInfo(
            new CreateFromExistingInfoSigningKeyInput(
                entityInfo,
                Kid.CreateFromExistingInfo(Guid.NewGuid().ToString()),
                "RS256",
                "public-key",
                "encrypted-private-key",
                SigningKeyStatus.Active,
                null,
                DateTimeOffset.UtcNow.AddDays(90)));

        // Act
        LogAct("Creating SigningKeyDataModel from SigningKey entity");
        var dataModel = SigningKeyDataModelFactory.Create(entity);

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

    private static SigningKey CreateTestEntity(
        string? kid = null,
        string? algorithm = null,
        string? publicKey = null,
        string? encryptedPrivateKey = null,
        SigningKeyStatus status = SigningKeyStatus.Active,
        DateTimeOffset? rotatedAt = null,
        DateTimeOffset? expiresAt = null)
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

        return SigningKey.CreateFromExistingInfo(
            new CreateFromExistingInfoSigningKeyInput(
                entityInfo,
                Kid.CreateFromExistingInfo(kid ?? Guid.NewGuid().ToString()),
                algorithm ?? "RS256",
                publicKey ?? "public-key",
                encryptedPrivateKey ?? "encrypted-private-key",
                status,
                rotatedAt,
                expiresAt ?? DateTimeOffset.UtcNow.AddDays(90)));
    }

    #endregion
}
