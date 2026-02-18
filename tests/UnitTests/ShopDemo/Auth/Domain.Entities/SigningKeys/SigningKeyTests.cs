using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using SigningKeyMetadata = ShopDemo.Auth.Domain.Entities.SigningKeys.SigningKey.SigningKeyMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.SigningKeys;

public class SigningKeyTests : TestBase
{
    public SigningKeyTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateSigningKey()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var kid = Kid.CreateNew("key-2024-01-01");
        var algorithm = "RS256";
        var publicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A";
        var encryptedPrivateKey = "encrypted-private-key-data";
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);
        var input = new RegisterNewSigningKeyInput(kid, algorithm, publicKey, encryptedPrivateKey, expiresAt);

        // Act
        LogAct("Registering new signing key");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying signing key was created successfully");
        signingKey.ShouldNotBeNull();
        signingKey.Kid.ShouldBe(kid);
        signingKey.Algorithm.ShouldBe(algorithm);
        signingKey.PublicKey.ShouldBe(publicKey);
        signingKey.EncryptedPrivateKey.ShouldBe(encryptedPrivateKey);
        signingKey.Status.ShouldBe(SigningKeyStatus.Active);
        signingKey.RotatedAt.ShouldBeNull();
        signingKey.ExpiresAt.ShouldBe(expiresAt);
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new signing key");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        signingKey.ShouldNotBeNull();
        signingKey.Status.ShouldBe(SigningKeyStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new signing key");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        signingKey.ShouldNotBeNull();
        signingKey.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldSetRotatedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new signing key");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying RotatedAt is null");
        signingKey.ShouldNotBeNull();
        signingKey.RotatedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithEmptyAlgorithm_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty Algorithm");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), "", "pubkey", "privkey", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with empty Algorithm");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullAlgorithm_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Algorithm");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), null!, "pubkey", "privkey", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with null Algorithm");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithAlgorithmExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Algorithm exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longAlgorithm = new('a', SigningKeyMetadata.AlgorithmMaxLength + 1);
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), longAlgorithm, "pubkey", "privkey", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with too-long Algorithm");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyPublicKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty PublicKey");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), "RS256", "", "privkey", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with empty PublicKey");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullPublicKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null PublicKey");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), "RS256", null!, "privkey", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with null PublicKey");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithPublicKeyExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with PublicKey exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longKey = new('x', SigningKeyMetadata.PublicKeyMaxLength + 1);
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), "RS256", longKey, "privkey", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with too-long PublicKey");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyEncryptedPrivateKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty EncryptedPrivateKey");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), "RS256", "pubkey", "", DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with empty EncryptedPrivateKey");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullEncryptedPrivateKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null EncryptedPrivateKey");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), "RS256", "pubkey", null!, DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with null EncryptedPrivateKey");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEncryptedPrivateKeyExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with EncryptedPrivateKey exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longKey = new('x', SigningKeyMetadata.EncryptedPrivateKeyMaxLength + 1);
        var input = new RegisterNewSigningKeyInput(
            Kid.CreateNew("kid"), "RS256", "pubkey", longKey, DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Registering new signing key with too-long EncryptedPrivateKey");
        var signingKey = SigningKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        signingKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateSigningKeyWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing signing key");
        var entityInfo = CreateTestEntityInfo();
        var kid = Kid.CreateFromExistingInfo("key-2024-01-01");
        var rotatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);
        var input = new CreateFromExistingInfoSigningKeyInput(
            entityInfo, kid, "RS256", "pubkey", "encrypted-privkey",
            SigningKeyStatus.Rotated, rotatedAt, expiresAt);

        // Act
        LogAct("Creating signing key from existing info");
        var signingKey = SigningKey.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        signingKey.EntityInfo.ShouldBe(entityInfo);
        signingKey.Kid.ShouldBe(kid);
        signingKey.Algorithm.ShouldBe("RS256");
        signingKey.PublicKey.ShouldBe("pubkey");
        signingKey.EncryptedPrivateKey.ShouldBe("encrypted-privkey");
        signingKey.Status.ShouldBe(SigningKeyStatus.Rotated);
        signingKey.RotatedAt.ShouldBe(rotatedAt);
        signingKey.ExpiresAt.ShouldBe(expiresAt);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with empty Algorithm (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoSigningKeyInput(
            entityInfo, Kid.CreateNew("kid"), "", "pubkey", "privkey",
            SigningKeyStatus.Active, null, DateTimeOffset.UtcNow.AddDays(365));

        // Act
        LogAct("Creating signing key from existing info with empty Algorithm");
        var signingKey = SigningKey.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying signing key was created without validation");
        signingKey.ShouldNotBeNull();
        signingKey.Algorithm.ShouldBe("");
    }

    #endregion

    #region Rotate Tests

    [Fact]
    public void Rotate_WithActiveSigningKey_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var input = new RotateSigningKeyInput();

        // Act
        LogAct("Rotating signing key");
        var result = signingKey.Rotate(executionContext, input);

        // Assert
        LogAssert("Verifying signing key was rotated");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(SigningKeyStatus.Rotated);
        result.RotatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Rotate_ShouldSetRotatedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var input = new RotateSigningKeyInput();

        // Act
        LogAct("Rotating signing key");
        var result = signingKey.Rotate(executionContext, input);

        // Assert
        LogAssert("Verifying RotatedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.RotatedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void Rotate_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var input = new RotateSigningKeyInput();

        // Act
        LogAct("Rotating signing key");
        var result = signingKey.Rotate(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(signingKey);
        signingKey.Status.ShouldBe(SigningKeyStatus.Active);
        result.Status.ShouldBe(SigningKeyStatus.Rotated);
    }

    [Fact]
    public void Rotate_WithRotatedSigningKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating rotated signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var rotatedKey = signingKey.Rotate(executionContext, new RotateSigningKeyInput())!;
        var input = new RotateSigningKeyInput();

        // Act
        LogAct("Attempting to rotate already rotated signing key");
        var newContext = CreateTestExecutionContext();
        var result = rotatedKey.Rotate(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Rotated -> Rotated transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void Rotate_WithRevokedSigningKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating revoked signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var revokedKey = signingKey.Revoke(executionContext, new RevokeSigningKeyInput())!;
        var input = new RotateSigningKeyInput();

        // Act
        LogAct("Attempting to rotate revoked signing key");
        var newContext = CreateTestExecutionContext();
        var result = revokedKey.Rotate(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Revoked -> Rotated transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_WithActiveSigningKey_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var input = new RevokeSigningKeyInput();

        // Act
        LogAct("Revoking signing key");
        var result = signingKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying signing key was revoked");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(SigningKeyStatus.Revoked);
    }

    [Fact]
    public void Revoke_WithRotatedSigningKey_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating rotated signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var rotatedKey = signingKey.Rotate(executionContext, new RotateSigningKeyInput())!;
        var input = new RevokeSigningKeyInput();

        // Act
        LogAct("Revoking rotated signing key");
        var result = rotatedKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying rotated signing key was revoked");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(SigningKeyStatus.Revoked);
    }

    [Fact]
    public void Revoke_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var input = new RevokeSigningKeyInput();

        // Act
        LogAct("Revoking signing key");
        var result = signingKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(signingKey);
        signingKey.Status.ShouldBe(SigningKeyStatus.Active);
        result.Status.ShouldBe(SigningKeyStatus.Revoked);
    }

    [Fact]
    public void Revoke_WithRevokedSigningKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating revoked signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var revokedKey = signingKey.Revoke(executionContext, new RevokeSigningKeyInput())!;
        var input = new RevokeSigningKeyInput();

        // Act
        LogAct("Attempting to revoke already revoked signing key");
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
        LogArrange("Creating signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);

        // Act
        LogAct("Cloning signing key");
        var clone = signingKey.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(signingKey);
        clone.Kid.ShouldBe(signingKey.Kid);
        clone.Algorithm.ShouldBe(signingKey.Algorithm);
        clone.PublicKey.ShouldBe(signingKey.PublicKey);
        clone.EncryptedPrivateKey.ShouldBe(signingKey.EncryptedPrivateKey);
        clone.Status.ShouldBe(signingKey.Status);
        clone.RotatedAt.ShouldBe(signingKey.RotatedAt);
        clone.ExpiresAt.ShouldBe(signingKey.ExpiresAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidSigningKey_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid signing key");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = signingKey.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidSigningKey_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating signing key with invalid state via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoSigningKeyInput(
            entityInfo, Kid.CreateNew("kid"), "", "pubkey", "privkey",
            SigningKeyStatus.Active, null, DateTimeOffset.UtcNow.AddDays(365));
        var signingKey = SigningKey.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on signing key with empty Algorithm");
        bool result = signingKey.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty Algorithm");
        result.ShouldBeFalse();
        validationContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateKid Tests

    [Fact]
    public void ValidateKid_WithValidKid_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var kid = Kid.CreateNew("key-2024-01-01");

        // Act
        LogAct("Validating valid Kid");
        bool result = SigningKey.ValidateKid(executionContext, kid);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKid_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Kid");
        bool result = SigningKey.ValidateKid(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateAlgorithm Tests

    [Fact]
    public void ValidateAlgorithm_WithValidAlgorithm_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Algorithm");
        bool result = SigningKey.ValidateAlgorithm(executionContext, "RS256");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateAlgorithm_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Algorithm");
        bool result = SigningKey.ValidateAlgorithm(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateAlgorithm_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Algorithm");
        bool result = SigningKey.ValidateAlgorithm(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateAlgorithm_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating Algorithm at max length");
        var executionContext = CreateTestExecutionContext();
        string algorithm = new('a', SigningKeyMetadata.AlgorithmMaxLength);

        // Act
        LogAct("Validating max-length Algorithm");
        bool result = SigningKey.ValidateAlgorithm(executionContext, algorithm);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateAlgorithm_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating Algorithm exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string algorithm = new('a', SigningKeyMetadata.AlgorithmMaxLength + 1);

        // Act
        LogAct("Validating too-long Algorithm");
        bool result = SigningKey.ValidateAlgorithm(executionContext, algorithm);

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
        bool result = SigningKey.ValidatePublicKey(executionContext, "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A");

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
        bool result = SigningKey.ValidatePublicKey(executionContext, null);

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
        bool result = SigningKey.ValidatePublicKey(executionContext, "");

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
        string key = new('x', SigningKeyMetadata.PublicKeyMaxLength);

        // Act
        LogAct("Validating max-length PublicKey");
        bool result = SigningKey.ValidatePublicKey(executionContext, key);

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
        string key = new('x', SigningKeyMetadata.PublicKeyMaxLength + 1);

        // Act
        LogAct("Validating too-long PublicKey");
        bool result = SigningKey.ValidatePublicKey(executionContext, key);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateEncryptedPrivateKey Tests

    [Fact]
    public void ValidateEncryptedPrivateKey_WithValidKey_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid EncryptedPrivateKey");
        bool result = SigningKey.ValidateEncryptedPrivateKey(executionContext, "encrypted-private-key-data");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedPrivateKey_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null EncryptedPrivateKey");
        bool result = SigningKey.ValidateEncryptedPrivateKey(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedPrivateKey_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty EncryptedPrivateKey");
        bool result = SigningKey.ValidateEncryptedPrivateKey(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedPrivateKey_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating EncryptedPrivateKey at max length");
        var executionContext = CreateTestExecutionContext();
        string key = new('x', SigningKeyMetadata.EncryptedPrivateKeyMaxLength);

        // Act
        LogAct("Validating max-length EncryptedPrivateKey");
        bool result = SigningKey.ValidateEncryptedPrivateKey(executionContext, key);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEncryptedPrivateKey_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EncryptedPrivateKey exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string key = new('x', SigningKeyMetadata.EncryptedPrivateKeyMaxLength + 1);

        // Act
        LogAct("Validating too-long EncryptedPrivateKey");
        bool result = SigningKey.ValidateEncryptedPrivateKey(executionContext, key);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(SigningKeyStatus.Active)]
    [InlineData(SigningKeyStatus.Rotated)]
    [InlineData(SigningKeyStatus.Revoked)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(SigningKeyStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = SigningKey.ValidateStatus(executionContext, status);

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
        bool result = SigningKey.ValidateStatus(executionContext, null);

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
        bool result = SigningKey.ValidateExpiresAt(executionContext, expiresAt);

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
        bool result = SigningKey.ValidateExpiresAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Theory]
    [InlineData(SigningKeyStatus.Active, SigningKeyStatus.Rotated)]
    [InlineData(SigningKeyStatus.Active, SigningKeyStatus.Revoked)]
    [InlineData(SigningKeyStatus.Rotated, SigningKeyStatus.Revoked)]
    public void ValidateStatusTransition_ValidTransitions_ShouldReturnTrue(
        SigningKeyStatus from, SigningKeyStatus to)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating transition {from} -> {to}");
        bool result = SigningKey.ValidateStatusTransition(executionContext, from, to);

        // Assert
        LogAssert("Verifying transition is valid");
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(SigningKeyStatus.Rotated, SigningKeyStatus.Active)]
    [InlineData(SigningKeyStatus.Revoked, SigningKeyStatus.Active)]
    [InlineData(SigningKeyStatus.Revoked, SigningKeyStatus.Rotated)]
    public void ValidateStatusTransition_InvalidTransitions_ShouldReturnFalse(
        SigningKeyStatus from, SigningKeyStatus to)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating transition {from} -> {to}");
        bool result = SigningKey.ValidateStatusTransition(executionContext, from, to);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(SigningKeyStatus.Active)]
    [InlineData(SigningKeyStatus.Rotated)]
    [InlineData(SigningKeyStatus.Revoked)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(SigningKeyStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = SigningKey.ValidateStatusTransition(executionContext, status, status);

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
        bool result = SigningKey.ValidateStatusTransition(executionContext, null, SigningKeyStatus.Revoked);

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
        bool result = SigningKey.ValidateStatusTransition(executionContext, SigningKeyStatus.Active, null);

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
        bool result = SigningKey.ValidateStatusTransition(
            executionContext, SigningKeyStatus.Active, (SigningKeyStatus)99);

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
        var kid = Kid.CreateNew("key-2024-01-01");
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);

        // Act
        LogAct("Calling IsValid");
        bool result = SigningKey.IsValid(
            executionContext, entityInfo, kid, "RS256", "pubkey", "privkey",
            SigningKeyStatus.Active, expiresAt);

        // Assert
        LogAssert("Verifying all fields are valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullKid_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Kid");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null Kid");
        bool result = SigningKey.IsValid(
            executionContext, entityInfo, null, "RS256", "pubkey", "privkey",
            SigningKeyStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullAlgorithm_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Algorithm");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null Algorithm");
        bool result = SigningKey.IsValid(
            executionContext, entityInfo, Kid.CreateNew("kid"), null, "pubkey", "privkey",
            SigningKeyStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

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
        bool result = SigningKey.IsValid(
            executionContext, entityInfo, Kid.CreateNew("kid"), "RS256", null, "privkey",
            SigningKeyStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullEncryptedPrivateKey_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null EncryptedPrivateKey");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null EncryptedPrivateKey");
        bool result = SigningKey.IsValid(
            executionContext, entityInfo, Kid.CreateNew("kid"), "RS256", "pubkey", null,
            SigningKeyStatus.Active, DateTimeOffset.UtcNow.AddDays(365));

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
        bool result = SigningKey.IsValid(
            executionContext, entityInfo, Kid.CreateNew("kid"), "RS256", "pubkey", "privkey",
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
        bool result = SigningKey.IsValid(
            executionContext, entityInfo, Kid.CreateNew("kid"), "RS256", "pubkey", "privkey",
            SigningKeyStatus.Active, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Change Tests

    [Fact]
    public void ChangeKidMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = SigningKeyMetadata.KidIsRequired;

        try
        {
            // Act
            LogAct("Changing Kid metadata");
            SigningKeyMetadata.ChangeKidMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            SigningKeyMetadata.KidIsRequired.ShouldBeFalse();
        }
        finally
        {
            SigningKeyMetadata.ChangeKidMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangeAlgorithmMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = SigningKeyMetadata.AlgorithmIsRequired;
        int originalMaxLength = SigningKeyMetadata.AlgorithmMaxLength;

        try
        {
            // Act
            LogAct("Changing Algorithm metadata");
            SigningKeyMetadata.ChangeAlgorithmMetadata(isRequired: false, maxLength: 50);

            // Assert
            LogAssert("Verifying metadata was updated");
            SigningKeyMetadata.AlgorithmIsRequired.ShouldBeFalse();
            SigningKeyMetadata.AlgorithmMaxLength.ShouldBe(50);
        }
        finally
        {
            SigningKeyMetadata.ChangeAlgorithmMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void ChangePublicKeyMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = SigningKeyMetadata.PublicKeyIsRequired;
        int originalMaxLength = SigningKeyMetadata.PublicKeyMaxLength;

        try
        {
            // Act
            LogAct("Changing PublicKey metadata");
            SigningKeyMetadata.ChangePublicKeyMetadata(isRequired: false, maxLength: 1024);

            // Assert
            LogAssert("Verifying metadata was updated");
            SigningKeyMetadata.PublicKeyIsRequired.ShouldBeFalse();
            SigningKeyMetadata.PublicKeyMaxLength.ShouldBe(1024);
        }
        finally
        {
            SigningKeyMetadata.ChangePublicKeyMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void ChangeEncryptedPrivateKeyMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = SigningKeyMetadata.EncryptedPrivateKeyIsRequired;
        int originalMaxLength = SigningKeyMetadata.EncryptedPrivateKeyMaxLength;

        try
        {
            // Act
            LogAct("Changing EncryptedPrivateKey metadata");
            SigningKeyMetadata.ChangeEncryptedPrivateKeyMetadata(isRequired: false, maxLength: 4096);

            // Assert
            LogAssert("Verifying metadata was updated");
            SigningKeyMetadata.EncryptedPrivateKeyIsRequired.ShouldBeFalse();
            SigningKeyMetadata.EncryptedPrivateKeyMaxLength.ShouldBe(4096);
        }
        finally
        {
            SigningKeyMetadata.ChangeEncryptedPrivateKeyMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = SigningKeyMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata");
            SigningKeyMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            SigningKeyMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            SigningKeyMetadata.ChangeStatusMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangeExpiresAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = SigningKeyMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata");
            SigningKeyMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            SigningKeyMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            SigningKeyMetadata.ChangeExpiresAtMetadata(isRequired: originalIsRequired);
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

    private static SigningKey CreateTestSigningKey(ExecutionContext executionContext)
    {
        var input = CreateValidRegisterNewInput();
        return SigningKey.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewSigningKeyInput CreateValidRegisterNewInput()
    {
        return new RegisterNewSigningKeyInput(
            Kid.CreateNew("key-2024-01-01"),
            "RS256",
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A",
            "encrypted-private-key-data",
            DateTimeOffset.UtcNow.AddDays(365));
    }

    #endregion
}
