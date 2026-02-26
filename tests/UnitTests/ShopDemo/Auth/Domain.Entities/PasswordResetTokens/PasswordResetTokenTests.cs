using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using PasswordResetTokenMetadata = ShopDemo.Auth.Domain.Entities.PasswordResetTokens.PasswordResetToken.PasswordResetTokenMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.PasswordResetTokens;

public class PasswordResetTokenTests : TestBase
{
    public PasswordResetTokenTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid properties");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = "valid-token-hash-sha256";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewPasswordResetTokenInput(userId, tokenHash, expiresAt);

        // Act
        LogAct("Registering new PasswordResetToken");
        var entity = PasswordResetToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.UserId.ShouldBe(userId);
        entity.TokenHash.ShouldBe(tokenHash);
        entity.ExpiresAt.ShouldBe(expiresAt);
        entity.IsUsed.ShouldBeFalse();
        entity.UsedAt.ShouldBeNull();
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetIsUsedToFalse()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new PasswordResetToken");
        var entity = PasswordResetToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying IsUsed is false");
        entity.ShouldNotBeNull();
        entity.IsUsed.ShouldBeFalse();
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetUsedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new PasswordResetToken");
        var entity = PasswordResetToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying UsedAt is null");
        entity.ShouldNotBeNull();
        entity.UsedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithDefaultUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default UserId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var userId = default(Id);
        var tokenHash = "valid-token-hash";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewPasswordResetTokenInput(userId, tokenHash, expiresAt);

        // Act
        LogAct("Registering new PasswordResetToken with default UserId");
        var entity = PasswordResetToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyTokenHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty TokenHash");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = string.Empty;
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewPasswordResetTokenInput(userId, tokenHash, expiresAt);

        // Act
        LogAct("Registering new PasswordResetToken with empty TokenHash");
        var entity = PasswordResetToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithTokenHashExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with TokenHash exceeding max length of 128");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = new string('a', PasswordResetTokenMetadata.TokenHashMaxLength + 1);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewPasswordResetTokenInput(userId, tokenHash, expiresAt);

        // Act
        LogAct("Registering new PasswordResetToken with too-long TokenHash");
        var entity = PasswordResetToken.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing PasswordResetToken");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = "existing-token-hash";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var isUsed = true;
        var usedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoPasswordResetTokenInput(
            entityInfo, userId, tokenHash, expiresAt, isUsed, usedAt);

        // Act
        LogAct("Creating PasswordResetToken from existing info");
        var entity = PasswordResetToken.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.UserId.ShouldBe(userId);
        entity.TokenHash.ShouldBe(tokenHash);
        entity.ExpiresAt.ShouldBe(expiresAt);
        entity.IsUsed.ShouldBe(isUsed);
        entity.UsedAt.ShouldBe(usedAt);
    }

    [Fact]
    public void CreateFromExistingInfo_WithUnusedState_ShouldPreserveNullUsedAt()
    {
        // Arrange
        LogArrange("Creating input with unused state");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoPasswordResetTokenInput(
            entityInfo, userId, "hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Creating PasswordResetToken from existing info with unused state");
        var entity = PasswordResetToken.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying IsUsed is false and UsedAt is null");
        entity.IsUsed.ShouldBeFalse();
        entity.UsedAt.ShouldBeNull();
    }

    #endregion

    #region MarkUsed Tests

    [Fact]
    public void MarkUsed_WithUnusedToken_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating unused PasswordResetToken");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestPasswordResetToken(executionContext);
        var input = new MarkUsedPasswordResetTokenInput();

        // Act
        LogAct("Marking token as used");
        var result = entity.MarkUsed(executionContext, input);

        // Assert
        LogAssert("Verifying token was marked as used");
        result.ShouldNotBeNull();
        result.IsUsed.ShouldBeTrue();
        result.UsedAt.ShouldNotBeNull();
    }

    [Fact]
    public void MarkUsed_ShouldSetUsedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating unused PasswordResetToken");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestPasswordResetToken(executionContext);
        var input = new MarkUsedPasswordResetTokenInput();

        // Act
        LogAct("Marking token as used");
        var result = entity.MarkUsed(executionContext, input);

        // Assert
        LogAssert("Verifying UsedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.UsedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void MarkUsed_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating unused PasswordResetToken");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestPasswordResetToken(executionContext);
        var input = new MarkUsedPasswordResetTokenInput();

        // Act
        LogAct("Marking token as used");
        var result = entity.MarkUsed(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(entity);
        entity.IsUsed.ShouldBeFalse();
        result.IsUsed.ShouldBeTrue();
    }

    [Fact]
    public void MarkUsed_WithAlreadyUsedToken_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating already-used PasswordResetToken");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestPasswordResetToken(executionContext);
        var usedEntity = entity.MarkUsed(executionContext, new MarkUsedPasswordResetTokenInput())!;
        var input = new MarkUsedPasswordResetTokenInput();

        // Act
        LogAct("Attempting to mark already-used token as used again");
        var newContext = CreateTestExecutionContext();
        var result = usedEntity.MarkUsed(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for already-used token");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating PasswordResetToken via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestPasswordResetToken(executionContext);

        // Act
        LogAct("Cloning PasswordResetToken");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.UserId.ShouldBe(entity.UserId);
        clone.TokenHash.ShouldBe(entity.TokenHash);
        clone.ExpiresAt.ShouldBe(entity.ExpiresAt);
        clone.IsUsed.ShouldBe(entity.IsUsed);
        clone.UsedAt.ShouldBe(entity.UsedAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid PasswordResetToken");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestPasswordResetToken(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = entity.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PasswordResetToken with empty TokenHash via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoPasswordResetTokenInput(
            entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()), string.Empty,
            DateTimeOffset.UtcNow.AddHours(1), false, null);
        var entity = PasswordResetToken.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on token with empty TokenHash");
        bool result = entity.IsValid(validationContext);

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
        LogArrange("Creating execution context and valid UserId");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating valid UserId");
        bool result = PasswordResetToken.ValidateUserId(executionContext, userId);

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
        bool result = PasswordResetToken.ValidateUserId(executionContext, null);

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
        LogArrange("Creating execution context and valid TokenHash");
        var executionContext = CreateTestExecutionContext();
        var tokenHash = "valid-token-hash";

        // Act
        LogAct("Validating valid TokenHash");
        bool result = PasswordResetToken.ValidateTokenHash(executionContext, tokenHash);

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
        LogAct("Validating null TokenHash");
        bool result = PasswordResetToken.ValidateTokenHash(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTokenHash_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty TokenHash");
        bool result = PasswordResetToken.ValidateTokenHash(executionContext, string.Empty);

        // Assert
        LogAssert("Verifying validation fails for empty string");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTokenHash_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating TokenHash at max length of 128");
        var executionContext = CreateTestExecutionContext();
        var tokenHash = new string('a', PasswordResetTokenMetadata.TokenHashMaxLength);

        // Act
        LogAct("Validating max-length TokenHash");
        bool result = PasswordResetToken.ValidateTokenHash(executionContext, tokenHash);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTokenHash_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating TokenHash exceeding max length of 128");
        var executionContext = CreateTestExecutionContext();
        var tokenHash = new string('a', PasswordResetTokenMetadata.TokenHashMaxLength + 1);

        // Act
        LogAct("Validating too-long TokenHash");
        bool result = PasswordResetToken.ValidateTokenHash(executionContext, tokenHash);

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
        LogArrange("Creating execution context and valid ExpiresAt");
        var executionContext = CreateTestExecutionContext();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        LogAct("Validating valid ExpiresAt");
        bool result = PasswordResetToken.ValidateExpiresAt(executionContext, expiresAt);

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
        bool result = PasswordResetToken.ValidateExpiresAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
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
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = "valid-hash";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        LogAct("Calling IsValid");
        bool result = PasswordResetToken.IsValid(executionContext, entityInfo, userId, tokenHash, expiresAt);

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
        bool result = PasswordResetToken.IsValid(
            executionContext, entityInfo, null, "valid-hash", DateTimeOffset.UtcNow.AddHours(1));

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
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Calling IsValid with null TokenHash");
        bool result = PasswordResetToken.IsValid(
            executionContext, entityInfo, userId, null, DateTimeOffset.UtcNow.AddHours(1));

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
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Calling IsValid with null ExpiresAt");
        bool result = PasswordResetToken.IsValid(
            executionContext, entityInfo, userId, "valid-hash", null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeUserIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original UserIdIsRequired value");
        bool originalIsRequired = PasswordResetTokenMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata to not required");
            PasswordResetTokenMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying UserIdIsRequired was updated");
            PasswordResetTokenMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            PasswordResetTokenMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeTokenHashMetadata_ShouldUpdateIsRequiredAndMaxLength()
    {
        // Arrange
        LogArrange("Saving original TokenHash metadata values");
        bool originalIsRequired = PasswordResetTokenMetadata.TokenHashIsRequired;
        int originalMaxLength = PasswordResetTokenMetadata.TokenHashMaxLength;

        try
        {
            // Act
            LogAct("Changing TokenHash metadata");
            PasswordResetTokenMetadata.ChangeTokenHashMetadata(isRequired: false, maxLength: 256);

            // Assert
            LogAssert("Verifying TokenHash metadata was updated");
            PasswordResetTokenMetadata.TokenHashIsRequired.ShouldBeFalse();
            PasswordResetTokenMetadata.TokenHashMaxLength.ShouldBe(256);
        }
        finally
        {
            PasswordResetTokenMetadata.ChangeTokenHashMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeExpiresAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ExpiresAtIsRequired value");
        bool originalIsRequired = PasswordResetTokenMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata to not required");
            PasswordResetTokenMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ExpiresAtIsRequired was updated");
            PasswordResetTokenMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            PasswordResetTokenMetadata.ChangeExpiresAtMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        LogArrange("Reading default metadata values");

        // Assert
        LogAssert("Verifying default metadata values");
        PasswordResetTokenMetadata.UserIdPropertyName.ShouldBe("UserId");
        PasswordResetTokenMetadata.UserIdIsRequired.ShouldBeTrue();
        PasswordResetTokenMetadata.TokenHashPropertyName.ShouldBe("TokenHash");
        PasswordResetTokenMetadata.TokenHashIsRequired.ShouldBeTrue();
        PasswordResetTokenMetadata.TokenHashMaxLength.ShouldBe(128);
        PasswordResetTokenMetadata.ExpiresAtPropertyName.ShouldBe("ExpiresAt");
        PasswordResetTokenMetadata.ExpiresAtIsRequired.ShouldBeTrue();
        PasswordResetTokenMetadata.IsUsedPropertyName.ShouldBe("IsUsed");
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

    private static PasswordResetToken CreateTestPasswordResetToken(ExecutionContext executionContext)
    {
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = "test-password-reset-token-hash";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewPasswordResetTokenInput(userId, tokenHash, expiresAt);
        return PasswordResetToken.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewPasswordResetTokenInput CreateValidRegisterNewInput()
    {
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = "valid-token-hash";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        return new RegisterNewPasswordResetTokenInput(userId, tokenHash, expiresAt);
    }

    #endregion
}
