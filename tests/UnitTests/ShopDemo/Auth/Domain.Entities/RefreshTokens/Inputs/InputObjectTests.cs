using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RefreshTokens.Inputs;

public class InputObjectTests : TestBase
{
    public InputObjectTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNewRefreshTokenInput Tests

    [Fact]
    public void RegisterNewRefreshTokenInput_ShouldStoreUserId()
    {
        // Arrange
        LogArrange("Creating UserId");
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew([1, 2, 3]);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Creating RegisterNewRefreshTokenInput");
        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);

        // Assert
        LogAssert("Verifying UserId is stored");
        input.UserId.ShouldBe(userId);
    }

    [Fact]
    public void RegisterNewRefreshTokenInput_ShouldStoreTokenHash()
    {
        // Arrange
        LogArrange("Creating TokenHash");
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        byte[] hashBytes = [10, 20, 30];
        var tokenHash = TokenHash.CreateNew(hashBytes);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Creating RegisterNewRefreshTokenInput");
        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);

        // Assert
        LogAssert("Verifying TokenHash is stored");
        input.TokenHash.Value.Span.SequenceEqual(hashBytes).ShouldBeTrue();
    }

    [Fact]
    public void RegisterNewRefreshTokenInput_ShouldStoreFamilyId()
    {
        // Arrange
        LogArrange("Creating FamilyId");
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew([1, 2, 3]);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Creating RegisterNewRefreshTokenInput");
        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);

        // Assert
        LogAssert("Verifying FamilyId is stored");
        input.FamilyId.ShouldBe(familyId);
    }

    [Fact]
    public void RegisterNewRefreshTokenInput_ShouldStoreExpiresAt()
    {
        // Arrange
        LogArrange("Creating ExpiresAt");
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew([1, 2, 3]);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Creating RegisterNewRefreshTokenInput");
        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);

        // Assert
        LogAssert("Verifying ExpiresAt is stored");
        input.ExpiresAt.ShouldBe(expiresAt);
    }

    [Fact]
    public void RegisterNewRefreshTokenInput_Equality_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two identical inputs");
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        byte[] hashBytes = [1, 2, 3];
        var hash1 = TokenHash.CreateNew(hashBytes);
        var hash2 = TokenHash.CreateNew(hashBytes);
        var familyId = TokenFamily.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var input1 = new RegisterNewRefreshTokenInput(userId, hash1, familyId, expiresAt);
        var input2 = new RegisterNewRefreshTokenInput(userId, hash2, familyId, expiresAt);

        // Act
        LogAct("Comparing inputs");
        bool result = input1 == input2;

        // Assert
        LogAssert("Verifying record struct equality");
        result.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfoRefreshTokenInput Tests

    [Fact]
    public void CreateFromExistingInfoRefreshTokenInput_ShouldStoreAllProperties()
    {
        // Arrange
        LogArrange("Creating all input properties");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew([1, 2, 3]);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var status = RefreshTokenStatus.Active;
        DateTimeOffset? revokedAt = null;
        Id? replacedByTokenId = null;

        // Act
        LogAct("Creating CreateFromExistingInfoRefreshTokenInput");
        var input = new CreateFromExistingInfoRefreshTokenInput(
            entityInfo, userId, tokenHash, familyId, expiresAt, status, revokedAt, replacedByTokenId);

        // Assert
        LogAssert("Verifying all properties are stored");
        input.EntityInfo.ShouldBe(entityInfo);
        input.UserId.ShouldBe(userId);
        input.TokenHash.Value.Span.SequenceEqual(new byte[] { 1, 2, 3 }).ShouldBeTrue();
        input.FamilyId.ShouldBe(familyId);
        input.ExpiresAt.ShouldBe(expiresAt);
        input.Status.ShouldBe(RefreshTokenStatus.Active);
        input.RevokedAt.ShouldBeNull();
        input.ReplacedByTokenId.ShouldBeNull();
    }

    [Fact]
    public void CreateFromExistingInfoRefreshTokenInput_WithRevokedFields_ShouldStoreAll()
    {
        // Arrange
        LogArrange("Creating input with revoked fields populated");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew([1, 2, 3]);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var revokedAt = DateTimeOffset.UtcNow;
        var replacedByTokenId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Creating CreateFromExistingInfoRefreshTokenInput with revoked data");
        var input = new CreateFromExistingInfoRefreshTokenInput(
            entityInfo, userId, tokenHash, familyId, expiresAt, RefreshTokenStatus.Used, revokedAt, replacedByTokenId);

        // Assert
        LogAssert("Verifying revoked fields are stored");
        input.Status.ShouldBe(RefreshTokenStatus.Used);
        input.RevokedAt.ShouldBe(revokedAt);
        input.ReplacedByTokenId.ShouldBe(replacedByTokenId);
    }

    #endregion

    #region MarkAsUsedRefreshTokenInput Tests

    [Fact]
    public void MarkAsUsedRefreshTokenInput_ShouldStoreReplacedByTokenId()
    {
        // Arrange
        LogArrange("Creating ReplacedByTokenId");
        var replacedByTokenId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Creating MarkAsUsedRefreshTokenInput");
        var input = new MarkAsUsedRefreshTokenInput(replacedByTokenId);

        // Assert
        LogAssert("Verifying ReplacedByTokenId is stored");
        input.ReplacedByTokenId.ShouldBe(replacedByTokenId);
    }

    #endregion

    #region RevokeRefreshTokenInput Tests

    [Fact]
    public void RevokeRefreshTokenInput_ShouldBeCreatable()
    {
        // Arrange & Act
        LogAct("Creating RevokeRefreshTokenInput");
        var input = new RevokeRefreshTokenInput();

        // Assert
        LogAssert("Verifying input was created (parameterless struct)");
        input.ShouldBe(default(RevokeRefreshTokenInput));
    }

    [Fact]
    public void RevokeRefreshTokenInput_Equality_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two instances");
        var input1 = new RevokeRefreshTokenInput();
        var input2 = new RevokeRefreshTokenInput();

        // Act
        LogAct("Comparing inputs");
        bool result = input1 == input2;

        // Assert
        LogAssert("Verifying record struct equality");
        result.ShouldBeTrue();
    }

    #endregion

    #region Helper Methods

    private static EntityInfo CreateTestEntityInfo()
    {
        return EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow,
                createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(),
                createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null,
                lastChangedBy: null,
                lastChangedCorrelationId: null,
                lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));
    }

    #endregion
}
