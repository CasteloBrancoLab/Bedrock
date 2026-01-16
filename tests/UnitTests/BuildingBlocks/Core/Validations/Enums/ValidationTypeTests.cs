using Bedrock.BuildingBlocks.Core.Validations.Enums;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Validations.Enums;

public class ValidationTypeTests : TestBase
{
    public ValidationTypeTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ValidationType_IsRequired_ShouldHaveValue1()
    {
        // Arrange
        LogArrange("Getting IsRequired value");

        // Act
        LogAct("Checking enum value");
        var value = (int)ValidationType.IsRequired;

        // Assert
        LogAssert("Verifying value is 1");
        value.ShouldBe(1);
        LogInfo("IsRequired = {0}", value);
    }

    [Fact]
    public void ValidationType_MinLength_ShouldHaveValue2()
    {
        // Arrange
        LogArrange("Getting MinLength value");

        // Act
        LogAct("Checking enum value");
        var value = (int)ValidationType.MinLength;

        // Assert
        LogAssert("Verifying value is 2");
        value.ShouldBe(2);
        LogInfo("MinLength = {0}", value);
    }

    [Fact]
    public void ValidationType_MaxLength_ShouldHaveValue3()
    {
        // Arrange
        LogArrange("Getting MaxLength value");

        // Act
        LogAct("Checking enum value");
        var value = (int)ValidationType.MaxLength;

        // Assert
        LogAssert("Verifying value is 3");
        value.ShouldBe(3);
        LogInfo("MaxLength = {0}", value);
    }

    [Fact]
    public void ValidationType_ToString_ShouldReturnName()
    {
        // Arrange
        LogArrange("Getting enum names");

        // Act & Assert
        LogAct("Checking ToString values");
        ValidationType.IsRequired.ToString().ShouldBe("IsRequired");
        ValidationType.MinLength.ToString().ShouldBe("MinLength");
        ValidationType.MaxLength.ToString().ShouldBe("MaxLength");

        LogAssert("All ToString values correct");
    }

    [Fact]
    public void ValidationType_AllValues_ShouldBeUnique()
    {
        // Arrange
        LogArrange("Getting all ValidationType values");

        // Act
        LogAct("Checking uniqueness");
        var values = Enum.GetValues<ValidationType>();
        var uniqueValues = values.Select(v => (int)v).Distinct().Count();

        // Assert
        LogAssert("Verifying all values are unique");
        uniqueValues.ShouldBe(values.Length);
        LogInfo("All {0} values are unique", values.Length);
    }
}
