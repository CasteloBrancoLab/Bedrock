using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ClaimDependencyMetadata = ShopDemo.Auth.Domain.Entities.ClaimDependencies.ClaimDependency.ClaimDependencyMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ClaimDependencies;

public class ClaimDependencyTests : TestBase
{
    public ClaimDependencyTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid Ids");
        var executionContext = CreateTestExecutionContext();
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var dependsOnClaimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewClaimDependencyInput(claimId, dependsOnClaimId);

        // Act
        LogAct("Registering new ClaimDependency");
        var entity = ClaimDependency.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.ClaimId.ShouldBe(claimId);
        entity.DependsOnClaimId.ShouldBe(dependsOnClaimId);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithDefaultClaimId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default ClaimId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var claimId = default(Id);
        var dependsOnClaimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewClaimDependencyInput(claimId, dependsOnClaimId);

        // Act
        LogAct("Registering new ClaimDependency with default ClaimId");
        var entity = ClaimDependency.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultDependsOnClaimId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default DependsOnClaimId (Guid.Empty)");
        var executionContext = CreateTestExecutionContext();
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var dependsOnClaimId = default(Id);
        var input = new RegisterNewClaimDependencyInput(claimId, dependsOnClaimId);

        // Act
        LogAct("Registering new ClaimDependency with default DependsOnClaimId");
        var entity = ClaimDependency.RegisterNew(executionContext, input);

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
        LogArrange("Creating all properties for existing ClaimDependency");
        var entityInfo = CreateTestEntityInfo();
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var dependsOnClaimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoClaimDependencyInput(entityInfo, claimId, dependsOnClaimId);

        // Act
        LogAct("Creating ClaimDependency from existing info");
        var entity = ClaimDependency.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.ClaimId.ShouldBe(claimId);
        entity.DependsOnClaimId.ShouldBe(dependsOnClaimId);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating ClaimDependency via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var dependsOnClaimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewClaimDependencyInput(claimId, dependsOnClaimId);
        var entity = ClaimDependency.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning ClaimDependency");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.ClaimId.ShouldBe(entity.ClaimId);
        clone.DependsOnClaimId.ShouldBe(entity.DependsOnClaimId);
    }

    #endregion

    #region ValidateClaimId Tests

    [Fact]
    public void ValidateClaimId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ClaimId");
        var executionContext = CreateTestExecutionContext();
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating valid ClaimId");
        bool result = ClaimDependency.ValidateClaimId(executionContext, claimId);

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
        bool result = ClaimDependency.ValidateClaimId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateDependsOnClaimId Tests

    [Fact]
    public void ValidateDependsOnClaimId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid DependsOnClaimId");
        var executionContext = CreateTestExecutionContext();
        var dependsOnClaimId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating valid DependsOnClaimId");
        bool result = ClaimDependency.ValidateDependsOnClaimId(executionContext, dependsOnClaimId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDependsOnClaimId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null DependsOnClaimId");
        bool result = ClaimDependency.ValidateDependsOnClaimId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeClaimIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ClaimIdIsRequired value");
        bool originalIsRequired = ClaimDependencyMetadata.ClaimIdIsRequired;

        try
        {
            // Act
            LogAct("Changing ClaimId metadata to not required");
            ClaimDependencyMetadata.ChangeClaimIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ClaimIdIsRequired was updated");
            ClaimDependencyMetadata.ClaimIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ClaimDependencyMetadata.ChangeClaimIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeDependsOnClaimIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original DependsOnClaimIdIsRequired value");
        bool originalIsRequired = ClaimDependencyMetadata.DependsOnClaimIdIsRequired;

        try
        {
            // Act
            LogAct("Changing DependsOnClaimId metadata to not required");
            ClaimDependencyMetadata.ChangeDependsOnClaimIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying DependsOnClaimIdIsRequired was updated");
            ClaimDependencyMetadata.DependsOnClaimIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ClaimDependencyMetadata.ChangeDependsOnClaimIdMetadata(isRequired: originalIsRequired);
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
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var dependsOnClaimId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating with static IsValid");
        bool result = ClaimDependency.IsValid(executionContext, entityInfo, claimId, dependsOnClaimId);

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
        var input = new RegisterNewClaimDependencyInput(
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            Id.CreateFromExistingInfo(Guid.NewGuid()));
        var entity = ClaimDependency.RegisterNew(executionContext, input)!;

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
