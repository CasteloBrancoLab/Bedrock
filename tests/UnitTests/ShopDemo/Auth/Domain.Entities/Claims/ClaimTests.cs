using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.Claims.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ClaimMetadata = ShopDemo.Auth.Domain.Entities.Claims.Claim.ClaimMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Claims;

public class ClaimTests : TestBase
{
    public ClaimTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid name and description");
        var executionContext = CreateTestExecutionContext();
        string name = "read:users";
        string description = "Allows reading user data";
        var input = new RegisterNewClaimInput(name, description);

        // Act
        LogAct("Registering new Claim");
        var entity = Claim.RegisterNew(executionContext, input);

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
        var input = new RegisterNewClaimInput(null!, "Some description");

        // Act
        LogAct("Registering new Claim with null name");
        var entity = Claim.RegisterNew(executionContext, input);

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
        var input = new RegisterNewClaimInput("", "Some description");

        // Act
        LogAct("Registering new Claim with empty name");
        var entity = Claim.RegisterNew(executionContext, input);

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
        string longName = new('a', ClaimMetadata.NameMaxLength + 1);
        var input = new RegisterNewClaimInput(longName, "Some description");

        // Act
        LogAct("Registering new Claim with too-long name");
        var entity = Claim.RegisterNew(executionContext, input);

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
        var input = new RegisterNewClaimInput("read:users", null);

        // Act
        LogAct("Registering new Claim with null description");
        var entity = Claim.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with null description");
        entity.ShouldNotBeNull();
        entity.Name.ShouldBe("read:users");
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithValidDescription_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with valid description");
        var executionContext = CreateTestExecutionContext();
        string description = "A valid description for the claim";
        var input = new RegisterNewClaimInput("read:users", description);

        // Act
        LogAct("Registering new Claim with valid description");
        var entity = Claim.RegisterNew(executionContext, input);

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
        string longDescription = new('a', ClaimMetadata.DescriptionMaxLength + 1);
        var input = new RegisterNewClaimInput("read:users", longDescription);

        // Act
        LogAct("Registering new Claim with too-long description");
        var entity = Claim.RegisterNew(executionContext, input);

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
        LogArrange("Creating all properties for existing Claim");
        var entityInfo = CreateTestEntityInfo();
        string name = "write:users";
        string description = "Allows writing user data";
        var input = new CreateFromExistingInfoClaimInput(entityInfo, name, description);

        // Act
        LogAct("Creating Claim from existing info");
        var entity = Claim.CreateFromExistingInfo(input);

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
        LogArrange("Creating Claim via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewClaimInput("read:users", "Allows reading user data");
        var entity = Claim.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning Claim");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.Name.ShouldBe(entity.Name);
        clone.Description.ShouldBe(entity.Description);
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
        bool result = Claim.ValidateName(executionContext, "read:users");

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
        bool result = Claim.ValidateName(executionContext, null);

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
        bool result = Claim.ValidateDescription(executionContext, null);

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
        string longDescription = new('a', ClaimMetadata.DescriptionMaxLength + 1);

        // Act
        LogAct("Validating too-long description");
        bool result = Claim.ValidateDescription(executionContext, longDescription);

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
        bool originalIsRequired = ClaimMetadata.NameIsRequired;
        int originalMinLength = ClaimMetadata.NameMinLength;
        int originalMaxLength = ClaimMetadata.NameMaxLength;

        try
        {
            // Act
            LogAct("Changing Name metadata");
            ClaimMetadata.ChangeNameMetadata(
                isRequired: false,
                minLength: 5,
                maxLength: 50
            );

            // Assert
            LogAssert("Verifying Name metadata was updated");
            ClaimMetadata.NameIsRequired.ShouldBeFalse();
            ClaimMetadata.NameMinLength.ShouldBe(5);
            ClaimMetadata.NameMaxLength.ShouldBe(50);
        }
        finally
        {
            ClaimMetadata.ChangeNameMetadata(originalIsRequired, originalMinLength, originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeDescriptionMetadata_ShouldUpdate()
    {
        // Arrange
        LogArrange("Saving original Description metadata values");
        bool originalIsRequired = ClaimMetadata.DescriptionIsRequired;
        int originalMaxLength = ClaimMetadata.DescriptionMaxLength;

        try
        {
            // Act
            LogAct("Changing Description metadata");
            ClaimMetadata.ChangeDescriptionMetadata(
                isRequired: true,
                maxLength: 500
            );

            // Assert
            LogAssert("Verifying Description metadata was updated");
            ClaimMetadata.DescriptionIsRequired.ShouldBeTrue();
            ClaimMetadata.DescriptionMaxLength.ShouldBe(500);
        }
        finally
        {
            ClaimMetadata.ChangeDescriptionMetadata(originalIsRequired, originalMaxLength);
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
