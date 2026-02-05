using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture;

public class TypeAnalysisResultTests : TestBase
{
    public TypeAnalysisResultTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void TypeAnalysisResult_Passed_ShouldStoreAllProperties()
    {
        // Arrange
        LogArrange("Creating a passed TypeAnalysisResult");

        // Act
        LogAct("Instantiating TypeAnalysisResult with Passed status");
        var result = new TypeAnalysisResult
        {
            TypeName = "OrderEntity",
            TypeFullName = "global::Bedrock.Domain.OrderEntity",
            File = "src/Domain/OrderEntity.cs",
            Line = 10,
            Status = TypeAnalysisStatus.Passed,
            Violation = null
        };

        // Assert
        LogAssert("Verifying all properties are correctly stored");
        result.TypeName.ShouldBe("OrderEntity");
        result.TypeFullName.ShouldBe("global::Bedrock.Domain.OrderEntity");
        result.File.ShouldBe("src/Domain/OrderEntity.cs");
        result.Line.ShouldBe(10);
        result.Status.ShouldBe(TypeAnalysisStatus.Passed);
        result.Violation.ShouldBeNull();
    }

    [Fact]
    public void TypeAnalysisResult_Failed_ShouldStoreViolation()
    {
        // Arrange
        LogArrange("Creating a failed TypeAnalysisResult with violation");
        var violation = new Violation
        {
            Rule = "SealedClass",
            Severity = Severity.Error,
            Adr = "docs/adr/001.md",
            Project = "Domain",
            File = "src/Domain/OrderEntity.cs",
            Line = 10,
            Message = "Must be sealed",
            LlmHint = "Add sealed modifier"
        };

        // Act
        LogAct("Instantiating TypeAnalysisResult with Failed status");
        var result = new TypeAnalysisResult
        {
            TypeName = "OrderEntity",
            TypeFullName = "global::Bedrock.Domain.OrderEntity",
            File = "src/Domain/OrderEntity.cs",
            Line = 10,
            Status = TypeAnalysisStatus.Failed,
            Violation = violation
        };

        // Assert
        LogAssert("Verifying violation is stored");
        result.Status.ShouldBe(TypeAnalysisStatus.Failed);
        result.Violation.ShouldNotBeNull();
        result.Violation.Rule.ShouldBe("SealedClass");
        result.Violation.Severity.ShouldBe(Severity.Error);
    }

    [Fact]
    public void TypeAnalysisResult_ViolationIsOptional_DefaultIsNull()
    {
        // Arrange
        LogArrange("Creating TypeAnalysisResult without explicit Violation");

        // Act
        LogAct("Instantiating with Passed status and no Violation");
        var result = new TypeAnalysisResult
        {
            TypeName = "Test",
            TypeFullName = "global::Test",
            File = "Test.cs",
            Line = 1,
            Status = TypeAnalysisStatus.Passed
        };

        // Assert
        LogAssert("Verifying Violation defaults to null");
        result.Violation.ShouldBeNull();
    }
}
