using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using KeyChainMetadata = ShopDemo.Auth.Domain.Entities.KeyChains.KeyChain.KeyChainMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.KeyChains;

public class KeyChainTests : TestBase
{
    public KeyChainTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateKeyChain()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var keyId = KeyId.CreateNew("kc-2024-01-01");
        var publicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A";
        var encryptedSharedSecret = "encrypted-shared-secret-data";
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);
        var input = new RegisterNewKeyChainInput(userId, keyId, publicKey, encryptedSharedSecret, expiresAt);

        // Act
        LogAct("Registering new key chain");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying key chain was created successfully");
        keyChain.ShouldNotBeNull();
        keyChain.UserId.ShouldBe(userId);
        keyChain.KeyId.ShouldBe(keyId);
        keyChain.PublicKey.ShouldBe(publicKey);
        keyChain.EncryptedSharedSecret.ShouldBe(encryptedSharedSecret);
        keyChain.Status.ShouldBe(KeyChainStatus.Active);
        keyChain.ExpiresAt.ShouldBe(expiresAt);
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new key chain");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        keyChain.ShouldNotBeNull();
        keyChain.Status.ShouldBe(KeyChainStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new key chain");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        keyChain.ShouldNotBeNull();
        keyChain.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithEmptyPublicKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty PublicKey");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewKeyChainInput(
            Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "",
            "encrypted-secret", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new key chain with empty PublicKey");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        keyChain.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullPublicKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null PublicKey");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewKeyChainInput(
            Id.GenerateNewId(), KeyId.CreateNew("kc-id"), null!,
            "encrypted-secret", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new key chain with null PublicKey");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        keyChain.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithPublicKeyExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with PublicKey exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longKey = new('x', KeyChainMetadata.PublicKeyMaxLength + 1);
        var input = new RegisterNewKeyChainInput(
            Id.GenerateNewId(), KeyId.CreateNew("kc-id"), longKey,
            "encrypted-secret", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new key chain with too-long PublicKey");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        keyChain.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyEncryptedSharedSecret_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty EncryptedSharedSecret");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewKeyChainInput(
            Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "pubkey",
            "", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new key chain with empty EncryptedSharedSecret");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        keyChain.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullEncryptedSharedSecret_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null EncryptedSharedSecret");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewKeyChainInput(
            Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "pubkey",
            null!, DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new key chain with null EncryptedSharedSecret");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        keyChain.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEncryptedSharedSecretExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with EncryptedSharedSecret exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longSecret = new('x', KeyChainMetadata.EncryptedSharedSecretMaxLength + 1);
        var input = new RegisterNewKeyChainInput(
            Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "pubkey",
            longSecret, DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new key chain with too-long EncryptedSharedSecret");
        var keyChain = KeyChain.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        keyChain.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateKeyChainWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing key chain");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var keyId = KeyId.CreateFromExistingInfo("kc-2024-01-01");
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);
        var input = new CreateFromExistingInfoKeyChainInput(
            entityInfo, userId, keyId, "pubkey", "encrypted-secret",
            KeyChainStatus.DecryptOnly, expiresAt);

        // Act
        LogAct("Creating key chain from existing info");
        var keyChain = KeyChain.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        keyChain.EntityInfo.ShouldBe(entityInfo);
        keyChain.UserId.ShouldBe(userId);
        keyChain.KeyId.ShouldBe(keyId);
        keyChain.PublicKey.ShouldBe("pubkey");
        keyChain.EncryptedSharedSecret.ShouldBe("encrypted-secret");
        keyChain.Status.ShouldBe(KeyChainStatus.DecryptOnly);
        keyChain.ExpiresAt.ShouldBe(expiresAt);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with empty PublicKey (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoKeyChainInput(
            entityInfo, Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "",
            "encrypted-secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Creating key chain from existing info with empty PublicKey");
        var keyChain = KeyChain.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying key chain was created without validation");
        keyChain.ShouldNotBeNull();
        keyChain.PublicKey.ShouldBe("");
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WithActiveKeyChain_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active key chain");
        var executionContext = CreateTestExecutionContext();
        var keyChain = CreateTestKeyChain(executionContext);
        var input = new DeactivateKeyChainInput();

        // Act
        LogAct("Deactivating key chain");
        var result = keyChain.Deactivate(executionContext, input);

        // Assert
        LogAssert("Verifying key chain was deactivated");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(KeyChainStatus.DecryptOnly);
    }

    [Fact]
    public void Deactivate_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active key chain");
        var executionContext = CreateTestExecutionContext();
        var keyChain = CreateTestKeyChain(executionContext);
        var input = new DeactivateKeyChainInput();

        // Act
        LogAct("Deactivating key chain");
        var result = keyChain.Deactivate(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(keyChain);
        keyChain.Status.ShouldBe(KeyChainStatus.Active);
        result.Status.ShouldBe(KeyChainStatus.DecryptOnly);
    }

    [Fact]
    public void Deactivate_WithDecryptOnlyKeyChain_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating decrypt-only key chain");
        var executionContext = CreateTestExecutionContext();
        var keyChain = CreateTestKeyChain(executionContext);
        var deactivatedChain = keyChain.Deactivate(executionContext, new DeactivateKeyChainInput())!;
        var input = new DeactivateKeyChainInput();

        // Act
        LogAct("Attempting to deactivate already deactivated key chain");
        var newContext = CreateTestExecutionContext();
        var result = deactivatedChain.Deactivate(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for DecryptOnly -> DecryptOnly transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating key chain");
        var executionContext = CreateTestExecutionContext();
        var keyChain = CreateTestKeyChain(executionContext);

        // Act
        LogAct("Cloning key chain");
        var clone = keyChain.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(keyChain);
        clone.UserId.ShouldBe(keyChain.UserId);
        clone.KeyId.ShouldBe(keyChain.KeyId);
        clone.PublicKey.ShouldBe(keyChain.PublicKey);
        clone.EncryptedSharedSecret.ShouldBe(keyChain.EncryptedSharedSecret);
        clone.Status.ShouldBe(keyChain.Status);
        clone.ExpiresAt.ShouldBe(keyChain.ExpiresAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidKeyChain_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid key chain");
        var executionContext = CreateTestExecutionContext();
        var keyChain = CreateTestKeyChain(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = keyChain.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidKeyChain_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating key chain with invalid state via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoKeyChainInput(
            entityInfo, Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "",
            "encrypted-secret", KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(365));
        var keyChain = KeyChain.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on key chain with empty PublicKey");
        bool result = keyChain.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty PublicKey");
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
        bool result = KeyChain.ValidateUserId(executionContext, userId);

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
        bool result = KeyChain.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateKeyId Tests

    [Fact]
    public void ValidateKeyId_WithValidKeyId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var keyId = KeyId.CreateNew("kc-2024-01-01");

        // Act
        LogAct("Validating valid KeyId");
        bool result = KeyChain.ValidateKeyId(executionContext, keyId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null KeyId");
        bool result = KeyChain.ValidateKeyId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidatePublicKey Tests

    [Fact]
    public void ValidatePublicKey_WithValidKey_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid PublicKey");
        bool result = KeyChain.ValidatePublicKey(executionContext, "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublicKey_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null PublicKey");
        bool result = KeyChain.ValidatePublicKey(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublicKey_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty PublicKey");
        bool result = KeyChain.ValidatePublicKey(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublicKey_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating PublicKey at max length");
        var executionContext = CreateTestExecutionContext();
        string key = new('x', KeyChainMetadata.PublicKeyMaxLength);

        // Act
        LogAct("Validating max-length PublicKey");
        bool result = KeyChain.ValidatePublicKey(executionContext, key);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublicKey_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PublicKey exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string key = new('x', KeyChainMetadata.PublicKeyMaxLength + 1);

        // Act
        LogAct("Validating too-long PublicKey");
        bool result = KeyChain.ValidatePublicKey(executionContext, key);

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
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid EncryptedSharedSecret");
        bool result = KeyChain.ValidateEncryptedSharedSecret(executionContext, "encrypted-shared-secret-data");

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
        bool result = KeyChain.ValidateEncryptedSharedSecret(executionContext, null);

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
        bool result = KeyChain.ValidateEncryptedSharedSecret(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedSharedSecret_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating EncryptedSharedSecret at max length");
        var executionContext = CreateTestExecutionContext();
        string secret = new('x', KeyChainMetadata.EncryptedSharedSecretMaxLength);

        // Act
        LogAct("Validating max-length EncryptedSharedSecret");
        bool result = KeyChain.ValidateEncryptedSharedSecret(executionContext, secret);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedSharedSecret_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EncryptedSharedSecret exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string secret = new('x', KeyChainMetadata.EncryptedSharedSecretMaxLength + 1);

        // Act
        LogAct("Validating too-long EncryptedSharedSecret");
        bool result = KeyChain.ValidateEncryptedSharedSecret(executionContext, secret);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(KeyChainStatus.Active)]
    [InlineData(KeyChainStatus.DecryptOnly)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(KeyChainStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = KeyChain.ValidateStatus(executionContext, status);

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
        bool result = KeyChain.ValidateStatus(executionContext, null);

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
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);

        // Act
        LogAct("Validating valid ExpiresAt");
        bool result = KeyChain.ValidateExpiresAt(executionContext, expiresAt);

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
        bool result = KeyChain.ValidateExpiresAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Fact]
    public void ValidateStatusTransition_ActiveToDecryptOnly_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> DecryptOnly transition");
        bool result = KeyChain.ValidateStatusTransition(
            executionContext, KeyChainStatus.Active, KeyChainStatus.DecryptOnly);

        // Assert
        LogAssert("Verifying transition is valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_DecryptOnlyToActive_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating DecryptOnly -> Active transition");
        bool result = KeyChain.ValidateStatusTransition(
            executionContext, KeyChainStatus.DecryptOnly, KeyChainStatus.Active);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(KeyChainStatus.Active)]
    [InlineData(KeyChainStatus.DecryptOnly)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(KeyChainStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = KeyChain.ValidateStatusTransition(executionContext, status, status);

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
        LogAct("Validating null -> DecryptOnly transition");
        bool result = KeyChain.ValidateStatusTransition(executionContext, null, KeyChainStatus.DecryptOnly);

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
        bool result = KeyChain.ValidateStatusTransition(executionContext, KeyChainStatus.Active, null);

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
        bool result = KeyChain.ValidateStatusTransition(
            executionContext, KeyChainStatus.Active, (KeyChainStatus)99);

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
        var keyId = KeyId.CreateNew("kc-2024-01-01");
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);

        // Act
        LogAct("Calling IsValid");
        bool result = KeyChain.IsValid(
            executionContext, entityInfo, userId, keyId, "pubkey", "encrypted-secret",
            KeyChainStatus.Active, expiresAt);

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
        bool result = KeyChain.IsValid(
            executionContext, entityInfo, null, KeyId.CreateNew("kc-id"), "pubkey", "secret",
            KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullKeyId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null KeyId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null KeyId");
        bool result = KeyChain.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), null, "pubkey", "secret",
            KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullPublicKey_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null PublicKey");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null PublicKey");
        bool result = KeyChain.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), KeyId.CreateNew("kc-id"), null, "secret",
            KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

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

        // Act
        LogAct("Calling IsValid with null EncryptedSharedSecret");
        bool result = KeyChain.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "pubkey", null,
            KeyChainStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

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
        bool result = KeyChain.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "pubkey", "secret",
            null, DateTimeOffset.UtcNow.AddDays(365));

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
        bool result = KeyChain.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), KeyId.CreateNew("kc-id"), "pubkey", "secret",
            KeyChainStatus.Active, null);

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
        bool originalIsRequired = KeyChainMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata");
            KeyChainMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            KeyChainMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            KeyChainMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangeKeyIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = KeyChainMetadata.KeyIdIsRequired;

        try
        {
            // Act
            LogAct("Changing KeyId metadata");
            KeyChainMetadata.ChangeKeyIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            KeyChainMetadata.KeyIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            KeyChainMetadata.ChangeKeyIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangePublicKeyMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = KeyChainMetadata.PublicKeyIsRequired;
        int originalMaxLength = KeyChainMetadata.PublicKeyMaxLength;

        try
        {
            // Act
            LogAct("Changing PublicKey metadata");
            KeyChainMetadata.ChangePublicKeyMetadata(isRequired: false, maxLength: 1024);

            // Assert
            LogAssert("Verifying metadata was updated");
            KeyChainMetadata.PublicKeyIsRequired.ShouldBeFalse();
            KeyChainMetadata.PublicKeyMaxLength.ShouldBe(1024);
        }
        finally
        {
            KeyChainMetadata.ChangePublicKeyMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void ChangeEncryptedSharedSecretMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = KeyChainMetadata.EncryptedSharedSecretIsRequired;
        int originalMaxLength = KeyChainMetadata.EncryptedSharedSecretMaxLength;

        try
        {
            // Act
            LogAct("Changing EncryptedSharedSecret metadata");
            KeyChainMetadata.ChangeEncryptedSharedSecretMetadata(isRequired: false, maxLength: 2048);

            // Assert
            LogAssert("Verifying metadata was updated");
            KeyChainMetadata.EncryptedSharedSecretIsRequired.ShouldBeFalse();
            KeyChainMetadata.EncryptedSharedSecretMaxLength.ShouldBe(2048);
        }
        finally
        {
            KeyChainMetadata.ChangeEncryptedSharedSecretMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = KeyChainMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata");
            KeyChainMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            KeyChainMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            KeyChainMetadata.ChangeStatusMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangeExpiresAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = KeyChainMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata");
            KeyChainMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            KeyChainMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            KeyChainMetadata.ChangeExpiresAtMetadata(isRequired: originalIsRequired);
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

    private static KeyChain CreateTestKeyChain(ExecutionContext executionContext)
    {
        var input = CreateValidRegisterNewInput();
        return KeyChain.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewKeyChainInput CreateValidRegisterNewInput()
    {
        return new RegisterNewKeyChainInput(
            Id.GenerateNewId(),
            KeyId.CreateNew("kc-2024-01-01"),
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A",
            "encrypted-shared-secret-data",
            DateTimeOffset.UtcNow.AddDays(365));
    }

    #endregion
}
