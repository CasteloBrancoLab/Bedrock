using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
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

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using RefreshTokenMetadata = ShopDemo.Auth.Domain.Entities.RefreshTokens.RefreshToken.RefreshTokenMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RefreshTokens;

public class RefreshTokenTests : TestBase
{
    public RefreshTokenTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateRefreshToken()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);

        // Act
        LogAct("Registering new refresh token");
        var token = RefreshToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying refresh token was created successfully");
        token.ShouldNotBeNull();
        token.UserId.ShouldBe(userId);
        token.TokenHash.Value.Span.SequenceEqual(CreateValidTokenHashBytes()).ShouldBeTrue();
        token.FamilyId.ShouldBe(familyId);
        token.ExpiresAt.ShouldBe(expiresAt);
        token.Status.ShouldBe(RefreshTokenStatus.Active);
        token.RevokedAt.ShouldBeNull();
        token.ReplacedByTokenId.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new refresh token");
        var token = RefreshToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        token.ShouldNotBeNull();
        token.Status.ShouldBe(RefreshTokenStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new refresh token");
        var token = RefreshToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        token.ShouldNotBeNull();
        token.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldSetRevokedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new refresh token");
        var token = RefreshToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt is null");
        token.ShouldNotBeNull();
        token.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldSetReplacedByTokenIdToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new refresh token");
        var token = RefreshToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying ReplacedByTokenId is null");
        token.ShouldNotBeNull();
        token.ReplacedByTokenId.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithEmptyTokenHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty token hash");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var tokenHash = default(TokenHash);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);

        // Act
        LogAct("Registering new refresh token with empty hash");
        var token = RefreshToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        token.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithTokenHashExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with token hash exceeding max length");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        byte[] longHash = new byte[RefreshTokenMetadata.TokenHashMaxLength + 1];
        longHash[0] = 1;
        var tokenHash = TokenHash.CreateNew(longHash);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);

        // Act
        LogAct("Registering new refresh token with too-long hash");
        var token = RefreshToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        token.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateRefreshTokenWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing refresh token");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var status = RefreshTokenStatus.Active;
        var input = new CreateFromExistingInfoRefreshTokenInput(
            entityInfo, userId, tokenHash, familyId, expiresAt, status, null, null);

        // Act
        LogAct("Creating refresh token from existing info");
        var token = RefreshToken.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        token.EntityInfo.ShouldBe(entityInfo);
        token.UserId.ShouldBe(userId);
        token.TokenHash.Value.Span.SequenceEqual(CreateValidTokenHashBytes()).ShouldBeTrue();
        token.FamilyId.ShouldBe(familyId);
        token.ExpiresAt.ShouldBe(expiresAt);
        token.Status.ShouldBe(RefreshTokenStatus.Active);
        token.RevokedAt.ShouldBeNull();
        token.ReplacedByTokenId.ShouldBeNull();
    }

    [Fact]
    public void CreateFromExistingInfo_WithUsedStatus_ShouldPreserveReplacedByTokenId()
    {
        // Arrange
        LogArrange("Creating input with Used status and ReplacedByTokenId");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var replacedByTokenId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoRefreshTokenInput(
            entityInfo, userId, tokenHash, familyId, expiresAt,
            RefreshTokenStatus.Used, null, replacedByTokenId);

        // Act
        LogAct("Creating refresh token from existing info");
        var token = RefreshToken.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying Used status and ReplacedByTokenId");
        token.Status.ShouldBe(RefreshTokenStatus.Used);
        token.ReplacedByTokenId.ShouldBe(replacedByTokenId);
    }

    [Fact]
    public void CreateFromExistingInfo_WithRevokedStatus_ShouldPreserveRevokedAt()
    {
        // Arrange
        LogArrange("Creating input with Revoked status and RevokedAt");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var revokedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoRefreshTokenInput(
            entityInfo, userId, tokenHash, familyId, expiresAt,
            RefreshTokenStatus.Revoked, revokedAt, null);

        // Act
        LogAct("Creating refresh token from existing info");
        var token = RefreshToken.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying Revoked status and RevokedAt");
        token.Status.ShouldBe(RefreshTokenStatus.Revoked);
        token.RevokedAt.ShouldBe(revokedAt);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with empty TokenHash (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = default(TokenHash);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var input = new CreateFromExistingInfoRefreshTokenInput(
            entityInfo, userId, tokenHash, familyId, expiresAt,
            RefreshTokenStatus.Active, null, null);

        // Act
        LogAct("Creating refresh token from existing info with empty hash");
        var token = RefreshToken.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying token was created without validation");
        token.ShouldNotBeNull();
        token.TokenHash.IsEmpty.ShouldBeTrue();
    }

    #endregion

    #region MarkAsUsed Tests

    [Fact]
    public void MarkAsUsed_WithActiveToken_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var replacedByTokenId = Id.GenerateNewId();
        var input = new MarkAsUsedRefreshTokenInput(replacedByTokenId);

        // Act
        LogAct("Marking token as used");
        var result = token.MarkAsUsed(executionContext, input);

        // Assert
        LogAssert("Verifying token was marked as used");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(RefreshTokenStatus.Used);
        result.ReplacedByTokenId.ShouldBe(replacedByTokenId);
    }

    [Fact]
    public void MarkAsUsed_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var input = new MarkAsUsedRefreshTokenInput(Id.GenerateNewId());

        // Act
        LogAct("Marking token as used");
        var result = token.MarkAsUsed(executionContext, input);

        // Assert
        LogAssert("Verifying new instance was returned (clone-modify-return)");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(token);
        token.Status.ShouldBe(RefreshTokenStatus.Active);
        result.Status.ShouldBe(RefreshTokenStatus.Used);
    }

    [Fact]
    public void MarkAsUsed_WithUsedToken_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating used refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var firstReplacedId = Id.GenerateNewId();
        var usedToken = token.MarkAsUsed(executionContext, new MarkAsUsedRefreshTokenInput(firstReplacedId))!;
        var secondReplacedId = Id.GenerateNewId();
        var input = new MarkAsUsedRefreshTokenInput(secondReplacedId);

        // Act
        LogAct("Attempting to mark used token as used again");
        var newContext = CreateTestExecutionContext();
        var result = usedToken.MarkAsUsed(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Used -> Used transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void MarkAsUsed_WithRevokedToken_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating revoked refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var revokedToken = token.Revoke(executionContext, new RevokeRefreshTokenInput())!;
        var input = new MarkAsUsedRefreshTokenInput(Id.GenerateNewId());

        // Act
        LogAct("Attempting to mark revoked token as used");
        var newContext = CreateTestExecutionContext();
        var result = revokedToken.MarkAsUsed(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Revoked -> Used transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_WithActiveToken_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var input = new RevokeRefreshTokenInput();

        // Act
        LogAct("Revoking token");
        var result = token.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying token was revoked");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(RefreshTokenStatus.Revoked);
        result.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var input = new RevokeRefreshTokenInput();

        // Act
        LogAct("Revoking token");
        var result = token.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.RevokedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void Revoke_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var input = new RevokeRefreshTokenInput();

        // Act
        LogAct("Revoking token");
        var result = token.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(token);
        token.Status.ShouldBe(RefreshTokenStatus.Active);
        result.Status.ShouldBe(RefreshTokenStatus.Revoked);
    }

    [Fact]
    public void Revoke_WithUsedToken_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating used refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var usedToken = token.MarkAsUsed(executionContext, new MarkAsUsedRefreshTokenInput(Id.GenerateNewId()))!;
        var input = new RevokeRefreshTokenInput();

        // Act
        LogAct("Attempting to revoke used token");
        var newContext = CreateTestExecutionContext();
        var result = usedToken.Revoke(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Used -> Revoked transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void Revoke_WithRevokedToken_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating revoked refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);
        var revokedToken = token.Revoke(executionContext, new RevokeRefreshTokenInput())!;
        var input = new RevokeRefreshTokenInput();

        // Act
        LogAct("Attempting to revoke already revoked token");
        var newContext = CreateTestExecutionContext();
        var result = revokedToken.Revoke(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Revoked -> Revoked transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);

        // Act
        LogAct("Cloning refresh token");
        var clone = token.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(token);
        clone.UserId.ShouldBe(token.UserId);
        clone.FamilyId.ShouldBe(token.FamilyId);
        clone.ExpiresAt.ShouldBe(token.ExpiresAt);
        clone.Status.ShouldBe(token.Status);
        clone.RevokedAt.ShouldBe(token.RevokedAt);
        clone.ReplacedByTokenId.ShouldBe(token.ReplacedByTokenId);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid refresh token");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestRefreshToken(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = token.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating token with invalid state via CreateFromExistingInfo (empty TokenHash)");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoRefreshTokenInput(
            entityInfo, Id.GenerateNewId(), default(TokenHash),
            TokenFamily.CreateNew(), DateTimeOffset.UtcNow.AddDays(7),
            RefreshTokenStatus.Active, null, null);
        var token = RefreshToken.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on token with empty TokenHash");
        bool result = token.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty TokenHash");
        result.ShouldBeFalse();
        validationContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateUserId Tests

    [Fact]
    public void ValidateUserId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        // Act
        LogAct("Validating valid UserId");
        bool result = RefreshToken.ValidateUserId(executionContext, userId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUserId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null UserId");
        bool result = RefreshToken.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateTokenHash Tests

    [Fact]
    public void ValidateTokenHash_WithValidHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var hash = TokenHash.CreateNew(CreateValidTokenHashBytes());

        // Act
        LogAct("Validating valid token hash");
        bool result = RefreshToken.ValidateTokenHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTokenHash_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null token hash");
        bool result = RefreshToken.ValidateTokenHash(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTokenHash_WithEmptyValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var hash = TokenHash.CreateNew([]);

        // Act
        LogAct("Validating empty token hash");
        bool result = RefreshToken.ValidateTokenHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation fails for empty hash");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTokenHash_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating hash at max length");
        var executionContext = CreateTestExecutionContext();
        byte[] hashBytes = new byte[RefreshTokenMetadata.TokenHashMaxLength];
        hashBytes[0] = 1;
        var hash = TokenHash.CreateNew(hashBytes);

        // Act
        LogAct("Validating max-length token hash");
        bool result = RefreshToken.ValidateTokenHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTokenHash_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating hash exceeding max length");
        var executionContext = CreateTestExecutionContext();
        byte[] hashBytes = new byte[RefreshTokenMetadata.TokenHashMaxLength + 1];
        hashBytes[0] = 1;
        var hash = TokenHash.CreateNew(hashBytes);

        // Act
        LogAct("Validating too-long token hash");
        bool result = RefreshToken.ValidateTokenHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateFamilyId Tests

    [Fact]
    public void ValidateFamilyId_WithValidFamily_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var familyId = TokenFamily.CreateNew();

        // Act
        LogAct("Validating valid FamilyId");
        bool result = RefreshToken.ValidateFamilyId(executionContext, familyId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateFamilyId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null FamilyId");
        bool result = RefreshToken.ValidateFamilyId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateExpiresAt Tests

    [Fact]
    public void ValidateExpiresAt_WithValidDate_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Validating valid ExpiresAt");
        bool result = RefreshToken.ValidateExpiresAt(executionContext, expiresAt);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateExpiresAt_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ExpiresAt");
        bool result = RefreshToken.ValidateExpiresAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(RefreshTokenStatus.Active)]
    [InlineData(RefreshTokenStatus.Used)]
    [InlineData(RefreshTokenStatus.Revoked)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(RefreshTokenStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = RefreshToken.ValidateStatus(executionContext, status);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatus_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null status");
        bool result = RefreshToken.ValidateStatus(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Theory]
    [InlineData(RefreshTokenStatus.Active, RefreshTokenStatus.Used)]
    [InlineData(RefreshTokenStatus.Active, RefreshTokenStatus.Revoked)]
    public void ValidateStatusTransition_ValidTransitions_ShouldReturnTrue(
        RefreshTokenStatus from, RefreshTokenStatus to)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating transition {from} -> {to}");
        bool result = RefreshToken.ValidateStatusTransition(executionContext, from, to);

        // Assert
        LogAssert("Verifying transition is valid");
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RefreshTokenStatus.Used, RefreshTokenStatus.Active)]
    [InlineData(RefreshTokenStatus.Used, RefreshTokenStatus.Revoked)]
    [InlineData(RefreshTokenStatus.Revoked, RefreshTokenStatus.Active)]
    [InlineData(RefreshTokenStatus.Revoked, RefreshTokenStatus.Used)]
    public void ValidateStatusTransition_InvalidTransitions_ShouldReturnFalse(
        RefreshTokenStatus from, RefreshTokenStatus to)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating transition {from} -> {to}");
        bool result = RefreshToken.ValidateStatusTransition(executionContext, from, to);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RefreshTokenStatus.Active)]
    [InlineData(RefreshTokenStatus.Used)]
    [InlineData(RefreshTokenStatus.Revoked)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(RefreshTokenStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = RefreshToken.ValidateStatusTransition(executionContext, status, status);

        // Assert
        LogAssert("Verifying same-status transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_WithNullFrom_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null -> Active transition");
        bool result = RefreshToken.ValidateStatusTransition(executionContext, null, RefreshTokenStatus.Active);

        // Assert
        LogAssert("Verifying null from is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_WithNullTo_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> null transition");
        bool result = RefreshToken.ValidateStatusTransition(executionContext, RefreshTokenStatus.Active, null);

        // Assert
        LogAssert("Verifying null to is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_ActiveToUndefinedEnumValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> undefined enum value transition");
        bool result = RefreshToken.ValidateStatusTransition(
            executionContext, RefreshTokenStatus.Active, (RefreshTokenStatus)99);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Static IsValid Tests

    [Fact]
    public void IsValid_WithAllValidFields_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating all valid fields");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.GenerateNewId();
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var status = RefreshTokenStatus.Active;

        // Act
        LogAct("Calling IsValid");
        bool result = RefreshToken.IsValid(
            executionContext, entityInfo, userId, tokenHash, familyId, expiresAt, status);

        // Assert
        LogAssert("Verifying all fields are valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullUserId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null UserId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Calling IsValid with null UserId");
        bool result = RefreshToken.IsValid(
            executionContext, entityInfo, null, tokenHash, familyId, expiresAt, RefreshTokenStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullTokenHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null TokenHash");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.GenerateNewId();
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Calling IsValid with null TokenHash");
        bool result = RefreshToken.IsValid(
            executionContext, entityInfo, userId, null, familyId, expiresAt, RefreshTokenStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullFamilyId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null FamilyId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.GenerateNewId();
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Calling IsValid with null FamilyId");
        bool result = RefreshToken.IsValid(
            executionContext, entityInfo, userId, tokenHash, null, expiresAt, RefreshTokenStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null ExpiresAt");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.GenerateNewId();
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();

        // Act
        LogAct("Calling IsValid with null ExpiresAt");
        bool result = RefreshToken.IsValid(
            executionContext, entityInfo, userId, tokenHash, familyId, null, RefreshTokenStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullStatus_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Status");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.GenerateNewId();
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Calling IsValid with null Status");
        bool result = RefreshToken.IsValid(
            executionContext, entityInfo, userId, tokenHash, familyId, expiresAt, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var timeProvider = TimeProvider.System;

        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);
    }

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

    private static RefreshToken CreateTestRefreshToken(ExecutionContext executionContext)
    {
        var userId = Id.GenerateNewId();
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);
        return RefreshToken.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewRefreshTokenInput CreateValidRegisterNewInput()
    {
        var userId = Id.GenerateNewId();
        var tokenHash = TokenHash.CreateNew(CreateValidTokenHashBytes());
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        return new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);
    }

    private static byte[] CreateValidTokenHashBytes()
    {
        byte[] bytes = new byte[32]; // SHA-256 = 32 bytes
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)((i + 1) % 256);
        }
        return bytes;
    }

    #endregion
}
