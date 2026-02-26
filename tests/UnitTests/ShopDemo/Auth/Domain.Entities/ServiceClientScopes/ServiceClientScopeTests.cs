using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ServiceClientScopeMetadata = ShopDemo.Auth.Domain.Entities.ServiceClientScopes.ServiceClientScope.ServiceClientScopeMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ServiceClientScopes;

public class ServiceClientScopeTests : TestBase
{
    public ServiceClientScopeTests(ITestOutputHelper outputHelper) : base(outputHelper)
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
        string scope = "read:users";
        var input = new RegisterNewServiceClientScopeInput(serviceClientId, scope);

        // Act
        LogAct("Registering new service client scope");
        var serviceClientScope = ServiceClientScope.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying service client scope was created successfully");
        serviceClientScope.ShouldNotBeNull();
        serviceClientScope.ServiceClientId.ShouldBe(serviceClientId);
        serviceClientScope.Scope.ShouldBe("read:users");
        serviceClientScope.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithDefaultServiceClientId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default ServiceClientId");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = default(Id);
        string scope = "read:users";
        var input = new RegisterNewServiceClientScopeInput(serviceClientId, scope);

        // Act
        LogAct("Registering new service client scope with default ServiceClientId");
        var serviceClientScope = ServiceClientScope.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        serviceClientScope.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullScope_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Scope");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientScopeInput(serviceClientId, null!);

        // Act
        LogAct("Registering new service client scope with null Scope");
        var serviceClientScope = ServiceClientScope.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        serviceClientScope.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyScope_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty Scope");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientScopeInput(serviceClientId, "");

        // Act
        LogAct("Registering new service client scope with empty Scope");
        var serviceClientScope = ServiceClientScope.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        serviceClientScope.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithScopeExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Scope exceeding max length");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        string longScope = new('a', ServiceClientScopeMetadata.ScopeMaxLength + 1);
        var input = new RegisterNewServiceClientScopeInput(serviceClientId, longScope);

        // Act
        LogAct("Registering new service client scope with too-long Scope");
        var serviceClientScope = ServiceClientScope.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        serviceClientScope.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing service client scope");
        var entityInfo = CreateTestEntityInfo();
        var serviceClientId = Id.GenerateNewId();
        string scope = "write:orders";
        var input = new CreateFromExistingInfoServiceClientScopeInput(entityInfo, serviceClientId, scope);

        // Act
        LogAct("Creating service client scope from existing info");
        var serviceClientScope = ServiceClientScope.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        serviceClientScope.ShouldNotBeNull();
        serviceClientScope.EntityInfo.ShouldBe(entityInfo);
        serviceClientScope.ServiceClientId.ShouldBe(serviceClientId);
        serviceClientScope.Scope.ShouldBe("write:orders");
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating service client scope");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.GenerateNewId();
        string scope = "read:users";
        var input = new RegisterNewServiceClientScopeInput(serviceClientId, scope);
        var serviceClientScope = ServiceClientScope.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning service client scope");
        var clone = serviceClientScope.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(serviceClientScope);
        clone.ServiceClientId.ShouldBe(serviceClientScope.ServiceClientId);
        clone.Scope.ShouldBe(serviceClientScope.Scope);
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
        bool result = ServiceClientScope.ValidateServiceClientId(executionContext, serviceClientId);

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
        bool result = ServiceClientScope.ValidateServiceClientId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateScope Tests

    [Fact]
    public void ValidateScope_WithValidScope_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Scope");
        bool result = ServiceClientScope.ValidateScope(executionContext, "read:users");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateScope_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Scope");
        bool result = ServiceClientScope.ValidateScope(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateScope_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Scope");
        bool result = ServiceClientScope.ValidateScope(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateScope_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating Scope at max length");
        var executionContext = CreateTestExecutionContext();
        string scope = new('a', ServiceClientScopeMetadata.ScopeMaxLength);

        // Act
        LogAct("Validating max-length Scope");
        bool result = ServiceClientScope.ValidateScope(executionContext, scope);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateScope_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating Scope exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string scope = new('a', ServiceClientScopeMetadata.ScopeMaxLength + 1);

        // Act
        LogAct("Validating too-long Scope");
        bool result = ServiceClientScope.ValidateScope(executionContext, scope);

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
        string name = ServiceClientScopeMetadata.ServiceClientIdPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("ServiceClientId");
    }

    [Fact]
    public void ScopePropertyName_ShouldBeScope()
    {
        // Arrange & Act
        LogAct("Reading ScopePropertyName");
        string name = ServiceClientScopeMetadata.ScopePropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("Scope");
    }

    [Fact]
    public void ServiceClientIdIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading ServiceClientIdIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        ServiceClientScopeMetadata.ServiceClientIdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ScopeIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading ScopeIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        ServiceClientScopeMetadata.ScopeIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ScopeMaxLength_Default_ShouldBe255()
    {
        // Arrange & Act
        LogAct("Reading ScopeMaxLength default");

        // Assert
        LogAssert("Verifying default is 255");
        ServiceClientScopeMetadata.ScopeMaxLength.ShouldBe(255);
    }

    [Fact]
    public void ChangeServiceClientIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = ServiceClientScopeMetadata.ServiceClientIdIsRequired;

        try
        {
            // Act
            LogAct("Changing ServiceClientId metadata");
            ServiceClientScopeMetadata.ChangeServiceClientIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            ServiceClientScopeMetadata.ServiceClientIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ServiceClientScopeMetadata.ChangeServiceClientIdMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeScopeMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = ServiceClientScopeMetadata.ScopeIsRequired;
        int originalMaxLength = ServiceClientScopeMetadata.ScopeMaxLength;

        try
        {
            // Act
            LogAct("Changing Scope metadata");
            ServiceClientScopeMetadata.ChangeScopeMetadata(
                isRequired: false,
                maxLength: 512
            );

            // Assert
            LogAssert("Verifying updated values");
            ServiceClientScopeMetadata.ScopeIsRequired.ShouldBeFalse();
            ServiceClientScopeMetadata.ScopeMaxLength.ShouldBe(512);
        }
        finally
        {
            ServiceClientScopeMetadata.ChangeScopeMetadata(originalIsRequired, originalMaxLength);
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

        // Act
        LogAct("Validating with static IsValid");
        bool result = ServiceClientScope.IsValid(executionContext, entityInfo, serviceClientId, "read:users");

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
        var input = new RegisterNewServiceClientScopeInput(Id.GenerateNewId(), "read:users");
        var entity = ServiceClientScope.RegisterNew(executionContext, input)!;

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
