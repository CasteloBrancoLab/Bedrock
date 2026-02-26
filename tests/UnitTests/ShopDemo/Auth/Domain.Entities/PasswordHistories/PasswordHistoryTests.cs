using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;
using ShopDemo.Auth.Domain.Entities.PasswordHistories.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using PasswordHistoryMetadata = ShopDemo.Auth.Domain.Entities.PasswordHistories.PasswordHistory.PasswordHistoryMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.PasswordHistories;

public class PasswordHistoryTests : TestBase
{
    public PasswordHistoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid data");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewPasswordHistoryInput(
            userId, "$argon2id$v=19$m=65536,t=3,p=1$hashvalue");

        // Act
        LogAct("Registering new PasswordHistory");
        var entity = PasswordHistory.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.UserId.ShouldBe(userId);
        entity.PasswordHash.ShouldBe("$argon2id$v=19$m=65536,t=3,p=1$hashvalue");
        entity.ChangedAt.ShouldNotBe(default);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldSetChangedAtFromExecutionContextTimestamp()
    {
        // Arrange
        LogArrange("Creating execution context to verify ChangedAt is set from timestamp");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewPasswordHistoryInput(userId, "hash-value");

        // Act
        LogAct("Registering new PasswordHistory");
        var entity = PasswordHistory.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying ChangedAt matches execution context timestamp");
        entity.ShouldNotBeNull();
        entity.ChangedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void RegisterNew_WithNullPasswordHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null PasswordHash");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewPasswordHistoryInput(userId, null!);

        // Act
        LogAct("Registering new PasswordHistory with null PasswordHash");
        var entity = PasswordHistory.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithPasswordHashExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with PasswordHash exceeding max length of 1024");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var longHash = new string('h', 1025);
        var input = new RegisterNewPasswordHistoryInput(userId, longHash);

        // Act
        LogAct("Registering new PasswordHistory with oversized PasswordHash");
        var entity = PasswordHistory.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithPasswordHashAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with PasswordHash at exactly max length of 1024");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var maxHash = new string('h', 1024);
        var input = new RegisterNewPasswordHistoryInput(userId, maxHash);

        // Act
        LogAct("Registering new PasswordHistory with PasswordHash at boundary");
        var entity = PasswordHistory.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.PasswordHash.ShouldBe(maxHash);
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing PasswordHistory");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var changedAt = DateTimeOffset.UtcNow.AddDays(-30);
        var input = new CreateFromExistingInfoPasswordHistoryInput(
            entityInfo, userId, "existing-hash-value", changedAt);

        // Act
        LogAct("Creating PasswordHistory from existing info");
        var entity = PasswordHistory.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.UserId.ShouldBe(userId);
        entity.PasswordHash.ShouldBe("existing-hash-value");
        entity.ChangedAt.ShouldBe(changedAt);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating PasswordHistory via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewPasswordHistoryInput(userId, "clone-hash-value");
        var entity = PasswordHistory.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning PasswordHistory");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.UserId.ShouldBe(entity.UserId);
        clone.PasswordHash.ShouldBe(entity.PasswordHash);
        clone.ChangedAt.ShouldBe(entity.ChangedAt);
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
        bool result = PasswordHistory.ValidateUserId(executionContext, userId);

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
        bool result = PasswordHistory.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidatePasswordHash Tests

    [Fact]
    public void ValidatePasswordHash_WithValidHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid PasswordHash");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid PasswordHash");
        bool result = PasswordHistory.ValidatePasswordHash(executionContext, "valid-hash");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePasswordHash_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null PasswordHash");
        bool result = PasswordHistory.ValidatePasswordHash(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePasswordHash_WithEmpty_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty PasswordHash (empty string is not null, passes IsRequired)");
        bool result = PasswordHistory.ValidatePasswordHash(executionContext, "");

        // Assert
        LogAssert("Verifying validation passes (empty string has length 0 which is within max length)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePasswordHash_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized PasswordHash");
        var executionContext = CreateTestExecutionContext();
        var longHash = new string('h', 1025);

        // Act
        LogAct("Validating oversized PasswordHash");
        bool result = PasswordHistory.ValidatePasswordHash(executionContext, longHash);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePasswordHash_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and PasswordHash at max length");
        var executionContext = CreateTestExecutionContext();
        var maxHash = new string('h', 1024);

        // Act
        LogAct("Validating PasswordHash at max length boundary");
        bool result = PasswordHistory.ValidatePasswordHash(executionContext, maxHash);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateChangedAt Tests

    [Fact]
    public void ValidateChangedAt_WithValidDate_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ChangedAt");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid ChangedAt");
        bool result = PasswordHistory.ValidateChangedAt(executionContext, DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateChangedAt_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ChangedAt");
        bool result = PasswordHistory.ValidateChangedAt(executionContext, null);

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
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating all fields");
        bool result = PasswordHistory.IsValid(
            executionContext, entityInfo, userId, "hash-value", DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullUserId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null UserId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Validating with null UserId");
        bool result = PasswordHistory.IsValid(
            executionContext, entityInfo, null, "hash-value", DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullPasswordHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null PasswordHash");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating with null PasswordHash");
        bool result = PasswordHistory.IsValid(
            executionContext, entityInfo, userId, null, DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullChangedAt_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null ChangedAt");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating with null ChangedAt");
        bool result = PasswordHistory.IsValid(
            executionContext, entityInfo, userId, "hash-value", null);

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
        bool originalIsRequired = PasswordHistoryMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata to not required");
            PasswordHistoryMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying UserIdIsRequired was updated");
            PasswordHistoryMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            PasswordHistoryMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangePasswordHashMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original PasswordHash metadata values");
        bool originalIsRequired = PasswordHistoryMetadata.PasswordHashIsRequired;
        int originalMaxLength = PasswordHistoryMetadata.PasswordHashMaxLength;

        try
        {
            // Act
            LogAct("Changing PasswordHash metadata");
            PasswordHistoryMetadata.ChangePasswordHashMetadata(isRequired: false, maxLength: 2048);

            // Assert
            LogAssert("Verifying PasswordHash metadata was updated");
            PasswordHistoryMetadata.PasswordHashIsRequired.ShouldBeFalse();
            PasswordHistoryMetadata.PasswordHashMaxLength.ShouldBe(2048);
        }
        finally
        {
            PasswordHistoryMetadata.ChangePasswordHashMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeChangedAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ChangedAtIsRequired value");
        bool originalIsRequired = PasswordHistoryMetadata.ChangedAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ChangedAt metadata to not required");
            PasswordHistoryMetadata.ChangeChangedAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ChangedAtIsRequired was updated");
            PasswordHistoryMetadata.ChangedAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            PasswordHistoryMetadata.ChangeChangedAtMetadata(isRequired: originalIsRequired);
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
