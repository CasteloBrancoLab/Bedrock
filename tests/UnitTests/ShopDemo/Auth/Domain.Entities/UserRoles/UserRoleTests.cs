using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.UserRoles;
using ShopDemo.Auth.Domain.Entities.UserRoles.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using UserRoleMetadata = ShopDemo.Auth.Domain.Entities.UserRoles.UserRole.UserRoleMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.UserRoles;

public class UserRoleTests : TestBase
{
    public UserRoleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid Ids");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewUserRoleInput(userId, roleId);

        // Act
        LogAct("Registering new UserRole");
        var entity = UserRole.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.UserId.ShouldBe(userId);
        entity.RoleId.ShouldBe(roleId);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithDefaultUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default UserId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var userId = default(Id);
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewUserRoleInput(userId, roleId);

        // Act
        LogAct("Registering new UserRole with default UserId");
        var entity = UserRole.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultRoleId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default RoleId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var roleId = default(Id);
        var input = new RegisterNewUserRoleInput(userId, roleId);

        // Act
        LogAct("Registering new UserRole with default RoleId");
        var entity = UserRole.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing UserRole");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoUserRoleInput(entityInfo, userId, roleId);

        // Act
        LogAct("Creating UserRole from existing info");
        var entity = UserRole.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.UserId.ShouldBe(userId);
        entity.RoleId.ShouldBe(roleId);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating UserRole via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewUserRoleInput(userId, roleId);
        var entity = UserRole.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning UserRole");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.UserId.ShouldBe(entity.UserId);
        clone.RoleId.ShouldBe(entity.RoleId);
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
        bool result = UserRole.ValidateUserId(executionContext, userId);

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
        bool result = UserRole.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateRoleId Tests

    [Fact]
    public void ValidateRoleId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid RoleId");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating valid RoleId");
        bool result = UserRole.ValidateRoleId(executionContext, roleId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRoleId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null RoleId");
        bool result = UserRole.ValidateRoleId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeUserIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original UserIdIsRequired value");
        bool originalIsRequired = UserRoleMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata to not required");
            UserRoleMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying UserIdIsRequired was updated");
            UserRoleMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            UserRoleMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeRoleIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original RoleIdIsRequired value");
        bool originalIsRequired = UserRoleMetadata.RoleIdIsRequired;

        try
        {
            // Act
            LogAct("Changing RoleId metadata to not required");
            UserRoleMetadata.ChangeRoleIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying RoleIdIsRequired was updated");
            UserRoleMetadata.RoleIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            UserRoleMetadata.ChangeRoleIdMetadata(isRequired: originalIsRequired);
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
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating with static IsValid");
        bool result = UserRole.IsValid(executionContext, entityInfo, userId, roleId);

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
        var input = new RegisterNewUserRoleInput(
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()));
        var entity = UserRole.RegisterNew(executionContext, input)!;

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

    #endregion
}
