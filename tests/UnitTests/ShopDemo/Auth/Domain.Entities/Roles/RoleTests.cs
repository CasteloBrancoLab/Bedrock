using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Roles;
using ShopDemo.Auth.Domain.Entities.Roles.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using RoleMetadata = ShopDemo.Auth.Domain.Entities.Roles.Role.RoleMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Roles;

public class RoleTests : TestBase
{
    public RoleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid name and description");
        var executionContext = CreateTestExecutionContext();
        string name = "Administrator";
        string description = "Full system access";
        var input = new RegisterNewRoleInput(name, description);

        // Act
        LogAct("Registering new Role");
        var entity = Role.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.Name.ShouldBe(name);
        entity.Description.ShouldBe(description);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithNullName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null name");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewRoleInput(null!, "Some description");

        // Act
        LogAct("Registering new Role with null name");
        var entity = Role.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty name (default for string)");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewRoleInput("", "Some description");

        // Act
        LogAct("Registering new Role with empty name");
        var entity = Role.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithTooLongName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with name exceeding max length (256 chars)");
        var executionContext = CreateTestExecutionContext();
        string longName = new('a', RoleMetadata.NameMaxLength + 1);
        var input = new RegisterNewRoleInput(longName, "Some description");

        // Act
        LogAct("Registering new Role with too-long name");
        var entity = Role.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to max length validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullDescription_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with null description (optional field)");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewRoleInput("Administrator", null);

        // Act
        LogAct("Registering new Role with null description");
        var entity = Role.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with null description");
        entity.ShouldNotBeNull();
        entity.Name.ShouldBe("Administrator");
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithValidDescription_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with valid description");
        var executionContext = CreateTestExecutionContext();
        string description = "A valid description for the role";
        var input = new RegisterNewRoleInput("Administrator", description);

        // Act
        LogAct("Registering new Role with valid description");
        var entity = Role.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct description");
        entity.ShouldNotBeNull();
        entity.Description.ShouldBe(description);
    }

    [Fact]
    public void RegisterNew_WithTooLongDescription_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with description exceeding max length (1001 chars)");
        var executionContext = CreateTestExecutionContext();
        string longDescription = new('a', RoleMetadata.DescriptionMaxLength + 1);
        var input = new RegisterNewRoleInput("Administrator", longDescription);

        // Act
        LogAct("Registering new Role with too-long description");
        var entity = Role.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to description max length validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing Role");
        var entityInfo = CreateTestEntityInfo();
        string name = "Manager";
        string description = "Management access";
        var input = new CreateFromExistingInfoRoleInput(entityInfo, name, description);

        // Act
        LogAct("Creating Role from existing info");
        var entity = Role.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.Name.ShouldBe(name);
        entity.Description.ShouldBe(description);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating Role via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewRoleInput("Administrator", "Full system access");
        var entity = Role.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning Role");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.Name.ShouldBe(entity.Name);
        clone.Description.ShouldBe(entity.Description);
    }

    #endregion

    #region Change Tests

    [Fact]
    public void Change_WithValidInput_ShouldUpdateProperties()
    {
        // Arrange
        LogArrange("Creating Role and preparing change input");
        var executionContext = CreateTestExecutionContext();
        var registerInput = new RegisterNewRoleInput("OldName", "Old description");
        var role = Role.RegisterNew(executionContext, registerInput)!;
        string newName = "NewName";
        string newDescription = "New description";
        var changeInput = new ChangeRoleInput(newName, newDescription);

        // Act
        LogAct("Changing Role properties");
        var result = role.Change(executionContext, changeInput);

        // Assert
        LogAssert("Verifying properties were updated on the returned instance");
        result.ShouldNotBeNull();
        result.Name.ShouldBe(newName);
        result.Description.ShouldBe(newDescription);
    }

    [Fact]
    public void Change_WithInvalidName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating Role and preparing change input with empty name");
        var executionContext = CreateTestExecutionContext();
        var registerInput = new RegisterNewRoleInput("ValidName", "Valid description");
        var role = Role.RegisterNew(executionContext, registerInput)!;
        var changeInput = new ChangeRoleInput("", "New description");

        // Act
        LogAct("Changing Role with empty name");
        var newContext = CreateTestExecutionContext();
        var result = role.Change(newContext, changeInput);

        // Assert
        LogAssert("Verifying null is returned due to invalid name");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void Change_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating Role");
        var executionContext = CreateTestExecutionContext();
        var registerInput = new RegisterNewRoleInput("OldName", "Old description");
        var role = Role.RegisterNew(executionContext, registerInput)!;
        var changeInput = new ChangeRoleInput("NewName", "New description");

        // Act
        LogAct("Changing Role");
        var result = role.Change(executionContext, changeInput);

        // Assert
        LogAssert("Verifying clone-modify-return pattern: new instance returned, original unchanged");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(role);
        role.Name.ShouldBe("OldName");
        result.Name.ShouldBe("NewName");
    }

    #endregion

    #region ValidateName Tests

    [Fact]
    public void ValidateName_WithValidName_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid name");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid name");
        bool result = Role.ValidateName(executionContext, "Administrator");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateName_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null name");
        bool result = Role.ValidateName(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateDescription Tests

    [Fact]
    public void ValidateDescription_WithNull_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null description (optional field)");
        bool result = Role.ValidateDescription(executionContext, null);

        // Assert
        LogAssert("Verifying validation passes for null optional field");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDescription_WithTooLong_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and too-long description");
        var executionContext = CreateTestExecutionContext();
        string longDescription = new('a', RoleMetadata.DescriptionMaxLength + 1);

        // Act
        LogAct("Validating too-long description");
        bool result = Role.ValidateDescription(executionContext, longDescription);

        // Assert
        LogAssert("Verifying validation fails for exceeding max length");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeNameMetadata_ShouldUpdate()
    {
        // Arrange
        LogArrange("Saving original Name metadata values");
        bool originalIsRequired = RoleMetadata.NameIsRequired;
        int originalMinLength = RoleMetadata.NameMinLength;
        int originalMaxLength = RoleMetadata.NameMaxLength;

        try
        {
            // Act
            LogAct("Changing Name metadata");
            RoleMetadata.ChangeNameMetadata(
                isRequired: false,
                minLength: 5,
                maxLength: 50
            );

            // Assert
            LogAssert("Verifying Name metadata was updated");
            RoleMetadata.NameIsRequired.ShouldBeFalse();
            RoleMetadata.NameMinLength.ShouldBe(5);
            RoleMetadata.NameMaxLength.ShouldBe(50);
        }
        finally
        {
            RoleMetadata.ChangeNameMetadata(originalIsRequired, originalMinLength, originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeDescriptionMetadata_ShouldUpdate()
    {
        // Arrange
        LogArrange("Saving original Description metadata values");
        bool originalIsRequired = RoleMetadata.DescriptionIsRequired;
        int originalMaxLength = RoleMetadata.DescriptionMaxLength;

        try
        {
            // Act
            LogAct("Changing Description metadata");
            RoleMetadata.ChangeDescriptionMetadata(
                isRequired: true,
                maxLength: 500
            );

            // Assert
            LogAssert("Verifying Description metadata was updated");
            RoleMetadata.DescriptionIsRequired.ShouldBeTrue();
            RoleMetadata.DescriptionMaxLength.ShouldBe(500);
        }
        finally
        {
            RoleMetadata.ChangeDescriptionMetadata(originalIsRequired, originalMaxLength);
        }
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
