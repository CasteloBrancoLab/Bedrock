using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Security.Passwords;

public class PasswordPolicyTests : TestBase
{
    public PasswordPolicyTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ValidatePassword_WithValidPassword_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating complex password");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "ValidPass123!");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_AtMinLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        string password = "Abcdefghij1!";

        // Act
        LogAct("Validating min-length password");
        bool result = PasswordPolicy.ValidatePassword(executionContext, password);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        string password = "Aa1!" + new string('x', PasswordPolicyMetadata.MaxLength - 4);

        // Act
        LogAct("Validating max-length password");
        bool result = PasswordPolicy.ValidatePassword(executionContext, password);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_TooShort_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        string password = "Aa1!short";

        // Act
        LogAct("Validating too-short password");
        bool result = PasswordPolicy.ValidatePassword(executionContext, password);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_TooLong_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        string password = "Aa1!" + new string('x', PasswordPolicyMetadata.MaxLength - 3);

        // Act
        LogAct("Validating too-long password");
        bool result = PasswordPolicy.ValidatePassword(executionContext, password);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null password");
        bool result = PasswordPolicy.ValidatePassword(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty password");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_WithSpaces_WhenAllowed_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password with spaces");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "A password 1!");

        // Assert
        LogAssert("Verifying spaces are allowed");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_WithSpaces_WhenNotAllowed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context and disabling spaces");
        var executionContext = CreateTestExecutionContext();
        SaveAndChangeMetadata(allowSpaces: false);

        try
        {
            // Act
            LogAct("Validating password with spaces when disallowed");
            bool result = PasswordPolicy.ValidatePassword(executionContext, "A password 1!");

            // Assert
            LogAssert("Verifying spaces are rejected");
            result.ShouldBeFalse();
            executionContext.Messages.Select(m => m.Code).ShouldContain("PasswordPolicy.Password.AllowSpaces");
        }
        finally
        {
            RestoreMetadata();
        }
    }

    [Fact]
    public void ValidatePassword_WithoutUppercase_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password without uppercase");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "nouppercase1!");

        // Assert
        LogAssert("Verifying uppercase is required");
        result.ShouldBeFalse();
        executionContext.Messages.Select(m => m.Code).ShouldContain("PasswordPolicy.Password.RequireUppercase");
    }

    [Fact]
    public void ValidatePassword_WithoutUppercase_WhenNotRequired_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and disabling uppercase requirement");
        var executionContext = CreateTestExecutionContext();
        SaveAndChangeMetadata(requireUppercase: false);

        try
        {
            // Act
            LogAct("Validating password without uppercase when not required");
            bool result = PasswordPolicy.ValidatePassword(executionContext, "nouppercase1!");

            // Assert
            LogAssert("Verifying validation passes");
            result.ShouldBeTrue();
        }
        finally
        {
            RestoreMetadata();
        }
    }

    [Fact]
    public void ValidatePassword_WithoutLowercase_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password without lowercase");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "NOLOWERCASE1!");

        // Assert
        LogAssert("Verifying lowercase is required");
        result.ShouldBeFalse();
        executionContext.Messages.Select(m => m.Code).ShouldContain("PasswordPolicy.Password.RequireLowercase");
    }

    [Fact]
    public void ValidatePassword_WithoutLowercase_WhenNotRequired_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and disabling lowercase requirement");
        var executionContext = CreateTestExecutionContext();
        SaveAndChangeMetadata(requireLowercase: false);

        try
        {
            // Act
            LogAct("Validating password without lowercase when not required");
            bool result = PasswordPolicy.ValidatePassword(executionContext, "NOLOWERCASE1!");

            // Assert
            LogAssert("Verifying validation passes");
            result.ShouldBeTrue();
        }
        finally
        {
            RestoreMetadata();
        }
    }

    [Fact]
    public void ValidatePassword_WithoutDigit_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password without digit");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "NoDigitHere!!");

        // Assert
        LogAssert("Verifying digit is required");
        result.ShouldBeFalse();
        executionContext.Messages.Select(m => m.Code).ShouldContain("PasswordPolicy.Password.RequireDigit");
    }

    [Fact]
    public void ValidatePassword_WithoutDigit_WhenNotRequired_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and disabling digit requirement");
        var executionContext = CreateTestExecutionContext();
        SaveAndChangeMetadata(requireDigit: false);

        try
        {
            // Act
            LogAct("Validating password without digit when not required");
            bool result = PasswordPolicy.ValidatePassword(executionContext, "NoDigitHere!!");

            // Assert
            LogAssert("Verifying validation passes");
            result.ShouldBeTrue();
        }
        finally
        {
            RestoreMetadata();
        }
    }

    [Fact]
    public void ValidatePassword_WithoutSpecialCharacter_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password without special character");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "NoSpecialChar1");

        // Assert
        LogAssert("Verifying special character is required");
        result.ShouldBeFalse();
        executionContext.Messages.Select(m => m.Code).ShouldContain("PasswordPolicy.Password.RequireSpecialCharacter");
    }

    [Fact]
    public void ValidatePassword_WithoutSpecialCharacter_WhenNotRequired_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context and disabling special character requirement");
        var executionContext = CreateTestExecutionContext();
        SaveAndChangeMetadata(requireSpecialCharacter: false);

        try
        {
            // Act
            LogAct("Validating password without special character when not required");
            bool result = PasswordPolicy.ValidatePassword(executionContext, "NoSpecialChar1");

            // Assert
            LogAssert("Verifying validation passes");
            result.ShouldBeTrue();
        }
        finally
        {
            RestoreMetadata();
        }
    }

    [Fact]
    public void ValidatePassword_WithTooFewUniqueCharacters_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        SaveAndChangeMetadata(requireUppercase: false, requireDigit: false, requireSpecialCharacter: false);

        try
        {
            // Act
            LogAct("Validating password with too few unique characters");
            bool result = PasswordPolicy.ValidatePassword(executionContext, "aaabbbaaabbb");

            // Assert
            LogAssert("Verifying min unique characters is enforced");
            result.ShouldBeFalse();
            executionContext.Messages.Select(m => m.Code).ShouldContain("PasswordPolicy.Password.MinUniqueCharacters");
        }
        finally
        {
            RestoreMetadata();
        }
    }

    [Fact]
    public void ValidatePassword_WithEnoughUniqueCharacters_ShouldPassUniqueCheck()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password with sufficient unique characters");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "ValidPass123!");

        // Assert
        LogAssert("Verifying unique characters check passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePassword_WithMultipleViolations_ShouldReportAll()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password with multiple violations");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "aaaaaaaaaaaa");

        // Assert
        LogAssert("Verifying multiple violations are reported");
        result.ShouldBeFalse();
        var codes = executionContext.Messages.Select(m => m.Code).ToList();
        codes.ShouldContain("PasswordPolicy.Password.RequireUppercase");
        codes.ShouldContain("PasswordPolicy.Password.RequireDigit");
        codes.ShouldContain("PasswordPolicy.Password.RequireSpecialCharacter");
        codes.ShouldContain("PasswordPolicy.Password.MinUniqueCharacters");
    }

    [Fact]
    public void ValidatePassword_NoComplexity_ShouldFail()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password without complexity");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "aaaaaaaaaaaa");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidatePassword_SpaceAsOnlySpecialChar_ShouldRequireSpecialCharacter()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating password where space is the only non-alphanumeric");
        bool result = PasswordPolicy.ValidatePassword(executionContext, "Valid Pass 12");

        // Assert
        LogAssert("Verifying space does not count as special character");
        result.ShouldBeFalse();
        executionContext.Messages.Select(m => m.Code).ShouldContain("PasswordPolicy.Password.RequireSpecialCharacter");
    }

    [Fact]
    public void ValidatePassword_MinUniqueDisabled_ShouldPassWithRepeatedChars()
    {
        // Arrange
        LogArrange("Creating execution context and disabling unique chars requirement");
        var executionContext = CreateTestExecutionContext();
        SaveAndChangeMetadata(
            requireUppercase: false,
            requireLowercase: false,
            requireDigit: false,
            requireSpecialCharacter: false,
            minUniqueCharacters: 0);

        try
        {
            // Act
            LogAct("Validating password with all same chars");
            bool result = PasswordPolicy.ValidatePassword(executionContext, "aaaaaaaaaaaa");

            // Assert
            LogAssert("Verifying validation passes when unique chars disabled");
            result.ShouldBeTrue();
        }
        finally
        {
            RestoreMetadata();
        }
    }

    private int _savedMinLength;
    private int _savedMaxLength;
    private bool _savedAllowSpaces;
    private bool _savedRequireUppercase;
    private bool _savedRequireLowercase;
    private bool _savedRequireDigit;
    private bool _savedRequireSpecialCharacter;
    private int _savedMinUniqueCharacters;

    private void SaveAndChangeMetadata(
        int? minLength = null,
        int? maxLength = null,
        bool? allowSpaces = null,
        bool? requireUppercase = null,
        bool? requireLowercase = null,
        bool? requireDigit = null,
        bool? requireSpecialCharacter = null,
        int? minUniqueCharacters = null)
    {
        _savedMinLength = PasswordPolicyMetadata.MinLength;
        _savedMaxLength = PasswordPolicyMetadata.MaxLength;
        _savedAllowSpaces = PasswordPolicyMetadata.AllowSpaces;
        _savedRequireUppercase = PasswordPolicyMetadata.RequireUppercase;
        _savedRequireLowercase = PasswordPolicyMetadata.RequireLowercase;
        _savedRequireDigit = PasswordPolicyMetadata.RequireDigit;
        _savedRequireSpecialCharacter = PasswordPolicyMetadata.RequireSpecialCharacter;
        _savedMinUniqueCharacters = PasswordPolicyMetadata.MinUniqueCharacters;

        PasswordPolicyMetadata.ChangeMetadata(
            minLength ?? _savedMinLength,
            maxLength ?? _savedMaxLength,
            allowSpaces ?? _savedAllowSpaces,
            requireUppercase ?? _savedRequireUppercase,
            requireLowercase ?? _savedRequireLowercase,
            requireDigit ?? _savedRequireDigit,
            requireSpecialCharacter ?? _savedRequireSpecialCharacter,
            minUniqueCharacters ?? _savedMinUniqueCharacters);
    }

    private void RestoreMetadata()
    {
        PasswordPolicyMetadata.ChangeMetadata(
            _savedMinLength, _savedMaxLength, _savedAllowSpaces,
            _savedRequireUppercase, _savedRequireLowercase, _savedRequireDigit,
            _savedRequireSpecialCharacter, _savedMinUniqueCharacters);
    }

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
}
