using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;
using ShopDemo.Auth.Domain.Entities.ExternalLogins.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ExternalLoginMetadata = ShopDemo.Auth.Domain.Entities.ExternalLogins.ExternalLogin.ExternalLoginMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ExternalLogins;

public class ExternalLoginTests : TestBase
{
    public ExternalLoginTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating execution context and input with valid data");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var provider = LoginProvider.Google;
        var input = new RegisterNewExternalLoginInput(
            userId, provider, "google-user-123", "user@gmail.com");

        // Act
        LogAct("Registering new ExternalLogin");
        var entity = ExternalLogin.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.UserId.ShouldBe(userId);
        entity.Provider.ShouldBe(provider);
        entity.ProviderUserId.ShouldBe("google-user-123");
        entity.Email.ShouldBe("user@gmail.com");
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithNullEmail_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with null Email (optional field)");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewExternalLoginInput(
            userId, LoginProvider.GitHub, "github-user-456", null);

        // Act
        LogAct("Registering new ExternalLogin with null Email");
        var entity = ExternalLogin.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with null Email");
        entity.ShouldNotBeNull();
        entity.Email.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithNullProviderUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null ProviderUserId");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewExternalLoginInput(
            userId, LoginProvider.Google, null!, null);

        // Act
        LogAct("Registering new ExternalLogin with null ProviderUserId");
        var entity = ExternalLogin.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithProviderUserIdExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with ProviderUserId exceeding max length of 255");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var longProviderUserId = new string('p', 256);
        var input = new RegisterNewExternalLoginInput(
            userId, LoginProvider.Google, longProviderUserId, null);

        // Act
        LogAct("Registering new ExternalLogin with oversized ProviderUserId");
        var entity = ExternalLogin.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithProviderUserIdAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with ProviderUserId at exactly max length of 255");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var maxProviderUserId = new string('p', 255);
        var input = new RegisterNewExternalLoginInput(
            userId, LoginProvider.Microsoft, maxProviderUserId, null);

        // Act
        LogAct("Registering new ExternalLogin with ProviderUserId at boundary");
        var entity = ExternalLogin.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.ProviderUserId.ShouldBe(maxProviderUserId);
    }

    [Fact]
    public void RegisterNew_WithDefaultProvider_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default LoginProvider (empty value)");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var provider = default(LoginProvider);
        var input = new RegisterNewExternalLoginInput(
            userId, provider, "provider-user-id", null);

        // Act
        LogAct("Registering new ExternalLogin with default Provider");
        var entity = ExternalLogin.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithProviderExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Provider exceeding max length of 50");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var longProviderValue = new string('g', 51);
        var provider = LoginProvider.CreateNew(longProviderValue);
        var input = new RegisterNewExternalLoginInput(
            userId, provider, "provider-user-id", null);

        // Act
        LogAct("Registering new ExternalLogin with oversized Provider");
        var entity = ExternalLogin.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithAllWellKnownProviders_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Testing with all well-known providers");
        var providers = new[] { LoginProvider.Google, LoginProvider.GitHub, LoginProvider.Microsoft, LoginProvider.Apple };

        foreach (var provider in providers)
        {
            var executionContext = CreateTestExecutionContext();
            var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
            var input = new RegisterNewExternalLoginInput(
                userId, provider, $"{provider}-user-id", null);

            // Act
            LogAct($"Registering new ExternalLogin with provider {provider}");
            var entity = ExternalLogin.RegisterNew(executionContext, input);

            // Assert
            LogAssert($"Verifying entity was created with provider {provider}");
            entity.ShouldNotBeNull();
            entity.Provider.ShouldBe(provider);
        }
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing ExternalLogin");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var provider = LoginProvider.GitHub;
        var input = new CreateFromExistingInfoExternalLoginInput(
            entityInfo, userId, provider, "github-user-789", "user@github.com");

        // Act
        LogAct("Creating ExternalLogin from existing info");
        var entity = ExternalLogin.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.UserId.ShouldBe(userId);
        entity.Provider.ShouldBe(provider);
        entity.ProviderUserId.ShouldBe("github-user-789");
        entity.Email.ShouldBe("user@github.com");
    }

    [Fact]
    public void CreateFromExistingInfo_WithNullEmail_ShouldCreateWithNullEmail()
    {
        // Arrange
        LogArrange("Creating properties for existing ExternalLogin with null Email");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoExternalLoginInput(
            entityInfo, userId, LoginProvider.Apple, "apple-user-id", null);

        // Act
        LogAct("Creating ExternalLogin from existing info with null Email");
        var entity = ExternalLogin.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying Email is null");
        entity.ShouldNotBeNull();
        entity.Email.ShouldBeNull();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating ExternalLogin via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewExternalLoginInput(
            userId, LoginProvider.Google, "google-clone-user", "clone@gmail.com");
        var entity = ExternalLogin.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning ExternalLogin");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.UserId.ShouldBe(entity.UserId);
        clone.Provider.ShouldBe(entity.Provider);
        clone.ProviderUserId.ShouldBe(entity.ProviderUserId);
        clone.Email.ShouldBe(entity.Email);
    }

    [Fact]
    public void Clone_WithNullEmail_ShouldPreserveNull()
    {
        // Arrange
        LogArrange("Creating ExternalLogin with null Email");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewExternalLoginInput(
            userId, LoginProvider.Microsoft, "ms-user-id", null);
        var entity = ExternalLogin.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning ExternalLogin with null Email");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone preserves null Email");
        clone.ShouldNotBeNull();
        clone.Email.ShouldBeNull();
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
        bool result = ExternalLogin.ValidateUserId(executionContext, userId);

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
        bool result = ExternalLogin.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateProvider Tests

    [Fact]
    public void ValidateProvider_WithValidProvider_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid LoginProvider");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Provider");
        bool result = ExternalLogin.ValidateProvider(executionContext, LoginProvider.Google);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProvider_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Provider");
        bool result = ExternalLogin.ValidateProvider(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProvider_WithProviderExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized provider");
        var executionContext = CreateTestExecutionContext();
        var longProvider = LoginProvider.CreateNew(new string('x', 51));

        // Act
        LogAct("Validating oversized Provider");
        bool result = ExternalLogin.ValidateProvider(executionContext, longProvider);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProvider_WithProviderAtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and provider at max length");
        var executionContext = CreateTestExecutionContext();
        var maxProvider = LoginProvider.CreateNew(new string('x', 50));

        // Act
        LogAct("Validating Provider at max length boundary");
        bool result = ExternalLogin.ValidateProvider(executionContext, maxProvider);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProvider_WithDefaultProvider_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and default (empty) provider");
        var executionContext = CreateTestExecutionContext();
        var provider = default(LoginProvider);

        // Act
        LogAct("Validating default Provider");
        bool result = ExternalLogin.ValidateProvider(executionContext, provider);

        // Assert
        LogAssert("Verifying validation fails due to empty value");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateProviderUserId Tests

    [Fact]
    public void ValidateProviderUserId_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ProviderUserId");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid ProviderUserId");
        bool result = ExternalLogin.ValidateProviderUserId(executionContext, "provider-user-123");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProviderUserId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ProviderUserId");
        bool result = ExternalLogin.ValidateProviderUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProviderUserId_WithEmpty_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty ProviderUserId (empty string is not null, passes IsRequired)");
        bool result = ExternalLogin.ValidateProviderUserId(executionContext, "");

        // Assert
        LogAssert("Verifying validation passes (empty string has length 0 which is within max length)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProviderUserId_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized ProviderUserId");
        var executionContext = CreateTestExecutionContext();
        var longValue = new string('p', 256);

        // Act
        LogAct("Validating oversized ProviderUserId");
        bool result = ExternalLogin.ValidateProviderUserId(executionContext, longValue);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProviderUserId_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and ProviderUserId at max length");
        var executionContext = CreateTestExecutionContext();
        var maxValue = new string('p', 255);

        // Act
        LogAct("Validating ProviderUserId at max length boundary");
        bool result = ExternalLogin.ValidateProviderUserId(executionContext, maxValue);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
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
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating all fields");
        bool result = ExternalLogin.IsValid(
            executionContext, entityInfo, userId,
            LoginProvider.Google, "provider-user-id");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullUserId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null UserId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Validating with null UserId");
        bool result = ExternalLogin.IsValid(
            executionContext, entityInfo, null,
            LoginProvider.Google, "provider-user-id");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ChangeUserIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original UserIdIsRequired value");
        bool originalIsRequired = ExternalLoginMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata to not required");
            ExternalLoginMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying UserIdIsRequired was updated");
            ExternalLoginMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ExternalLoginMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeProviderMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original Provider metadata values");
        bool originalIsRequired = ExternalLoginMetadata.ProviderIsRequired;
        int originalMaxLength = ExternalLoginMetadata.ProviderMaxLength;

        try
        {
            // Act
            LogAct("Changing Provider metadata");
            ExternalLoginMetadata.ChangeProviderMetadata(isRequired: false, maxLength: 100);

            // Assert
            LogAssert("Verifying Provider metadata was updated");
            ExternalLoginMetadata.ProviderIsRequired.ShouldBeFalse();
            ExternalLoginMetadata.ProviderMaxLength.ShouldBe(100);
        }
        finally
        {
            ExternalLoginMetadata.ChangeProviderMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeProviderUserIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original ProviderUserId metadata values");
        bool originalIsRequired = ExternalLoginMetadata.ProviderUserIdIsRequired;
        int originalMaxLength = ExternalLoginMetadata.ProviderUserIdMaxLength;

        try
        {
            // Act
            LogAct("Changing ProviderUserId metadata");
            ExternalLoginMetadata.ChangeProviderUserIdMetadata(isRequired: false, maxLength: 512);

            // Assert
            LogAssert("Verifying ProviderUserId metadata was updated");
            ExternalLoginMetadata.ProviderUserIdIsRequired.ShouldBeFalse();
            ExternalLoginMetadata.ProviderUserIdMaxLength.ShouldBe(512);
        }
        finally
        {
            ExternalLoginMetadata.ChangeProviderUserIdMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeEmailMetadata_ShouldUpdateMaxLength()
    {
        // Arrange
        LogArrange("Saving original Email metadata values");
        int originalMaxLength = ExternalLoginMetadata.EmailMaxLength;

        try
        {
            // Act
            LogAct("Changing Email metadata");
            ExternalLoginMetadata.ChangeEmailMetadata(maxLength: 500);

            // Assert
            LogAssert("Verifying Email metadata was updated");
            ExternalLoginMetadata.EmailMaxLength.ShouldBe(500);
        }
        finally
        {
            ExternalLoginMetadata.ChangeEmailMetadata(maxLength: originalMaxLength);
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
