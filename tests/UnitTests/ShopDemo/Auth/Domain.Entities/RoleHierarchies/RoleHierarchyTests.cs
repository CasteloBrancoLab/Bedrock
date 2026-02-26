using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using RoleHierarchyMetadata = ShopDemo.Auth.Domain.Entities.RoleHierarchies.RoleHierarchy.RoleHierarchyMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RoleHierarchies;

public class RoleHierarchyTests : TestBase
{
    public RoleHierarchyTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid Ids");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var parentRoleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewRoleHierarchyInput(roleId, parentRoleId);

        // Act
        LogAct("Registering new RoleHierarchy");
        var entity = RoleHierarchy.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.RoleId.ShouldBe(roleId);
        entity.ParentRoleId.ShouldBe(parentRoleId);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithDefaultRoleId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default RoleId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var roleId = default(Id);
        var parentRoleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewRoleHierarchyInput(roleId, parentRoleId);

        // Act
        LogAct("Registering new RoleHierarchy with default RoleId");
        var entity = RoleHierarchy.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultParentRoleId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default ParentRoleId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var parentRoleId = default(Id);
        var input = new RegisterNewRoleHierarchyInput(roleId, parentRoleId);

        // Act
        LogAct("Registering new RoleHierarchy with default ParentRoleId");
        var entity = RoleHierarchy.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithSameRoleAndParentId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with same RoleId and ParentRoleId (self-reference)");
        var executionContext = CreateTestExecutionContext();
        var sameId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewRoleHierarchyInput(sameId, sameId);

        // Act
        LogAct("Registering new RoleHierarchy with self-reference");
        var entity = RoleHierarchy.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to self-reference prohibition");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing RoleHierarchy");
        var entityInfo = CreateTestEntityInfo();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var parentRoleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoRoleHierarchyInput(entityInfo, roleId, parentRoleId);

        // Act
        LogAct("Creating RoleHierarchy from existing info");
        var entity = RoleHierarchy.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.RoleId.ShouldBe(roleId);
        entity.ParentRoleId.ShouldBe(parentRoleId);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating RoleHierarchy via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var parentRoleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewRoleHierarchyInput(roleId, parentRoleId);
        var entity = RoleHierarchy.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning RoleHierarchy");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.RoleId.ShouldBe(entity.RoleId);
        clone.ParentRoleId.ShouldBe(entity.ParentRoleId);
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
        bool result = RoleHierarchy.ValidateRoleId(executionContext, roleId);

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
        bool result = RoleHierarchy.ValidateRoleId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateParentRoleId Tests

    [Fact]
    public void ValidateParentRoleId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ParentRoleId");
        var executionContext = CreateTestExecutionContext();
        var parentRoleId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating valid ParentRoleId");
        bool result = RoleHierarchy.ValidateParentRoleId(executionContext, parentRoleId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateParentRoleId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ParentRoleId");
        bool result = RoleHierarchy.ValidateParentRoleId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeRoleIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original RoleIdIsRequired value");
        bool originalIsRequired = RoleHierarchyMetadata.RoleIdIsRequired;

        try
        {
            // Act
            LogAct("Changing RoleId metadata to not required");
            RoleHierarchyMetadata.ChangeRoleIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying RoleIdIsRequired was updated");
            RoleHierarchyMetadata.RoleIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            RoleHierarchyMetadata.ChangeRoleIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeParentRoleIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ParentRoleIdIsRequired value");
        bool originalIsRequired = RoleHierarchyMetadata.ParentRoleIdIsRequired;

        try
        {
            // Act
            LogAct("Changing ParentRoleId metadata to not required");
            RoleHierarchyMetadata.ChangeParentRoleIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ParentRoleIdIsRequired was updated");
            RoleHierarchyMetadata.ParentRoleIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            RoleHierarchyMetadata.ChangeParentRoleIdMetadata(isRequired: originalIsRequired);
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
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var parentRoleId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating with static IsValid");
        bool result = RoleHierarchy.IsValid(executionContext, entityInfo, roleId, parentRoleId);

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
        var input = new RegisterNewRoleHierarchyInput(
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()));
        var entity = RoleHierarchy.RegisterNew(executionContext, input)!;

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
