using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture;

public class SeverityTests : TestBase
{
    public SeverityTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Severity_Error_ShouldHaveValue0()
    {
        // Arrange
        LogArrange("Getting Error value");

        // Act
        LogAct("Checking enum value");
        var value = (int)Severity.Error;

        // Assert
        LogAssert("Verifying value is 0");
        value.ShouldBe(0);
    }

    [Fact]
    public void Severity_Warning_ShouldHaveValue1()
    {
        // Arrange
        LogArrange("Getting Warning value");

        // Act
        LogAct("Checking enum value");
        var value = (int)Severity.Warning;

        // Assert
        LogAssert("Verifying value is 1");
        value.ShouldBe(1);
    }

    [Fact]
    public void Severity_Info_ShouldHaveValue2()
    {
        // Arrange
        LogArrange("Getting Info value");

        // Act
        LogAct("Checking enum value");
        var value = (int)Severity.Info;

        // Assert
        LogAssert("Verifying value is 2");
        value.ShouldBe(2);
    }

    [Fact]
    public void Severity_ToString_ShouldReturnName()
    {
        // Arrange
        LogArrange("Getting enum names");

        // Act & Assert
        LogAct("Checking ToString values");
        Severity.Error.ToString().ShouldBe("Error");
        Severity.Warning.ToString().ShouldBe("Warning");
        Severity.Info.ToString().ShouldBe("Info");

        LogAssert("All ToString values correct");
    }

    [Fact]
    public void Severity_AllValues_ShouldBeUnique()
    {
        // Arrange
        LogArrange("Getting all Severity values");

        // Act
        LogAct("Checking uniqueness");
        var values = Enum.GetValues<Severity>();
        var uniqueValues = values.Select(v => (int)v).Distinct().Count();

        // Assert
        LogAssert("Verifying all values are unique");
        uniqueValues.ShouldBe(values.Length);
    }

    [Fact]
    public void Severity_ShouldHaveExactly3Values()
    {
        // Arrange
        LogArrange("Getting all Severity values");

        // Act
        LogAct("Counting values");
        var values = Enum.GetValues<Severity>();

        // Assert
        LogAssert("Verifying count is 3");
        values.Length.ShouldBe(3);
    }
}
