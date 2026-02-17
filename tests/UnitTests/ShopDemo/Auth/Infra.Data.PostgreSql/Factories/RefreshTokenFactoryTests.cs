using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class RefreshTokenFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public RefreshTokenFactoryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Criando data model com UserId especifico");
        Guid expectedUserId = Guid.NewGuid();
        RefreshTokenDataModel dataModel = CreateTestDataModel(userId: expectedUserId);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando que UserId foi mapeado corretamente");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapTokenHashCorrectly()
    {
        // Arrange
        LogArrange("Criando data model com TokenHash especifico");
        byte[] expectedHash = Faker.Random.Bytes(32);
        RefreshTokenDataModel dataModel = CreateTestDataModel(tokenHash: expectedHash);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando que TokenHash foi mapeado corretamente");
        entity.TokenHash.Value.ToArray().ShouldBe(expectedHash);
    }

    [Fact]
    public void Create_ShouldMapFamilyIdCorrectly()
    {
        // Arrange
        LogArrange("Criando data model com FamilyId especifico");
        Guid expectedFamilyId = Guid.NewGuid();
        RefreshTokenDataModel dataModel = CreateTestDataModel(familyId: expectedFamilyId);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando que FamilyId foi mapeado corretamente");
        entity.FamilyId.Value.ShouldBe(expectedFamilyId);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Criando data model com ExpiresAt especifico");
        DateTimeOffset expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(7);
        RefreshTokenDataModel dataModel = CreateTestDataModel(expiresAt: expectedExpiresAt);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando que ExpiresAt foi mapeado corretamente");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData((short)1, RefreshTokenStatus.Active)]
    [InlineData((short)2, RefreshTokenStatus.Used)]
    [InlineData((short)3, RefreshTokenStatus.Revoked)]
    public void Create_ShouldMapStatusFromShortCorrectly(short statusValue, RefreshTokenStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Criando data model com Status short {statusValue}");
        RefreshTokenDataModel dataModel = CreateTestDataModel(status: statusValue);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert($"Verificando que Status foi mapeado para {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtCorrectly_WhenNotNull()
    {
        // Arrange
        LogArrange("Criando data model com RevokedAt preenchido");
        DateTimeOffset expectedRevokedAt = DateTimeOffset.UtcNow;
        RefreshTokenDataModel dataModel = CreateTestDataModel(
            status: (short)RefreshTokenStatus.Revoked,
            revokedAt: expectedRevokedAt);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando que RevokedAt foi mapeado corretamente");
        entity.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtAsNull_WhenNull()
    {
        // Arrange
        LogArrange("Criando data model com RevokedAt nulo");
        RefreshTokenDataModel dataModel = CreateTestDataModel(revokedAt: null);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando que RevokedAt e null");
        entity.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapReplacedByTokenIdCorrectly_WhenNotNull()
    {
        // Arrange
        LogArrange("Criando data model com ReplacedByTokenId preenchido");
        Guid expectedReplacedByTokenId = Guid.NewGuid();
        RefreshTokenDataModel dataModel = CreateTestDataModel(
            status: (short)RefreshTokenStatus.Used,
            replacedByTokenId: expectedReplacedByTokenId);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando que ReplacedByTokenId foi mapeado corretamente");
        entity.ReplacedByTokenId.ShouldNotBeNull();
        entity.ReplacedByTokenId.Value.Value.ShouldBe(expectedReplacedByTokenId);
    }

    [Fact]
    public void Create_ShouldMapReplacedByTokenIdAsNull_WhenNull()
    {
        // Arrange
        LogArrange("Criando data model com ReplacedByTokenId nulo");
        RefreshTokenDataModel dataModel = CreateTestDataModel(replacedByTokenId: null);

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando que ReplacedByTokenId e null");
        entity.ReplacedByTokenId.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapEntityInfoBaseFieldsCorrectly()
    {
        // Arrange
        LogArrange("Criando data model com campos base especificos");
        Guid expectedId = Guid.NewGuid();
        Guid expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = "test-creator";
        DateTimeOffset expectedCreatedAt = DateTimeOffset.UtcNow.AddHours(-1);
        Guid expectedCreatedCorrelationId = Guid.NewGuid();
        string expectedCreatedExecutionOrigin = "UnitTest";
        string expectedCreatedBusinessOperationCode = "CREATE_TOKEN";
        long expectedVersion = 3;

        var dataModel = new RefreshTokenDataModel
        {
            Id = expectedId,
            TenantCode = expectedTenantCode,
            CreatedBy = expectedCreatedBy,
            CreatedAt = expectedCreatedAt,
            CreatedCorrelationId = expectedCreatedCorrelationId,
            CreatedExecutionOrigin = expectedCreatedExecutionOrigin,
            CreatedBusinessOperationCode = expectedCreatedBusinessOperationCode,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = expectedVersion,
            UserId = Guid.NewGuid(),
            TokenHash = Faker.Random.Bytes(32),
            FamilyId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            Status = (short)RefreshTokenStatus.Active,
            RevokedAt = null,
            ReplacedByTokenId = null
        };

        // Act
        LogAct("Chamando RefreshTokenFactory.Create");
        RefreshToken entity = RefreshTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verificando campos base do EntityInfo");
        entity.EntityInfo.Id.Value.ShouldBe(expectedId);
        entity.EntityInfo.TenantInfo.Code.ShouldBe(expectedTenantCode);
        entity.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe(expectedCreatedBy);
        entity.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(expectedCreatedAt);
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(expectedCreatedCorrelationId);
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(expectedCreatedExecutionOrigin);
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(expectedCreatedBusinessOperationCode);
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        entity.EntityInfo.EntityVersion.Value.ShouldBe(expectedVersion);
    }

    #region Helper Methods

    private static RefreshTokenDataModel CreateTestDataModel(
        Guid? userId = null,
        byte[]? tokenHash = null,
        Guid? familyId = null,
        DateTimeOffset? expiresAt = null,
        short status = (short)RefreshTokenStatus.Active,
        DateTimeOffset? revokedAt = null,
        Guid? replacedByTokenId = null)
    {
        return new RefreshTokenDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_TOKEN",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = userId ?? Guid.NewGuid(),
            TokenHash = tokenHash ?? Faker.Random.Bytes(32),
            FamilyId = familyId ?? Guid.NewGuid(),
            ExpiresAt = expiresAt ?? DateTimeOffset.UtcNow.AddDays(7),
            Status = status,
            RevokedAt = revokedAt,
            ReplacedByTokenId = replacedByTokenId
        };
    }

    #endregion
}
