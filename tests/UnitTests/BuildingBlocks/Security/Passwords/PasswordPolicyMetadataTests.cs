using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Security.Passwords;

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
    }

    [Fact]
    public void ChangeMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        int originalMinLength = PasswordPolicyMetadata.MinLength;
        int originalMaxLength = PasswordPolicyMetadata.MaxLength;
        bool originalAllowSpaces = PasswordPolicyMetadata.AllowSpaces;

        try
        {
            // Act
            LogAct("Changing metadata");
            PasswordPolicyMetadata.ChangeMetadata(8, 64, false);

            // Assert
            LogAssert("Verifying changed values");
            PasswordPolicyMetadata.MinLength.ShouldBe(8);
            PasswordPolicyMetadata.MaxLength.ShouldBe(64);
            PasswordPolicyMetadata.AllowSpaces.ShouldBeFalse();
        }
        finally
        {
            PasswordPolicyMetadata.ChangeMetadata(originalMinLength, originalMaxLength, originalAllowSpaces);
        }
    }
}
