using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class DPoPKeyFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public DPoPKeyFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel with specific UserId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedUserId, "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), (short)DPoPKeyStatus.Active, null);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapJwkThumbprintFromDataModel()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel with specific JwkThumbprint");
        string expectedJwkThumbprint = Faker.Random.String2(43);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedJwkThumbprint, "{}", DateTimeOffset.UtcNow.AddHours(1), (short)DPoPKeyStatus.Active, null);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying JwkThumbprint mapping");
        entity.JwkThumbprint.Value.ShouldBe(expectedJwkThumbprint);
    }

    [Fact]
    public void Create_ShouldMapPublicKeyJwkFromDataModel()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel with specific PublicKeyJwk");
        string expectedPublicKeyJwk = "{\"kty\":\"EC\",\"crv\":\"P-256\"}";
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "thumbprint-abc", expectedPublicKeyJwk, DateTimeOffset.UtcNow.AddHours(1), (short)DPoPKeyStatus.Active, null);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying PublicKeyJwk mapping");
        entity.PublicKeyJwk.ShouldBe(expectedPublicKeyJwk);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel with specific ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(48);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "thumbprint-abc", "{}", expectedExpiresAt, (short)DPoPKeyStatus.Active, null);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData((short)1, DPoPKeyStatus.Active)]
    [InlineData((short)2, DPoPKeyStatus.Revoked)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, DPoPKeyStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating DPoPKeyDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), statusValue, null);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel with specific RevokedAt");
        DateTimeOffset? expectedRevokedAt = DateTimeOffset.UtcNow.AddMinutes(-15);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), (short)DPoPKeyStatus.Revoked, expectedRevokedAt);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        entity.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel with specific base fields");
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

        var dataModel = new DPoPKeyDataModel
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
            JwkThumbprint = "thumbprint-abc",
            PublicKeyJwk = "{}",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Status = (short)DPoPKeyStatus.Active,
            RevokedAt = null
        };

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

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
        LogArrange("Creating DPoPKeyDataModel with null last-changed fields");
        var dataModel = new DPoPKeyDataModel
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
            JwkThumbprint = "thumbprint-abc",
            PublicKeyJwk = "{}",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Status = (short)DPoPKeyStatus.Active,
            RevokedAt = null
        };

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel with nulls");
        var entity = DPoPKeyFactory.Create(dataModel);

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
        LogArrange("Creating DPoPKeyDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), (short)DPoPKeyStatus.Active, null);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), (short)DPoPKeyStatus.Active, null);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "thumbprint-abc", "{}", DateTimeOffset.UtcNow.AddHours(1), (short)DPoPKeyStatus.Active, null);

        // Act
        LogAct("Creating DPoPKey from DPoPKeyDataModel");
        var entity = DPoPKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static DPoPKeyDataModel CreateTestDataModel(
        Guid userId,
        string jwkThumbprint,
        string publicKeyJwk,
        DateTimeOffset expiresAt,
        short status,
        DateTimeOffset? revokedAt)
    {
        return new DPoPKeyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_DPOP_KEY",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = userId,
            JwkThumbprint = jwkThumbprint,
            PublicKeyJwk = publicKeyJwk,
            ExpiresAt = expiresAt,
            Status = status,
            RevokedAt = revokedAt
        };
    }

    #endregion
}
