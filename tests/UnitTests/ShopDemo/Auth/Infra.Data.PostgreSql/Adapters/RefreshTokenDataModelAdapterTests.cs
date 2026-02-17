using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class RefreshTokenDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public RefreshTokenDataModelAdapterTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Adapt_ShouldReturnSameDataModelInstance()
    {
        // Arrange
        LogArrange("Criando data model e entity");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        RefreshToken entity = CreateTestRefreshToken();

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModel result = RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que retorna a mesma instancia");
        result.ShouldBeSameAs(dataModel);
    }

    [Fact]
    public void Adapt_ShouldUpdateUserId()
    {
        // Arrange
        LogArrange("Criando data model e entity com UserId diferente");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        Guid expectedUserId = Guid.NewGuid();
        RefreshToken entity = CreateTestRefreshToken(userId: expectedUserId);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que UserId foi atualizado");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateTokenHash()
    {
        // Arrange
        LogArrange("Criando data model e entity com TokenHash diferente");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        byte[] expectedHash = Faker.Random.Bytes(32);
        RefreshToken entity = CreateTestRefreshToken(tokenHash: expectedHash);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que TokenHash foi atualizado");
        dataModel.TokenHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Adapt_ShouldUpdateFamilyId()
    {
        // Arrange
        LogArrange("Criando data model e entity com FamilyId diferente");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        Guid expectedFamilyId = Guid.NewGuid();
        RefreshToken entity = CreateTestRefreshToken(familyId: expectedFamilyId);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que FamilyId foi atualizado");
        dataModel.FamilyId.ShouldBe(expectedFamilyId);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAt()
    {
        // Arrange
        LogArrange("Criando data model e entity com ExpiresAt diferente");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        DateTimeOffset expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(14);
        RefreshToken entity = CreateTestRefreshToken(expiresAt: expectedExpiresAt);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que ExpiresAt foi atualizado");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData(RefreshTokenStatus.Active, 1)]
    [InlineData(RefreshTokenStatus.Used, 2)]
    [InlineData(RefreshTokenStatus.Revoked, 3)]
    public void Adapt_ShouldUpdateStatus(RefreshTokenStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Criando data model e entity com Status {status}");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        RefreshToken entity = CreateTestRefreshToken(status: status);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert($"Verificando que Status foi atualizado para {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Adapt_ShouldUpdateRevokedAt_WhenNotNull()
    {
        // Arrange
        LogArrange("Criando data model e entity com RevokedAt preenchido");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        DateTimeOffset expectedRevokedAt = DateTimeOffset.UtcNow;
        RefreshToken entity = CreateTestRefreshToken(
            status: RefreshTokenStatus.Revoked,
            revokedAt: expectedRevokedAt);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que RevokedAt foi atualizado");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateRevokedAtToNull()
    {
        // Arrange
        LogArrange("Criando data model com RevokedAt preenchido e entity com null");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        dataModel.RevokedAt = DateTimeOffset.UtcNow;
        RefreshToken entity = CreateTestRefreshToken(revokedAt: null);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que RevokedAt foi atualizado para null");
        dataModel.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void Adapt_ShouldUpdateReplacedByTokenId_WhenNotNull()
    {
        // Arrange
        LogArrange("Criando data model e entity com ReplacedByTokenId preenchido");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        Guid expectedReplacedByTokenId = Guid.NewGuid();
        RefreshToken entity = CreateTestRefreshToken(
            status: RefreshTokenStatus.Used,
            replacedByTokenId: expectedReplacedByTokenId);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que ReplacedByTokenId foi atualizado");
        dataModel.ReplacedByTokenId.ShouldBe(expectedReplacedByTokenId);
    }

    [Fact]
    public void Adapt_ShouldUpdateReplacedByTokenIdToNull()
    {
        // Arrange
        LogArrange("Criando data model com ReplacedByTokenId preenchido e entity com null");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        dataModel.ReplacedByTokenId = Guid.NewGuid();
        RefreshToken entity = CreateTestRefreshToken(replacedByTokenId: null);

        // Act
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando que ReplacedByTokenId foi atualizado para null");
        dataModel.ReplacedByTokenId.ShouldBeNull();
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Criando data model e entity com EntityInfo especifico");
        RefreshTokenDataModel dataModel = CreateTestDataModel();
        Guid expectedId = Guid.NewGuid();
        Guid expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = "updated-creator";
        DateTimeOffset expectedCreatedAt = DateTimeOffset.UtcNow.AddHours(-2);
        long expectedVersion = 10;

        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(expectedId),
            tenantInfo: TenantInfo.Create(expectedTenantCode),
            createdAt: expectedCreatedAt,
            createdBy: expectedCreatedBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "UPDATE_TOKEN",
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
        LogAct("Chamando RefreshTokenDataModelAdapter.Adapt");
        RefreshTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verificando campos base do EntityInfo");
        dataModel.Id.ShouldBe(expectedId);
        dataModel.TenantCode.ShouldBe(expectedTenantCode);
        dataModel.CreatedBy.ShouldBe(expectedCreatedBy);
        dataModel.CreatedAt.ShouldBe(expectedCreatedAt);
        dataModel.EntityVersion.ShouldBe(expectedVersion);
    }

    #region Helper Methods

    private static RefreshTokenDataModel CreateTestDataModel()
    {
        return new RefreshTokenDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "original-creator",
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-5),
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "ORIGINAL_OP",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            TokenHash = Faker.Random.Bytes(32),
            FamilyId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            Status = 1,
            RevokedAt = null,
            ReplacedByTokenId = null
        };
    }

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
