using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture;

public class RuleAnalysisResultTests : TestBase
{
    public RuleAnalysisResultTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void RuleAnalysisResult_ShouldStoreAllRequiredProperties()
    {
        // Arrange
        LogArrange("Creating RuleAnalysisResult with all properties");
        var typeResults = new List<TypeAnalysisResult>();

        // Act
        LogAct("Instantiating RuleAnalysisResult");
        var result = new RuleAnalysisResult
        {
            RuleName = "DE001_SealedClass",
            RuleDescription = "Classes must be sealed",
            DefaultSeverity = Severity.Error,
            AdrPath = "docs/adr/001.md",
            ProjectName = "Domain",
            TypeResults = typeResults
        };

        // Assert
        LogAssert("Verifying all properties");
        result.RuleName.ShouldBe("DE001_SealedClass");
        result.RuleDescription.ShouldBe("Classes must be sealed");
        result.DefaultSeverity.ShouldBe(Severity.Error);
        result.AdrPath.ShouldBe("docs/adr/001.md");
        result.ProjectName.ShouldBe("Domain");
        result.TypeResults.ShouldBeSameAs(typeResults);
    }

    [Fact]
    public void PassedCount_WithNoResults_ShouldReturn0()
    {
        // Arrange
        LogArrange("Creating RuleAnalysisResult with empty TypeResults");

        // Act
        LogAct("Getting PassedCount");
        var result = CreateResult([]);

        // Assert
        LogAssert("Verifying PassedCount is 0");
        result.PassedCount.ShouldBe(0);
    }

    [Fact]
    public void FailedCount_WithNoResults_ShouldReturn0()
    {
        // Arrange
        LogArrange("Creating RuleAnalysisResult with empty TypeResults");

        // Act
        LogAct("Getting FailedCount");
        var result = CreateResult([]);

        // Assert
        LogAssert("Verifying FailedCount is 0");
        result.FailedCount.ShouldBe(0);
    }

    [Fact]
    public void PassedCount_WithMixedResults_ShouldCountOnlyPassed()
    {
        // Arrange
        LogArrange("Creating RuleAnalysisResult with mixed results");
        var typeResults = new List<TypeAnalysisResult>
        {
            CreateTypeResult("Type1", TypeAnalysisStatus.Passed),
            CreateTypeResult("Type2", TypeAnalysisStatus.Failed),
            CreateTypeResult("Type3", TypeAnalysisStatus.Passed),
            CreateTypeResult("Type4", TypeAnalysisStatus.Passed)
        };

        // Act
        LogAct("Getting PassedCount");
        var result = CreateResult(typeResults);

        // Assert
        LogAssert("Verifying PassedCount is 3");
        result.PassedCount.ShouldBe(3);
    }

    [Fact]
    public void FailedCount_WithMixedResults_ShouldCountOnlyFailed()
    {
        // Arrange
        LogArrange("Creating RuleAnalysisResult with mixed results");
        var typeResults = new List<TypeAnalysisResult>
        {
            CreateTypeResult("Type1", TypeAnalysisStatus.Passed),
            CreateTypeResult("Type2", TypeAnalysisStatus.Failed),
            CreateTypeResult("Type3", TypeAnalysisStatus.Failed)
        };

        // Act
        LogAct("Getting FailedCount");
        var result = CreateResult(typeResults);

        // Assert
        LogAssert("Verifying FailedCount is 2");
        result.FailedCount.ShouldBe(2);
    }

    [Fact]
    public void PassedCount_WithAllPassed_ShouldEqualTotalCount()
    {
        // Arrange
        LogArrange("Creating RuleAnalysisResult with all passed");
        var typeResults = new List<TypeAnalysisResult>
        {
            CreateTypeResult("Type1", TypeAnalysisStatus.Passed),
            CreateTypeResult("Type2", TypeAnalysisStatus.Passed)
        };

        // Act
        LogAct("Getting PassedCount");
        var result = CreateResult(typeResults);

        // Assert
        LogAssert("Verifying PassedCount equals total");
        result.PassedCount.ShouldBe(2);
        result.FailedCount.ShouldBe(0);
    }

    [Fact]
    public void FailedCount_WithAllFailed_ShouldEqualTotalCount()
    {
        // Arrange
        LogArrange("Creating RuleAnalysisResult with all failed");
        var typeResults = new List<TypeAnalysisResult>
        {
            CreateTypeResult("Type1", TypeAnalysisStatus.Failed),
            CreateTypeResult("Type2", TypeAnalysisStatus.Failed)
        };

        // Act
        LogAct("Getting FailedCount");
        var result = CreateResult(typeResults);

        // Assert
        LogAssert("Verifying FailedCount equals total");
        result.FailedCount.ShouldBe(2);
        result.PassedCount.ShouldBe(0);
    }

    private static RuleAnalysisResult CreateResult(IReadOnlyList<TypeAnalysisResult> typeResults) =>
        new()
        {
            RuleName = "TestRule",
            RuleDescription = "Test description",
            DefaultSeverity = Severity.Error,
            AdrPath = "docs/adr/test.md",
            ProjectName = "TestProject",
            TypeResults = typeResults
        };

    private static TypeAnalysisResult CreateTypeResult(string typeName, TypeAnalysisStatus status) =>
        new()
        {
            TypeName = typeName,
            TypeFullName = $"global::{typeName}",
            File = $"{typeName}.cs",
            Line = 1,
            Status = status,
            Violation = status == TypeAnalysisStatus.Failed
                ? new Violation
                {
                    Rule = "TestRule",
                    Severity = Severity.Error,
                    Adr = "docs/adr/test.md",
                    Project = "TestProject",
                    File = $"{typeName}.cs",
                    Line = 1,
                    Message = "Test violation",
                    LlmHint = "Fix it"
                }
                : null
        };
}
