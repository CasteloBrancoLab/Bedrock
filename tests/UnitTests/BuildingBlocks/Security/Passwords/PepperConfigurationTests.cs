using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Security.Passwords;

public class PepperConfigurationTests : TestBase
{
    public PepperConfigurationTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Constructor_WithValidConfig_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating valid pepper config");
        var peppers = new Dictionary<int, byte[]>
        {
            { 1, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } }
        };

        // Act
        LogAct("Creating PepperConfiguration");
        var config = new PepperConfiguration(1, peppers);

        // Assert
        LogAssert("Verifying properties");
        config.ActivePepperVersion.ShouldBe(1);
        config.Peppers.Count.ShouldBe(1);
    }

    [Fact]
    public void Constructor_WithMultiplePeppers_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating config with multiple peppers");
        var peppers = new Dictionary<int, byte[]>
        {
            { 1, new byte[] { 1, 2, 3 } },
            { 2, new byte[] { 4, 5, 6 } },
            { 3, new byte[] { 7, 8, 9 } }
        };

        // Act
        LogAct("Creating PepperConfiguration with version 2 active");
        var config = new PepperConfiguration(2, peppers);

        // Assert
        LogAssert("Verifying active version and pepper count");
        config.ActivePepperVersion.ShouldBe(2);
        config.Peppers.Count.ShouldBe(3);
    }

    [Fact]
    public void Constructor_WithNullPeppers_ShouldThrowWithMessage()
    {
        // Arrange
        LogArrange("Preparing null peppers");

        // Act & Assert
        LogAct("Creating PepperConfiguration with null peppers");
        LogAssert("Verifying ArgumentException is thrown with correct message");
        var exception = Should.Throw<ArgumentException>(() => new PepperConfiguration(1, null!));
        exception.Message.ShouldContain("At least one pepper must be configured.");
    }

    [Fact]
    public void Constructor_WithEmptyPeppers_ShouldThrowWithMessage()
    {
        // Arrange
        LogArrange("Preparing empty peppers dictionary");
        var peppers = new Dictionary<int, byte[]>();

        // Act & Assert
        LogAct("Creating PepperConfiguration with empty peppers");
        LogAssert("Verifying ArgumentException is thrown with correct message");
        var exception = Should.Throw<ArgumentException>(() => new PepperConfiguration(1, peppers));
        exception.Message.ShouldContain("At least one pepper must be configured.");
    }

    [Fact]
    public void Constructor_WithActiveVersionNotInDictionary_ShouldThrowWithMessage()
    {
        // Arrange
        LogArrange("Creating peppers without active version");
        var peppers = new Dictionary<int, byte[]>
        {
            { 1, new byte[] { 1, 2, 3 } }
        };

        // Act & Assert
        LogAct("Creating PepperConfiguration with version 2 not in dictionary");
        LogAssert("Verifying ArgumentException is thrown with correct message");
        var exception = Should.Throw<ArgumentException>(() => new PepperConfiguration(2, peppers));
        exception.Message.ShouldContain("Active pepper version 2 is not in the peppers dictionary.");
    }
}
