using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class RefreshTokenDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public RefreshTokenDataModelFactoryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Criando RefreshToken com UserId especifico");
        Guid expectedUserId = Guid.NewGuid();
        RefreshToken entity = CreateTestRefreshToken(userId: expectedUserId);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando que UserId foi mapeado corretamente");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapTokenHashCorrectly()
    {
        // Arrange
        LogArrange("Criando RefreshToken com TokenHash especifico");
        byte[] expectedHash = Faker.Random.Bytes(32);
        RefreshToken entity = CreateTestRefreshToken(tokenHash: expectedHash);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando que TokenHash foi mapeado corretamente");
        dataModel.TokenHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Create_ShouldMapFamilyIdCorrectly()
    {
        // Arrange
        LogArrange("Criando RefreshToken com FamilyId especifico");
        Guid expectedFamilyId = Guid.NewGuid();
        RefreshToken entity = CreateTestRefreshToken(familyId: expectedFamilyId);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando que FamilyId foi mapeado corretamente");
        dataModel.FamilyId.ShouldBe(expectedFamilyId);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Criando RefreshToken com ExpiresAt especifico");
        DateTimeOffset expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(7);
        RefreshToken entity = CreateTestRefreshToken(expiresAt: expectedExpiresAt);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando que ExpiresAt foi mapeado corretamente");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData(RefreshTokenStatus.Active, 1)]
    [InlineData(RefreshTokenStatus.Used, 2)]
    [InlineData(RefreshTokenStatus.Revoked, 3)]
    public void Create_ShouldMapStatusAsShortCorrectly(RefreshTokenStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Criando RefreshToken com Status {status}");
        RefreshToken entity = CreateTestRefreshToken(status: status);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verificando que Status foi mapeado para short {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtCorrectly_WhenNotNull()
    {
        // Arrange
        LogArrange("Criando RefreshToken com RevokedAt preenchido");
        DateTimeOffset expectedRevokedAt = DateTimeOffset.UtcNow;
        RefreshToken entity = CreateTestRefreshToken(
            status: RefreshTokenStatus.Revoked,
            revokedAt: expectedRevokedAt);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando que RevokedAt foi mapeado corretamente");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtAsNull_WhenNull()
    {
        // Arrange
        LogArrange("Criando RefreshToken com RevokedAt nulo");
        RefreshToken entity = CreateTestRefreshToken(revokedAt: null);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando que RevokedAt e null");
        dataModel.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapReplacedByTokenIdCorrectly_WhenNotNull()
    {
        // Arrange
        LogArrange("Criando RefreshToken com ReplacedByTokenId preenchido");
        Guid expectedReplacedByTokenId = Guid.NewGuid();
        RefreshToken entity = CreateTestRefreshToken(
            status: RefreshTokenStatus.Used,
            replacedByTokenId: expectedReplacedByTokenId);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando que ReplacedByTokenId foi mapeado corretamente");
        dataModel.ReplacedByTokenId.ShouldBe(expectedReplacedByTokenId);
    }

    [Fact]
    public void Create_ShouldMapReplacedByTokenIdAsNull_WhenNull()
    {
        // Arrange
        LogArrange("Criando RefreshToken com ReplacedByTokenId nulo");
        RefreshToken entity = CreateTestRefreshToken(replacedByTokenId: null);

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando que ReplacedByTokenId e null");
        dataModel.ReplacedByTokenId.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Criando RefreshToken com EntityInfo especifico");
        Guid entityId = Guid.NewGuid();
        Guid tenantCode = Guid.NewGuid();
        string createdBy = "test-creator";
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddHours(-1);
        Guid createdCorrelationId = Guid.NewGuid();
        string createdExecutionOrigin = "UnitTest";
        string createdBusinessOperationCode = "CREATE_TOKEN";
        long expectedVersion = 5;

        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId),
            tenantInfo: TenantInfo.Create(tenantCode),
            createdAt: createdAt,
            createdBy: createdBy,
            createdCorrelationId: createdCorrelationId,
            createdExecutionOrigin: createdExecutionOrigin,
            createdBusinessOperationCode: createdBusinessOperationCode,
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(expectedVersion));

        RefreshToken entity = RefreshToken.CreateFromExistingInfo(
            new CreateFromExistingInfoRefreshTokenInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                TokenHash.CreateNew(Faker.Random.Bytes(32)),
                TokenFamily.CreateFromExistingInfo(Guid.NewGuid()),
                DateTimeOffset.UtcNow.AddDays(7),
                RefreshTokenStatus.Active,
                null,
                null));

        // Act
        LogAct("Chamando RefreshTokenDataModelFactory.Create");
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verificando campos base do EntityInfo");
        dataModel.Id.ShouldBe(entityId);
        dataModel.TenantCode.ShouldBe(tenantCode);
        dataModel.CreatedBy.ShouldBe(createdBy);
        dataModel.CreatedAt.ShouldBe(createdAt);
        dataModel.CreatedCorrelationId.ShouldBe(createdCorrelationId);
        dataModel.CreatedExecutionOrigin.ShouldBe(createdExecutionOrigin);
        dataModel.CreatedBusinessOperationCode.ShouldBe(createdBusinessOperationCode);
        dataModel.LastChangedBy.ShouldBeNull();
        dataModel.LastChangedAt.ShouldBeNull();
        dataModel.EntityVersion.ShouldBe(expectedVersion);
    }

    #region Helper Methods

    private static RefreshToken CreateTestRefreshToken(
        Guid? userId = null,
        byte[]? tokenHash = null,
        Guid? familyId = null,
        DateTimeOffset? expiresAt = null,
        RefreshTokenStatus status = RefreshTokenStatus.Active,
        DateTimeOffset? revokedAt = null,
        Guid? replacedByTokenId = null)
    {
        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "CREATE_TOKEN",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(1));

        return RefreshToken.CreateFromExistingInfo(
            new CreateFromExistingInfoRefreshTokenInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId ?? Guid.NewGuid()),
                TokenHash.CreateNew(tokenHash ?? Faker.Random.Bytes(32)),
                TokenFamily.CreateFromExistingInfo(familyId ?? Guid.NewGuid()),
                expiresAt ?? DateTimeOffset.UtcNow.AddDays(7),
                status,
                revokedAt,
                replacedByTokenId.HasValue
                    ? Id.CreateFromExistingInfo(replacedByTokenId.Value)
                    : null));
    }

    #endregion
}
