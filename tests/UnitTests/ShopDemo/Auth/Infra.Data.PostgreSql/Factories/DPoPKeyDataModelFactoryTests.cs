using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class DPoPKeyDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public DPoPKeyDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating DPoPKey entity with known UserId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(
            userId: expectedUserId,
            jwkThumbprint: "thumbprint-abc",
            publicKeyJwk: "{}",
            expiresAt: DateTimeOffset.UtcNow.AddHours(1),
            status: DPoPKeyStatus.Active,
            revokedAt: null);

        // Act
        LogAct("Creating DPoPKeyDataModel from DPoPKey entity");
        var dataModel = DPoPKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapJwkThumbprintCorrectly()
    {
        // Arrange
        LogArrange("Creating DPoPKey entity with known JwkThumbprint");
        string expectedJwkThumbprint = Faker.Random.String2(43);
        var entity = CreateTestEntity(
            userId: Guid.NewGuid(),
            jwkThumbprint: expectedJwkThumbprint,
            publicKeyJwk: "{}",
            expiresAt: DateTimeOffset.UtcNow.AddHours(1),
            status: DPoPKeyStatus.Active,
            revokedAt: null);

        // Act
        LogAct("Creating DPoPKeyDataModel from DPoPKey entity");
        var dataModel = DPoPKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying JwkThumbprint mapping");
        dataModel.JwkThumbprint.ShouldBe(expectedJwkThumbprint);
    }

    [Fact]
    public void Create_ShouldMapPublicKeyJwkCorrectly()
    {
        // Arrange
        LogArrange("Creating DPoPKey entity with known PublicKeyJwk");
        string expectedPublicKeyJwk = "{\"kty\":\"EC\",\"crv\":\"P-256\"}";
        var entity = CreateTestEntity(
            userId: Guid.NewGuid(),
            jwkThumbprint: "thumbprint-abc",
            publicKeyJwk: expectedPublicKeyJwk,
            expiresAt: DateTimeOffset.UtcNow.AddHours(1),
            status: DPoPKeyStatus.Active,
            revokedAt: null);

        // Act
        LogAct("Creating DPoPKeyDataModel from DPoPKey entity");
        var dataModel = DPoPKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying PublicKeyJwk mapping");
        dataModel.PublicKeyJwk.ShouldBe(expectedPublicKeyJwk);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating DPoPKey entity with known ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(24);
        var entity = CreateTestEntity(
            userId: Guid.NewGuid(),
            jwkThumbprint: "thumbprint-abc",
            publicKeyJwk: "{}",
            expiresAt: expectedExpiresAt,
            status: DPoPKeyStatus.Active,
            revokedAt: null);

        // Act
        LogAct("Creating DPoPKeyDataModel from DPoPKey entity");
        var dataModel = DPoPKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData(DPoPKeyStatus.Active, 1)]
    [InlineData(DPoPKeyStatus.Revoked, 2)]
    public void Create_ShouldMapStatusAsShortCorrectly(DPoPKeyStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating DPoPKey entity with status {status}");
        var entity = CreateTestEntity(
            userId: Guid.NewGuid(),
            jwkThumbprint: "thumbprint-abc",
            publicKeyJwk: "{}",
            expiresAt: DateTimeOffset.UtcNow.AddHours(1),
            status: status,
            revokedAt: null);

        // Act
        LogAct("Creating DPoPKeyDataModel from DPoPKey entity");
        var dataModel = DPoPKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating DPoPKey entity with known RevokedAt");
        DateTimeOffset? expectedRevokedAt = DateTimeOffset.UtcNow.AddMinutes(-30);
        var entity = CreateTestEntity(
            userId: Guid.NewGuid(),
            jwkThumbprint: "thumbprint-abc",
            publicKeyJwk: "{}",
            expiresAt: DateTimeOffset.UtcNow.AddHours(1),
            status: DPoPKeyStatus.Revoked,
            revokedAt: expectedRevokedAt);

        // Act
        LogAct("Creating DPoPKeyDataModel from DPoPKey entity");
        var dataModel = DPoPKeyDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating DPoPKey entity with specific EntityInfo values");
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

        var entity = DPoPKey.CreateFromExistingInfo(
            new CreateFromExistingInfoDPoPKeyInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                JwkThumbprint.CreateNew("thumbprint-abc"),
                "{}",
                DateTimeOffset.UtcNow.AddHours(1),
                DPoPKeyStatus.Active,
                null));

        // Act
        LogAct("Creating DPoPKeyDataModel from DPoPKey entity");
        var dataModel = DPoPKeyDataModelFactory.Create(entity);

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

    private static DPoPKey CreateTestEntity(
        Guid userId,
        string jwkThumbprint,
        string publicKeyJwk,
        DateTimeOffset expiresAt,
        DPoPKeyStatus status,
        DateTimeOffset? revokedAt)
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

        return DPoPKey.CreateFromExistingInfo(
            new CreateFromExistingInfoDPoPKeyInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                JwkThumbprint.CreateNew(jwkThumbprint),
                publicKeyJwk,
                expiresAt,
                status,
                revokedAt));
    }

    #endregion
}
