using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ConsentTermMetadata = ShopDemo.Auth.Domain.Entities.ConsentTerms.ConsentTerm.ConsentTermMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ConsentTerms;

public class ConsentTermTests : TestBase
{
    public ConsentTermTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid data");
        var executionContext = CreateTestExecutionContext();
        var publishedAt = DateTimeOffset.UtcNow;
        var input = new RegisterNewConsentTermInput(
            ConsentTermType.TermsOfUse, "1.0", "Terms content here", publishedAt);

        // Act
        LogAct("Registering new ConsentTerm");
        var entity = ConsentTerm.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.Type.ShouldBe(ConsentTermType.TermsOfUse);
        entity.Version.ShouldBe("1.0");
        entity.Content.ShouldBe("Terms content here");
        entity.PublishedAt.ShouldBe(publishedAt);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(ConsentTermType.TermsOfUse)]
    [InlineData(ConsentTermType.PrivacyPolicy)]
    [InlineData(ConsentTermType.Marketing)]
    public void RegisterNew_WithAllConsentTermTypes_ShouldCreateEntity(ConsentTermType type)
    {
        // Arrange
        LogArrange($"Creating input with ConsentTermType {type}");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewConsentTermInput(
            type, "1.0", "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Registering new ConsentTerm");
        var entity = ConsentTerm.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct type");
        entity.ShouldNotBeNull();
        entity.Type.ShouldBe(type);
    }

    [Fact]
    public void RegisterNew_WithNullVersion_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Version");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewConsentTermInput(
            ConsentTermType.TermsOfUse, null!, "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Registering new ConsentTerm with null Version");
        var entity = ConsentTerm.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithVersionExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Version exceeding max length of 50");
        var executionContext = CreateTestExecutionContext();
        var longVersion = new string('v', 51);
        var input = new RegisterNewConsentTermInput(
            ConsentTermType.TermsOfUse, longVersion, "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Registering new ConsentTerm with oversized Version");
        var entity = ConsentTerm.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullContent_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Content");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewConsentTermInput(
            ConsentTermType.TermsOfUse, "1.0", null!, DateTimeOffset.UtcNow);

        // Act
        LogAct("Registering new ConsentTerm with null Content");
        var entity = ConsentTerm.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithContentExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Content exceeding max length of 100000");
        var executionContext = CreateTestExecutionContext();
        var longContent = new string('c', 100001);
        var input = new RegisterNewConsentTermInput(
            ConsentTermType.TermsOfUse, "1.0", longContent, DateTimeOffset.UtcNow);

        // Act
        LogAct("Registering new ConsentTerm with oversized Content");
        var entity = ConsentTerm.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithVersionAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with Version at exactly max length of 50");
        var executionContext = CreateTestExecutionContext();
        var maxVersion = new string('v', 50);
        var input = new RegisterNewConsentTermInput(
            ConsentTermType.PrivacyPolicy, maxVersion, "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Registering new ConsentTerm with Version at boundary");
        var entity = ConsentTerm.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.Version.ShouldBe(maxVersion);
    }

    [Fact]
    public void RegisterNew_WithContentAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with Content at exactly max length of 100000");
        var executionContext = CreateTestExecutionContext();
        var maxContent = new string('c', 100000);
        var input = new RegisterNewConsentTermInput(
            ConsentTermType.Marketing, "1.0", maxContent, DateTimeOffset.UtcNow);

        // Act
        LogAct("Registering new ConsentTerm with Content at boundary");
        var entity = ConsentTerm.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.Content.ShouldBe(maxContent);
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing ConsentTerm");
        var entityInfo = CreateTestEntityInfo();
        var publishedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoConsentTermInput(
            entityInfo, ConsentTermType.PrivacyPolicy, "2.0", "Privacy content", publishedAt);

        // Act
        LogAct("Creating ConsentTerm from existing info");
        var entity = ConsentTerm.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.Type.ShouldBe(ConsentTermType.PrivacyPolicy);
        entity.Version.ShouldBe("2.0");
        entity.Content.ShouldBe("Privacy content");
        entity.PublishedAt.ShouldBe(publishedAt);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating ConsentTerm via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var publishedAt = DateTimeOffset.UtcNow;
        var input = new RegisterNewConsentTermInput(
            ConsentTermType.TermsOfUse, "1.0", "Clone content", publishedAt);
        var entity = ConsentTerm.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning ConsentTerm");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.Type.ShouldBe(entity.Type);
        clone.Version.ShouldBe(entity.Version);
        clone.Content.ShouldBe(entity.Content);
        clone.PublishedAt.ShouldBe(entity.PublishedAt);
    }

    #endregion

    #region ValidateType Tests

    [Fact]
    public void ValidateType_WithValidType_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ConsentTermType");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Type");
        bool result = ConsentTerm.ValidateType(executionContext, ConsentTermType.TermsOfUse);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateType_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Type");
        bool result = ConsentTerm.ValidateType(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateVersion Tests

    [Fact]
    public void ValidateVersion_WithValidVersion_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid version");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Version");
        bool result = ConsentTerm.ValidateVersion(executionContext, "1.0.0");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateVersion_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Version");
        bool result = ConsentTerm.ValidateVersion(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateVersion_WithEmpty_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Version (empty string is not null, passes IsRequired)");
        bool result = ConsentTerm.ValidateVersion(executionContext, "");

        // Assert
        LogAssert("Verifying validation passes (empty string has length 0 which is within max length)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateVersion_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized version");
        var executionContext = CreateTestExecutionContext();
        var longVersion = new string('v', 51);

        // Act
        LogAct("Validating oversized Version");
        bool result = ConsentTerm.ValidateVersion(executionContext, longVersion);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateVersion_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and version at max length");
        var executionContext = CreateTestExecutionContext();
        var maxVersion = new string('v', 50);

        // Act
        LogAct("Validating Version at max length boundary");
        bool result = ConsentTerm.ValidateVersion(executionContext, maxVersion);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateContent Tests

    [Fact]
    public void ValidateContent_WithValidContent_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid content");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Content");
        bool result = ConsentTerm.ValidateContent(executionContext, "Valid content");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateContent_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Content");
        bool result = ConsentTerm.ValidateContent(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateContent_WithEmpty_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Content (empty string is not null, passes IsRequired)");
        bool result = ConsentTerm.ValidateContent(executionContext, "");

        // Assert
        LogAssert("Verifying validation passes (empty string has length 0 which is within max length)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateContent_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized content");
        var executionContext = CreateTestExecutionContext();
        var longContent = new string('c', 100001);

        // Act
        LogAct("Validating oversized Content");
        bool result = ConsentTerm.ValidateContent(executionContext, longContent);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateContent_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and content at max length");
        var executionContext = CreateTestExecutionContext();
        var maxContent = new string('c', 100000);

        // Act
        LogAct("Validating Content at max length boundary");
        bool result = ConsentTerm.ValidateContent(executionContext, maxContent);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidatePublishedAt Tests

    [Fact]
    public void ValidatePublishedAt_WithValidDate_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid PublishedAt");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid PublishedAt");
        bool result = ConsentTerm.ValidatePublishedAt(executionContext, DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePublishedAt_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null PublishedAt");
        bool result = ConsentTerm.ValidatePublishedAt(executionContext, null);

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

        // Act
        LogAct("Validating all fields");
        bool result = ConsentTerm.IsValid(
            executionContext, entityInfo, ConsentTermType.TermsOfUse,
            "1.0", "Content", DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullVersion_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null Version");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Validating with null Version");
        bool result = ConsentTerm.IsValid(
            executionContext, entityInfo, ConsentTermType.TermsOfUse,
            null, "Content", DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeTypeMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original TypeIsRequired value");
        bool originalIsRequired = ConsentTermMetadata.TypeIsRequired;

        try
        {
            // Act
            LogAct("Changing Type metadata to not required");
            ConsentTermMetadata.ChangeTypeMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying TypeIsRequired was updated");
            ConsentTermMetadata.TypeIsRequired.ShouldBeFalse();
        }
        finally
        {
            ConsentTermMetadata.ChangeTypeMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeVersionMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original Version metadata values");
        bool originalIsRequired = ConsentTermMetadata.VersionIsRequired;
        int originalMaxLength = ConsentTermMetadata.VersionMaxLength;

        try
        {
            // Act
            LogAct("Changing Version metadata");
            ConsentTermMetadata.ChangeVersionMetadata(isRequired: false, maxLength: 100);

            // Assert
            LogAssert("Verifying Version metadata was updated");
            ConsentTermMetadata.VersionIsRequired.ShouldBeFalse();
            ConsentTermMetadata.VersionMaxLength.ShouldBe(100);
        }
        finally
        {
            ConsentTermMetadata.ChangeVersionMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeContentMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original Content metadata values");
        bool originalIsRequired = ConsentTermMetadata.ContentIsRequired;
        int originalMaxLength = ConsentTermMetadata.ContentMaxLength;

        try
        {
            // Act
            LogAct("Changing Content metadata");
            ConsentTermMetadata.ChangeContentMetadata(isRequired: false, maxLength: 50000);

            // Assert
            LogAssert("Verifying Content metadata was updated");
            ConsentTermMetadata.ContentIsRequired.ShouldBeFalse();
            ConsentTermMetadata.ContentMaxLength.ShouldBe(50000);
        }
        finally
        {
            ConsentTermMetadata.ChangeContentMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangePublishedAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original PublishedAtIsRequired value");
        bool originalIsRequired = ConsentTermMetadata.PublishedAtIsRequired;

        try
        {
            // Act
            LogAct("Changing PublishedAt metadata to not required");
            ConsentTermMetadata.ChangePublishedAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying PublishedAtIsRequired was updated");
            ConsentTermMetadata.PublishedAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            ConsentTermMetadata.ChangePublishedAtMetadata(isRequired: originalIsRequired);
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
