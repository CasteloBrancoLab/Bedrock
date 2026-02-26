using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ServiceClientClaimMetadata = ShopDemo.Auth.Domain.Entities.ServiceClientClaims.ServiceClientClaim.ServiceClientClaimMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ServiceClientClaims;

public class ServiceClientClaimTests : TestBase
{
    public ServiceClientClaimTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and valid input");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientClaimInput(serviceClientId, claimId, ClaimValue.Granted);

        // Act
        LogAct("Registering new service client claim");
        var claim = ServiceClientClaim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying service client claim was created successfully");
        claim.ShouldNotBeNull();
        claim.ServiceClientId.ShouldBe(serviceClientId);
        claim.ClaimId.ShouldBe(claimId);
        claim.Value.ShouldBe(ClaimValue.Granted);
        claim.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithDefaultServiceClientId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default ServiceClientId");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = default(Id);
        var claimId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientClaimInput(serviceClientId, claimId, ClaimValue.Granted);

        // Act
        LogAct("Registering new service client claim with default ServiceClientId");
        var claim = ServiceClientClaim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        claim.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultClaimId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default ClaimId");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        var claimId = default(Id);
        var input = new RegisterNewServiceClientClaimInput(serviceClientId, claimId, ClaimValue.Granted);

        // Act
        LogAct("Registering new service client claim with default ClaimId");
        var claim = ServiceClientClaim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        claim.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithInvalidClaimValue_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with invalid ClaimValue");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var invalidValue = ClaimValue.CreateNew(99);
        var input = new RegisterNewServiceClientClaimInput(serviceClientId, claimId, invalidValue);

        // Act
        LogAct("Registering new service client claim with invalid value");
        var claim = ServiceClientClaim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        claim.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing service client claim");
        var entityInfo = CreateTestEntityInfo();
        var serviceClientId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var input = new CreateFromExistingInfoServiceClientClaimInput(entityInfo, serviceClientId, claimId, ClaimValue.Denied);

        // Act
        LogAct("Creating service client claim from existing info");
        var claim = ServiceClientClaim.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        claim.ShouldNotBeNull();
        claim.EntityInfo.ShouldBe(entityInfo);
        claim.ServiceClientId.ShouldBe(serviceClientId);
        claim.ClaimId.ShouldBe(claimId);
        claim.Value.ShouldBe(ClaimValue.Denied);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating service client claim");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientClaimInput(serviceClientId, claimId, ClaimValue.Granted);
        var claim = ServiceClientClaim.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning service client claim");
        var clone = claim.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(claim);
        clone.ServiceClientId.ShouldBe(claim.ServiceClientId);
        clone.ClaimId.ShouldBe(claim.ClaimId);
        clone.Value.ShouldBe(claim.Value);
    }

    #endregion

    #region ValidateServiceClientId Tests

    [Fact]
    public void ValidateServiceClientId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();

        // Act
        LogAct("Validating valid ServiceClientId");
        bool result = ServiceClientClaim.ValidateServiceClientId(executionContext, serviceClientId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateServiceClientId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ServiceClientId");
        bool result = ServiceClientClaim.ValidateServiceClientId(executionContext, null);

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
        bool result = ServiceClientClaim.ValidateClaimId(executionContext, claimId);

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
        bool result = ServiceClientClaim.ValidateClaimId(executionContext, null);

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
        bool result = ServiceClientClaim.ValidateValue(executionContext, ClaimValue.Granted);

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
        bool result = ServiceClientClaim.ValidateValue(executionContext, ClaimValue.CreateNew(99));

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
        bool result = ServiceClientClaim.ValidateValue(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void ServiceClientIdPropertyName_ShouldBeServiceClientId()
    {
        // Arrange & Act
        LogAct("Reading ServiceClientIdPropertyName");
        string name = ServiceClientClaimMetadata.ServiceClientIdPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("ServiceClientId");
    }

    [Fact]
    public void ClaimIdPropertyName_ShouldBeClaimId()
    {
        // Arrange & Act
        LogAct("Reading ClaimIdPropertyName");
        string name = ServiceClientClaimMetadata.ClaimIdPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("ClaimId");
    }

    [Fact]
    public void ValuePropertyName_ShouldBeValue()
    {
        // Arrange & Act
        LogAct("Reading ValuePropertyName");
        string name = ServiceClientClaimMetadata.ValuePropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("Value");
    }

    [Fact]
    public void ServiceClientIdIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading ServiceClientIdIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        ServiceClientClaimMetadata.ServiceClientIdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ClaimIdIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading ClaimIdIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        ServiceClientClaimMetadata.ClaimIdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ValueIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading ValueIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        ServiceClientClaimMetadata.ValueIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ChangeServiceClientIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = ServiceClientClaimMetadata.ServiceClientIdIsRequired;

        try
        {
            // Act
            LogAct("Changing ServiceClientId metadata");
            ServiceClientClaimMetadata.ChangeServiceClientIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            ServiceClientClaimMetadata.ServiceClientIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ServiceClientClaimMetadata.ChangeServiceClientIdMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeClaimIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = ServiceClientClaimMetadata.ClaimIdIsRequired;

        try
        {
            // Act
            LogAct("Changing ClaimId metadata");
            ServiceClientClaimMetadata.ChangeClaimIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            ServiceClientClaimMetadata.ClaimIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ServiceClientClaimMetadata.ChangeClaimIdMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeValueMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = ServiceClientClaimMetadata.ValueIsRequired;

        try
        {
            // Act
            LogAct("Changing Value metadata");
            ServiceClientClaimMetadata.ChangeValueMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            ServiceClientClaimMetadata.ValueIsRequired.ShouldBeFalse();
        }
        finally
        {
            ServiceClientClaimMetadata.ChangeValueMetadata(originalIsRequired);
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
        var serviceClientId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();

        // Act
        LogAct("Validating with static IsValid");
        bool result = ServiceClientClaim.IsValid(executionContext, entityInfo, serviceClientId, claimId, ClaimValue.Granted);

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
        var input = new RegisterNewServiceClientClaimInput(Id.GenerateNewId(), Id.GenerateNewId(), ClaimValue.Granted);
        var entity = ServiceClientClaim.RegisterNew(executionContext, input)!;

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
