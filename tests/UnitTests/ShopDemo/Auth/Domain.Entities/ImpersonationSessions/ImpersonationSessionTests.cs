using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Enums;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ImpersonationSessionMetadata = ShopDemo.Auth.Domain.Entities.ImpersonationSessions.ImpersonationSession.ImpersonationSessionMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ImpersonationSessions;

public class ImpersonationSessionTests : TestBase
{
    public ImpersonationSessionTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid properties");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var targetUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewImpersonationSessionInput(operatorUserId, targetUserId, expiresAt);

        // Act
        LogAct("Registering new ImpersonationSession");
        var entity = ImpersonationSession.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.OperatorUserId.ShouldBe(operatorUserId);
        entity.TargetUserId.ShouldBe(targetUserId);
        entity.ExpiresAt.ShouldBe(expiresAt);
        entity.Status.ShouldBe(ImpersonationSessionStatus.Active);
        entity.EndedAt.ShouldBeNull();
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new ImpersonationSession");
        var entity = ImpersonationSession.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying Status is Active");
        entity.ShouldNotBeNull();
        entity.Status.ShouldBe(ImpersonationSessionStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetEndedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new ImpersonationSession");
        var entity = ImpersonationSession.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EndedAt is null");
        entity.ShouldNotBeNull();
        entity.EndedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithDefaultOperatorUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default OperatorUserId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = default(Id);
        var targetUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewImpersonationSessionInput(operatorUserId, targetUserId, expiresAt);

        // Act
        LogAct("Registering new ImpersonationSession with default OperatorUserId");
        var entity = ImpersonationSession.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultTargetUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default TargetUserId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var targetUserId = default(Id);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewImpersonationSessionInput(operatorUserId, targetUserId, expiresAt);

        // Act
        LogAct("Registering new ImpersonationSession with default TargetUserId");
        var entity = ImpersonationSession.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithSameOperatorAndTargetUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input where OperatorUserId equals TargetUserId (self-impersonation)");
        var executionContext = CreateTestExecutionContext();
        var sameUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewImpersonationSessionInput(sameUserId, sameUserId, expiresAt);

        // Act
        LogAct("Registering new ImpersonationSession with self-impersonation");
        var entity = ImpersonationSession.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to self-impersonation prohibition");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing ImpersonationSession");
        var entityInfo = CreateTestEntityInfo();
        var operatorUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var targetUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var status = ImpersonationSessionStatus.Ended;
        var endedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoImpersonationSessionInput(
            entityInfo, operatorUserId, targetUserId, expiresAt, status, endedAt);

        // Act
        LogAct("Creating ImpersonationSession from existing info");
        var entity = ImpersonationSession.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.OperatorUserId.ShouldBe(operatorUserId);
        entity.TargetUserId.ShouldBe(targetUserId);
        entity.ExpiresAt.ShouldBe(expiresAt);
        entity.Status.ShouldBe(status);
        entity.EndedAt.ShouldBe(endedAt);
    }

    [Fact]
    public void CreateFromExistingInfo_WithActiveState_ShouldPreserveNullEndedAt()
    {
        // Arrange
        LogArrange("Creating input with active state");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoImpersonationSessionInput(
            entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            DateTimeOffset.UtcNow.AddHours(1),
            ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Creating ImpersonationSession from existing info with active state");
        var entity = ImpersonationSession.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying Status is Active and EndedAt is null");
        entity.Status.ShouldBe(ImpersonationSessionStatus.Active);
        entity.EndedAt.ShouldBeNull();
    }

    #endregion

    #region End Tests

    [Fact]
    public void End_WithActiveSession_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active ImpersonationSession");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestImpersonationSession(executionContext);
        var input = new EndImpersonationSessionInput();

        // Act
        LogAct("Ending ImpersonationSession");
        var result = entity.End(executionContext, input);

        // Assert
        LogAssert("Verifying session was ended");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ImpersonationSessionStatus.Ended);
        result.EndedAt.ShouldNotBeNull();
    }

    [Fact]
    public void End_ShouldSetEndedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active ImpersonationSession");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestImpersonationSession(executionContext);
        var input = new EndImpersonationSessionInput();

        // Act
        LogAct("Ending ImpersonationSession");
        var result = entity.End(executionContext, input);

        // Assert
        LogAssert("Verifying EndedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.EndedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void End_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active ImpersonationSession");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestImpersonationSession(executionContext);
        var input = new EndImpersonationSessionInput();

        // Act
        LogAct("Ending ImpersonationSession");
        var result = entity.End(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(entity);
        entity.Status.ShouldBe(ImpersonationSessionStatus.Active);
        result.Status.ShouldBe(ImpersonationSessionStatus.Ended);
    }

    [Fact]
    public void End_WithEndedSession_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating already-ended ImpersonationSession");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestImpersonationSession(executionContext);
        var endedEntity = entity.End(executionContext, new EndImpersonationSessionInput())!;
        var input = new EndImpersonationSessionInput();

        // Act
        LogAct("Attempting to end already-ended session");
        var newContext = CreateTestExecutionContext();
        var result = endedEntity.End(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Ended -> Ended transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating ImpersonationSession via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestImpersonationSession(executionContext);

        // Act
        LogAct("Cloning ImpersonationSession");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.OperatorUserId.ShouldBe(entity.OperatorUserId);
        clone.TargetUserId.ShouldBe(entity.TargetUserId);
        clone.ExpiresAt.ShouldBe(entity.ExpiresAt);
        clone.Status.ShouldBe(entity.Status);
        clone.EndedAt.ShouldBe(entity.EndedAt);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidSession_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid ImpersonationSession");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestImpersonationSession(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = entity.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateOperatorUserId Tests

    [Fact]
    public void ValidateOperatorUserId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid OperatorUserId");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating valid OperatorUserId");
        bool result = ImpersonationSession.ValidateOperatorUserId(executionContext, operatorUserId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateOperatorUserId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null OperatorUserId");
        bool result = ImpersonationSession.ValidateOperatorUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateTargetUserId Tests

    [Fact]
    public void ValidateTargetUserId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid TargetUserId");
        var executionContext = CreateTestExecutionContext();
        var targetUserId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating valid TargetUserId");
        bool result = ImpersonationSession.ValidateTargetUserId(executionContext, targetUserId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTargetUserId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null TargetUserId");
        bool result = ImpersonationSession.ValidateTargetUserId(executionContext, null);

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
        bool result = ImpersonationSession.ValidateExpiresAt(executionContext, expiresAt);

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
        bool result = ImpersonationSession.ValidateExpiresAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(ImpersonationSessionStatus.Active)]
    [InlineData(ImpersonationSessionStatus.Ended)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(ImpersonationSessionStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = ImpersonationSession.ValidateStatus(executionContext, status);

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
        bool result = ImpersonationSession.ValidateStatus(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Fact]
    public void ValidateStatusTransition_ActiveToEnded_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating transition Active -> Ended");
        bool result = ImpersonationSession.ValidateStatusTransition(
            executionContext, ImpersonationSessionStatus.Active, ImpersonationSessionStatus.Ended);

        // Assert
        LogAssert("Verifying transition is valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_EndedToActive_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating transition Ended -> Active");
        bool result = ImpersonationSession.ValidateStatusTransition(
            executionContext, ImpersonationSessionStatus.Ended, ImpersonationSessionStatus.Active);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ImpersonationSessionStatus.Active)]
    [InlineData(ImpersonationSessionStatus.Ended)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(ImpersonationSessionStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = ImpersonationSession.ValidateStatusTransition(executionContext, status, status);

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
        LogAct("Validating null -> Ended transition");
        bool result = ImpersonationSession.ValidateStatusTransition(
            executionContext, null, ImpersonationSessionStatus.Ended);

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
        bool result = ImpersonationSession.ValidateStatusTransition(
            executionContext, ImpersonationSessionStatus.Active, null);

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
        bool result = ImpersonationSession.ValidateStatusTransition(
            executionContext, ImpersonationSessionStatus.Active, (ImpersonationSessionStatus)99);

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
        var operatorUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var targetUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var status = ImpersonationSessionStatus.Active;

        // Act
        LogAct("Calling IsValid");
        bool result = ImpersonationSession.IsValid(
            executionContext, entityInfo, operatorUserId, targetUserId, expiresAt, status);

        // Assert
        LogAssert("Verifying all fields are valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullOperatorUserId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null OperatorUserId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null OperatorUserId");
        bool result = ImpersonationSession.IsValid(
            executionContext, entityInfo, null, Id.CreateFromExistingInfo(Guid.NewGuid()),
            DateTimeOffset.UtcNow.AddHours(1), ImpersonationSessionStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullTargetUserId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null TargetUserId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null TargetUserId");
        bool result = ImpersonationSession.IsValid(
            executionContext, entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()), null,
            DateTimeOffset.UtcNow.AddHours(1), ImpersonationSessionStatus.Active);

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
        bool result = ImpersonationSession.IsValid(
            executionContext, entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()), null, ImpersonationSessionStatus.Active);

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
        bool result = ImpersonationSession.IsValid(
            executionContext, entityInfo, Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()), DateTimeOffset.UtcNow.AddHours(1), null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeOperatorUserIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original OperatorUserIdIsRequired value");
        bool originalIsRequired = ImpersonationSessionMetadata.OperatorUserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing OperatorUserId metadata to not required");
            ImpersonationSessionMetadata.ChangeOperatorUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying OperatorUserIdIsRequired was updated");
            ImpersonationSessionMetadata.OperatorUserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ImpersonationSessionMetadata.ChangeOperatorUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeTargetUserIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original TargetUserIdIsRequired value");
        bool originalIsRequired = ImpersonationSessionMetadata.TargetUserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing TargetUserId metadata to not required");
            ImpersonationSessionMetadata.ChangeTargetUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying TargetUserIdIsRequired was updated");
            ImpersonationSessionMetadata.TargetUserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ImpersonationSessionMetadata.ChangeTargetUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeExpiresAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ExpiresAtIsRequired value");
        bool originalIsRequired = ImpersonationSessionMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata to not required");
            ImpersonationSessionMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ExpiresAtIsRequired was updated");
            ImpersonationSessionMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            ImpersonationSessionMetadata.ChangeExpiresAtMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeStatusMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original StatusIsRequired value");
        bool originalIsRequired = ImpersonationSessionMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata to not required");
            ImpersonationSessionMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying StatusIsRequired was updated");
            ImpersonationSessionMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            ImpersonationSessionMetadata.ChangeStatusMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        LogArrange("Reading default metadata values");

        // Assert
        LogAssert("Verifying default metadata values");
        ImpersonationSessionMetadata.OperatorUserIdPropertyName.ShouldBe("OperatorUserId");
        ImpersonationSessionMetadata.OperatorUserIdIsRequired.ShouldBeTrue();
        ImpersonationSessionMetadata.TargetUserIdPropertyName.ShouldBe("TargetUserId");
        ImpersonationSessionMetadata.TargetUserIdIsRequired.ShouldBeTrue();
        ImpersonationSessionMetadata.ExpiresAtPropertyName.ShouldBe("ExpiresAt");
        ImpersonationSessionMetadata.ExpiresAtIsRequired.ShouldBeTrue();
        ImpersonationSessionMetadata.StatusPropertyName.ShouldBe("Status");
        ImpersonationSessionMetadata.StatusIsRequired.ShouldBeTrue();
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

    private static ImpersonationSession CreateTestImpersonationSession(ExecutionContext executionContext)
    {
        var operatorUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var targetUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewImpersonationSessionInput(operatorUserId, targetUserId, expiresAt);
        return ImpersonationSession.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewImpersonationSessionInput CreateValidRegisterNewInput()
    {
        var operatorUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var targetUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        return new RegisterNewImpersonationSessionInput(operatorUserId, targetUserId, expiresAt);
    }

    #endregion
}
