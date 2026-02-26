using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Domain.Entities.RoleClaims.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using RoleClaimMetadata = ShopDemo.Auth.Domain.Entities.RoleClaims.RoleClaim.RoleClaimMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RoleClaims;

public class RoleClaimTests : TestBase
{
    public RoleClaimTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and valid input");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var input = new RegisterNewRoleClaimInput(roleId, claimId, ClaimValue.Granted);

        // Act
        LogAct("Registering new role claim");
        var roleClaim = RoleClaim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying role claim was created successfully");
        roleClaim.ShouldNotBeNull();
        roleClaim.RoleId.ShouldBe(roleId);
        roleClaim.ClaimId.ShouldBe(claimId);
        roleClaim.Value.ShouldBe(ClaimValue.Granted);
        roleClaim.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithDefaultRoleId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default RoleId");
        var executionContext = CreateTestExecutionContext();
        var roleId = default(Id);
        var claimId = Id.GenerateNewId();
        var input = new RegisterNewRoleClaimInput(roleId, claimId, ClaimValue.Granted);

        // Act
        LogAct("Registering new role claim with default RoleId");
        var roleClaim = RoleClaim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        roleClaim.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultClaimId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default ClaimId");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = default(Id);
        var input = new RegisterNewRoleClaimInput(roleId, claimId, ClaimValue.Granted);

        // Act
        LogAct("Registering new role claim with default ClaimId");
        var roleClaim = RoleClaim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        roleClaim.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithInvalidClaimValue_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with invalid ClaimValue");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var invalidValue = ClaimValue.CreateNew(99);
        var input = new RegisterNewRoleClaimInput(roleId, claimId, invalidValue);

        // Act
        LogAct("Registering new role claim with invalid value");
        var roleClaim = RoleClaim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        roleClaim.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing role claim");
        var entityInfo = CreateTestEntityInfo();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var input = new CreateFromExistingInfoRoleClaimInput(entityInfo, roleId, claimId, ClaimValue.Granted);

        // Act
        LogAct("Creating role claim from existing info");
        var roleClaim = RoleClaim.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        roleClaim.ShouldNotBeNull();
        roleClaim.EntityInfo.ShouldBe(entityInfo);
        roleClaim.RoleId.ShouldBe(roleId);
        roleClaim.ClaimId.ShouldBe(claimId);
        roleClaim.Value.ShouldBe(ClaimValue.Granted);
    }

    #endregion

    #region ChangeValue Tests

    [Fact]
    public void ChangeValue_WithValidValue_ShouldUpdateValue()
    {
        // Arrange
        LogArrange("Creating role claim with Granted value");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var registerInput = new RegisterNewRoleClaimInput(roleId, claimId, ClaimValue.Granted);
        var roleClaim = RoleClaim.RegisterNew(executionContext, registerInput)!;

        // Act
        LogAct("Changing value to Denied");
        var result = roleClaim.ChangeValue(executionContext, new ChangeRoleClaimValueInput(ClaimValue.Denied));

        // Assert
        LogAssert("Verifying value was updated");
        result.ShouldNotBeNull();
        result.Value.ShouldBe(ClaimValue.Denied);
    }

    [Fact]
    public void ChangeValue_WithInvalidValue_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating role claim with valid value");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var registerInput = new RegisterNewRoleClaimInput(roleId, claimId, ClaimValue.Granted);
        var roleClaim = RoleClaim.RegisterNew(executionContext, registerInput)!;

        // Act
        LogAct("Changing value to invalid ClaimValue");
        var changeContext = CreateTestExecutionContext();
        var result = roleClaim.ChangeValue(changeContext, new ChangeRoleClaimValueInput(ClaimValue.CreateNew(99)));

        // Assert
        LogAssert("Verifying null is returned for invalid value");
        result.ShouldBeNull();
        changeContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ChangeValue_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating role claim");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var registerInput = new RegisterNewRoleClaimInput(roleId, claimId, ClaimValue.Granted);
        var roleClaim = RoleClaim.RegisterNew(executionContext, registerInput)!;

        // Act
        LogAct("Changing value");
        var result = roleClaim.ChangeValue(executionContext, new ChangeRoleClaimValueInput(ClaimValue.Denied));

        // Assert
        LogAssert("Verifying new instance was returned (clone-modify-return)");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(roleClaim);
        roleClaim.Value.ShouldBe(ClaimValue.Granted);
        result.Value.ShouldBe(ClaimValue.Denied);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating role claim");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var registerInput = new RegisterNewRoleClaimInput(roleId, claimId, ClaimValue.Granted);
        var roleClaim = RoleClaim.RegisterNew(executionContext, registerInput)!;

        // Act
        LogAct("Cloning role claim");
        var clone = roleClaim.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(roleClaim);
        clone.RoleId.ShouldBe(roleClaim.RoleId);
        clone.ClaimId.ShouldBe(roleClaim.ClaimId);
        clone.Value.ShouldBe(roleClaim.Value);
    }

    #endregion

    #region ValidateRoleId Tests

    [Fact]
    public void ValidateRoleId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();

        // Act
        LogAct("Validating valid RoleId");
        bool result = RoleClaim.ValidateRoleId(executionContext, roleId);

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
        bool result = RoleClaim.ValidateRoleId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateClaimId Tests

    [Fact]
    public void ValidateClaimId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var claimId = Id.GenerateNewId();

        // Act
        LogAct("Validating valid ClaimId");
        bool result = RoleClaim.ValidateClaimId(executionContext, claimId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClaimId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ClaimId");
        bool result = RoleClaim.ValidateClaimId(executionContext, null);

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
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Granted ClaimValue");
        bool result = RoleClaim.ValidateValue(executionContext, ClaimValue.Granted);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateValue_WithInvalidValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating invalid ClaimValue");
        bool result = RoleClaim.ValidateValue(executionContext, ClaimValue.CreateNew(99));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateValue_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ClaimValue");
        bool result = RoleClaim.ValidateValue(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void RoleIdPropertyName_ShouldBeRoleId()
    {
        // Arrange & Act
        LogAct("Reading RoleIdPropertyName");
        string name = RoleClaimMetadata.RoleIdPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("RoleId");
    }

    [Fact]
    public void ClaimIdPropertyName_ShouldBeClaimId()
    {
        // Arrange & Act
        LogAct("Reading ClaimIdPropertyName");
        string name = RoleClaimMetadata.ClaimIdPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("ClaimId");
    }

    [Fact]
    public void ValuePropertyName_ShouldBeValue()
    {
        // Arrange & Act
        LogAct("Reading ValuePropertyName");
        string name = RoleClaimMetadata.ValuePropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("Value");
    }

    [Fact]
    public void RoleIdIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading RoleIdIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        RoleClaimMetadata.RoleIdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ClaimIdIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading ClaimIdIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        RoleClaimMetadata.ClaimIdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ValueIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading ValueIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        RoleClaimMetadata.ValueIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ChangeRoleIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = RoleClaimMetadata.RoleIdIsRequired;

        try
        {
            // Act
            LogAct("Changing RoleId metadata");
            RoleClaimMetadata.ChangeRoleIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            RoleClaimMetadata.RoleIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            RoleClaimMetadata.ChangeRoleIdMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeClaimIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = RoleClaimMetadata.ClaimIdIsRequired;

        try
        {
            // Act
            LogAct("Changing ClaimId metadata");
            RoleClaimMetadata.ChangeClaimIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            RoleClaimMetadata.ClaimIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            RoleClaimMetadata.ChangeClaimIdMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeValueMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = RoleClaimMetadata.ValueIsRequired;

        try
        {
            // Act
            LogAct("Changing Value metadata");
            RoleClaimMetadata.ChangeValueMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            RoleClaimMetadata.ValueIsRequired.ShouldBeFalse();
        }
        finally
        {
            RoleClaimMetadata.ChangeValueMetadata(originalIsRequired);
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
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();

        // Act
        LogAct("Validating with static IsValid");
        bool result = RoleClaim.IsValid(executionContext, entityInfo, roleId, claimId, ClaimValue.Granted);

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
        var input = new RegisterNewRoleClaimInput(Id.GenerateNewId(), Id.GenerateNewId(), ClaimValue.Granted);
        var entity = RoleClaim.RegisterNew(executionContext, input)!;

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
                createdAt: DateTimeOffset.UtcNow, createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(), createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null, lastChangedBy: null,
                lastChangedCorrelationId: null, lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));
    }

    #endregion
}
