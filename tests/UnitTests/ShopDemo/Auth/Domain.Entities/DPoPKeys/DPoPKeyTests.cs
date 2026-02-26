using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using DPoPKeyMetadata = ShopDemo.Auth.Domain.Entities.DPoPKeys.DPoPKey.DPoPKeyMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.DPoPKeys;

public class DPoPKeyTests : TestBase
{
    public DPoPKeyTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateDPoPKey()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var jwkThumbprint = JwkThumbprint.CreateNew("NzbLsXh8uDCcd-6MNwXF4W_7noWXFZAfHkxZsRGC9Xs");
        var publicKeyJwk = "{\"kty\":\"EC\",\"crv\":\"P-256\"}";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
        var input = new RegisterNewDPoPKeyInput(userId, jwkThumbprint, publicKeyJwk, expiresAt);

        // Act
        LogAct("Registering new DPoP key");
        var dpopKey = DPoPKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying DPoP key was created successfully");
        dpopKey.ShouldNotBeNull();
        dpopKey.UserId.ShouldBe(userId);
        dpopKey.JwkThumbprint.ShouldBe(jwkThumbprint);
        dpopKey.PublicKeyJwk.ShouldBe(publicKeyJwk);
        dpopKey.ExpiresAt.ShouldBe(expiresAt);
        dpopKey.Status.ShouldBe(DPoPKeyStatus.Active);
        dpopKey.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new DPoP key");
        var dpopKey = DPoPKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        dpopKey.ShouldNotBeNull();
        dpopKey.Status.ShouldBe(DPoPKeyStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new DPoP key");
        var dpopKey = DPoPKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        dpopKey.ShouldNotBeNull();
        dpopKey.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldSetRevokedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new DPoP key");
        var dpopKey = DPoPKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt is null");
        dpopKey.ShouldNotBeNull();
        dpopKey.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithEmptyPublicKeyJwk_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty PublicKeyJwk");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewDPoPKeyInput(
            Id.GenerateNewId(),
            JwkThumbprint.CreateNew("thumbprint"),
            "",
            DateTimeOffset.UtcNow.AddHours(24));

        // Act
        LogAct("Registering new DPoP key with empty PublicKeyJwk");
        var dpopKey = DPoPKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        dpopKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullPublicKeyJwk_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null PublicKeyJwk");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewDPoPKeyInput(
            Id.GenerateNewId(),
            JwkThumbprint.CreateNew("thumbprint"),
            null!,
            DateTimeOffset.UtcNow.AddHours(24));

        // Act
        LogAct("Registering new DPoP key with null PublicKeyJwk");
        var dpopKey = DPoPKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        dpopKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithPublicKeyJwkExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with PublicKeyJwk exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longJwk = new('x', DPoPKeyMetadata.PublicKeyJwkMaxLength + 1);
        var input = new RegisterNewDPoPKeyInput(
            Id.GenerateNewId(),
            JwkThumbprint.CreateNew("thumbprint"),
            longJwk,
            DateTimeOffset.UtcNow.AddHours(24));

        // Act
        LogAct("Registering new DPoP key with too-long PublicKeyJwk");
        var dpopKey = DPoPKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        dpopKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateDPoPKeyWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing DPoP key");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var jwkThumbprint = JwkThumbprint.CreateFromExistingInfo("thumbprint_value");
        var publicKeyJwk = "{\"kty\":\"EC\",\"crv\":\"P-256\"}";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
        var revokedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoDPoPKeyInput(
            entityInfo, userId, jwkThumbprint, publicKeyJwk, expiresAt,
            DPoPKeyStatus.Revoked, revokedAt);

        // Act
        LogAct("Creating DPoP key from existing info");
        var dpopKey = DPoPKey.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        dpopKey.EntityInfo.ShouldBe(entityInfo);
        dpopKey.UserId.ShouldBe(userId);
        dpopKey.JwkThumbprint.ShouldBe(jwkThumbprint);
        dpopKey.PublicKeyJwk.ShouldBe(publicKeyJwk);
        dpopKey.ExpiresAt.ShouldBe(expiresAt);
        dpopKey.Status.ShouldBe(DPoPKeyStatus.Revoked);
        dpopKey.RevokedAt.ShouldBe(revokedAt);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with empty PublicKeyJwk (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoDPoPKeyInput(
            entityInfo, Id.GenerateNewId(), JwkThumbprint.CreateNew("t"), "",
            DateTimeOffset.UtcNow.AddHours(24), DPoPKeyStatus.Active, null);

        // Act
        LogAct("Creating DPoP key from existing info with empty PublicKeyJwk");
        var dpopKey = DPoPKey.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying DPoP key was created without validation");
        dpopKey.ShouldNotBeNull();
        dpopKey.PublicKeyJwk.ShouldBe("");
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_WithActiveDPoPKey_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active DPoP key");
        var executionContext = CreateTestExecutionContext();
        var dpopKey = CreateTestDPoPKey(executionContext);
        var input = new RevokeDPoPKeyInput();

        // Act
        LogAct("Revoking DPoP key");
        var result = dpopKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying DPoP key was revoked");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(DPoPKeyStatus.Revoked);
        result.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active DPoP key");
        var executionContext = CreateTestExecutionContext();
        var dpopKey = CreateTestDPoPKey(executionContext);
        var input = new RevokeDPoPKeyInput();

        // Act
        LogAct("Revoking DPoP key");
        var result = dpopKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.RevokedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void Revoke_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active DPoP key");
        var executionContext = CreateTestExecutionContext();
        var dpopKey = CreateTestDPoPKey(executionContext);
        var input = new RevokeDPoPKeyInput();

        // Act
        LogAct("Revoking DPoP key");
        var result = dpopKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(dpopKey);
        dpopKey.Status.ShouldBe(DPoPKeyStatus.Active);
        result.Status.ShouldBe(DPoPKeyStatus.Revoked);
    }

    [Fact]
    public void Revoke_WithRevokedDPoPKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating revoked DPoP key");
        var executionContext = CreateTestExecutionContext();
        var dpopKey = CreateTestDPoPKey(executionContext);
        var revokedKey = dpopKey.Revoke(executionContext, new RevokeDPoPKeyInput())!;
        var input = new RevokeDPoPKeyInput();

        // Act
        LogAct("Attempting to revoke already revoked DPoP key");
        var newContext = CreateTestExecutionContext();
        var result = revokedKey.Revoke(newContext, input);

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
        LogArrange("Creating DPoP key");
        var executionContext = CreateTestExecutionContext();
        var dpopKey = CreateTestDPoPKey(executionContext);

        // Act
        LogAct("Cloning DPoP key");
        var clone = dpopKey.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(dpopKey);
        clone.UserId.ShouldBe(dpopKey.UserId);
        clone.JwkThumbprint.ShouldBe(dpopKey.JwkThumbprint);
        clone.PublicKeyJwk.ShouldBe(dpopKey.PublicKeyJwk);
        clone.ExpiresAt.ShouldBe(dpopKey.ExpiresAt);
        clone.Status.ShouldBe(dpopKey.Status);
        clone.RevokedAt.ShouldBe(dpopKey.RevokedAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidDPoPKey_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid DPoP key");
        var executionContext = CreateTestExecutionContext();
        var dpopKey = CreateTestDPoPKey(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = dpopKey.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidDPoPKey_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating DPoP key with invalid state via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoDPoPKeyInput(
            entityInfo, Id.GenerateNewId(), JwkThumbprint.CreateNew("t"), "",
            DateTimeOffset.UtcNow.AddHours(24), DPoPKeyStatus.Active, null);
        var dpopKey = DPoPKey.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on DPoP key with empty PublicKeyJwk");
        bool result = dpopKey.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty PublicKeyJwk");
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
        bool result = DPoPKey.ValidateUserId(executionContext, userId);

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
        bool result = DPoPKey.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateJwkThumbprint Tests

    [Fact]
    public void ValidateJwkThumbprint_WithValidThumbprint_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var thumbprint = JwkThumbprint.CreateNew("NzbLsXh8uDCcd-6MNwXF4W_7noWXFZAfHkxZsRGC9Xs");

        // Act
        LogAct("Validating valid JwkThumbprint");
        bool result = DPoPKey.ValidateJwkThumbprint(executionContext, thumbprint);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateJwkThumbprint_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null JwkThumbprint");
        bool result = DPoPKey.ValidateJwkThumbprint(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidatePublicKeyJwk Tests

    [Fact]
    public void ValidatePublicKeyJwk_WithValidJwk_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid PublicKeyJwk");
        bool result = DPoPKey.ValidatePublicKeyJwk(executionContext, "{\"kty\":\"EC\"}");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublicKeyJwk_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null PublicKeyJwk");
        bool result = DPoPKey.ValidatePublicKeyJwk(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublicKeyJwk_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty PublicKeyJwk");
        bool result = DPoPKey.ValidatePublicKeyJwk(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublicKeyJwk_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating PublicKeyJwk at max length");
        var executionContext = CreateTestExecutionContext();
        string jwk = new('x', DPoPKeyMetadata.PublicKeyJwkMaxLength);

        // Act
        LogAct("Validating max-length PublicKeyJwk");
        bool result = DPoPKey.ValidatePublicKeyJwk(executionContext, jwk);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublicKeyJwk_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PublicKeyJwk exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string jwk = new('x', DPoPKeyMetadata.PublicKeyJwkMaxLength + 1);

        // Act
        LogAct("Validating too-long PublicKeyJwk");
        bool result = DPoPKey.ValidatePublicKeyJwk(executionContext, jwk);

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
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        // Act
        LogAct("Validating valid ExpiresAt");
        bool result = DPoPKey.ValidateExpiresAt(executionContext, expiresAt);

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
        bool result = DPoPKey.ValidateExpiresAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(DPoPKeyStatus.Active)]
    [InlineData(DPoPKeyStatus.Revoked)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(DPoPKeyStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = DPoPKey.ValidateStatus(executionContext, status);

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
        bool result = DPoPKey.ValidateStatus(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Fact]
    public void ValidateStatusTransition_ActiveToRevoked_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> Revoked transition");
        bool result = DPoPKey.ValidateStatusTransition(executionContext, DPoPKeyStatus.Active, DPoPKeyStatus.Revoked);

        // Assert
        LogAssert("Verifying transition is valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_RevokedToActive_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Revoked -> Active transition");
        bool result = DPoPKey.ValidateStatusTransition(executionContext, DPoPKeyStatus.Revoked, DPoPKeyStatus.Active);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(DPoPKeyStatus.Active)]
    [InlineData(DPoPKeyStatus.Revoked)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(DPoPKeyStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = DPoPKey.ValidateStatusTransition(executionContext, status, status);

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
        LogAct("Validating null -> Revoked transition");
        bool result = DPoPKey.ValidateStatusTransition(executionContext, null, DPoPKeyStatus.Revoked);

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
        bool result = DPoPKey.ValidateStatusTransition(executionContext, DPoPKeyStatus.Active, null);

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
        bool result = DPoPKey.ValidateStatusTransition(executionContext, DPoPKeyStatus.Active, (DPoPKeyStatus)99);

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
        var jwkThumbprint = JwkThumbprint.CreateNew("thumbprint");
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        // Act
        LogAct("Calling IsValid");
        bool result = DPoPKey.IsValid(
            executionContext, entityInfo, userId, jwkThumbprint, "{\"kty\":\"EC\"}", expiresAt, DPoPKeyStatus.Active);

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

        // Act
        LogAct("Calling IsValid with null UserId");
        bool result = DPoPKey.IsValid(
            executionContext, entityInfo, null, JwkThumbprint.CreateNew("t"), "{\"kty\":\"EC\"}",
            DateTimeOffset.UtcNow.AddHours(24), DPoPKeyStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullJwkThumbprint_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null JwkThumbprint");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null JwkThumbprint");
        bool result = DPoPKey.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), null, "{\"kty\":\"EC\"}",
            DateTimeOffset.UtcNow.AddHours(24), DPoPKeyStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullPublicKeyJwk_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null PublicKeyJwk");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null PublicKeyJwk");
        bool result = DPoPKey.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), JwkThumbprint.CreateNew("t"), null,
            DateTimeOffset.UtcNow.AddHours(24), DPoPKeyStatus.Active);

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

        // Act
        LogAct("Calling IsValid with null ExpiresAt");
        bool result = DPoPKey.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), JwkThumbprint.CreateNew("t"), "{\"kty\":\"EC\"}",
            null, DPoPKeyStatus.Active);

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

        // Act
        LogAct("Calling IsValid with null Status");
        bool result = DPoPKey.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), JwkThumbprint.CreateNew("t"), "{\"kty\":\"EC\"}",
            DateTimeOffset.UtcNow.AddHours(24), null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Change Tests

    [Fact]
    public void ChangeUserIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = DPoPKeyMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata");
            DPoPKeyMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            DPoPKeyMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            DPoPKeyMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangeJwkThumbprintMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = DPoPKeyMetadata.JwkThumbprintIsRequired;

        try
        {
            // Act
            LogAct("Changing JwkThumbprint metadata");
            DPoPKeyMetadata.ChangeJwkThumbprintMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            DPoPKeyMetadata.JwkThumbprintIsRequired.ShouldBeFalse();
        }
        finally
        {
            DPoPKeyMetadata.ChangeJwkThumbprintMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangePublicKeyJwkMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = DPoPKeyMetadata.PublicKeyJwkIsRequired;
        int originalMaxLength = DPoPKeyMetadata.PublicKeyJwkMaxLength;

        try
        {
            // Act
            LogAct("Changing PublicKeyJwk metadata");
            DPoPKeyMetadata.ChangePublicKeyJwkMetadata(isRequired: false, maxLength: 8192);

            // Assert
            LogAssert("Verifying metadata was updated");
            DPoPKeyMetadata.PublicKeyJwkIsRequired.ShouldBeFalse();
            DPoPKeyMetadata.PublicKeyJwkMaxLength.ShouldBe(8192);
        }
        finally
        {
            DPoPKeyMetadata.ChangePublicKeyJwkMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void ChangeExpiresAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = DPoPKeyMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata");
            DPoPKeyMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            DPoPKeyMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            DPoPKeyMetadata.ChangeExpiresAtMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = DPoPKeyMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata");
            DPoPKeyMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            DPoPKeyMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            DPoPKeyMetadata.ChangeStatusMetadata(isRequired: originalIsRequired);
        }
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
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

    private static DPoPKey CreateTestDPoPKey(ExecutionContext executionContext)
    {
        var input = CreateValidRegisterNewInput();
        return DPoPKey.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewDPoPKeyInput CreateValidRegisterNewInput()
    {
        return new RegisterNewDPoPKeyInput(
            Id.GenerateNewId(),
            JwkThumbprint.CreateNew("NzbLsXh8uDCcd-6MNwXF4W_7noWXFZAfHkxZsRGC9Xs"),
            "{\"kty\":\"EC\",\"crv\":\"P-256\"}",
            DateTimeOffset.UtcNow.AddHours(24));
    }

    #endregion
}
