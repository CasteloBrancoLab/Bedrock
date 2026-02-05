using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture;

public class TypeAnalysisStatusTests : TestBase
{
    public TypeAnalysisStatusTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void TypeAnalysisStatus_Passed_ShouldHaveValue0()
    {
        // Arrange
        LogArrange("Getting Passed value");

        // Act
        LogAct("Checking enum value");
        var value = (int)TypeAnalysisStatus.Passed;

        // Assert
        LogAssert("Verifying value is 0");
        value.ShouldBe(0);
    }

    [Fact]
    public void TypeAnalysisStatus_Failed_ShouldHaveValue1()
    {
        // Arrange
        LogArrange("Getting Failed value");

        // Act
        LogAct("Checking enum value");
        var value = (int)TypeAnalysisStatus.Failed;

        // Assert
        LogAssert("Verifying value is 1");
        value.ShouldBe(1);
    }

    [Fact]
    public void TypeAnalysisStatus_ShouldHaveExactly2Values()
    {
        // Arrange
        LogArrange("Getting all TypeAnalysisStatus values");

        // Act
        LogAct("Counting values");
        var values = Enum.GetValues<TypeAnalysisStatus>();

        // Assert
        LogAssert("Verifying count is 2");
        values.Length.ShouldBe(2);
    }

    [Fact]
    public void TypeAnalysisStatus_AllValues_ShouldBeUnique()
    {
        // Arrange
        LogArrange("Getting all TypeAnalysisStatus values");

        // Act
        LogAct("Checking uniqueness");
        var values = Enum.GetValues<TypeAnalysisStatus>();
        var uniqueValues = values.Select(v => (int)v).Distinct().Count();

        // Assert
        LogAssert("Verifying all values are unique");
        uniqueValues.ShouldBe(values.Length);
    }

    [Fact]
    public void TypeAnalysisStatus_ToString_ShouldReturnName()
    {
        // Arrange
        LogArrange("Getting enum names");

        // Act & Assert
        LogAct("Checking ToString values");
        TypeAnalysisStatus.Passed.ToString().ShouldBe("Passed");
        TypeAnalysisStatus.Failed.ToString().ShouldBe("Failed");

        LogAssert("All ToString values correct");
    }
}
