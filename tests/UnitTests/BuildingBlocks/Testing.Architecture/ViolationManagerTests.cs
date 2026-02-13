using System.Text.Json;
using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture;

public class ViolationManagerTests : TestBase
{
    public ViolationManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        ViolationManager.ResetSharedState();
    }

    [Fact]
    public void NewManager_ShouldHaveEmptyCollections()
    {
        // Arrange
        LogArrange("Creating new ViolationManager");

        // Act
        LogAct("Instantiating ViolationManager");
        var manager = new ViolationManager();

        // Assert
        LogAssert("Verifying empty state");
        manager.Violations.ShouldBeEmpty();
        manager.RuleResults.ShouldBeEmpty();
        manager.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void AddRuleResults_WithPassedResults_ShouldNotAddViolations()
    {
        // Arrange
        LogArrange("Creating manager with passed results");
        var manager = new ViolationManager();
        var results = new[]
        {
            CreateRuleResult("Rule1", [
                CreateTypeResult("Type1", TypeAnalysisStatus.Passed),
                CreateTypeResult("Type2", TypeAnalysisStatus.Passed)
            ])
        };

        // Act
        LogAct("Adding passed rule results");
        manager.AddRuleResults(results);

        // Assert
        LogAssert("Verifying no violations added");
        manager.Violations.ShouldBeEmpty();
        manager.RuleResults.Count.ShouldBe(1);
        manager.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void AddRuleResults_WithFailedResults_ShouldExtractViolations()
    {
        // Arrange
        LogArrange("Creating manager with failed results");
        var manager = new ViolationManager();
        var violation = CreateViolation("Rule1", Severity.Error);
        var results = new[]
        {
            CreateRuleResult("Rule1", [
                CreateTypeResult("Type1", TypeAnalysisStatus.Passed),
                CreateTypeResultWithViolation("Type2", violation)
            ])
        };

        // Act
        LogAct("Adding results with violations");
        manager.AddRuleResults(results);

        // Assert
        LogAssert("Verifying violations extracted");
        manager.Violations.Count.ShouldBe(1);
        manager.Violations[0].ShouldBeSameAs(violation);
        manager.RuleResults.Count.ShouldBe(1);
    }

    [Fact]
    public void HasErrors_WithErrorViolations_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating manager with error violations");
        var manager = new ViolationManager();
        var violation = CreateViolation("Rule1", Severity.Error);
        var results = new[]
        {
            CreateRuleResult("Rule1", [
                CreateTypeResultWithViolation("Type1", violation)
            ])
        };

        // Act
        LogAct("Adding error violations");
        manager.AddRuleResults(results);

        // Assert
        LogAssert("Verifying HasErrors is true");
        manager.HasErrors.ShouldBeTrue();
    }

    [Fact]
    public void HasErrors_WithOnlyWarnings_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating manager with warning-only violations");
        var manager = new ViolationManager();
        var violation = CreateViolation("Rule1", Severity.Warning);
        var results = new[]
        {
            CreateRuleResult("Rule1", [
                CreateTypeResultWithViolation("Type1", violation)
            ])
        };

        // Act
        LogAct("Adding warning violations");
        manager.AddRuleResults(results);

        // Assert
        LogAssert("Verifying HasErrors is false");
        manager.HasErrors.ShouldBeFalse();
        manager.Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void HasErrors_WithOnlyInfos_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating manager with info-only violations");
        var manager = new ViolationManager();
        var violation = CreateViolation("Rule1", Severity.Info);
        var results = new[]
        {
            CreateRuleResult("Rule1", [
                CreateTypeResultWithViolation("Type1", violation)
            ])
        };

        // Act
        LogAct("Adding info violations");
        manager.AddRuleResults(results);

        // Assert
        LogAssert("Verifying HasErrors is false");
        manager.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void AddRuleResults_MultipleCalls_ShouldAccumulate()
    {
        // Arrange
        LogArrange("Creating manager for multiple AddRuleResults calls");
        var manager = new ViolationManager();
        var violation1 = CreateViolation("Rule1", Severity.Error);
        var violation2 = CreateViolation("Rule2", Severity.Warning);

        // Act
        LogAct("Adding results in multiple calls");
        manager.AddRuleResults([CreateRuleResult("Rule1", [CreateTypeResultWithViolation("Type1", violation1)])]);
        manager.AddRuleResults([CreateRuleResult("Rule2", [CreateTypeResultWithViolation("Type2", violation2)])]);

        // Assert
        LogAssert("Verifying accumulation");
        manager.Violations.Count.ShouldBe(2);
        manager.RuleResults.Count.ShouldBe(2);
    }

    [Fact]
    public void WritePendingFiles_ShouldCreateDirectoryAndFiles()
    {
        // Arrange
        LogArrange("Creating manager with violations for pending files");
        var manager = new ViolationManager();
        var violation = CreateViolation("TestRule", Severity.Error);
        manager.AddRuleResults([CreateRuleResult("TestRule", [CreateTypeResultWithViolation("Type1", violation)])]);

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");

        try
        {
            // Act
            LogAct("Writing pending files");
            manager.WritePendingFiles(tempDir);

            // Assert
            LogAssert("Verifying pending files created");
            Directory.Exists(tempDir).ShouldBeTrue();
            var files = Directory.GetFiles(tempDir, "architecture_*.txt");
            files.Length.ShouldBe(1);

            var content = File.ReadAllText(files[0]);
            content.ShouldContain("RULE: TestRule");
            content.ShouldContain("SEVERITY: Error");
            content.ShouldContain("ADR: docs/adr/test.md");
            content.ShouldContain("PROJECT: TestProject");
            content.ShouldContain("FILE: Type1.cs");
            content.ShouldContain("LINE: 1");
            content.ShouldContain("MESSAGE: Test violation");
            content.ShouldContain("LLM_HINT: Fix it");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WritePendingFiles_WithMultipleViolations_ShouldCreateMultipleFiles()
    {
        // Arrange
        LogArrange("Creating manager with multiple violations");
        var manager = new ViolationManager();
        var violation1 = CreateViolation("RuleA", Severity.Error);
        var violation2 = CreateViolation("RuleB", Severity.Warning);
        manager.AddRuleResults([
            CreateRuleResult("RuleA", [CreateTypeResultWithViolation("Type1", violation1)]),
            CreateRuleResult("RuleB", [CreateTypeResultWithViolation("Type2", violation2)])
        ]);

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");

        try
        {
            // Act
            LogAct("Writing pending files");
            manager.WritePendingFiles(tempDir);

            // Assert
            LogAssert("Verifying multiple files created");
            var files = Directory.GetFiles(tempDir, "architecture_*.txt");
            files.Length.ShouldBe(2);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WritePendingFiles_FileNaming_ShouldUseLowercaseRuleAndSequentialNumber()
    {
        // Arrange
        LogArrange("Creating manager to verify file naming convention");
        var manager = new ViolationManager();
        var violation = CreateViolation("DE001_SealedClass", Severity.Error);
        manager.AddRuleResults([CreateRuleResult("DE001_SealedClass", [CreateTypeResultWithViolation("Type1", violation)])]);

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");

        try
        {
            // Act
            LogAct("Writing pending files");
            manager.WritePendingFiles(tempDir);

            // Assert
            LogAssert("Verifying file naming convention");
            var files = Directory.GetFiles(tempDir, "*.txt");
            var fileName = Path.GetFileName(files[0]);
            fileName.ShouldBe("architecture_de001_sealedclass_001.txt");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WritePendingFiles_WithNoViolations_ShouldCreateEmptyDirectory()
    {
        // Arrange
        LogArrange("Creating manager without violations");
        var manager = new ViolationManager();

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");

        try
        {
            // Act
            LogAct("Writing pending files with no violations");
            manager.WritePendingFiles(tempDir);

            // Assert
            LogAssert("Verifying directory created with no files");
            Directory.Exists(tempDir).ShouldBeTrue();
            Directory.GetFiles(tempDir).Length.ShouldBe(0);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WriteJsonReport_ShouldCreateValidJsonFile()
    {
        // Arrange
        LogArrange("Creating manager for JSON report");
        var manager = new ViolationManager();
        var violation = CreateViolation("TestRule", Severity.Error);
        manager.AddRuleResults([
            CreateRuleResult("TestRule", [
                CreateTypeResult("PassedType", TypeAnalysisStatus.Passed),
                CreateTypeResultWithViolation("FailedType", violation)
            ])
        ]);

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");
        var outputPath = Path.Combine(tempDir, "report.json");

        try
        {
            // Act
            LogAct("Writing JSON report");
            manager.WriteJsonReport(outputPath);

            // Assert
            LogAssert("Verifying JSON report");
            File.Exists(outputPath).ShouldBeTrue();

            var json = File.ReadAllText(outputPath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            root.GetProperty("totalTypesAnalyzed").GetInt32().ShouldBe(2);
            root.GetProperty("totalPassed").GetInt32().ShouldBe(1);
            root.GetProperty("totalViolations").GetInt32().ShouldBe(1);
            root.GetProperty("errors").GetInt32().ShouldBe(1);
            root.GetProperty("warnings").GetInt32().ShouldBe(0);
            root.GetProperty("infos").GetInt32().ShouldBe(0);
            root.GetProperty("ruleResults").GetArrayLength().ShouldBe(1);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WriteJsonReport_ShouldIncludeTimestamp()
    {
        // Arrange
        LogArrange("Creating manager for timestamp check");
        var manager = new ViolationManager();
        manager.AddRuleResults([CreateRuleResult("TestRule", [CreateTypeResult("Type1", TypeAnalysisStatus.Passed)])]);

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");
        var outputPath = Path.Combine(tempDir, "report.json");

        try
        {
            // Act
            LogAct("Writing JSON report");
            manager.WriteJsonReport(outputPath);

            // Assert
            LogAssert("Verifying timestamp exists");
            var json = File.ReadAllText(outputPath);
            var doc = JsonDocument.Parse(json);
            var timestamp = doc.RootElement.GetProperty("timestamp").GetString();
            timestamp.ShouldNotBeNullOrEmpty();
            timestamp.ShouldEndWith("Z");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WriteJsonReport_WithMixedSeverities_ShouldCountCorrectly()
    {
        // Arrange
        LogArrange("Creating manager with mixed severity violations");
        var manager = new ViolationManager();
        manager.AddRuleResults([
            CreateRuleResult("Rule1", [
                CreateTypeResultWithViolation("Type1", CreateViolation("Rule1", Severity.Error)),
                CreateTypeResultWithViolation("Type2", CreateViolation("Rule1", Severity.Warning)),
                CreateTypeResultWithViolation("Type3", CreateViolation("Rule1", Severity.Info)),
                CreateTypeResultWithViolation("Type4", CreateViolation("Rule1", Severity.Error))
            ])
        ]);

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");
        var outputPath = Path.Combine(tempDir, "report.json");

        try
        {
            // Act
            LogAct("Writing JSON report");
            manager.WriteJsonReport(outputPath);

            // Assert
            LogAssert("Verifying severity counts");
            var json = File.ReadAllText(outputPath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            root.GetProperty("errors").GetInt32().ShouldBe(2);
            root.GetProperty("warnings").GetInt32().ShouldBe(1);
            root.GetProperty("infos").GetInt32().ShouldBe(1);
            root.GetProperty("totalViolations").GetInt32().ShouldBe(4);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WriteJsonReport_WithNoResults_ShouldCreateEmptyReport()
    {
        // Arrange
        LogArrange("Creating empty manager for JSON report");
        var manager = new ViolationManager();

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");
        var outputPath = Path.Combine(tempDir, "report.json");

        try
        {
            // Act
            LogAct("Writing empty JSON report");
            manager.WriteJsonReport(outputPath);

            // Assert
            LogAssert("Verifying empty report");
            var json = File.ReadAllText(outputPath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            root.GetProperty("totalTypesAnalyzed").GetInt32().ShouldBe(0);
            root.GetProperty("totalPassed").GetInt32().ShouldBe(0);
            root.GetProperty("totalViolations").GetInt32().ShouldBe(0);
            root.GetProperty("ruleResults").GetArrayLength().ShouldBe(0);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WriteJsonReport_RuleResults_ShouldBeOrderedByProjectThenRule()
    {
        // Arrange
        LogArrange("Creating manager with multiple projects and rules");
        var manager = new ViolationManager();
        manager.AddRuleResults([
            CreateRuleResultForProject("RuleB", "ProjectB", [CreateTypeResult("Type1", TypeAnalysisStatus.Passed)]),
            CreateRuleResultForProject("RuleA", "ProjectA", [CreateTypeResult("Type2", TypeAnalysisStatus.Passed)]),
            CreateRuleResultForProject("RuleA", "ProjectB", [CreateTypeResult("Type3", TypeAnalysisStatus.Passed)])
        ]);

        var tempDir = Path.Combine(Path.GetTempPath(), $"bedrock_test_{Guid.NewGuid():N}");
        var outputPath = Path.Combine(tempDir, "report.json");

        try
        {
            // Act
            LogAct("Writing JSON report");
            manager.WriteJsonReport(outputPath);

            // Assert
            LogAssert("Verifying ordering by project then rule");
            var json = File.ReadAllText(outputPath);
            var doc = JsonDocument.Parse(json);
            var ruleResults = doc.RootElement.GetProperty("ruleResults");

            ruleResults[0].GetProperty("projectName").GetString().ShouldBe("ProjectA");
            ruleResults[0].GetProperty("ruleName").GetString().ShouldBe("RuleA");
            ruleResults[1].GetProperty("projectName").GetString().ShouldBe("ProjectB");
            ruleResults[1].GetProperty("ruleName").GetString().ShouldBe("RuleA");
            ruleResults[2].GetProperty("projectName").GetString().ShouldBe("ProjectB");
            ruleResults[2].GetProperty("ruleName").GetString().ShouldBe("RuleB");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    #region Helpers

    private static Violation CreateViolation(string ruleName, Severity severity) =>
        new()
        {
            Rule = ruleName,
            Severity = severity,
            Adr = "docs/adr/test.md",
            Project = "TestProject",
            File = "Type1.cs",
            Line = 1,
            Message = "Test violation",
            LlmHint = "Fix it"
        };

    private static TypeAnalysisResult CreateTypeResult(string typeName, TypeAnalysisStatus status) =>
        new()
        {
            TypeName = typeName,
            TypeFullName = $"global::{typeName}",
            File = $"{typeName}.cs",
            Line = 1,
            Status = status
        };

    private static TypeAnalysisResult CreateTypeResultWithViolation(string typeName, Violation violation) =>
        new()
        {
            TypeName = typeName,
            TypeFullName = $"global::{typeName}",
            File = $"{typeName}.cs",
            Line = 1,
            Status = TypeAnalysisStatus.Failed,
            Violation = violation
        };

    private static RuleAnalysisResult CreateRuleResult(string ruleName, IReadOnlyList<TypeAnalysisResult> typeResults) =>
        CreateRuleResultForProject(ruleName, "TestProject", typeResults);

    private static RuleAnalysisResult CreateRuleResultForProject(string ruleName, string projectName, IReadOnlyList<TypeAnalysisResult> typeResults) =>
        new()
        {
            RuleCategory = "Test",
            RuleName = ruleName,
            RuleDescription = $"{ruleName} description",
            DefaultSeverity = Severity.Error,
            AdrPath = "docs/adr/test.md",
            ProjectName = projectName,
            TypeResults = typeResults
        };

    #endregion
}
