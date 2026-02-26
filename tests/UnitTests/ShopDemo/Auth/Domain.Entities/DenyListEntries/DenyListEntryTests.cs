using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using DenyListEntryMetadata = ShopDemo.Auth.Domain.Entities.DenyListEntries.DenyListEntry.DenyListEntryMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.DenyListEntries;

public class DenyListEntryTests : TestBase
{
    public DenyListEntryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid data");
        var executionContext = CreateTestExecutionContext();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.Jti, "some-jti-value", expiresAt, "Compromised token");

        // Act
        LogAct("Registering new DenyListEntry");
        var entity = DenyListEntry.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.Type.ShouldBe(DenyListEntryType.Jti);
        entity.Value.ShouldBe("some-jti-value");
        entity.ExpiresAt.ShouldBe(expiresAt);
        entity.Reason.ShouldBe("Compromised token");
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithNullReason_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with null Reason (optional field)");
        var executionContext = CreateTestExecutionContext();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.UserId, "user-id-value", expiresAt, null);

        // Act
        LogAct("Registering new DenyListEntry with null Reason");
        var entity = DenyListEntry.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with null Reason");
        entity.ShouldNotBeNull();
        entity.Reason.ShouldBeNull();
    }

    [Theory]
    [InlineData(DenyListEntryType.Jti)]
    [InlineData(DenyListEntryType.UserId)]
    public void RegisterNew_WithAllDenyListEntryTypes_ShouldCreateEntity(DenyListEntryType type)
    {
        // Arrange
        LogArrange($"Creating input with DenyListEntryType {type}");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewDenyListEntryInput(
            type, "value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Registering new DenyListEntry");
        var entity = DenyListEntry.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct type");
        entity.ShouldNotBeNull();
        entity.Type.ShouldBe(type);
    }

    [Fact]
    public void RegisterNew_WithNullValue_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Value");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.Jti, null!, DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Registering new DenyListEntry with null Value");
        var entity = DenyListEntry.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithValueExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Value exceeding max length of 1024");
        var executionContext = CreateTestExecutionContext();
        var longValue = new string('x', 1025);
        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.Jti, longValue, DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Registering new DenyListEntry with oversized Value");
        var entity = DenyListEntry.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithValueAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with Value at exactly max length of 1024");
        var executionContext = CreateTestExecutionContext();
        var maxValue = new string('x', 1024);
        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.UserId, maxValue, DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Registering new DenyListEntry with Value at boundary");
        var entity = DenyListEntry.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.Value.ShouldBe(maxValue);
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing DenyListEntry");
        var entityInfo = CreateTestEntityInfo();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var input = new CreateFromExistingInfoDenyListEntryInput(
            entityInfo, DenyListEntryType.UserId, "blocked-user", expiresAt, "Policy violation");

        // Act
        LogAct("Creating DenyListEntry from existing info");
        var entity = DenyListEntry.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.Type.ShouldBe(DenyListEntryType.UserId);
        entity.Value.ShouldBe("blocked-user");
        entity.ExpiresAt.ShouldBe(expiresAt);
        entity.Reason.ShouldBe("Policy violation");
    }

    [Fact]
    public void CreateFromExistingInfo_WithNullReason_ShouldCreateWithNullReason()
    {
        // Arrange
        LogArrange("Creating properties for existing DenyListEntry with null Reason");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoDenyListEntryInput(
            entityInfo, DenyListEntryType.Jti, "jti-value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntry from existing info with null Reason");
        var entity = DenyListEntry.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying Reason is null");
        entity.ShouldNotBeNull();
        entity.Reason.ShouldBeNull();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating DenyListEntry via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.Jti, "clone-value", DateTimeOffset.UtcNow.AddHours(1), "Clone reason");
        var entity = DenyListEntry.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning DenyListEntry");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.Type.ShouldBe(entity.Type);
        clone.Value.ShouldBe(entity.Value);
        clone.ExpiresAt.ShouldBe(entity.ExpiresAt);
        clone.Reason.ShouldBe(entity.Reason);
    }

    [Fact]
    public void Clone_WithNullReason_ShouldPreserveNull()
    {
        // Arrange
        LogArrange("Creating DenyListEntry with null Reason");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.UserId, "user-value", DateTimeOffset.UtcNow.AddHours(1), null);
        var entity = DenyListEntry.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning DenyListEntry with null Reason");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone preserves null Reason");
        clone.ShouldNotBeNull();
        clone.Reason.ShouldBeNull();
    }

    #endregion

    #region ValidateType Tests

    [Fact]
    public void ValidateType_WithValidType_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid DenyListEntryType");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Type");
        bool result = DenyListEntry.ValidateType(executionContext, DenyListEntryType.Jti);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateType_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Type");
        bool result = DenyListEntry.ValidateType(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateValue Tests

    [Fact]
    public void ValidateValue_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid value");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Value");
        bool result = DenyListEntry.ValidateValue(executionContext, "valid-value");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateValue_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Value");
        bool result = DenyListEntry.ValidateValue(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateValue_WithEmpty_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Value (empty string is not null, passes IsRequired)");
        bool result = DenyListEntry.ValidateValue(executionContext, "");

        // Assert
        LogAssert("Verifying validation passes (empty string has length 0 which is within max length)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateValue_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized value");
        var executionContext = CreateTestExecutionContext();
        var longValue = new string('x', 1025);

        // Act
        LogAct("Validating oversized Value");
        bool result = DenyListEntry.ValidateValue(executionContext, longValue);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateValue_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and value at max length");
        var executionContext = CreateTestExecutionContext();
        var maxValue = new string('x', 1024);

        // Act
        LogAct("Validating Value at max length boundary");
        bool result = DenyListEntry.ValidateValue(executionContext, maxValue);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateExpiresAt Tests

    [Fact]
    public void ValidateExpiresAt_WithValidDate_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ExpiresAt");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid ExpiresAt");
        bool result = DenyListEntry.ValidateExpiresAt(executionContext, DateTimeOffset.UtcNow.AddHours(1));

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
        bool result = DenyListEntry.ValidateExpiresAt(executionContext, null);

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
        bool result = DenyListEntry.IsValid(
            executionContext, entityInfo, DenyListEntryType.Jti,
            "value", DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null Value");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Validating with null Value");
        bool result = DenyListEntry.IsValid(
            executionContext, entityInfo, DenyListEntryType.Jti,
            null, DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeTypeMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original TypeIsRequired value");
        bool originalIsRequired = DenyListEntryMetadata.TypeIsRequired;

        try
        {
            // Act
            LogAct("Changing Type metadata to not required");
            DenyListEntryMetadata.ChangeTypeMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying TypeIsRequired was updated");
            DenyListEntryMetadata.TypeIsRequired.ShouldBeFalse();
        }
        finally
        {
            DenyListEntryMetadata.ChangeTypeMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeValueMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original Value metadata values");
        bool originalIsRequired = DenyListEntryMetadata.ValueIsRequired;
        int originalMaxLength = DenyListEntryMetadata.ValueMaxLength;

        try
        {
            // Act
            LogAct("Changing Value metadata");
            DenyListEntryMetadata.ChangeValueMetadata(isRequired: false, maxLength: 2048);

            // Assert
            LogAssert("Verifying Value metadata was updated");
            DenyListEntryMetadata.ValueIsRequired.ShouldBeFalse();
            DenyListEntryMetadata.ValueMaxLength.ShouldBe(2048);
        }
        finally
        {
            DenyListEntryMetadata.ChangeValueMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeExpiresAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ExpiresAtIsRequired value");
        bool originalIsRequired = DenyListEntryMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata to not required");
            DenyListEntryMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ExpiresAtIsRequired was updated");
            DenyListEntryMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            DenyListEntryMetadata.ChangeExpiresAtMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeReasonMetadata_ShouldUpdateMaxLength()
    {
        // Arrange
        LogArrange("Saving original Reason metadata values");
        int originalMaxLength = DenyListEntryMetadata.ReasonMaxLength;

        try
        {
            // Act
            LogAct("Changing Reason metadata");
            DenyListEntryMetadata.ChangeReasonMetadata(maxLength: 2000);

            // Assert
            LogAssert("Verifying Reason metadata was updated");
            DenyListEntryMetadata.ReasonMaxLength.ShouldBe(2000);
        }
        finally
        {
            DenyListEntryMetadata.ChangeReasonMetadata(maxLength: originalMaxLength);
        }
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_Static_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context, entity info, and valid properties");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Validating with static IsValid");
        bool result = DenyListEntry.IsValid(executionContext, entityInfo,
            DenyListEntryType.Jti, "some-jti-value", DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        LogAssert("Verifying static validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithValidEntity_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid entity via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.Jti, "some-jti-value", DateTimeOffset.UtcNow.AddHours(1), "Compromised token");
        var entity = DenyListEntry.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Validating instance with IsValid");
        var validationContext = CreateTestExecutionContext();
        bool result = entity.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
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
