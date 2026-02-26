using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.UserConsents;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;
using ShopDemo.Auth.Domain.Entities.UserConsents.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using UserConsentMetadata = ShopDemo.Auth.Domain.Entities.UserConsents.UserConsent.UserConsentMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.UserConsents;

public class UserConsentTests : TestBase
{
    public UserConsentTests(ITestOutputHelper outputHelper) : base(outputHelper)
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
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var ipAddress = "192.168.1.1";
        var input = new RegisterNewUserConsentInput(userId, consentTermId, ipAddress);

        // Act
        LogAct("Registering new UserConsent");
        var entity = UserConsent.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.UserId.ShouldBe(userId);
        entity.ConsentTermId.ShouldBe(consentTermId);
        entity.AcceptedAt.ShouldBe(executionContext.Timestamp);
        entity.Status.ShouldBe(UserConsentStatus.Active);
        entity.RevokedAt.ShouldBeNull();
        entity.IpAddress.ShouldBe(ipAddress);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithNullIpAddress_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with null IpAddress");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewUserConsentInput(userId, consentTermId, null);

        // Act
        LogAct("Registering new UserConsent with null IpAddress");
        var entity = UserConsent.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with null IpAddress");
        entity.ShouldNotBeNull();
        entity.IpAddress.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldSetAcceptedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new UserConsent");
        var entity = UserConsent.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying AcceptedAt matches ExecutionContext.Timestamp");
        entity.ShouldNotBeNull();
        entity.AcceptedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new UserConsent");
        var entity = UserConsent.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying Status is Active");
        entity.ShouldNotBeNull();
        entity.Status.ShouldBe(UserConsentStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetRevokedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new UserConsent");
        var entity = UserConsent.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt is null");
        entity.ShouldNotBeNull();
        entity.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithDefaultUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default UserId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var userId = default(Id);
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewUserConsentInput(userId, consentTermId, null);

        // Act
        LogAct("Registering new UserConsent with default UserId");
        var entity = UserConsent.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultConsentTermId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default ConsentTermId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var consentTermId = default(Id);
        var input = new RegisterNewUserConsentInput(userId, consentTermId, null);

        // Act
        LogAct("Registering new UserConsent with default ConsentTermId");
        var entity = UserConsent.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithIpAddressExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with IpAddress exceeding max length of 45");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var ipAddress = new string('1', UserConsentMetadata.IpAddressMaxLength + 1);
        var input = new RegisterNewUserConsentInput(userId, consentTermId, ipAddress);

        // Act
        LogAct("Registering new UserConsent with too-long IpAddress");
        var entity = UserConsent.RegisterNew(executionContext, input);

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
        LogArrange("Creating all properties for existing UserConsent");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var acceptedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var status = UserConsentStatus.Revoked;
        var revokedAt = DateTimeOffset.UtcNow;
        var ipAddress = "10.0.0.1";
        var input = new CreateFromExistingInfoUserConsentInput(
            entityInfo, userId, consentTermId, acceptedAt, status, revokedAt, ipAddress);

        // Act
        LogAct("Creating UserConsent from existing info");
        var entity = UserConsent.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.UserId.ShouldBe(userId);
        entity.ConsentTermId.ShouldBe(consentTermId);
        entity.AcceptedAt.ShouldBe(acceptedAt);
        entity.Status.ShouldBe(status);
        entity.RevokedAt.ShouldBe(revokedAt);
        entity.IpAddress.ShouldBe(ipAddress);
    }

    [Fact]
    public void CreateFromExistingInfo_WithActiveState_ShouldPreserveNullRevokedAt()
    {
        // Arrange
        LogArrange("Creating input with active state");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoUserConsentInput(
            entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            DateTimeOffset.UtcNow, UserConsentStatus.Active, null, null);

        // Act
        LogAct("Creating UserConsent from existing info with active state");
        var entity = UserConsent.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying Status is Active and RevokedAt is null");
        entity.Status.ShouldBe(UserConsentStatus.Active);
        entity.RevokedAt.ShouldBeNull();
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_WithActiveConsent_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active UserConsent");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestUserConsent(executionContext);
        var input = new RevokeUserConsentInput();

        // Act
        LogAct("Revoking UserConsent");
        var result = entity.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying consent was revoked");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(UserConsentStatus.Revoked);
        result.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active UserConsent");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestUserConsent(executionContext);
        var input = new RevokeUserConsentInput();

        // Act
        LogAct("Revoking UserConsent");
        var result = entity.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.RevokedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void Revoke_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active UserConsent");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestUserConsent(executionContext);
        var input = new RevokeUserConsentInput();

        // Act
        LogAct("Revoking UserConsent");
        var result = entity.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(entity);
        entity.Status.ShouldBe(UserConsentStatus.Active);
        result.Status.ShouldBe(UserConsentStatus.Revoked);
    }

    [Fact]
    public void Revoke_WithRevokedConsent_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating already-revoked UserConsent");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestUserConsent(executionContext);
        var revokedEntity = entity.Revoke(executionContext, new RevokeUserConsentInput())!;
        var input = new RevokeUserConsentInput();

        // Act
        LogAct("Attempting to revoke already-revoked consent");
        var newContext = CreateTestExecutionContext();
        var result = revokedEntity.Revoke(newContext, input);

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
        LogArrange("Creating UserConsent via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestUserConsent(executionContext);

        // Act
        LogAct("Cloning UserConsent");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.UserId.ShouldBe(entity.UserId);
        clone.ConsentTermId.ShouldBe(entity.ConsentTermId);
        clone.AcceptedAt.ShouldBe(entity.AcceptedAt);
        clone.Status.ShouldBe(entity.Status);
        clone.RevokedAt.ShouldBe(entity.RevokedAt);
        clone.IpAddress.ShouldBe(entity.IpAddress);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidConsent_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid UserConsent");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestUserConsent(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = entity.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
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
        bool result = UserConsent.ValidateUserId(executionContext, userId);

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
        bool result = UserConsent.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateConsentTermId Tests

    [Fact]
    public void ValidateConsentTermId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ConsentTermId");
        var executionContext = CreateTestExecutionContext();
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating valid ConsentTermId");
        bool result = UserConsent.ValidateConsentTermId(executionContext, consentTermId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateConsentTermId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ConsentTermId");
        bool result = UserConsent.ValidateConsentTermId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateAcceptedAt Tests

    [Fact]
    public void ValidateAcceptedAt_WithValidDate_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid AcceptedAt");
        var executionContext = CreateTestExecutionContext();
        var acceptedAt = DateTimeOffset.UtcNow;

        // Act
        LogAct("Validating valid AcceptedAt");
        bool result = UserConsent.ValidateAcceptedAt(executionContext, acceptedAt);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateAcceptedAt_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null AcceptedAt");
        bool result = UserConsent.ValidateAcceptedAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(UserConsentStatus.Active)]
    [InlineData(UserConsentStatus.Revoked)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(UserConsentStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = UserConsent.ValidateStatus(executionContext, status);

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
        bool result = UserConsent.ValidateStatus(executionContext, null);

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
        LogAct("Validating transition Active -> Revoked");
        bool result = UserConsent.ValidateStatusTransition(
            executionContext, UserConsentStatus.Active, UserConsentStatus.Revoked);

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
        LogAct("Validating transition Revoked -> Active");
        bool result = UserConsent.ValidateStatusTransition(
            executionContext, UserConsentStatus.Revoked, UserConsentStatus.Active);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(UserConsentStatus.Active)]
    [InlineData(UserConsentStatus.Revoked)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(UserConsentStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = UserConsent.ValidateStatusTransition(executionContext, status, status);

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
        bool result = UserConsent.ValidateStatusTransition(
            executionContext, null, UserConsentStatus.Revoked);

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
        bool result = UserConsent.ValidateStatusTransition(
            executionContext, UserConsentStatus.Active, null);

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
        bool result = UserConsent.ValidateStatusTransition(
            executionContext, UserConsentStatus.Active, (UserConsentStatus)99);

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
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var acceptedAt = DateTimeOffset.UtcNow;
        var status = UserConsentStatus.Active;

        // Act
        LogAct("Calling IsValid");
        bool result = UserConsent.IsValid(
            executionContext, entityInfo, userId, consentTermId, acceptedAt, status);

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
        bool result = UserConsent.IsValid(
            executionContext, entityInfo, null, Id.CreateFromExistingInfo(Guid.NewGuid()),
            DateTimeOffset.UtcNow, UserConsentStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullConsentTermId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null ConsentTermId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null ConsentTermId");
        bool result = UserConsent.IsValid(
            executionContext, entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()), null,
            DateTimeOffset.UtcNow, UserConsentStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullAcceptedAt_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null AcceptedAt");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null AcceptedAt");
        bool result = UserConsent.IsValid(
            executionContext, entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()), null, UserConsentStatus.Active);

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
        bool result = UserConsent.IsValid(
            executionContext, entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()), DateTimeOffset.UtcNow, null);

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
        bool originalIsRequired = UserConsentMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata to not required");
            UserConsentMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying UserIdIsRequired was updated");
            UserConsentMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            UserConsentMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeConsentTermIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ConsentTermIdIsRequired value");
        bool originalIsRequired = UserConsentMetadata.ConsentTermIdIsRequired;

        try
        {
            // Act
            LogAct("Changing ConsentTermId metadata to not required");
            UserConsentMetadata.ChangeConsentTermIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ConsentTermIdIsRequired was updated");
            UserConsentMetadata.ConsentTermIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            UserConsentMetadata.ChangeConsentTermIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeAcceptedAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original AcceptedAtIsRequired value");
        bool originalIsRequired = UserConsentMetadata.AcceptedAtIsRequired;

        try
        {
            // Act
            LogAct("Changing AcceptedAt metadata to not required");
            UserConsentMetadata.ChangeAcceptedAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying AcceptedAtIsRequired was updated");
            UserConsentMetadata.AcceptedAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            UserConsentMetadata.ChangeAcceptedAtMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeStatusMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original StatusIsRequired value");
        bool originalIsRequired = UserConsentMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata to not required");
            UserConsentMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying StatusIsRequired was updated");
            UserConsentMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            UserConsentMetadata.ChangeStatusMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeIpAddressMetadata_ShouldUpdateMaxLength()
    {
        // Arrange
        LogArrange("Saving original IpAddress metadata values");
        int originalMaxLength = UserConsentMetadata.IpAddressMaxLength;

        try
        {
            // Act
            LogAct("Changing IpAddress metadata");
            UserConsentMetadata.ChangeIpAddressMetadata(maxLength: 100);

            // Assert
            LogAssert("Verifying IpAddress metadata was updated");
            UserConsentMetadata.IpAddressMaxLength.ShouldBe(100);
        }
        finally
        {
            UserConsentMetadata.ChangeIpAddressMetadata(maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        LogArrange("Reading default metadata values");

        // Assert
        LogAssert("Verifying default metadata values");
        UserConsentMetadata.UserIdPropertyName.ShouldBe("UserId");
        UserConsentMetadata.UserIdIsRequired.ShouldBeTrue();
        UserConsentMetadata.ConsentTermIdPropertyName.ShouldBe("ConsentTermId");
        UserConsentMetadata.ConsentTermIdIsRequired.ShouldBeTrue();
        UserConsentMetadata.AcceptedAtPropertyName.ShouldBe("AcceptedAt");
        UserConsentMetadata.AcceptedAtIsRequired.ShouldBeTrue();
        UserConsentMetadata.StatusPropertyName.ShouldBe("Status");
        UserConsentMetadata.StatusIsRequired.ShouldBeTrue();
        UserConsentMetadata.IpAddressPropertyName.ShouldBe("IpAddress");
        UserConsentMetadata.IpAddressMaxLength.ShouldBe(45);
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

    private static UserConsent CreateTestUserConsent(ExecutionContext executionContext)
    {
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var ipAddress = "192.168.1.100";
        var input = new RegisterNewUserConsentInput(userId, consentTermId, ipAddress);
        return UserConsent.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewUserConsentInput CreateValidRegisterNewInput()
    {
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var consentTermId = Id.CreateFromExistingInfo(Guid.NewGuid());
        return new RegisterNewUserConsentInput(userId, consentTermId, "10.0.0.1");
    }

    #endregion
}
