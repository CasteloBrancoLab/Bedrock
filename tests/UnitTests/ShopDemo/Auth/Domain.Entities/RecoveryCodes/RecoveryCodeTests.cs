using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using RecoveryCodeMetadata = ShopDemo.Auth.Domain.Entities.RecoveryCodes.RecoveryCode.RecoveryCodeMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RecoveryCodes;

public class RecoveryCodeTests : TestBase
{
    public RecoveryCodeTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid UserId and CodeHash");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var codeHash = "valid-code-hash-sha256";
        var input = new RegisterNewRecoveryCodeInput(userId, codeHash);

        // Act
        LogAct("Registering new RecoveryCode");
        var entity = RecoveryCode.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.UserId.ShouldBe(userId);
        entity.CodeHash.ShouldBe(codeHash);
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
        LogAct("Registering new RecoveryCode");
        var entity = RecoveryCode.RegisterNew(executionContext, input);

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
        LogAct("Registering new RecoveryCode");
        var entity = RecoveryCode.RegisterNew(executionContext, input);

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
        var codeHash = "valid-code-hash";
        var input = new RegisterNewRecoveryCodeInput(userId, codeHash);

        // Act
        LogAct("Registering new RecoveryCode with default UserId");
        var entity = RecoveryCode.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyCodeHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty CodeHash");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var codeHash = string.Empty;
        var input = new RegisterNewRecoveryCodeInput(userId, codeHash);

        // Act
        LogAct("Registering new RecoveryCode with empty CodeHash");
        var entity = RecoveryCode.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithCodeHashExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with CodeHash exceeding max length of 128");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var codeHash = new string('a', RecoveryCodeMetadata.CodeHashMaxLength + 1);
        var input = new RegisterNewRecoveryCodeInput(userId, codeHash);

        // Act
        LogAct("Registering new RecoveryCode with too-long CodeHash");
        var entity = RecoveryCode.RegisterNew(executionContext, input);

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
        LogArrange("Creating all properties for existing RecoveryCode");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var codeHash = "existing-code-hash";
        var isUsed = true;
        var usedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoRecoveryCodeInput(entityInfo, userId, codeHash, isUsed, usedAt);

        // Act
        LogAct("Creating RecoveryCode from existing info");
        var entity = RecoveryCode.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.UserId.ShouldBe(userId);
        entity.CodeHash.ShouldBe(codeHash);
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
        var input = new CreateFromExistingInfoRecoveryCodeInput(entityInfo, userId, "hash", false, null);

        // Act
        LogAct("Creating RecoveryCode from existing info with unused state");
        var entity = RecoveryCode.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying IsUsed is false and UsedAt is null");
        entity.IsUsed.ShouldBeFalse();
        entity.UsedAt.ShouldBeNull();
    }

    #endregion

    #region MarkUsed Tests

    [Fact]
    public void MarkUsed_WithUnusedCode_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating unused RecoveryCode");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRecoveryCode(executionContext);
        var input = new MarkUsedRecoveryCodeInput();

        // Act
        LogAct("Marking code as used");
        var result = entity.MarkUsed(executionContext, input);

        // Assert
        LogAssert("Verifying code was marked as used");
        result.ShouldNotBeNull();
        result.IsUsed.ShouldBeTrue();
        result.UsedAt.ShouldNotBeNull();
    }

    [Fact]
    public void MarkUsed_ShouldSetUsedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating unused RecoveryCode");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRecoveryCode(executionContext);
        var input = new MarkUsedRecoveryCodeInput();

        // Act
        LogAct("Marking code as used");
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
        LogArrange("Creating unused RecoveryCode");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRecoveryCode(executionContext);
        var input = new MarkUsedRecoveryCodeInput();

        // Act
        LogAct("Marking code as used");
        var result = entity.MarkUsed(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(entity);
        entity.IsUsed.ShouldBeFalse();
        result.IsUsed.ShouldBeTrue();
    }

    [Fact]
    public void MarkUsed_WithAlreadyUsedCode_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating already-used RecoveryCode");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRecoveryCode(executionContext);
        var usedEntity = entity.MarkUsed(executionContext, new MarkUsedRecoveryCodeInput())!;
        var input = new MarkUsedRecoveryCodeInput();

        // Act
        LogAct("Attempting to mark already-used code as used again");
        var newContext = CreateTestExecutionContext();
        var result = usedEntity.MarkUsed(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for already-used code");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating RecoveryCode via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRecoveryCode(executionContext);

        // Act
        LogAct("Cloning RecoveryCode");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.UserId.ShouldBe(entity.UserId);
        clone.CodeHash.ShouldBe(entity.CodeHash);
        clone.IsUsed.ShouldBe(entity.IsUsed);
        clone.UsedAt.ShouldBe(entity.UsedAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidCode_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid RecoveryCode");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRecoveryCode(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = entity.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidCode_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating RecoveryCode with empty CodeHash via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoRecoveryCodeInput(
            entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()), string.Empty, false, null);
        var entity = RecoveryCode.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on code with empty CodeHash");
        bool result = entity.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty CodeHash");
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
        bool result = RecoveryCode.ValidateUserId(executionContext, userId);

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
        bool result = RecoveryCode.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateCodeHash Tests

    [Fact]
    public void ValidateCodeHash_WithValidHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid CodeHash");
        var executionContext = CreateTestExecutionContext();
        var codeHash = "valid-code-hash";

        // Act
        LogAct("Validating valid CodeHash");
        bool result = RecoveryCode.ValidateCodeHash(executionContext, codeHash);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateCodeHash_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null CodeHash");
        bool result = RecoveryCode.ValidateCodeHash(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateCodeHash_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty CodeHash");
        bool result = RecoveryCode.ValidateCodeHash(executionContext, string.Empty);

        // Assert
        LogAssert("Verifying validation fails for empty string");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateCodeHash_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating CodeHash at max length of 128");
        var executionContext = CreateTestExecutionContext();
        var codeHash = new string('a', RecoveryCodeMetadata.CodeHashMaxLength);

        // Act
        LogAct("Validating max-length CodeHash");
        bool result = RecoveryCode.ValidateCodeHash(executionContext, codeHash);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateCodeHash_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating CodeHash exceeding max length of 128");
        var executionContext = CreateTestExecutionContext();
        var codeHash = new string('a', RecoveryCodeMetadata.CodeHashMaxLength + 1);

        // Act
        LogAct("Validating too-long CodeHash");
        bool result = RecoveryCode.ValidateCodeHash(executionContext, codeHash);

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
        var codeHash = "valid-hash";

        // Act
        LogAct("Calling IsValid");
        bool result = RecoveryCode.IsValid(executionContext, entityInfo, userId, codeHash);

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
        bool result = RecoveryCode.IsValid(executionContext, entityInfo, null, "valid-hash");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullCodeHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null CodeHash");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Calling IsValid with null CodeHash");
        bool result = RecoveryCode.IsValid(executionContext, entityInfo, userId, null);

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
        bool originalIsRequired = RecoveryCodeMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata to not required");
            RecoveryCodeMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying UserIdIsRequired was updated");
            RecoveryCodeMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            RecoveryCodeMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeCodeHashMetadata_ShouldUpdateIsRequiredAndMaxLength()
    {
        // Arrange
        LogArrange("Saving original CodeHash metadata values");
        bool originalIsRequired = RecoveryCodeMetadata.CodeHashIsRequired;
        int originalMaxLength = RecoveryCodeMetadata.CodeHashMaxLength;

        try
        {
            // Act
            LogAct("Changing CodeHash metadata");
            RecoveryCodeMetadata.ChangeCodeHashMetadata(isRequired: false, maxLength: 256);

            // Assert
            LogAssert("Verifying CodeHash metadata was updated");
            RecoveryCodeMetadata.CodeHashIsRequired.ShouldBeFalse();
            RecoveryCodeMetadata.CodeHashMaxLength.ShouldBe(256);
        }
        finally
        {
            RecoveryCodeMetadata.ChangeCodeHashMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        LogArrange("Reading default metadata values");

        // Assert
        LogAssert("Verifying default metadata values");
        RecoveryCodeMetadata.UserIdPropertyName.ShouldBe("UserId");
        RecoveryCodeMetadata.UserIdIsRequired.ShouldBeTrue();
        RecoveryCodeMetadata.CodeHashPropertyName.ShouldBe("CodeHash");
        RecoveryCodeMetadata.CodeHashIsRequired.ShouldBeTrue();
        RecoveryCodeMetadata.CodeHashMaxLength.ShouldBe(128);
        RecoveryCodeMetadata.IsUsedPropertyName.ShouldBe("IsUsed");
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

    private static RecoveryCode CreateTestRecoveryCode(ExecutionContext executionContext)
    {
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var codeHash = "test-recovery-code-hash";
        var input = new RegisterNewRecoveryCodeInput(userId, codeHash);
        return RecoveryCode.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewRecoveryCodeInput CreateValidRegisterNewInput()
    {
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var codeHash = "valid-code-hash";
        return new RegisterNewRecoveryCodeInput(userId, codeHash);
    }

    #endregion
}
