using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;
using ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using LoginAttemptMetadata = ShopDemo.Auth.Domain.Entities.LoginAttempts.LoginAttempt.LoginAttemptMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.LoginAttempts;

public class LoginAttemptTests : TestBase
{
    public LoginAttemptTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid data");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewLoginAttemptInput(
            "john.doe", "192.168.1.1", true, null);

        // Act
        LogAct("Registering new LoginAttempt");
        var entity = LoginAttempt.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.Username.ShouldBe("john.doe");
        entity.IpAddress.ShouldBe("192.168.1.1");
        entity.IsSuccessful.ShouldBeTrue();
        entity.FailureReason.ShouldBeNull();
        entity.AttemptedAt.ShouldNotBe(default);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithFailedAttempt_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input for failed login attempt");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewLoginAttemptInput(
            "john.doe", "10.0.0.1", false, "Invalid password");

        // Act
        LogAct("Registering new failed LoginAttempt");
        var entity = LoginAttempt.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with failure details");
        entity.ShouldNotBeNull();
        entity.IsSuccessful.ShouldBeFalse();
        entity.FailureReason.ShouldBe("Invalid password");
    }

    [Fact]
    public void RegisterNew_WithNullIpAddress_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with null IpAddress (optional field)");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewLoginAttemptInput(
            "jane.doe", null, true, null);

        // Act
        LogAct("Registering new LoginAttempt with null IpAddress");
        var entity = LoginAttempt.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with null IpAddress");
        entity.ShouldNotBeNull();
        entity.IpAddress.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithNullFailureReason_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with null FailureReason");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewLoginAttemptInput(
            "user@example.com", "127.0.0.1", true, null);

        // Act
        LogAct("Registering new LoginAttempt with null FailureReason");
        var entity = LoginAttempt.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with null FailureReason");
        entity.ShouldNotBeNull();
        entity.FailureReason.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithNullUsername_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Username");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewLoginAttemptInput(
            null!, "192.168.1.1", true, null);

        // Act
        LogAct("Registering new LoginAttempt with null Username");
        var entity = LoginAttempt.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithUsernameExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Username exceeding max length of 255");
        var executionContext = CreateTestExecutionContext();
        var longUsername = new string('u', 256);
        var input = new RegisterNewLoginAttemptInput(
            longUsername, "192.168.1.1", true, null);

        // Act
        LogAct("Registering new LoginAttempt with oversized Username");
        var entity = LoginAttempt.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithUsernameAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with Username at exactly max length of 255");
        var executionContext = CreateTestExecutionContext();
        var maxUsername = new string('u', 255);
        var input = new RegisterNewLoginAttemptInput(
            maxUsername, null, true, null);

        // Act
        LogAct("Registering new LoginAttempt with Username at boundary");
        var entity = LoginAttempt.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.Username.ShouldBe(maxUsername);
    }

    [Fact]
    public void RegisterNew_ShouldSetAttemptedAtFromExecutionContextTimestamp()
    {
        // Arrange
        LogArrange("Creating execution context to verify AttemptedAt is set from timestamp");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewLoginAttemptInput(
            "user", null, true, null);

        // Act
        LogAct("Registering new LoginAttempt");
        var entity = LoginAttempt.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying AttemptedAt matches execution context timestamp");
        entity.ShouldNotBeNull();
        entity.AttemptedAt.ShouldBe(executionContext.Timestamp);
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing LoginAttempt");
        var entityInfo = CreateTestEntityInfo();
        var attemptedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var input = new CreateFromExistingInfoLoginAttemptInput(
            entityInfo, "existing.user", "10.20.30.40", attemptedAt, false, "Account locked");

        // Act
        LogAct("Creating LoginAttempt from existing info");
        var entity = LoginAttempt.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.Username.ShouldBe("existing.user");
        entity.IpAddress.ShouldBe("10.20.30.40");
        entity.AttemptedAt.ShouldBe(attemptedAt);
        entity.IsSuccessful.ShouldBeFalse();
        entity.FailureReason.ShouldBe("Account locked");
    }

    [Fact]
    public void CreateFromExistingInfo_WithNullOptionalFields_ShouldPreserveNulls()
    {
        // Arrange
        LogArrange("Creating properties with null optional fields");
        var entityInfo = CreateTestEntityInfo();
        var attemptedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoLoginAttemptInput(
            entityInfo, "user", null, attemptedAt, true, null);

        // Act
        LogAct("Creating LoginAttempt from existing info with nulls");
        var entity = LoginAttempt.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying null fields are preserved");
        entity.ShouldNotBeNull();
        entity.IpAddress.ShouldBeNull();
        entity.FailureReason.ShouldBeNull();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating LoginAttempt via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewLoginAttemptInput(
            "clone.user", "172.16.0.1", false, "Timeout");
        var entity = LoginAttempt.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning LoginAttempt");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.Username.ShouldBe(entity.Username);
        clone.IpAddress.ShouldBe(entity.IpAddress);
        clone.AttemptedAt.ShouldBe(entity.AttemptedAt);
        clone.IsSuccessful.ShouldBe(entity.IsSuccessful);
        clone.FailureReason.ShouldBe(entity.FailureReason);
    }

    [Fact]
    public void Clone_WithNullOptionalFields_ShouldPreserveNulls()
    {
        // Arrange
        LogArrange("Creating LoginAttempt with null optional fields");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewLoginAttemptInput(
            "user", null, true, null);
        var entity = LoginAttempt.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning LoginAttempt with null optional fields");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone preserves null fields");
        clone.ShouldNotBeNull();
        clone.IpAddress.ShouldBeNull();
        clone.FailureReason.ShouldBeNull();
    }

    #endregion

    #region ValidateUsername Tests

    [Fact]
    public void ValidateUsername_WithValidUsername_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid username");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Username");
        bool result = LoginAttempt.ValidateUsername(executionContext, "valid.user");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Username");
        bool result = LoginAttempt.ValidateUsername(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_WithEmpty_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Username (empty string is not null, passes IsRequired)");
        bool result = LoginAttempt.ValidateUsername(executionContext, "");

        // Assert
        LogAssert("Verifying validation passes (empty string has length 0 which is within max length)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized username");
        var executionContext = CreateTestExecutionContext();
        var longUsername = new string('u', 256);

        // Act
        LogAct("Validating oversized Username");
        bool result = LoginAttempt.ValidateUsername(executionContext, longUsername);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and username at max length");
        var executionContext = CreateTestExecutionContext();
        var maxUsername = new string('u', 255);

        // Act
        LogAct("Validating Username at max length boundary");
        bool result = LoginAttempt.ValidateUsername(executionContext, maxUsername);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateAttemptedAt Tests

    [Fact]
    public void ValidateAttemptedAt_WithValidDate_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid AttemptedAt");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid AttemptedAt");
        bool result = LoginAttempt.ValidateAttemptedAt(executionContext, DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateAttemptedAt_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null AttemptedAt");
        bool result = LoginAttempt.ValidateAttemptedAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_WithAllValidFields_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and all valid fields");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Validating all fields");
        bool result = LoginAttempt.IsValid(
            executionContext, entityInfo, "valid.user", DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullUsername_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null Username");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Validating with null Username");
        bool result = LoginAttempt.IsValid(
            executionContext, entityInfo, null, DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullAttemptedAt_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null AttemptedAt");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Validating with null AttemptedAt");
        bool result = LoginAttempt.IsValid(
            executionContext, entityInfo, "valid.user", null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeUsernameMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original Username metadata values");
        bool originalIsRequired = LoginAttemptMetadata.UsernameIsRequired;
        int originalMaxLength = LoginAttemptMetadata.UsernameMaxLength;

        try
        {
            // Act
            LogAct("Changing Username metadata");
            LoginAttemptMetadata.ChangeUsernameMetadata(isRequired: false, maxLength: 500);

            // Assert
            LogAssert("Verifying Username metadata was updated");
            LoginAttemptMetadata.UsernameIsRequired.ShouldBeFalse();
            LoginAttemptMetadata.UsernameMaxLength.ShouldBe(500);
        }
        finally
        {
            LoginAttemptMetadata.ChangeUsernameMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeIpAddressMetadata_ShouldUpdateMaxLength()
    {
        // Arrange
        LogArrange("Saving original IpAddress metadata values");
        int originalMaxLength = LoginAttemptMetadata.IpAddressMaxLength;

        try
        {
            // Act
            LogAct("Changing IpAddress metadata");
            LoginAttemptMetadata.ChangeIpAddressMetadata(maxLength: 100);

            // Assert
            LogAssert("Verifying IpAddress metadata was updated");
            LoginAttemptMetadata.IpAddressMaxLength.ShouldBe(100);
        }
        finally
        {
            LoginAttemptMetadata.ChangeIpAddressMetadata(maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeAttemptedAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original AttemptedAtIsRequired value");
        bool originalIsRequired = LoginAttemptMetadata.AttemptedAtIsRequired;

        try
        {
            // Act
            LogAct("Changing AttemptedAt metadata to not required");
            LoginAttemptMetadata.ChangeAttemptedAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying AttemptedAtIsRequired was updated");
            LoginAttemptMetadata.AttemptedAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            LoginAttemptMetadata.ChangeAttemptedAtMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeFailureReasonMetadata_ShouldUpdateMaxLength()
    {
        // Arrange
        LogArrange("Saving original FailureReason metadata values");
        int originalMaxLength = LoginAttemptMetadata.FailureReasonMaxLength;

        try
        {
            // Act
            LogAct("Changing FailureReason metadata");
            LoginAttemptMetadata.ChangeFailureReasonMetadata(maxLength: 500);

            // Assert
            LogAssert("Verifying FailureReason metadata was updated");
            LoginAttemptMetadata.FailureReasonMaxLength.ShouldBe(500);
        }
        finally
        {
            LoginAttemptMetadata.ChangeFailureReasonMetadata(maxLength: originalMaxLength);
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

    #endregion
}
