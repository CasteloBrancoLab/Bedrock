using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ApiKeyMetadata = ShopDemo.Auth.Domain.Entities.ApiKeys.ApiKey.ApiKeyMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ApiKeys;

public class ApiKeyTests : TestBase
{
    public ApiKeyTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateApiKey()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(90);
        var input = new RegisterNewApiKeyInput(serviceClientId, "sk_live_", "hash_abc123", expiresAt);

        // Act
        LogAct("Registering new API key");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying API key was created successfully");
        apiKey.ShouldNotBeNull();
        apiKey.ServiceClientId.ShouldBe(serviceClientId);
        apiKey.KeyPrefix.ShouldBe("sk_live_");
        apiKey.KeyHash.ShouldBe("hash_abc123");
        apiKey.Status.ShouldBe(ApiKeyStatus.Active);
        apiKey.ExpiresAt.ShouldBe(expiresAt);
        apiKey.LastUsedAt.ShouldBeNull();
        apiKey.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new API key");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        apiKey.ShouldNotBeNull();
        apiKey.Status.ShouldBe(ApiKeyStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new API key");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        apiKey.ShouldNotBeNull();
        apiKey.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldSetLastUsedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new API key");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying LastUsedAt is null");
        apiKey.ShouldNotBeNull();
        apiKey.LastUsedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldSetRevokedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new API key");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt is null");
        apiKey.ShouldNotBeNull();
        apiKey.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithNullExpiresAt_ShouldCreateApiKey()
    {
        // Arrange
        LogArrange("Creating input with null ExpiresAt");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewApiKeyInput(Id.GenerateNewId(), "sk_live_", "hash_abc123", null);

        // Act
        LogAct("Registering new API key with null ExpiresAt");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying API key was created with null ExpiresAt");
        apiKey.ShouldNotBeNull();
        apiKey.ExpiresAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithEmptyKeyPrefix_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty KeyPrefix");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewApiKeyInput(Id.GenerateNewId(), "", "hash_abc123", null);

        // Act
        LogAct("Registering new API key with empty KeyPrefix");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        apiKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullKeyPrefix_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null KeyPrefix");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewApiKeyInput(Id.GenerateNewId(), null!, "hash_abc123", null);

        // Act
        LogAct("Registering new API key with null KeyPrefix");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        apiKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithKeyPrefixExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with KeyPrefix exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longPrefix = new('a', ApiKeyMetadata.KeyPrefixMaxLength + 1);
        var input = new RegisterNewApiKeyInput(Id.GenerateNewId(), longPrefix, "hash_abc123", null);

        // Act
        LogAct("Registering new API key with too-long KeyPrefix");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        apiKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyKeyHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty KeyHash");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewApiKeyInput(Id.GenerateNewId(), "sk_live_", "", null);

        // Act
        LogAct("Registering new API key with empty KeyHash");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        apiKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullKeyHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null KeyHash");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewApiKeyInput(Id.GenerateNewId(), "sk_live_", null!, null);

        // Act
        LogAct("Registering new API key with null KeyHash");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        apiKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithKeyHashExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with KeyHash exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longHash = new('x', ApiKeyMetadata.KeyHashMaxLength + 1);
        var input = new RegisterNewApiKeyInput(Id.GenerateNewId(), "sk_live_", longHash, null);

        // Act
        LogAct("Registering new API key with too-long KeyHash");
        var apiKey = ApiKey.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        apiKey.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateApiKeyWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing API key");
        var entityInfo = CreateTestEntityInfo();
        var serviceClientId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddDays(90);
        var lastUsedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var revokedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoApiKeyInput(
            entityInfo, serviceClientId, "sk_live_", "hash_abc123",
            ApiKeyStatus.Revoked, expiresAt, lastUsedAt, revokedAt);

        // Act
        LogAct("Creating API key from existing info");
        var apiKey = ApiKey.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        apiKey.EntityInfo.ShouldBe(entityInfo);
        apiKey.ServiceClientId.ShouldBe(serviceClientId);
        apiKey.KeyPrefix.ShouldBe("sk_live_");
        apiKey.KeyHash.ShouldBe("hash_abc123");
        apiKey.Status.ShouldBe(ApiKeyStatus.Revoked);
        apiKey.ExpiresAt.ShouldBe(expiresAt);
        apiKey.LastUsedAt.ShouldBe(lastUsedAt);
        apiKey.RevokedAt.ShouldBe(revokedAt);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with empty KeyPrefix (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoApiKeyInput(
            entityInfo, Id.GenerateNewId(), "", "hash",
            ApiKeyStatus.Active, null, null, null);

        // Act
        LogAct("Creating API key from existing info with empty KeyPrefix");
        var apiKey = ApiKey.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying API key was created without validation");
        apiKey.ShouldNotBeNull();
        apiKey.KeyPrefix.ShouldBe("");
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_WithActiveApiKey_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);
        var input = new RevokeApiKeyInput();

        // Act
        LogAct("Revoking API key");
        var result = apiKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying API key was revoked");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApiKeyStatus.Revoked);
        result.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);
        var input = new RevokeApiKeyInput();

        // Act
        LogAct("Revoking API key");
        var result = apiKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.RevokedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void Revoke_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);
        var input = new RevokeApiKeyInput();

        // Act
        LogAct("Revoking API key");
        var result = apiKey.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(apiKey);
        apiKey.Status.ShouldBe(ApiKeyStatus.Active);
        result.Status.ShouldBe(ApiKeyStatus.Revoked);
    }

    [Fact]
    public void Revoke_WithRevokedApiKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating revoked API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);
        var revokedKey = apiKey.Revoke(executionContext, new RevokeApiKeyInput())!;
        var input = new RevokeApiKeyInput();

        // Act
        LogAct("Attempting to revoke already revoked API key");
        var newContext = CreateTestExecutionContext();
        var result = revokedKey.Revoke(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Revoked -> Revoked transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region RecordUsage Tests

    [Fact]
    public void RecordUsage_WithActiveApiKey_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);
        var input = new RecordApiKeyUsageInput();

        // Act
        LogAct("Recording usage");
        var result = apiKey.RecordUsage(executionContext, input);

        // Assert
        LogAssert("Verifying usage was recorded");
        result.ShouldNotBeNull();
        result.LastUsedAt.ShouldNotBeNull();
    }

    [Fact]
    public void RecordUsage_ShouldSetLastUsedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);
        var input = new RecordApiKeyUsageInput();

        // Act
        LogAct("Recording usage");
        var result = apiKey.RecordUsage(executionContext, input);

        // Assert
        LogAssert("Verifying LastUsedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.LastUsedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void RecordUsage_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);
        var input = new RecordApiKeyUsageInput();

        // Act
        LogAct("Recording usage");
        var result = apiKey.RecordUsage(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(apiKey);
        apiKey.LastUsedAt.ShouldBeNull();
        result.LastUsedAt.ShouldNotBeNull();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);

        // Act
        LogAct("Cloning API key");
        var clone = apiKey.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(apiKey);
        clone.ServiceClientId.ShouldBe(apiKey.ServiceClientId);
        clone.KeyPrefix.ShouldBe(apiKey.KeyPrefix);
        clone.KeyHash.ShouldBe(apiKey.KeyHash);
        clone.Status.ShouldBe(apiKey.Status);
        clone.ExpiresAt.ShouldBe(apiKey.ExpiresAt);
        clone.LastUsedAt.ShouldBe(apiKey.LastUsedAt);
        clone.RevokedAt.ShouldBe(apiKey.RevokedAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidApiKey_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid API key");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = apiKey.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidApiKey_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating API key with invalid state via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoApiKeyInput(
            entityInfo, Id.GenerateNewId(), "", "hash",
            ApiKeyStatus.Active, null, null, null);
        var apiKey = ApiKey.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on API key with empty KeyPrefix");
        bool result = apiKey.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty KeyPrefix");
        result.ShouldBeFalse();
        validationContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateServiceClientId Tests

    [Fact]
    public void ValidateServiceClientId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();

        // Act
        LogAct("Validating valid ServiceClientId");
        bool result = ApiKey.ValidateServiceClientId(executionContext, serviceClientId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateServiceClientId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ServiceClientId");
        bool result = ApiKey.ValidateServiceClientId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateKeyPrefix Tests

    [Fact]
    public void ValidateKeyPrefix_WithValidPrefix_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid KeyPrefix");
        bool result = ApiKey.ValidateKeyPrefix(executionContext, "sk_live_");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyPrefix_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null KeyPrefix");
        bool result = ApiKey.ValidateKeyPrefix(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyPrefix_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty KeyPrefix");
        bool result = ApiKey.ValidateKeyPrefix(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyPrefix_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating KeyPrefix at max length");
        var executionContext = CreateTestExecutionContext();
        string prefix = new('a', ApiKeyMetadata.KeyPrefixMaxLength);

        // Act
        LogAct("Validating max-length KeyPrefix");
        bool result = ApiKey.ValidateKeyPrefix(executionContext, prefix);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyPrefix_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating KeyPrefix exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string prefix = new('a', ApiKeyMetadata.KeyPrefixMaxLength + 1);

        // Act
        LogAct("Validating too-long KeyPrefix");
        bool result = ApiKey.ValidateKeyPrefix(executionContext, prefix);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateKeyHash Tests

    [Fact]
    public void ValidateKeyHash_WithValidHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid KeyHash");
        bool result = ApiKey.ValidateKeyHash(executionContext, "hash_abc123");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyHash_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null KeyHash");
        bool result = ApiKey.ValidateKeyHash(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyHash_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty KeyHash");
        bool result = ApiKey.ValidateKeyHash(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyHash_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating KeyHash at max length");
        var executionContext = CreateTestExecutionContext();
        string hash = new('x', ApiKeyMetadata.KeyHashMaxLength);

        // Act
        LogAct("Validating max-length KeyHash");
        bool result = ApiKey.ValidateKeyHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateKeyHash_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating KeyHash exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string hash = new('x', ApiKeyMetadata.KeyHashMaxLength + 1);

        // Act
        LogAct("Validating too-long KeyHash");
        bool result = ApiKey.ValidateKeyHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(ApiKeyStatus.Active)]
    [InlineData(ApiKeyStatus.Revoked)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(ApiKeyStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = ApiKey.ValidateStatus(executionContext, status);

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
        bool result = ApiKey.ValidateStatus(executionContext, null);

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
        bool result = ApiKey.ValidateStatusTransition(executionContext, ApiKeyStatus.Active, ApiKeyStatus.Revoked);

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
        bool result = ApiKey.ValidateStatusTransition(executionContext, ApiKeyStatus.Revoked, ApiKeyStatus.Active);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ApiKeyStatus.Active)]
    [InlineData(ApiKeyStatus.Revoked)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(ApiKeyStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = ApiKey.ValidateStatusTransition(executionContext, status, status);

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
        bool result = ApiKey.ValidateStatusTransition(executionContext, null, ApiKeyStatus.Revoked);

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
        bool result = ApiKey.ValidateStatusTransition(executionContext, ApiKeyStatus.Active, null);

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
        bool result = ApiKey.ValidateStatusTransition(executionContext, ApiKeyStatus.Active, (ApiKeyStatus)99);

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
        var serviceClientId = Id.GenerateNewId();

        // Act
        LogAct("Calling IsValid");
        bool result = ApiKey.IsValid(
            executionContext, entityInfo, serviceClientId, "sk_live_", "hash_abc123", ApiKeyStatus.Active);

        // Assert
        LogAssert("Verifying all fields are valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullServiceClientId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null ServiceClientId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null ServiceClientId");
        bool result = ApiKey.IsValid(
            executionContext, entityInfo, null, "sk_live_", "hash_abc123", ApiKeyStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullKeyPrefix_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null KeyPrefix");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null KeyPrefix");
        bool result = ApiKey.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), null, "hash_abc123", ApiKeyStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullKeyHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null KeyHash");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null KeyHash");
        bool result = ApiKey.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), "sk_live_", null, ApiKeyStatus.Active);

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
        bool result = ApiKey.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), "sk_live_", "hash_abc123", null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Change Tests

    [Fact]
    public void ChangeServiceClientIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = ApiKeyMetadata.ServiceClientIdIsRequired;

        try
        {
            // Act
            LogAct("Changing ServiceClientId metadata");
            ApiKeyMetadata.ChangeServiceClientIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            ApiKeyMetadata.ServiceClientIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ApiKeyMetadata.ChangeServiceClientIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void ChangeKeyPrefixMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = ApiKeyMetadata.KeyPrefixIsRequired;
        int originalMaxLength = ApiKeyMetadata.KeyPrefixMaxLength;

        try
        {
            // Act
            LogAct("Changing KeyPrefix metadata");
            ApiKeyMetadata.ChangeKeyPrefixMetadata(isRequired: false, maxLength: 64);

            // Assert
            LogAssert("Verifying metadata was updated");
            ApiKeyMetadata.KeyPrefixIsRequired.ShouldBeFalse();
            ApiKeyMetadata.KeyPrefixMaxLength.ShouldBe(64);
        }
        finally
        {
            ApiKeyMetadata.ChangeKeyPrefixMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void ChangeKeyHashMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = ApiKeyMetadata.KeyHashIsRequired;
        int originalMaxLength = ApiKeyMetadata.KeyHashMaxLength;

        try
        {
            // Act
            LogAct("Changing KeyHash metadata");
            ApiKeyMetadata.ChangeKeyHashMetadata(isRequired: false, maxLength: 256);

            // Assert
            LogAssert("Verifying metadata was updated");
            ApiKeyMetadata.KeyHashIsRequired.ShouldBeFalse();
            ApiKeyMetadata.KeyHashMaxLength.ShouldBe(256);
        }
        finally
        {
            ApiKeyMetadata.ChangeKeyHashMetadata(isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        bool originalIsRequired = ApiKeyMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata");
            ApiKeyMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying metadata was updated");
            ApiKeyMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            ApiKeyMetadata.ChangeStatusMetadata(isRequired: originalIsRequired);
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

    private static ApiKey CreateTestApiKey(ExecutionContext executionContext)
    {
        var serviceClientId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(90);
        var input = new RegisterNewApiKeyInput(serviceClientId, "sk_live_", "hash_abc123", expiresAt);
        return ApiKey.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewApiKeyInput CreateValidRegisterNewInput()
    {
        return new RegisterNewApiKeyInput(
            Id.GenerateNewId(), "sk_live_", "hash_abc123", DateTimeOffset.UtcNow.AddDays(90));
    }

    #endregion
}
