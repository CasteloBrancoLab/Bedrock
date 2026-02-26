using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.MfaSetups;
using ShopDemo.Auth.Domain.Entities.MfaSetups.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using MfaSetupMetadata = ShopDemo.Auth.Domain.Entities.MfaSetups.MfaSetup.MfaSetupMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.MfaSetups;

public class MfaSetupTests : TestBase
{
    public MfaSetupTests(ITestOutputHelper outputHelper) : base(outputHelper)
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
        var encryptedSharedSecret = "encrypted-shared-secret-base64";
        var input = new RegisterNewMfaSetupInput(userId, encryptedSharedSecret);

        // Act
        LogAct("Registering new MfaSetup");
        var entity = MfaSetup.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.UserId.ShouldBe(userId);
        entity.EncryptedSharedSecret.ShouldBe(encryptedSharedSecret);
        entity.IsEnabled.ShouldBeFalse();
        entity.EnabledAt.ShouldBeNull();
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetIsEnabledToFalse()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new MfaSetup");
        var entity = MfaSetup.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying IsEnabled is false");
        entity.ShouldNotBeNull();
        entity.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetEnabledAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new MfaSetup");
        var entity = MfaSetup.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EnabledAt is null");
        entity.ShouldNotBeNull();
        entity.EnabledAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithDefaultUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default UserId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var userId = default(Id);
        var encryptedSharedSecret = "encrypted-shared-secret";
        var input = new RegisterNewMfaSetupInput(userId, encryptedSharedSecret);

        // Act
        LogAct("Registering new MfaSetup with default UserId");
        var entity = MfaSetup.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyEncryptedSharedSecret_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty EncryptedSharedSecret");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var encryptedSharedSecret = string.Empty;
        var input = new RegisterNewMfaSetupInput(userId, encryptedSharedSecret);

        // Act
        LogAct("Registering new MfaSetup with empty EncryptedSharedSecret");
        var entity = MfaSetup.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEncryptedSharedSecretExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with EncryptedSharedSecret exceeding max length of 1024");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var encryptedSharedSecret = new string('a', MfaSetupMetadata.EncryptedSharedSecretMaxLength + 1);
        var input = new RegisterNewMfaSetupInput(userId, encryptedSharedSecret);

        // Act
        LogAct("Registering new MfaSetup with too-long EncryptedSharedSecret");
        var entity = MfaSetup.RegisterNew(executionContext, input);

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
        LogArrange("Creating all properties for existing MfaSetup");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var encryptedSharedSecret = "existing-encrypted-secret";
        var isEnabled = true;
        var enabledAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoMfaSetupInput(
            entityInfo, userId, encryptedSharedSecret, isEnabled, enabledAt);

        // Act
        LogAct("Creating MfaSetup from existing info");
        var entity = MfaSetup.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.UserId.ShouldBe(userId);
        entity.EncryptedSharedSecret.ShouldBe(encryptedSharedSecret);
        entity.IsEnabled.ShouldBe(isEnabled);
        entity.EnabledAt.ShouldBe(enabledAt);
    }

    [Fact]
    public void CreateFromExistingInfo_WithDisabledState_ShouldPreserveNullEnabledAt()
    {
        // Arrange
        LogArrange("Creating input with disabled state");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoMfaSetupInput(
            entityInfo, userId, "secret", false, null);

        // Act
        LogAct("Creating MfaSetup from existing info with disabled state");
        var entity = MfaSetup.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying IsEnabled is false and EnabledAt is null");
        entity.IsEnabled.ShouldBeFalse();
        entity.EnabledAt.ShouldBeNull();
    }

    #endregion

    #region Enable Tests

    [Fact]
    public void Enable_WithDisabledSetup_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating disabled MfaSetup");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);
        var input = new EnableMfaSetupInput();

        // Act
        LogAct("Enabling MfaSetup");
        var result = entity.Enable(executionContext, input);

        // Assert
        LogAssert("Verifying MfaSetup was enabled");
        result.ShouldNotBeNull();
        result.IsEnabled.ShouldBeTrue();
        result.EnabledAt.ShouldNotBeNull();
    }

    [Fact]
    public void Enable_ShouldSetEnabledAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating disabled MfaSetup");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);
        var input = new EnableMfaSetupInput();

        // Act
        LogAct("Enabling MfaSetup");
        var result = entity.Enable(executionContext, input);

        // Assert
        LogAssert("Verifying EnabledAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.EnabledAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void Enable_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating disabled MfaSetup");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);
        var input = new EnableMfaSetupInput();

        // Act
        LogAct("Enabling MfaSetup");
        var result = entity.Enable(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(entity);
        entity.IsEnabled.ShouldBeFalse();
        result.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Enable_WithAlreadyEnabledSetup_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating already-enabled MfaSetup");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);
        var enabledEntity = entity.Enable(executionContext, new EnableMfaSetupInput())!;
        var input = new EnableMfaSetupInput();

        // Act
        LogAct("Attempting to enable already-enabled MfaSetup");
        var newContext = CreateTestExecutionContext();
        var result = enabledEntity.Enable(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for already-enabled setup");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Disable Tests

    [Fact]
    public void Disable_WithEnabledSetup_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating enabled MfaSetup");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);
        var enabledEntity = entity.Enable(executionContext, new EnableMfaSetupInput())!;
        var input = new DisableMfaSetupInput();

        // Act
        LogAct("Disabling MfaSetup");
        var result = enabledEntity.Disable(executionContext, input);

        // Assert
        LogAssert("Verifying MfaSetup was disabled");
        result.ShouldNotBeNull();
        result.IsEnabled.ShouldBeFalse();
        result.EnabledAt.ShouldBeNull();
    }

    [Fact]
    public void Disable_ShouldSetEnabledAtToNull()
    {
        // Arrange
        LogArrange("Creating enabled MfaSetup");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);
        var enabledEntity = entity.Enable(executionContext, new EnableMfaSetupInput())!;
        var input = new DisableMfaSetupInput();

        // Act
        LogAct("Disabling MfaSetup");
        var result = enabledEntity.Disable(executionContext, input);

        // Assert
        LogAssert("Verifying EnabledAt is null after disable");
        result.ShouldNotBeNull();
        result.EnabledAt.ShouldBeNull();
    }

    [Fact]
    public void Disable_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating enabled MfaSetup");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);
        var enabledEntity = entity.Enable(executionContext, new EnableMfaSetupInput())!;
        var input = new DisableMfaSetupInput();

        // Act
        LogAct("Disabling MfaSetup");
        var result = enabledEntity.Disable(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(enabledEntity);
        enabledEntity.IsEnabled.ShouldBeTrue();
        result.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void Disable_WithAlreadyDisabledSetup_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating already-disabled MfaSetup (newly registered is disabled)");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);
        var input = new DisableMfaSetupInput();

        // Act
        LogAct("Attempting to disable already-disabled MfaSetup");
        var newContext = CreateTestExecutionContext();
        var result = entity.Disable(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for already-disabled setup");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating MfaSetup via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);

        // Act
        LogAct("Cloning MfaSetup");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.UserId.ShouldBe(entity.UserId);
        clone.EncryptedSharedSecret.ShouldBe(entity.EncryptedSharedSecret);
        clone.IsEnabled.ShouldBe(entity.IsEnabled);
        clone.EnabledAt.ShouldBe(entity.EnabledAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidSetup_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid MfaSetup");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestMfaSetup(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = entity.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidSetup_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating MfaSetup with empty EncryptedSharedSecret via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoMfaSetupInput(
            entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()), string.Empty, false, null);
        var entity = MfaSetup.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on setup with empty EncryptedSharedSecret");
        bool result = entity.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty EncryptedSharedSecret");
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
        bool result = MfaSetup.ValidateUserId(executionContext, userId);

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
        bool result = MfaSetup.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateEncryptedSharedSecret Tests

    [Fact]
    public void ValidateEncryptedSharedSecret_WithValidSecret_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid EncryptedSharedSecret");
        var executionContext = CreateTestExecutionContext();
        var secret = "valid-encrypted-secret";

        // Act
        LogAct("Validating valid EncryptedSharedSecret");
        bool result = MfaSetup.ValidateEncryptedSharedSecret(executionContext, secret);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedSharedSecret_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null EncryptedSharedSecret");
        bool result = MfaSetup.ValidateEncryptedSharedSecret(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedSharedSecret_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty EncryptedSharedSecret");
        bool result = MfaSetup.ValidateEncryptedSharedSecret(executionContext, string.Empty);

        // Assert
        LogAssert("Verifying validation fails for empty string");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedSharedSecret_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating EncryptedSharedSecret at max length of 1024");
        var executionContext = CreateTestExecutionContext();
        var secret = new string('a', MfaSetupMetadata.EncryptedSharedSecretMaxLength);

        // Act
        LogAct("Validating max-length EncryptedSharedSecret");
        bool result = MfaSetup.ValidateEncryptedSharedSecret(executionContext, secret);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedSharedSecret_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EncryptedSharedSecret exceeding max length of 1024");
        var executionContext = CreateTestExecutionContext();
        var secret = new string('a', MfaSetupMetadata.EncryptedSharedSecretMaxLength + 1);

        // Act
        LogAct("Validating too-long EncryptedSharedSecret");
        bool result = MfaSetup.ValidateEncryptedSharedSecret(executionContext, secret);

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
        var encryptedSharedSecret = "valid-secret";

        // Act
        LogAct("Calling IsValid");
        bool result = MfaSetup.IsValid(executionContext, entityInfo, userId, encryptedSharedSecret);

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
        bool result = MfaSetup.IsValid(executionContext, entityInfo, null, "valid-secret");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullEncryptedSharedSecret_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null EncryptedSharedSecret");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Calling IsValid with null EncryptedSharedSecret");
        bool result = MfaSetup.IsValid(executionContext, entityInfo, userId, null);

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
        bool originalIsRequired = MfaSetupMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata to not required");
            MfaSetupMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying UserIdIsRequired was updated");
            MfaSetupMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            MfaSetupMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeEncryptedSharedSecretMetadata_ShouldUpdateIsRequiredAndMaxLength()
    {
        // Arrange
        LogArrange("Saving original EncryptedSharedSecret metadata values");
        bool originalIsRequired = MfaSetupMetadata.EncryptedSharedSecretIsRequired;
        int originalMaxLength = MfaSetupMetadata.EncryptedSharedSecretMaxLength;

        try
        {
            // Act
            LogAct("Changing EncryptedSharedSecret metadata");
            MfaSetupMetadata.ChangeEncryptedSharedSecretMetadata(isRequired: false, maxLength: 2048);

            // Assert
            LogAssert("Verifying EncryptedSharedSecret metadata was updated");
            MfaSetupMetadata.EncryptedSharedSecretIsRequired.ShouldBeFalse();
            MfaSetupMetadata.EncryptedSharedSecretMaxLength.ShouldBe(2048);
        }
        finally
        {
            MfaSetupMetadata.ChangeEncryptedSharedSecretMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        LogArrange("Reading default metadata values");

        // Assert
        LogAssert("Verifying default metadata values");
        MfaSetupMetadata.UserIdPropertyName.ShouldBe("UserId");
        MfaSetupMetadata.UserIdIsRequired.ShouldBeTrue();
        MfaSetupMetadata.EncryptedSharedSecretPropertyName.ShouldBe("EncryptedSharedSecret");
        MfaSetupMetadata.EncryptedSharedSecretIsRequired.ShouldBeTrue();
        MfaSetupMetadata.EncryptedSharedSecretMaxLength.ShouldBe(1024);
        MfaSetupMetadata.IsEnabledPropertyName.ShouldBe("IsEnabled");
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

    private static MfaSetup CreateTestMfaSetup(ExecutionContext executionContext)
    {
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var encryptedSharedSecret = "test-encrypted-shared-secret";
        var input = new RegisterNewMfaSetupInput(userId, encryptedSharedSecret);
        return MfaSetup.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewMfaSetupInput CreateValidRegisterNewInput()
    {
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var encryptedSharedSecret = "valid-encrypted-secret";
        return new RegisterNewMfaSetupInput(userId, encryptedSharedSecret);
    }

    #endregion
}
