using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using TokenExchangeMetadata = ShopDemo.Auth.Domain.Entities.TokenExchanges.TokenExchange.TokenExchangeMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.TokenExchanges;

public class TokenExchangeTests : TestBase
{
    public TokenExchangeTests(ITestOutputHelper outputHelper) : base(outputHelper)
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
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new RegisterNewTokenExchangeInput(
            userId, "subject-jti-abc123", "https://api.example.com",
            "issued-jti-def456", expiresAt);

        // Act
        LogAct("Registering new TokenExchange");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created with correct properties");
        entity.ShouldNotBeNull();
        entity.UserId.ShouldBe(userId);
        entity.SubjectTokenJti.ShouldBe("subject-jti-abc123");
        entity.RequestedAudience.ShouldBe("https://api.example.com");
        entity.IssuedTokenJti.ShouldBe("issued-jti-def456");
        entity.ExpiresAt.ShouldBe(expiresAt);
        entity.IssuedAt.ShouldNotBe(default);
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_ShouldSetIssuedAtFromExecutionContextTimestamp()
    {
        // Arrange
        LogArrange("Creating execution context to verify IssuedAt is set from timestamp");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewTokenExchangeInput(
            userId, "sub-jti", "audience", "issued-jti",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying IssuedAt matches execution context timestamp");
        entity.ShouldNotBeNull();
        entity.IssuedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void RegisterNew_WithNullSubjectTokenJti_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null SubjectTokenJti");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewTokenExchangeInput(
            userId, null!, "audience", "issued-jti",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with null SubjectTokenJti");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithSubjectTokenJtiExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with SubjectTokenJti exceeding max length of 36");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var longJti = new string('j', 37);
        var input = new RegisterNewTokenExchangeInput(
            userId, longJti, "audience", "issued-jti",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with oversized SubjectTokenJti");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithSubjectTokenJtiAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with SubjectTokenJti at exactly max length of 36");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var maxJti = new string('j', 36);
        var input = new RegisterNewTokenExchangeInput(
            userId, maxJti, "audience", "issued-jti",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with SubjectTokenJti at boundary");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.SubjectTokenJti.ShouldBe(maxJti);
    }

    [Fact]
    public void RegisterNew_WithNullRequestedAudience_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null RequestedAudience");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewTokenExchangeInput(
            userId, "sub-jti", null!, "issued-jti",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with null RequestedAudience");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithRequestedAudienceExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with RequestedAudience exceeding max length of 255");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var longAudience = new string('a', 256);
        var input = new RegisterNewTokenExchangeInput(
            userId, "sub-jti", longAudience, "issued-jti",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with oversized RequestedAudience");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithRequestedAudienceAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with RequestedAudience at exactly max length of 255");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var maxAudience = new string('a', 255);
        var input = new RegisterNewTokenExchangeInput(
            userId, "sub-jti", maxAudience, "issued-jti",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with RequestedAudience at boundary");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.RequestedAudience.ShouldBe(maxAudience);
    }

    [Fact]
    public void RegisterNew_WithNullIssuedTokenJti_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null IssuedTokenJti");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewTokenExchangeInput(
            userId, "sub-jti", "audience", null!,
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with null IssuedTokenJti");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to IsRequired validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithIssuedTokenJtiExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with IssuedTokenJti exceeding max length of 36");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var longJti = new string('i', 37);
        var input = new RegisterNewTokenExchangeInput(
            userId, "sub-jti", "audience", longJti,
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with oversized IssuedTokenJti");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned due to MaxLength validation failure");
        entity.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithIssuedTokenJtiAtMaxLength_ShouldCreateEntity()
    {
        // Arrange
        LogArrange("Creating input with IssuedTokenJti at exactly max length of 36");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var maxJti = new string('i', 36);
        var input = new RegisterNewTokenExchangeInput(
            userId, "sub-jti", "audience", maxJti,
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        LogAct("Registering new TokenExchange with IssuedTokenJti at boundary");
        var entity = TokenExchange.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying entity was created successfully");
        entity.ShouldNotBeNull();
        entity.IssuedTokenJti.ShouldBe(maxJti);
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing TokenExchange");
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var issuedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var input = new CreateFromExistingInfoTokenExchangeInput(
            entityInfo, userId, "existing-sub-jti", "https://api.example.com",
            "existing-issued-jti", issuedAt, expiresAt);

        // Act
        LogAct("Creating TokenExchange from existing info");
        var entity = TokenExchange.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entity.ShouldNotBeNull();
        entity.EntityInfo.ShouldBe(entityInfo);
        entity.UserId.ShouldBe(userId);
        entity.SubjectTokenJti.ShouldBe("existing-sub-jti");
        entity.RequestedAudience.ShouldBe("https://api.example.com");
        entity.IssuedTokenJti.ShouldBe("existing-issued-jti");
        entity.IssuedAt.ShouldBe(issuedAt);
        entity.ExpiresAt.ShouldBe(expiresAt);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating TokenExchange via RegisterNew");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new RegisterNewTokenExchangeInput(
            userId, "clone-sub-jti", "clone-audience", "clone-issued-jti",
            DateTimeOffset.UtcNow.AddHours(1));
        var entity = TokenExchange.RegisterNew(executionContext, input)!;

        // Act
        LogAct("Cloning TokenExchange");
        var clone = entity.Clone();

        // Assert
        LogAssert("Verifying clone has same values but is a different instance");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(entity);
        clone.UserId.ShouldBe(entity.UserId);
        clone.SubjectTokenJti.ShouldBe(entity.SubjectTokenJti);
        clone.RequestedAudience.ShouldBe(entity.RequestedAudience);
        clone.IssuedTokenJti.ShouldBe(entity.IssuedTokenJti);
        clone.IssuedAt.ShouldBe(entity.IssuedAt);
        clone.ExpiresAt.ShouldBe(entity.ExpiresAt);
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
        bool result = TokenExchange.ValidateUserId(executionContext, userId);

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
        bool result = TokenExchange.ValidateUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateSubjectTokenJti Tests

    [Fact]
    public void ValidateSubjectTokenJti_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid SubjectTokenJti");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid SubjectTokenJti");
        bool result = TokenExchange.ValidateSubjectTokenJti(executionContext, "valid-jti");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSubjectTokenJti_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null SubjectTokenJti");
        bool result = TokenExchange.ValidateSubjectTokenJti(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSubjectTokenJti_WithEmpty_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty SubjectTokenJti (empty string is not null, passes IsRequired)");
        bool result = TokenExchange.ValidateSubjectTokenJti(executionContext, "");

        // Assert
        LogAssert("Verifying validation passes (empty string has length 0 which is within max length)");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSubjectTokenJti_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized SubjectTokenJti");
        var executionContext = CreateTestExecutionContext();
        var longJti = new string('j', 37);

        // Act
        LogAct("Validating oversized SubjectTokenJti");
        bool result = TokenExchange.ValidateSubjectTokenJti(executionContext, longJti);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSubjectTokenJti_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and SubjectTokenJti at max length");
        var executionContext = CreateTestExecutionContext();
        var maxJti = new string('j', 36);

        // Act
        LogAct("Validating SubjectTokenJti at max length boundary");
        bool result = TokenExchange.ValidateSubjectTokenJti(executionContext, maxJti);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateRequestedAudience Tests

    [Fact]
    public void ValidateRequestedAudience_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid RequestedAudience");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid RequestedAudience");
        bool result = TokenExchange.ValidateRequestedAudience(executionContext, "https://api.example.com");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRequestedAudience_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null RequestedAudience");
        bool result = TokenExchange.ValidateRequestedAudience(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRequestedAudience_WithEmpty_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty RequestedAudience");
        bool result = TokenExchange.ValidateRequestedAudience(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRequestedAudience_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized RequestedAudience");
        var executionContext = CreateTestExecutionContext();
        var longAudience = new string('a', 256);

        // Act
        LogAct("Validating oversized RequestedAudience");
        bool result = TokenExchange.ValidateRequestedAudience(executionContext, longAudience);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRequestedAudience_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and RequestedAudience at max length");
        var executionContext = CreateTestExecutionContext();
        var maxAudience = new string('a', 255);

        // Act
        LogAct("Validating RequestedAudience at max length boundary");
        bool result = TokenExchange.ValidateRequestedAudience(executionContext, maxAudience);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateIssuedTokenJti Tests

    [Fact]
    public void ValidateIssuedTokenJti_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid IssuedTokenJti");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid IssuedTokenJti");
        bool result = TokenExchange.ValidateIssuedTokenJti(executionContext, "valid-issued-jti");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIssuedTokenJti_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null IssuedTokenJti");
        bool result = TokenExchange.ValidateIssuedTokenJti(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIssuedTokenJti_WithEmpty_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty IssuedTokenJti");
        bool result = TokenExchange.ValidateIssuedTokenJti(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIssuedTokenJti_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and oversized IssuedTokenJti");
        var executionContext = CreateTestExecutionContext();
        var longJti = new string('i', 37);

        // Act
        LogAct("Validating oversized IssuedTokenJti");
        bool result = TokenExchange.ValidateIssuedTokenJti(executionContext, longJti);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIssuedTokenJti_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and IssuedTokenJti at max length");
        var executionContext = CreateTestExecutionContext();
        var maxJti = new string('i', 36);

        // Act
        LogAct("Validating IssuedTokenJti at max length boundary");
        bool result = TokenExchange.ValidateIssuedTokenJti(executionContext, maxJti);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateIssuedAt Tests

    [Fact]
    public void ValidateIssuedAt_WithValidDate_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid IssuedAt");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid IssuedAt");
        bool result = TokenExchange.ValidateIssuedAt(executionContext, DateTimeOffset.UtcNow);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateIssuedAt_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null IssuedAt");
        bool result = TokenExchange.ValidateIssuedAt(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateExpiresAt Tests

    [Fact]
    public void ValidateExpiresAt_WithValidDate_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and valid ExpiresAt");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid ExpiresAt");
        bool result = TokenExchange.ValidateExpiresAt(executionContext, DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateExpiresAt_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ExpiresAt");
        bool result = TokenExchange.ValidateExpiresAt(executionContext, null);

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
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating all fields");
        bool result = TokenExchange.IsValid(
            executionContext, entityInfo, userId,
            "sub-jti", "audience", "issued-jti",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

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
        bool result = TokenExchange.IsValid(
            executionContext, entityInfo, null,
            "sub-jti", "audience", "issued-jti",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullSubjectTokenJti_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null SubjectTokenJti");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating with null SubjectTokenJti");
        bool result = TokenExchange.IsValid(
            executionContext, entityInfo, userId,
            null, "audience", "issued-jti",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullRequestedAudience_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null RequestedAudience");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating with null RequestedAudience");
        bool result = TokenExchange.IsValid(
            executionContext, entityInfo, userId,
            "sub-jti", null, "issued-jti",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullIssuedTokenJti_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context with null IssuedTokenJti");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        // Act
        LogAct("Validating with null IssuedTokenJti");
        bool result = TokenExchange.IsValid(
            executionContext, entityInfo, userId,
            "sub-jti", "audience", null,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

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
        bool originalIsRequired = TokenExchangeMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata to not required");
            TokenExchangeMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying UserIdIsRequired was updated");
            TokenExchangeMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            TokenExchangeMetadata.ChangeUserIdMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeSubjectTokenJtiMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original SubjectTokenJti metadata values");
        bool originalIsRequired = TokenExchangeMetadata.SubjectTokenJtiIsRequired;
        int originalMaxLength = TokenExchangeMetadata.SubjectTokenJtiMaxLength;

        try
        {
            // Act
            LogAct("Changing SubjectTokenJti metadata");
            TokenExchangeMetadata.ChangeSubjectTokenJtiMetadata(isRequired: false, maxLength: 64);

            // Assert
            LogAssert("Verifying SubjectTokenJti metadata was updated");
            TokenExchangeMetadata.SubjectTokenJtiIsRequired.ShouldBeFalse();
            TokenExchangeMetadata.SubjectTokenJtiMaxLength.ShouldBe(64);
        }
        finally
        {
            TokenExchangeMetadata.ChangeSubjectTokenJtiMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeRequestedAudienceMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original RequestedAudience metadata values");
        bool originalIsRequired = TokenExchangeMetadata.RequestedAudienceIsRequired;
        int originalMaxLength = TokenExchangeMetadata.RequestedAudienceMaxLength;

        try
        {
            // Act
            LogAct("Changing RequestedAudience metadata");
            TokenExchangeMetadata.ChangeRequestedAudienceMetadata(isRequired: false, maxLength: 512);

            // Assert
            LogAssert("Verifying RequestedAudience metadata was updated");
            TokenExchangeMetadata.RequestedAudienceIsRequired.ShouldBeFalse();
            TokenExchangeMetadata.RequestedAudienceMaxLength.ShouldBe(512);
        }
        finally
        {
            TokenExchangeMetadata.ChangeRequestedAudienceMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeIssuedTokenJtiMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original IssuedTokenJti metadata values");
        bool originalIsRequired = TokenExchangeMetadata.IssuedTokenJtiIsRequired;
        int originalMaxLength = TokenExchangeMetadata.IssuedTokenJtiMaxLength;

        try
        {
            // Act
            LogAct("Changing IssuedTokenJti metadata");
            TokenExchangeMetadata.ChangeIssuedTokenJtiMetadata(isRequired: false, maxLength: 64);

            // Assert
            LogAssert("Verifying IssuedTokenJti metadata was updated");
            TokenExchangeMetadata.IssuedTokenJtiIsRequired.ShouldBeFalse();
            TokenExchangeMetadata.IssuedTokenJtiMaxLength.ShouldBe(64);
        }
        finally
        {
            TokenExchangeMetadata.ChangeIssuedTokenJtiMetadata(
                isRequired: originalIsRequired, maxLength: originalMaxLength);
        }
    }

    [Fact]
    public void Metadata_ChangeIssuedAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original IssuedAtIsRequired value");
        bool originalIsRequired = TokenExchangeMetadata.IssuedAtIsRequired;

        try
        {
            // Act
            LogAct("Changing IssuedAt metadata to not required");
            TokenExchangeMetadata.ChangeIssuedAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying IssuedAtIsRequired was updated");
            TokenExchangeMetadata.IssuedAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            TokenExchangeMetadata.ChangeIssuedAtMetadata(isRequired: originalIsRequired);
        }
    }

    [Fact]
    public void Metadata_ChangeExpiresAtMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Saving original ExpiresAtIsRequired value");
        bool originalIsRequired = TokenExchangeMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata to not required");
            TokenExchangeMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying ExpiresAtIsRequired was updated");
            TokenExchangeMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            TokenExchangeMetadata.ChangeExpiresAtMetadata(isRequired: originalIsRequired);
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
