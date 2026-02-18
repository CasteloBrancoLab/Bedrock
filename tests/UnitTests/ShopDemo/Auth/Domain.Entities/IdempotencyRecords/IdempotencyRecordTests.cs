using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using IdempotencyRecordMetadata = ShopDemo.Auth.Domain.Entities.IdempotencyRecords.IdempotencyRecord.IdempotencyRecordMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.IdempotencyRecords;

public class IdempotencyRecordTests : TestBase
{
    public IdempotencyRecordTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid properties");
        var executionContext = CreateTestExecutionContext();
        var idempotencyKey = Guid.NewGuid().ToString();
        var requestHash = "request-hash-sha256";
        var input = new RegisterNewIdempotencyRecordInput(idempotencyKey, requestHash);

        // Act
        LogAct("Registering new IdempotencyRecord");
        var entity = IdempotencyRecord.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.IdempotencyKey.ShouldBe(idempotencyKey);
        entity.RequestHash.ShouldBe(requestHash);
        entity.ResponseBody.ShouldBeNull();
        entity.StatusCode.ShouldBe(0);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldSetResponseBodyToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new IdempotencyRecord");
        var entity = IdempotencyRecord.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying ResponseBody is null");
        entity.ShouldNotBeNull();
        entity.ResponseBody.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldSetStatusCodeToZero()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new IdempotencyRecord");
        var entity = IdempotencyRecord.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying StatusCode is 0");
        entity.ShouldNotBeNull();
        entity.StatusCode.ShouldBe(0);
    }

    [Fact]
    public void RegisterNew_ShouldSetExpiresAtToTimestampPlus24Hours()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new IdempotencyRecord");
        var entity = IdempotencyRecord.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying ExpiresAt is approximately now + 24 hours");
        entity.ShouldNotBeNull();
        var expectedExpiresAt = executionContext.Timestamp.Add(TimeSpan.FromHours(24));
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void RegisterNew_WithEmptyIdempotencyKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty IdempotencyKey");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewIdempotencyRecordInput(string.Empty, "valid-hash");

        // Act
        LogAct("Registering new IdempotencyRecord with empty IdempotencyKey");
        var entity = IdempotencyRecord.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithIdempotencyKeyExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with IdempotencyKey exceeding max length of 36");
        var executionContext = CreateTestExecutionContext();
        var idempotencyKey = new string('a', IdempotencyRecordMetadata.IdempotencyKeyMaxLength + 1);
        var input = new RegisterNewIdempotencyRecordInput(idempotencyKey, "valid-hash");

        // Act
        LogAct("Registering new IdempotencyRecord with too-long IdempotencyKey");
        var entity = IdempotencyRecord.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyRequestHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty RequestHash");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewIdempotencyRecordInput(Guid.NewGuid().ToString(), string.Empty);

        // Act
        LogAct("Registering new IdempotencyRecord with empty RequestHash");
        var entity = IdempotencyRecord.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithRequestHashExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with RequestHash exceeding max length of 128");
        var executionContext = CreateTestExecutionContext();
        var requestHash = new string('a', IdempotencyRecordMetadata.RequestHashMaxLength + 1);
        var input = new RegisterNewIdempotencyRecordInput(Guid.NewGuid().ToString(), requestHash);

        // Act
        LogAct("Registering new IdempotencyRecord with too-long RequestHash");
        var entity = IdempotencyRecord.RegisterNew(executionContext, input);

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
        LogArrange("Creating all properties for existing IdempotencyRecord");
        var entityInfo = CreateTestEntityInfo();
        var idempotencyKey = Guid.NewGuid().ToString();
        var requestHash = "existing-request-hash";
        var responseBody = "{\"result\": \"ok\"}";
        var statusCode = 200;
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
        var input = new CreateFromExistingInfoIdempotencyRecordInput(
            entityInfo, idempotencyKey, requestHash, responseBody, statusCode, expiresAt);

        // Act
        LogAct("Creating IdempotencyRecord from existing info");
        var entity = IdempotencyRecord.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.IdempotencyKey.ShouldBe(idempotencyKey);
        entity.RequestHash.ShouldBe(requestHash);
        entity.ResponseBody.ShouldBe(responseBody);
        entity.StatusCode.ShouldBe(statusCode);
        entity.ExpiresAt.ShouldBe(expiresAt);
    }

    [Fact]
    public void CreateFromExistingInfo_WithNullResponseBody_ShouldPreserveNull()
    {
        // Arrange
        LogArrange("Creating input with null ResponseBody");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoIdempotencyRecordInput(
            entityInfo, "key", "hash", null, 0, DateTimeOffset.UtcNow.AddHours(24));

        // Act
        LogAct("Creating IdempotencyRecord from existing info with null ResponseBody");
        var entity = IdempotencyRecord.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying ResponseBody is null");
        entity.ResponseBody.ShouldBeNull();
        entity.StatusCode.ShouldBe(0);
    }

    #endregion

    #region SetResponse Tests

    [Fact]
    public void SetResponse_WithValidInput_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord without response");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestIdempotencyRecord(executionContext);
        var responseBody = "{\"result\": \"success\"}";
        var statusCode = 200;
        var input = new SetResponseIdempotencyRecordInput(responseBody, statusCode);

        // Act
        LogAct("Setting response on IdempotencyRecord");
        var result = entity.SetResponse(executionContext, input);

        // Assert
        LogAssert("Verifying response was set correctly");
        result.ShouldNotBeNull();
        result.ResponseBody.ShouldBe(responseBody);
        result.StatusCode.ShouldBe(statusCode);
    }

    [Fact]
    public void SetResponse_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord without response");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestIdempotencyRecord(executionContext);
        var input = new SetResponseIdempotencyRecordInput("{\"ok\": true}", 201);

        // Act
        LogAct("Setting response on IdempotencyRecord");
        var result = entity.SetResponse(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(entity);
        entity.ResponseBody.ShouldBeNull();
        result.ResponseBody.ShouldNotBeNull();
    }

    [Fact]
    public void SetResponse_WithResponseBodyExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord and response body exceeding max length");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestIdempotencyRecord(executionContext);
        var responseBody = new string('a', IdempotencyRecordMetadata.ResponseBodyMaxLength + 1);
        var input = new SetResponseIdempotencyRecordInput(responseBody, 200);

        // Act
        LogAct("Setting too-long response on IdempotencyRecord");
        var newContext = CreateTestExecutionContext();
        var result = entity.SetResponse(newContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestIdempotencyRecord(executionContext);

        // Act
        LogAct("Cloning IdempotencyRecord");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.IdempotencyKey.ShouldBe(entity.IdempotencyKey);
        clone.RequestHash.ShouldBe(entity.RequestHash);
        clone.ResponseBody.ShouldBe(entity.ResponseBody);
        clone.StatusCode.ShouldBe(entity.StatusCode);
        clone.ExpiresAt.ShouldBe(entity.ExpiresAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidRecord_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid IdempotencyRecord");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestIdempotencyRecord(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = entity.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidRecord_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord with empty IdempotencyKey via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoIdempotencyRecordInput(
            entityInfo, string.Empty, "hash", null, 0, DateTimeOffset.UtcNow.AddHours(24));
        var entity = IdempotencyRecord.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on record with empty IdempotencyKey");
        bool result = entity.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty IdempotencyKey");
        result.ShouldBeFalse();
        validationContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateIdempotencyKey Tests

    [Fact]
    public void ValidateIdempotencyKey_WithValidKey_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid IdempotencyKey");
        var executionContext = CreateTestExecutionContext();
        var key = Guid.NewGuid().ToString();

        // Act
        LogAct("Validating valid IdempotencyKey");
        bool result = IdempotencyRecord.ValidateIdempotencyKey(executionContext, key);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIdempotencyKey_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null IdempotencyKey");
        bool result = IdempotencyRecord.ValidateIdempotencyKey(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIdempotencyKey_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty IdempotencyKey");
        bool result = IdempotencyRecord.ValidateIdempotencyKey(executionContext, string.Empty);

        // Assert
        LogAssert("Verifying validation fails for empty string");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIdempotencyKey_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating IdempotencyKey at max length of 36");
        var executionContext = CreateTestExecutionContext();
        var key = new string('a', IdempotencyRecordMetadata.IdempotencyKeyMaxLength);

        // Act
        LogAct("Validating max-length IdempotencyKey");
        bool result = IdempotencyRecord.ValidateIdempotencyKey(executionContext, key);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIdempotencyKey_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating IdempotencyKey exceeding max length of 36");
        var executionContext = CreateTestExecutionContext();
        var key = new string('a', IdempotencyRecordMetadata.IdempotencyKeyMaxLength + 1);

        // Act
        LogAct("Validating too-long IdempotencyKey");
        bool result = IdempotencyRecord.ValidateIdempotencyKey(executionContext, key);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateRequestHash Tests

    [Fact]
    public void ValidateRequestHash_WithValidHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid RequestHash");
        var executionContext = CreateTestExecutionContext();
        var hash = "valid-request-hash";

        // Act
        LogAct("Validating valid RequestHash");
        bool result = IdempotencyRecord.ValidateRequestHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRequestHash_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null RequestHash");
        bool result = IdempotencyRecord.ValidateRequestHash(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRequestHash_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty RequestHash");
        bool result = IdempotencyRecord.ValidateRequestHash(executionContext, string.Empty);

        // Assert
        LogAssert("Verifying validation fails for empty string");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRequestHash_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating RequestHash at max length of 128");
        var executionContext = CreateTestExecutionContext();
        var hash = new string('a', IdempotencyRecordMetadata.RequestHashMaxLength);

        // Act
        LogAct("Validating max-length RequestHash");
        bool result = IdempotencyRecord.ValidateRequestHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRequestHash_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating RequestHash exceeding max length of 128");
        var executionContext = CreateTestExecutionContext();
        var hash = new string('a', IdempotencyRecordMetadata.RequestHashMaxLength + 1);

        // Act
        LogAct("Validating too-long RequestHash");
        bool result = IdempotencyRecord.ValidateRequestHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateResponseBody Tests

    [Fact]
    public void ValidateResponseBody_WithNull_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ResponseBody");
        bool result = IdempotencyRecord.ValidateResponseBody(executionContext, null);

        // Assert
        LogAssert("Verifying null is valid (ResponseBody is optional)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateResponseBody_WithValidBody_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ResponseBody");
        var executionContext = CreateTestExecutionContext();
        var body = "{\"result\": \"ok\"}";

        // Act
        LogAct("Validating valid ResponseBody");
        bool result = IdempotencyRecord.ValidateResponseBody(executionContext, body);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateResponseBody_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating ResponseBody at max length of 1048576");
        var executionContext = CreateTestExecutionContext();
        var body = new string('a', IdempotencyRecordMetadata.ResponseBodyMaxLength);

        // Act
        LogAct("Validating max-length ResponseBody");
        bool result = IdempotencyRecord.ValidateResponseBody(executionContext, body);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateResponseBody_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating ResponseBody exceeding max length of 1048576");
        var executionContext = CreateTestExecutionContext();
        var body = new string('a', IdempotencyRecordMetadata.ResponseBodyMaxLength + 1);

        // Act
        LogAct("Validating too-long ResponseBody");
        bool result = IdempotencyRecord.ValidateResponseBody(executionContext, body);

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
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        // Act
        LogAct("Validating valid ExpiresAt");
        bool result = IdempotencyRecord.ValidateExpiresAt(executionContext, expiresAt);

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
        bool result = IdempotencyRecord.ValidateExpiresAt(executionContext, null);

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
        var idempotencyKey = Guid.NewGuid().ToString();
        var requestHash = "valid-hash";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        // Act
        LogAct("Calling IsValid");
        bool result = IdempotencyRecord.IsValid(
            executionContext, entityInfo, idempotencyKey, requestHash, expiresAt);

        // Assert
        LogAssert("Verifying all fields are valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullIdempotencyKey_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null IdempotencyKey");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null IdempotencyKey");
        bool result = IdempotencyRecord.IsValid(
            executionContext, entityInfo, null, "valid-hash", DateTimeOffset.UtcNow.AddHours(24));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullRequestHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null RequestHash");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null RequestHash");
        bool result = IdempotencyRecord.IsValid(
            executionContext, entityInfo, "key", null, DateTimeOffset.UtcNow.AddHours(24));

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
        bool result = IdempotencyRecord.IsValid(
            executionContext, entityInfo, "key", "hash", null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeIdempotencyKeyMetadata_ShouldUpdateIsRequiredAndMaxLength()
    {
        // Arrange
        LogArrange("Saving original IdempotencyKey metadata values");
        bool originalIsRequired = IdempotencyRecordMetadata.IdempotencyKeyIsRequired;
        int originalMaxLength = IdempotencyRecordMetadata.IdempotencyKeyMaxLength;

        try
        {
            // Act
            LogAct("Changing IdempotencyKey metadata");
            IdempotencyRecordMetadata.ChangeIdempotencyKeyMetadata(isRequired: false, maxLength: 64);

            // Assert
            LogAssert("Verifying IdempotencyKey metadata was updated");
            IdempotencyRecordMetadata.IdempotencyKeyIsRequired.ShouldBeFalse();
            IdempotencyRecordMetadata.IdempotencyKeyMaxLength.ShouldBe(64);
        }
        finally
        {
            IdempotencyRecordMetadata.ChangeIdempotencyKeyMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeRequestHashMetadata_ShouldUpdateIsRequiredAndMaxLength()
    {
        // Arrange
        LogArrange("Saving original RequestHash metadata values");
        bool originalIsRequired = IdempotencyRecordMetadata.RequestHashIsRequired;
        int originalMaxLength = IdempotencyRecordMetadata.RequestHashMaxLength;

        try
        {
            // Act
            LogAct("Changing RequestHash metadata");
            IdempotencyRecordMetadata.ChangeRequestHashMetadata(isRequired: false, maxLength: 256);

            // Assert
            LogAssert("Verifying RequestHash metadata was updated");
            IdempotencyRecordMetadata.RequestHashIsRequired.ShouldBeFalse();
            IdempotencyRecordMetadata.RequestHashMaxLength.ShouldBe(256);
        }
        finally
        {
            IdempotencyRecordMetadata.ChangeRequestHashMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeResponseBodyMetadata_ShouldUpdateMaxLength()
    {
        // Arrange
        LogArrange("Saving original ResponseBody metadata values");
        int originalMaxLength = IdempotencyRecordMetadata.ResponseBodyMaxLength;

        try
        {
            // Act
            LogAct("Changing ResponseBody metadata");
            IdempotencyRecordMetadata.ChangeResponseBodyMetadata(maxLength: 2097152);

            // Assert
            LogAssert("Verifying ResponseBody metadata was updated");
            IdempotencyRecordMetadata.ResponseBodyMaxLength.ShouldBe(2097152);
        }
        finally
        {
            IdempotencyRecordMetadata.ChangeResponseBodyMetadata(maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeExpiresAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ExpiresAtIsRequired value");
        bool originalIsRequired = IdempotencyRecordMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata to not required");
            IdempotencyRecordMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ExpiresAtIsRequired was updated");
            IdempotencyRecordMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            IdempotencyRecordMetadata.ChangeExpiresAtMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        LogArrange("Reading default metadata values");

        // Assert
        LogAssert("Verifying default metadata values");
        IdempotencyRecordMetadata.IdempotencyKeyPropertyName.ShouldBe("IdempotencyKey");
        IdempotencyRecordMetadata.IdempotencyKeyIsRequired.ShouldBeTrue();
        IdempotencyRecordMetadata.IdempotencyKeyMaxLength.ShouldBe(36);
        IdempotencyRecordMetadata.RequestHashPropertyName.ShouldBe("RequestHash");
        IdempotencyRecordMetadata.RequestHashIsRequired.ShouldBeTrue();
        IdempotencyRecordMetadata.RequestHashMaxLength.ShouldBe(128);
        IdempotencyRecordMetadata.ResponseBodyPropertyName.ShouldBe("ResponseBody");
        IdempotencyRecordMetadata.ResponseBodyMaxLength.ShouldBe(1048576);
        IdempotencyRecordMetadata.ExpiresAtPropertyName.ShouldBe("ExpiresAt");
        IdempotencyRecordMetadata.ExpiresAtIsRequired.ShouldBeTrue();
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

    private static IdempotencyRecord CreateTestIdempotencyRecord(ExecutionContext executionContext)
    {
        var idempotencyKey = Guid.NewGuid().ToString();
        var requestHash = "test-request-hash";
        var input = new RegisterNewIdempotencyRecordInput(idempotencyKey, requestHash);
        return IdempotencyRecord.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewIdempotencyRecordInput CreateValidRegisterNewInput()
    {
        var idempotencyKey = Guid.NewGuid().ToString();
        var requestHash = "valid-request-hash";
        return new RegisterNewIdempotencyRecordInput(idempotencyKey, requestHash);
    }

    #endregion
}
