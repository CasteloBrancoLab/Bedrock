using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using SessionMetadata = ShopDemo.Auth.Domain.Entities.Sessions.Session.SessionMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Sessions;

public class SessionTests : TestBase
{
    public SessionTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateSession()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewSessionInput(
            userId, refreshTokenId, "Chrome on Windows", "192.168.1.1",
            "Mozilla/5.0", expiresAt);

        // Act
        LogAct("Registering new session");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying session was created successfully");
        session.ShouldNotBeNull();
        session.UserId.ShouldBe(userId);
        session.RefreshTokenId.ShouldBe(refreshTokenId);
        session.DeviceInfo.ShouldBe("Chrome on Windows");
        session.IpAddress.ShouldBe("192.168.1.1");
        session.UserAgent.ShouldBe("Mozilla/5.0");
        session.ExpiresAt.ShouldBe(expiresAt);
        session.Status.ShouldBe(SessionStatus.Active);
        session.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new session");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        session.ShouldNotBeNull();
        session.Status.ShouldBe(SessionStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldSetLastActivityAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new session");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying LastActivityAt matches ExecutionContext.Timestamp");
        session.ShouldNotBeNull();
        session.LastActivityAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void RegisterNew_ShouldSetRevokedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new session");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt is null");
        session.ShouldNotBeNull();
        session.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new session");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        session.ShouldNotBeNull();
        session.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithNullOptionalFields_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating input with null optional fields");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewSessionInput(
            userId, refreshTokenId, null, null, null, expiresAt);

        // Act
        LogAct("Registering new session with null optional fields");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying session was created with null optional fields");
        session.ShouldNotBeNull();
        session.DeviceInfo.ShouldBeNull();
        session.IpAddress.ShouldBeNull();
        session.UserAgent.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithDefaultUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default UserId");
        var executionContext = CreateTestExecutionContext();
        var userId = default(Id);
        var refreshTokenId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewSessionInput(
            userId, refreshTokenId, null, null, null, expiresAt);

        // Act
        LogAct("Registering new session with default UserId");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        session.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultRefreshTokenId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default RefreshTokenId");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = default(Id);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewSessionInput(
            userId, refreshTokenId, null, null, null, expiresAt);

        // Act
        LogAct("Registering new session with default RefreshTokenId");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        session.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDeviceInfoExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with DeviceInfo exceeding max length");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();
        string longDeviceInfo = new('a', SessionMetadata.DeviceInfoMaxLength + 1);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewSessionInput(
            userId, refreshTokenId, longDeviceInfo, null, null, expiresAt);

        // Act
        LogAct("Registering new session with too-long DeviceInfo");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        session.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithIpAddressExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with IpAddress exceeding max length");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();
        string longIpAddress = new('1', SessionMetadata.IpAddressMaxLength + 1);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewSessionInput(
            userId, refreshTokenId, null, longIpAddress, null, expiresAt);

        // Act
        LogAct("Registering new session with too-long IpAddress");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        session.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithUserAgentExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with UserAgent exceeding max length");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();
        string longUserAgent = new('a', SessionMetadata.UserAgentMaxLength + 1);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewSessionInput(
            userId, refreshTokenId, null, null, longUserAgent, expiresAt);

        // Act
        LogAct("Registering new session with too-long UserAgent");
        var session = Session.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        session.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateSessionWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing session");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var refreshTokenId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var lastActivityAt = DateTimeOffset.UtcNow;
        var revokedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var input = new CreateFromExistingInfoSessionInput(
            entityInfo, userId, refreshTokenId, "Device", "10.0.0.1",
            "UserAgent", expiresAt, SessionStatus.Revoked, lastActivityAt, revokedAt);

        // Act
        LogAct("Creating session from existing info");
        var session = Session.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        session.EntityInfo.ShouldBe(entityInfo);
        session.UserId.ShouldBe(userId);
        session.RefreshTokenId.ShouldBe(refreshTokenId);
        session.DeviceInfo.ShouldBe("Device");
        session.IpAddress.ShouldBe("10.0.0.1");
        session.UserAgent.ShouldBe("UserAgent");
        session.ExpiresAt.ShouldBe(expiresAt);
        session.Status.ShouldBe(SessionStatus.Revoked);
        session.LastActivityAt.ShouldBe(lastActivityAt);
        session.RevokedAt.ShouldBe(revokedAt);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with default UserId (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoSessionInput(
            entityInfo, default(Id), default(Id), null, null, null,
            DateTimeOffset.UtcNow.AddHours(1), SessionStatus.Active,
            DateTimeOffset.UtcNow, null);

        // Act
        LogAct("Creating session from existing info with default UserId");
        var session = Session.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying session was created without validation");
        session.ShouldNotBeNull();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating session");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);

        // Act
        LogAct("Cloning session");
        var clone = session.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(session);
        clone.UserId.ShouldBe(session.UserId);
        clone.RefreshTokenId.ShouldBe(session.RefreshTokenId);
        clone.DeviceInfo.ShouldBe(session.DeviceInfo);
        clone.IpAddress.ShouldBe(session.IpAddress);
        clone.UserAgent.ShouldBe(session.UserAgent);
        clone.ExpiresAt.ShouldBe(session.ExpiresAt);
        clone.Status.ShouldBe(session.Status);
        clone.LastActivityAt.ShouldBe(session.LastActivityAt);
        clone.RevokedAt.ShouldBe(session.RevokedAt);
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_WithActiveSession_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active session");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);
        var input = new RevokeSessionInput();

        // Act
        LogAct("Revoking session");
        var result = session.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying session was revoked");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(SessionStatus.Revoked);
        result.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active session");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);
        var input = new RevokeSessionInput();

        // Act
        LogAct("Revoking session");
        var result = session.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.RevokedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void Revoke_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active session");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);
        var input = new RevokeSessionInput();

        // Act
        LogAct("Revoking session");
        var result = session.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(session);
        session.Status.ShouldBe(SessionStatus.Active);
        result.Status.ShouldBe(SessionStatus.Revoked);
    }

    [Fact]
    public void Revoke_WithAlreadyRevokedSession_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating revoked session");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);
        var revokedSession = session.Revoke(executionContext, new RevokeSessionInput())!;
        var input = new RevokeSessionInput();

        // Act
        LogAct("Attempting to revoke already revoked session");
        var newContext = CreateTestExecutionContext();
        var result = revokedSession.Revoke(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Revoked -> Revoked transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region UpdateActivity Tests

    [Fact]
    public void UpdateActivity_WithActiveSession_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active session");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);
        var input = new UpdateSessionActivityInput();

        // Act
        LogAct("Updating activity (same tenant context)");
        var result = session.UpdateActivity(executionContext, input);

        // Assert
        LogAssert("Verifying activity was updated");
        result.ShouldNotBeNull();
        result.LastActivityAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void UpdateActivity_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active session");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);
        var input = new UpdateSessionActivityInput();

        // Act
        LogAct("Updating activity (same tenant context)");
        var result = session.UpdateActivity(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(session);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidSession_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid session");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = session.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidSession_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating session with invalid state via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoSessionInput(
            entityInfo, default(Id), Id.GenerateNewId(), null, null, null,
            DateTimeOffset.UtcNow.AddHours(1), SessionStatus.Active,
            DateTimeOffset.UtcNow, null);
        var session = Session.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on session with default UserId");
        bool result = session.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for default UserId");
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
        bool result = Session.ValidateUserId(executionContext, userId);

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
        bool result = Session.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateRefreshTokenId Tests

    [Fact]
    public void ValidateRefreshTokenId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var refreshTokenId = Id.GenerateNewId();

        // Act
        LogAct("Validating valid RefreshTokenId");
        bool result = Session.ValidateRefreshTokenId(executionContext, refreshTokenId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRefreshTokenId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null RefreshTokenId");
        bool result = Session.ValidateRefreshTokenId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateDeviceInfo Tests

    [Fact]
    public void ValidateDeviceInfo_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid DeviceInfo");
        bool result = Session.ValidateDeviceInfo(executionContext, "Chrome on Windows");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDeviceInfo_WithNull_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null DeviceInfo (optional field)");
        bool result = Session.ValidateDeviceInfo(executionContext, null);

        // Assert
        LogAssert("Verifying validation passes for null (optional)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDeviceInfo_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating DeviceInfo at max length");
        var executionContext = CreateTestExecutionContext();
        string deviceInfo = new('a', SessionMetadata.DeviceInfoMaxLength);

        // Act
        LogAct("Validating max-length DeviceInfo");
        bool result = Session.ValidateDeviceInfo(executionContext, deviceInfo);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDeviceInfo_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating DeviceInfo exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string deviceInfo = new('a', SessionMetadata.DeviceInfoMaxLength + 1);

        // Act
        LogAct("Validating too-long DeviceInfo");
        bool result = Session.ValidateDeviceInfo(executionContext, deviceInfo);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateIpAddress Tests

    [Fact]
    public void ValidateIpAddress_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid IpAddress");
        bool result = Session.ValidateIpAddress(executionContext, "192.168.1.1");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIpAddress_WithNull_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null IpAddress (optional field)");
        bool result = Session.ValidateIpAddress(executionContext, null);

        // Assert
        LogAssert("Verifying validation passes for null (optional)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIpAddress_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating IpAddress at max length");
        var executionContext = CreateTestExecutionContext();
        string ipAddress = new('1', SessionMetadata.IpAddressMaxLength);

        // Act
        LogAct("Validating max-length IpAddress");
        bool result = Session.ValidateIpAddress(executionContext, ipAddress);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIpAddress_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating IpAddress exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string ipAddress = new('1', SessionMetadata.IpAddressMaxLength + 1);

        // Act
        LogAct("Validating too-long IpAddress");
        bool result = Session.ValidateIpAddress(executionContext, ipAddress);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateUserAgent Tests

    [Fact]
    public void ValidateUserAgent_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid UserAgent");
        bool result = Session.ValidateUserAgent(executionContext, "Mozilla/5.0");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUserAgent_WithNull_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null UserAgent (optional field)");
        bool result = Session.ValidateUserAgent(executionContext, null);

        // Assert
        LogAssert("Verifying validation passes for null (optional)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUserAgent_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating UserAgent at max length");
        var executionContext = CreateTestExecutionContext();
        string userAgent = new('a', SessionMetadata.UserAgentMaxLength);

        // Act
        LogAct("Validating max-length UserAgent");
        bool result = Session.ValidateUserAgent(executionContext, userAgent);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUserAgent_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating UserAgent exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string userAgent = new('a', SessionMetadata.UserAgentMaxLength + 1);

        // Act
        LogAct("Validating too-long UserAgent");
        bool result = Session.ValidateUserAgent(executionContext, userAgent);

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
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        LogAct("Validating valid ExpiresAt");
        bool result = Session.ValidateExpiresAt(executionContext, expiresAt);

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
        bool result = Session.ValidateExpiresAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(SessionStatus.Active)]
    [InlineData(SessionStatus.Revoked)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(SessionStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = Session.ValidateStatus(executionContext, status);

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
        bool result = Session.ValidateStatus(executionContext, null);

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
        bool result = Session.ValidateStatusTransition(executionContext, SessionStatus.Active, SessionStatus.Revoked);

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
        bool result = Session.ValidateStatusTransition(executionContext, SessionStatus.Revoked, SessionStatus.Active);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(SessionStatus.Active)]
    [InlineData(SessionStatus.Revoked)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(SessionStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = Session.ValidateStatusTransition(executionContext, status, status);

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
        bool result = Session.ValidateStatusTransition(executionContext, null, SessionStatus.Active);

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
        bool result = Session.ValidateStatusTransition(executionContext, SessionStatus.Active, null);

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
        bool result = Session.ValidateStatusTransition(
            executionContext, SessionStatus.Active, (SessionStatus)99);

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
        var refreshTokenId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        LogAct("Calling IsValid");
        bool result = Session.IsValid(
            executionContext, entityInfo, userId, refreshTokenId,
            "Device", "10.0.0.1", "UserAgent", expiresAt, SessionStatus.Active);

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
        bool result = Session.IsValid(
            executionContext, entityInfo, null, Id.GenerateNewId(),
            null, null, null, DateTimeOffset.UtcNow.AddHours(1), SessionStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullRefreshTokenId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null RefreshTokenId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null RefreshTokenId");
        bool result = Session.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), null,
            null, null, null, DateTimeOffset.UtcNow.AddHours(1), SessionStatus.Active);

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
        bool result = Session.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), Id.GenerateNewId(),
            null, null, null, null, SessionStatus.Active);

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
        bool result = Session.IsValid(
            executionContext, entityInfo, Id.GenerateNewId(), Id.GenerateNewId(),
            null, null, null, DateTimeOffset.UtcNow.AddHours(1), null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Change Tests

    [Fact]
    public void ChangeUserIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = SessionMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata");
            SessionMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            SessionMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            SessionMetadata.ChangeUserIdMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeRefreshTokenIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = SessionMetadata.RefreshTokenIdIsRequired;

        try
        {
            // Act
            LogAct("Changing RefreshTokenId metadata");
            SessionMetadata.ChangeRefreshTokenIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            SessionMetadata.RefreshTokenIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            SessionMetadata.ChangeRefreshTokenIdMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeDeviceInfoMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        int originalMaxLength = SessionMetadata.DeviceInfoMaxLength;

        try
        {
            // Act
            LogAct("Changing DeviceInfo metadata");
            SessionMetadata.ChangeDeviceInfoMetadata(maxLength: 1000);

            // Assert
            LogAssert("Verifying updated value");
            SessionMetadata.DeviceInfoMaxLength.ShouldBe(1000);
        }
        finally
        {
            SessionMetadata.ChangeDeviceInfoMetadata(originalMaxLength);
        }
    }

    [Fact]
    public void ChangeIpAddressMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        int originalMaxLength = SessionMetadata.IpAddressMaxLength;

        try
        {
            // Act
            LogAct("Changing IpAddress metadata");
            SessionMetadata.ChangeIpAddressMetadata(maxLength: 100);

            // Assert
            LogAssert("Verifying updated value");
            SessionMetadata.IpAddressMaxLength.ShouldBe(100);
        }
        finally
        {
            SessionMetadata.ChangeIpAddressMetadata(originalMaxLength);
        }
    }

    [Fact]
    public void ChangeUserAgentMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        int originalMaxLength = SessionMetadata.UserAgentMaxLength;

        try
        {
            // Act
            LogAct("Changing UserAgent metadata");
            SessionMetadata.ChangeUserAgentMetadata(maxLength: 2048);

            // Assert
            LogAssert("Verifying updated value");
            SessionMetadata.UserAgentMaxLength.ShouldBe(2048);
        }
        finally
        {
            SessionMetadata.ChangeUserAgentMetadata(originalMaxLength);
        }
    }

    [Fact]
    public void ChangeExpiresAtMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = SessionMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata");
            SessionMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            SessionMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            SessionMetadata.ChangeExpiresAtMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = SessionMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata");
            SessionMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            SessionMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            SessionMetadata.ChangeStatusMetadata(originalIsRequired);
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

    private static Session CreateTestSession(ExecutionContext executionContext)
    {
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewSessionInput(
            userId, refreshTokenId, "Chrome on Windows",
            "192.168.1.1", "Mozilla/5.0", expiresAt);
        return Session.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewSessionInput CreateValidRegisterNewInput()
    {
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        return new RegisterNewSessionInput(
            userId, refreshTokenId, "Chrome on Windows",
            "192.168.1.1", "Mozilla/5.0", expiresAt);
    }

    #endregion
}
