using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Security.Passwords;

[Collection("PasswordPolicy")]
public class PasswordPolicyMetadataTests : TestBase
{
    public PasswordPolicyMetadataTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Reading default metadata values");

        // Act & Assert
        LogAssert("Verifying default values");
        PasswordPolicyMetadata.MinLength.ShouldBe(12);
        PasswordPolicyMetadata.MaxLength.ShouldBe(128);
        PasswordPolicyMetadata.AllowSpaces.ShouldBeTrue();
        PasswordPolicyMetadata.RequireUppercase.ShouldBeTrue();
        PasswordPolicyMetadata.RequireLowercase.ShouldBeTrue();
        PasswordPolicyMetadata.RequireDigit.ShouldBeTrue();
        PasswordPolicyMetadata.RequireSpecialCharacter.ShouldBeTrue();
        PasswordPolicyMetadata.MinUniqueCharacters.ShouldBe(4);
    }

    [Fact]
    public void ChangeMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        int originalMinLength = PasswordPolicyMetadata.MinLength;
        int originalMaxLength = PasswordPolicyMetadata.MaxLength;
        bool originalAllowSpaces = PasswordPolicyMetadata.AllowSpaces;
        bool originalRequireUppercase = PasswordPolicyMetadata.RequireUppercase;
        bool originalRequireLowercase = PasswordPolicyMetadata.RequireLowercase;
        bool originalRequireDigit = PasswordPolicyMetadata.RequireDigit;
        bool originalRequireSpecialCharacter = PasswordPolicyMetadata.RequireSpecialCharacter;
        int originalMinUniqueCharacters = PasswordPolicyMetadata.MinUniqueCharacters;

        try
        {
            // Act
            LogAct("Changing metadata");
            PasswordPolicyMetadata.ChangeMetadata(8, 64, false, false, false, false, false, 2);

            // Assert
            LogAssert("Verifying changed values");
            PasswordPolicyMetadata.MinLength.ShouldBe(8);
            PasswordPolicyMetadata.MaxLength.ShouldBe(64);
            PasswordPolicyMetadata.AllowSpaces.ShouldBeFalse();
            PasswordPolicyMetadata.RequireUppercase.ShouldBeFalse();
            PasswordPolicyMetadata.RequireLowercase.ShouldBeFalse();
            PasswordPolicyMetadata.RequireDigit.ShouldBeFalse();
            PasswordPolicyMetadata.RequireSpecialCharacter.ShouldBeFalse();
            PasswordPolicyMetadata.MinUniqueCharacters.ShouldBe(2);
        }
        finally
        {
            PasswordPolicyMetadata.ChangeMetadata(
                originalMinLength, originalMaxLength, originalAllowSpaces,
                originalRequireUppercase, originalRequireLowercase, originalRequireDigit,
                originalRequireSpecialCharacter, originalMinUniqueCharacters);
        }
    }
}
