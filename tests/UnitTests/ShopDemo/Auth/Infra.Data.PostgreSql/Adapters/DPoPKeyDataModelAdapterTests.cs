using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class DPoPKeyDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public DPoPKeyDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel and DPoPKey with different UserIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), DPoPKeyStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        DPoPKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateJwkThumbprintFromEntity()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel and DPoPKey with different JwkThumbprints");
        var dataModel = CreateTestDataModel();
        string expectedJwkThumbprint = Faker.Random.String2(43);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedJwkThumbprint, "{}", DateTimeOffset.UtcNow.AddHours(1), DPoPKeyStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        DPoPKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying JwkThumbprint was updated");
        dataModel.JwkThumbprint.ShouldBe(expectedJwkThumbprint);
    }

    [Fact]
    public void Adapt_ShouldUpdatePublicKeyJwkFromEntity()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel and DPoPKey with different PublicKeyJwks");
        var dataModel = CreateTestDataModel();
        string expectedPublicKeyJwk = "{\"kty\":\"RSA\"}";
        var entity = CreateTestEntity(Guid.NewGuid(), "thumbprint-abc", expectedPublicKeyJwk, DateTimeOffset.UtcNow.AddHours(1), DPoPKeyStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        DPoPKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying PublicKeyJwk was updated");
        dataModel.PublicKeyJwk.ShouldBe(expectedPublicKeyJwk);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel and DPoPKey with different ExpiresAt values");
        var dataModel = CreateTestDataModel();
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(3);
        var entity = CreateTestEntity(Guid.NewGuid(), "thumbprint-abc", "{}", expectedExpiresAt, DPoPKeyStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        DPoPKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel and DPoPKey with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)DPoPKeyStatus.Active;
        var entity = CreateTestEntity(Guid.NewGuid(), "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), DPoPKeyStatus.Revoked, null);

        // Act
        LogAct("Adapting data model from entity");
        DPoPKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)DPoPKeyStatus.Revoked);
    }

    [Fact]
    public void Adapt_ShouldUpdateRevokedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel and DPoPKey with different RevokedAt values");
        var dataModel = CreateTestDataModel();
        DateTimeOffset? expectedRevokedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var entity = CreateTestEntity(Guid.NewGuid(), "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), DPoPKeyStatus.Revoked, expectedRevokedAt);

        // Act
        LogAct("Adapting data model from entity");
        DPoPKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying RevokedAt was updated");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel and DPoPKey with different EntityInfo values");
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
        LogAct("Adapting data model from entity");
        DPoPKeyDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating DPoPKeyDataModel and DPoPKey");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), DPoPKeyStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        var result = DPoPKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static DPoPKeyDataModel CreateTestDataModel()
    {
        return new DPoPKeyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            JwkThumbprint = "initial-thumbprint",
            PublicKeyJwk = "{\"initial\":true}",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            Status = (short)DPoPKeyStatus.Active,
            RevokedAt = null
        };
    }

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
