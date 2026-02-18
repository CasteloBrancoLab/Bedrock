using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using ShopDemo.Auth.Domain.Entities.Tenants.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using TenantMetadata = ShopDemo.Auth.Domain.Entities.Tenants.Tenant.TenantMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Tenants;

public class TenantTests : TestBase
{
    public TenantTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateTenant()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewTenantInput(
            "Acme Corp", "acme.example.com", "acme_schema", TenantTier.Professional);

        // Act
        LogAct("Registering new tenant");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying tenant was created successfully");
        tenant.ShouldNotBeNull();
        tenant.Name.ShouldBe("Acme Corp");
        tenant.Domain.ShouldBe("acme.example.com");
        tenant.SchemaName.ShouldBe("acme_schema");
        tenant.Status.ShouldBe(TenantStatus.Active);
        tenant.Tier.ShouldBe(TenantTier.Professional);
        tenant.DbVersion.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new tenant");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        tenant.ShouldNotBeNull();
        tenant.Status.ShouldBe(TenantStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldSetDbVersionToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new tenant");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying DbVersion is null");
        tenant.ShouldNotBeNull();
        tenant.DbVersion.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new tenant");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        tenant.ShouldNotBeNull();
        tenant.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithNullName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Name");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewTenantInput(
            null!, "acme.example.com", "acme_schema", TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with null Name");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty Name");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewTenantInput(
            "", "acme.example.com", "acme_schema", TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with empty Name");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNameExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Name exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longName = new('a', TenantMetadata.NameMaxLength + 1);
        var input = new RegisterNewTenantInput(
            longName, "acme.example.com", "acme_schema", TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with too-long Name");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullDomain_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Domain");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewTenantInput(
            "Acme Corp", null!, "acme_schema", TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with null Domain");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyDomain_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty Domain");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewTenantInput(
            "Acme Corp", "", "acme_schema", TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with empty Domain");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDomainExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Domain exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longDomain = new('a', TenantMetadata.DomainMaxLength + 1);
        var input = new RegisterNewTenantInput(
            "Acme Corp", longDomain, "acme_schema", TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with too-long Domain");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullSchemaName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null SchemaName");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewTenantInput(
            "Acme Corp", "acme.example.com", null!, TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with null SchemaName");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptySchemaName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty SchemaName");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewTenantInput(
            "Acme Corp", "acme.example.com", "", TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with empty SchemaName");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithSchemaNameExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with SchemaName exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longSchemaName = new('a', TenantMetadata.SchemaNameMaxLength + 1);
        var input = new RegisterNewTenantInput(
            "Acme Corp", "acme.example.com", longSchemaName, TenantTier.Basic);

        // Act
        LogAct("Registering new tenant with too-long SchemaName");
        var tenant = Tenant.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        tenant.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateTenantWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing tenant");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoTenantInput(
            entityInfo, "Acme Corp", "acme.example.com", "acme_schema",
            TenantStatus.Suspended, TenantTier.Enterprise, "1.2.3");

        // Act
        LogAct("Creating tenant from existing info");
        var tenant = Tenant.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        tenant.EntityInfo.ShouldBe(entityInfo);
        tenant.Name.ShouldBe("Acme Corp");
        tenant.Domain.ShouldBe("acme.example.com");
        tenant.SchemaName.ShouldBe("acme_schema");
        tenant.Status.ShouldBe(TenantStatus.Suspended);
        tenant.Tier.ShouldBe(TenantTier.Enterprise);
        tenant.DbVersion.ShouldBe("1.2.3");
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with empty Name (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoTenantInput(
            entityInfo, "", "", "", TenantStatus.Active, TenantTier.Basic, null);

        // Act
        LogAct("Creating tenant from existing info with empty Name");
        var tenant = Tenant.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying tenant was created without validation");
        tenant.ShouldNotBeNull();
        tenant.Name.ShouldBe("");
    }

    [Fact]
    public void CreateFromExistingInfo_WithNullDbVersion_ShouldPreserveNull()
    {
        // Arrange
        LogArrange("Creating input with null DbVersion");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoTenantInput(
            entityInfo, "Test", "test.com", "test_schema",
            TenantStatus.Active, TenantTier.Basic, null);

        // Act
        LogAct("Creating tenant from existing info");
        var tenant = Tenant.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying DbVersion is null");
        tenant.DbVersion.ShouldBeNull();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);

        // Act
        LogAct("Cloning tenant");
        var clone = tenant.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(tenant);
        clone.Name.ShouldBe(tenant.Name);
        clone.Domain.ShouldBe(tenant.Domain);
        clone.SchemaName.ShouldBe(tenant.SchemaName);
        clone.Status.ShouldBe(tenant.Status);
        clone.Tier.ShouldBe(tenant.Tier);
        clone.DbVersion.ShouldBe(tenant.DbVersion);
    }

    #endregion

    #region ChangeStatus Tests

    [Theory]
    [InlineData(TenantStatus.Active, TenantStatus.Suspended)]
    [InlineData(TenantStatus.Active, TenantStatus.Maintenance)]
    [InlineData(TenantStatus.Suspended, TenantStatus.Active)]
    [InlineData(TenantStatus.Suspended, TenantStatus.Maintenance)]
    [InlineData(TenantStatus.Maintenance, TenantStatus.Active)]
    [InlineData(TenantStatus.Maintenance, TenantStatus.Suspended)]
    public void ChangeStatus_WithValidTransition_ShouldSucceed(TenantStatus from, TenantStatus to)
    {
        // Arrange
        LogArrange($"Creating tenant with status {from}");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenantWithStatus(executionContext, from);
        var input = new ChangeTenantStatusInput(to);

        // Act
        LogAct($"Changing status from {from} to {to}");
        var result = tenant.ChangeStatus(executionContext, input);

        // Assert
        LogAssert($"Verifying status changed to {to}");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(to);
    }

    [Fact]
    public void ChangeStatus_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        var input = new ChangeTenantStatusInput(TenantStatus.Suspended);

        // Act
        LogAct("Changing status");
        var result = tenant.ChangeStatus(executionContext, input);

        // Assert
        LogAssert("Verifying new instance was returned (clone-modify-return)");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(tenant);
        tenant.Status.ShouldBe(TenantStatus.Active);
        result.Status.ShouldBe(TenantStatus.Suspended);
    }

    [Theory]
    [InlineData(TenantStatus.Active)]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Maintenance)]
    public void ChangeStatus_ToSameStatus_ShouldReturnNull(TenantStatus status)
    {
        // Arrange
        LogArrange($"Creating tenant with status {status}");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenantWithStatus(executionContext, status);
        var input = new ChangeTenantStatusInput(status);

        // Act
        LogAct("Changing to same status");
        var newContext = CreateTestExecutionContext();
        var result = tenant.ChangeStatus(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for same-status transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ChangeDbVersion Tests

    [Fact]
    public void ChangeDbVersion_WithValidVersion_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        var input = new ChangeTenantDbVersionInput("2.0.0");

        // Act
        LogAct("Changing DbVersion");
        var result = tenant.ChangeDbVersion(executionContext, input);

        // Assert
        LogAssert("Verifying DbVersion was changed");
        result.ShouldNotBeNull();
        result.DbVersion.ShouldBe("2.0.0");
    }

    [Fact]
    public void ChangeDbVersion_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        var input = new ChangeTenantDbVersionInput("2.0.0");

        // Act
        LogAct("Changing DbVersion");
        var result = tenant.ChangeDbVersion(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(tenant);
    }

    [Fact]
    public void ChangeDbVersion_WithNullVersion_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        var input = new ChangeTenantDbVersionInput(null!);

        // Act
        LogAct("Changing DbVersion to null");
        var result = tenant.ChangeDbVersion(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for null DbVersion");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ChangeDbVersion_WithEmptyVersion_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        var input = new ChangeTenantDbVersionInput("");

        // Act
        LogAct("Changing DbVersion to empty string");
        var result = tenant.ChangeDbVersion(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for empty DbVersion");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ChangeDbVersion_ExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        string longVersion = new('v', TenantMetadata.DbVersionMaxLength + 1);
        var input = new ChangeTenantDbVersionInput(longVersion);

        // Act
        LogAct("Changing DbVersion to overly long value");
        var result = tenant.ChangeDbVersion(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for too-long DbVersion");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidTenant_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = tenant.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidTenant_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating tenant with invalid state via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoTenantInput(
            entityInfo, "", "test.com", "test_schema",
            TenantStatus.Active, TenantTier.Basic, null);
        var tenant = Tenant.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on tenant with empty Name");
        bool result = tenant.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty Name");
        result.ShouldBeFalse();
        validationContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateName Tests

    [Fact]
    public void ValidateName_WithValidName_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Name");
        bool result = Tenant.ValidateName(executionContext, "Acme Corp");

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
        LogAct("Validating null Name");
        bool result = Tenant.ValidateName(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateName_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Name");
        bool result = Tenant.ValidateName(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateName_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating Name at max length");
        var executionContext = CreateTestExecutionContext();
        string name = new('a', TenantMetadata.NameMaxLength);

        // Act
        LogAct("Validating max-length Name");
        bool result = Tenant.ValidateName(executionContext, name);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateName_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating Name exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string name = new('a', TenantMetadata.NameMaxLength + 1);

        // Act
        LogAct("Validating too-long Name");
        bool result = Tenant.ValidateName(executionContext, name);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateDomain Tests

    [Fact]
    public void ValidateDomain_WithValidDomain_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Domain");
        bool result = Tenant.ValidateDomain(executionContext, "acme.example.com");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDomain_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Domain");
        bool result = Tenant.ValidateDomain(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDomain_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Domain");
        bool result = Tenant.ValidateDomain(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDomain_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating Domain at max length");
        var executionContext = CreateTestExecutionContext();
        string domain = new('a', TenantMetadata.DomainMaxLength);

        // Act
        LogAct("Validating max-length Domain");
        bool result = Tenant.ValidateDomain(executionContext, domain);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDomain_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating Domain exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string domain = new('a', TenantMetadata.DomainMaxLength + 1);

        // Act
        LogAct("Validating too-long Domain");
        bool result = Tenant.ValidateDomain(executionContext, domain);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateSchemaName Tests

    [Fact]
    public void ValidateSchemaName_WithValidSchemaName_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid SchemaName");
        bool result = Tenant.ValidateSchemaName(executionContext, "acme_schema");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSchemaName_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null SchemaName");
        bool result = Tenant.ValidateSchemaName(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSchemaName_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty SchemaName");
        bool result = Tenant.ValidateSchemaName(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSchemaName_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating SchemaName at max length");
        var executionContext = CreateTestExecutionContext();
        string schemaName = new('a', TenantMetadata.SchemaNameMaxLength);

        // Act
        LogAct("Validating max-length SchemaName");
        bool result = Tenant.ValidateSchemaName(executionContext, schemaName);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSchemaName_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating SchemaName exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string schemaName = new('a', TenantMetadata.SchemaNameMaxLength + 1);

        // Act
        LogAct("Validating too-long SchemaName");
        bool result = Tenant.ValidateSchemaName(executionContext, schemaName);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(TenantStatus.Active)]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Maintenance)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(TenantStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = Tenant.ValidateStatus(executionContext, status);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatus_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null status");
        bool result = Tenant.ValidateStatus(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateTier Tests

    [Theory]
    [InlineData(TenantTier.Basic)]
    [InlineData(TenantTier.Professional)]
    [InlineData(TenantTier.Enterprise)]
    public void ValidateTier_WithValidTier_ShouldReturnTrue(TenantTier tier)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating tier: {tier}");
        bool result = Tenant.ValidateTier(executionContext, tier);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTier_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null tier");
        bool result = Tenant.ValidateTier(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Theory]
    [InlineData(TenantStatus.Active, TenantStatus.Suspended)]
    [InlineData(TenantStatus.Active, TenantStatus.Maintenance)]
    [InlineData(TenantStatus.Suspended, TenantStatus.Active)]
    [InlineData(TenantStatus.Suspended, TenantStatus.Maintenance)]
    [InlineData(TenantStatus.Maintenance, TenantStatus.Active)]
    [InlineData(TenantStatus.Maintenance, TenantStatus.Suspended)]
    public void ValidateStatusTransition_ValidTransitions_ShouldReturnTrue(
        TenantStatus from, TenantStatus to)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating transition {from} -> {to}");
        bool result = Tenant.ValidateStatusTransition(executionContext, from, to);

        // Assert
        LogAssert("Verifying transition is valid");
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(TenantStatus.Active)]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Maintenance)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(TenantStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = Tenant.ValidateStatusTransition(executionContext, status, status);

        // Assert
        LogAssert("Verifying same-status transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_WithNullFrom_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null -> Active transition");
        bool result = Tenant.ValidateStatusTransition(executionContext, null, TenantStatus.Active);

        // Assert
        LogAssert("Verifying null from is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_WithNullTo_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> null transition");
        bool result = Tenant.ValidateStatusTransition(executionContext, TenantStatus.Active, null);

        // Assert
        LogAssert("Verifying null to is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateDbVersion Tests

    [Fact]
    public void ValidateDbVersion_WithValidVersion_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid DbVersion");
        bool result = Tenant.ValidateDbVersion(executionContext, "1.0.0");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDbVersion_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null DbVersion");
        bool result = Tenant.ValidateDbVersion(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDbVersion_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty DbVersion");
        bool result = Tenant.ValidateDbVersion(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDbVersion_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating DbVersion at max length");
        var executionContext = CreateTestExecutionContext();
        string dbVersion = new('v', TenantMetadata.DbVersionMaxLength);

        // Act
        LogAct("Validating max-length DbVersion");
        bool result = Tenant.ValidateDbVersion(executionContext, dbVersion);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDbVersion_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating DbVersion exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string dbVersion = new('v', TenantMetadata.DbVersionMaxLength + 1);

        // Act
        LogAct("Validating too-long DbVersion");
        bool result = Tenant.ValidateDbVersion(executionContext, dbVersion);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Static IsValid Tests

    [Fact]
    public void IsValid_WithAllValidFields_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating all valid fields");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid");
        bool result = Tenant.IsValid(
            executionContext, entityInfo, "Acme Corp", "acme.example.com",
            "acme_schema", TenantStatus.Active, TenantTier.Professional);

        // Assert
        LogAssert("Verifying all fields are valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullName_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Name");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null Name");
        bool result = Tenant.IsValid(
            executionContext, entityInfo, null, "acme.example.com",
            "acme_schema", TenantStatus.Active, TenantTier.Professional);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullDomain_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Domain");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null Domain");
        bool result = Tenant.IsValid(
            executionContext, entityInfo, "Acme Corp", null,
            "acme_schema", TenantStatus.Active, TenantTier.Professional);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullSchemaName_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null SchemaName");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null SchemaName");
        bool result = Tenant.IsValid(
            executionContext, entityInfo, "Acme Corp", "acme.example.com",
            null, TenantStatus.Active, TenantTier.Professional);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullStatus_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Status");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null Status");
        bool result = Tenant.IsValid(
            executionContext, entityInfo, "Acme Corp", "acme.example.com",
            "acme_schema", null, TenantTier.Professional);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullTier_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Tier");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null Tier");
        bool result = Tenant.IsValid(
            executionContext, entityInfo, "Acme Corp", "acme.example.com",
            "acme_schema", TenantStatus.Active, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Change Tests

    [Fact]
    public void ChangeNameMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = TenantMetadata.NameIsRequired;
        int originalMaxLength = TenantMetadata.NameMaxLength;

        try
        {
            // Act
            LogAct("Changing Name metadata");
            TenantMetadata.ChangeNameMetadata(isRequired: false, maxLength: 512);

            // Assert
            LogAssert("Verifying updated values");
            TenantMetadata.NameIsRequired.ShouldBeFalse();
            TenantMetadata.NameMaxLength.ShouldBe(512);
        }
        finally
        {
            TenantMetadata.ChangeNameMetadata(originalIsRequired, originalMaxLength);
        }
    }

    [Fact]
    public void ChangeDomainMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = TenantMetadata.DomainIsRequired;
        int originalMaxLength = TenantMetadata.DomainMaxLength;

        try
        {
            // Act
            LogAct("Changing Domain metadata");
            TenantMetadata.ChangeDomainMetadata(isRequired: false, maxLength: 512);

            // Assert
            LogAssert("Verifying updated values");
            TenantMetadata.DomainIsRequired.ShouldBeFalse();
            TenantMetadata.DomainMaxLength.ShouldBe(512);
        }
        finally
        {
            TenantMetadata.ChangeDomainMetadata(originalIsRequired, originalMaxLength);
        }
    }

    [Fact]
    public void ChangeSchemaNameMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = TenantMetadata.SchemaNameIsRequired;
        int originalMaxLength = TenantMetadata.SchemaNameMaxLength;

        try
        {
            // Act
            LogAct("Changing SchemaName metadata");
            TenantMetadata.ChangeSchemaNameMetadata(isRequired: false, maxLength: 128);

            // Assert
            LogAssert("Verifying updated values");
            TenantMetadata.SchemaNameIsRequired.ShouldBeFalse();
            TenantMetadata.SchemaNameMaxLength.ShouldBe(128);
        }
        finally
        {
            TenantMetadata.ChangeSchemaNameMetadata(originalIsRequired, originalMaxLength);
        }
    }

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = TenantMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata");
            TenantMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            TenantMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            TenantMetadata.ChangeStatusMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeTierMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = TenantMetadata.TierIsRequired;

        try
        {
            // Act
            LogAct("Changing Tier metadata");
            TenantMetadata.ChangeTierMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            TenantMetadata.TierIsRequired.ShouldBeFalse();
        }
        finally
        {
            TenantMetadata.ChangeTierMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeDbVersionMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = TenantMetadata.DbVersionIsRequired;
        int originalMaxLength = TenantMetadata.DbVersionMaxLength;

        try
        {
            // Act
            LogAct("Changing DbVersion metadata");
            TenantMetadata.ChangeDbVersionMetadata(isRequired: false, maxLength: 100);

            // Assert
            LogAssert("Verifying updated values");
            TenantMetadata.DbVersionIsRequired.ShouldBeFalse();
            TenantMetadata.DbVersionMaxLength.ShouldBe(100);
        }
        finally
        {
            TenantMetadata.ChangeDbVersionMetadata(originalIsRequired, originalMaxLength);
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

    private static Tenant CreateTestTenant(ExecutionContext executionContext)
    {
        var input = CreateValidRegisterNewInput();
        return Tenant.RegisterNew(executionContext, input)!;
    }

    private static Tenant CreateTestTenantWithStatus(ExecutionContext executionContext, TenantStatus status)
    {
        var tenant = CreateTestTenant(executionContext);

        if (status != TenantStatus.Active)
        {
            var changeStatusInput = new ChangeTenantStatusInput(status);
            tenant = tenant.ChangeStatus(executionContext, changeStatusInput)!;
        }

        return tenant;
    }

    private static RegisterNewTenantInput CreateValidRegisterNewInput()
    {
        return new RegisterNewTenantInput(
            "Acme Corp", "acme.example.com", "acme_schema", TenantTier.Professional);
    }

    #endregion
}
