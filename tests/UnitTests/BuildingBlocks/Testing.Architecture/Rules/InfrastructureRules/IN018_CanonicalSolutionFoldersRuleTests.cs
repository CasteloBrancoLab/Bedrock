using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN018_CanonicalSolutionFoldersRuleTests : TestBase
{
    public IN018_CanonicalSolutionFoldersRuleTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    #region Rule Properties

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new IN018_CanonicalSolutionFoldersRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN018_CanonicalSolutionFolders");
        rule.Description.ShouldContain("solution folders");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-018-solution-folders-canonicos-bounded-context.md");
        rule.Category.ShouldBe("Infrastructure");
    }

    #endregion

    #region Non-BC Projects Should Be Ignored

    [Theory]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    [InlineData("Bedrock.BuildingBlocks.Testing")]
    [InlineData("Bedrock.BuildingBlocks.Configuration")]
    public void BuildingBlocksProject_ShouldBeIgnored(string projectName)
    {
        // Arrange
        LogArrange($"Testing BuildingBlocks project '{projectName}'");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var (rootDir, _) = CreateSlnWithProject(projectName, "src\\SomeFolder\\" + projectName + ".csproj",
            "1 - Api", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing BuildingBlocks project");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying project is ignored");
        results.ShouldBeEmpty();
    }

    [Fact]
    public void UnknownLayerProject_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Testing project with unknown layer classification");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "SomeRandom.Project";
        var (rootDir, _) = CreateSlnWithProject(projectName, "src\\SomeRandom\\" + projectName + ".csproj",
            "1 - Api", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing unknown layer project");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying project is ignored");
        results.ShouldBeEmpty();
    }

    [Fact]
    public void TestProject_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Testing test project path (not under src/)");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Api";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "tests\\UnitTests\\ShopDemo\\Auth\\Api\\" + projectName + ".csproj",
            "Api", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing test project");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying test project is ignored (path not under src/)");
        results.ShouldBeEmpty();
    }

    [Fact]
    public void SingleSegmentBcPrefix_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Testing project with single-segment BC prefix (not a real BC)");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        // "SomeLib.Api" → ClassifyLayer = Api, ExtractBcPrefix = "SomeLib" (no dot → not a BC)
        var projectName = "SomeLib.Api";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "src\\SomeLib\\Api\\" + projectName + ".csproj",
            "1 - Api", "SomeLib");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing project with single-segment prefix");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying project is ignored (BC prefix must be compound like Company.BcName)");
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ProjectInBenchmarks_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Testing project under benchmarks/ (not under src/)");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Api";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "benchmarks\\ShopDemo\\Auth\\Api\\" + projectName + ".csproj",
            "1 - Api", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing benchmarks project");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying project is ignored (whitelist: only src/)");
        results.ShouldBeEmpty();
    }

    #endregion

    #region Valid Placements

    [Fact]
    public void ApiProject_InCorrectFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating Api project in '1 - Api' folder under Auth");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Api";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "src\\ShopDemo\\Auth\\Api\\" + projectName + ".csproj",
            "1 - Api", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Api project placement");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying no violations");
        var violations = GetViolations(results);
        violations.ShouldBeEmpty();
        results.Count.ShouldBe(1);
        results[0].TypeResults[0].Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ApplicationProject_InCorrectFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating Application project in '2 - Application' folder");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Application";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "src\\ShopDemo\\Auth\\Application\\" + projectName + ".csproj",
            "2 - Application", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Application project placement");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying no violations");
        var violations = GetViolations(results);
        violations.ShouldBeEmpty();
    }

    [Fact]
    public void DomainEntitiesProject_InCorrectFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating Domain.Entities project in '3 - Domain' folder");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Domain.Entities";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "src\\ShopDemo\\Auth\\Domain.Entities\\" + projectName + ".csproj",
            "3 - Domain", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Domain.Entities project placement");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying no violations");
        var violations = GetViolations(results);
        violations.ShouldBeEmpty();
    }

    [Fact]
    public void DomainProject_InCorrectFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating Domain project in '3 - Domain' folder");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Domain";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "src\\ShopDemo\\Auth\\Domain\\" + projectName + ".csproj",
            "3 - Domain", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Domain project placement");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying no violations");
        var violations = GetViolations(results);
        violations.ShouldBeEmpty();
    }

    [Fact]
    public void InfraDataProject_InCorrectNestedFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating Infra.Data project in '4.1 - Data' under '4 - Infra'");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Infra.Data";
        var (rootDir, _) = CreateSlnWithNestedInfraProject(projectName,
            "src\\ShopDemo\\Auth\\Infra.Data\\" + projectName + ".csproj",
            "4.1 - Data", "4 - Infra", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Infra.Data project placement");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying no violations");
        var violations = GetViolations(results);
        violations.ShouldBeEmpty();
    }

    [Fact]
    public void InfraDataTechProject_InCorrectNestedFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating Infra.Data.PostgreSql project in '4.1 - Data' under '4 - Infra'");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Infra.Data.PostgreSql";
        var (rootDir, _) = CreateSlnWithNestedInfraProject(projectName,
            "src\\ShopDemo\\Auth\\Infra.Data.PostgreSql\\" + projectName + ".csproj",
            "4.1 - Data", "4 - Infra", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Infra.Data.PostgreSql project placement");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying no violations");
        var violations = GetViolations(results);
        violations.ShouldBeEmpty();
    }

    [Fact]
    public void ConfigurationProject_InCorrectNestedFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating Configuration project in '4.2 - CrossCutting' under '4 - Infra'");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Infra.CrossCutting.Configuration";
        var (rootDir, _) = CreateSlnWithNestedInfraProject(projectName,
            "src\\ShopDemo\\Auth\\Infra.CrossCutting.Configuration\\" + projectName + ".csproj",
            "4.2 - CrossCutting", "4 - Infra", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Configuration project placement");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying no violations");
        var violations = GetViolations(results);
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Invalid Placements

    [Fact]
    public void ApiProject_InWrongFolder_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating Api project in '3 - Domain' folder instead of '1 - Api'");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Api";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "src\\ShopDemo\\Auth\\Api\\" + projectName + ".csproj",
            "3 - Domain", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Api project in wrong folder");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying violation for wrong folder");
        var violations = GetViolations(results);
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("ShopDemo.Auth.Api");
        violations[0].Message.ShouldContain("3 - Domain");
        violations[0].Rule.ShouldBe("IN018_CanonicalSolutionFolders");
        violations[0].Severity.ShouldBe(Severity.Error);
    }

    [Fact]
    public void Project_WithNoSolutionFolder_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating project without any solution folder (orphan in root)");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Api";
        var (rootDir, _) = CreateSlnWithOrphanProject(projectName,
            "src\\ShopDemo\\Auth\\Api\\" + projectName + ".csproj");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing orphan project");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying violation for missing solution folder");
        var violations = GetViolations(results);
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao esta em nenhum solution folder");
    }

    [Fact]
    public void InfraProject_WithWrongGrandparent_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating Infra.Data project in '4.1 - Data' but under '2 - Application' instead of '4 - Infra'");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Infra.Data";
        var (rootDir, _) = CreateSlnWithNestedInfraProject(projectName,
            "src\\ShopDemo\\Auth\\Infra.Data\\" + projectName + ".csproj",
            "4.1 - Data", "2 - Application", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Infra.Data project with wrong grandparent");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying violation for wrong grandparent");
        var violations = GetViolations(results);
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("2 - Application");
    }

    [Fact]
    public void InfraProject_WithoutGrandparent_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating Infra.Data project in '4.1 - Data' directly under Auth (no '4 - Infra' in between)");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Infra.Data";
        // Place in "4.1 - Data" directly under "Auth" — grandparent is "Auth" instead of "4 - Infra"
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "src\\ShopDemo\\Auth\\Infra.Data\\" + projectName + ".csproj",
            "4.1 - Data", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing Infra.Data project without Infra grandparent");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying violation for wrong grandparent (Auth instead of Infra)");
        var violations = GetViolations(results);
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("Auth");
        violations[0].Message.ShouldContain("Infra");
    }

    [Fact]
    public void Project_WithoutBcAncestor_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating Api project in '1 - Api' but without BC folder 'Auth' as ancestor");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Api";
        var (rootDir, _) = CreateSlnWithProject(projectName,
            "src\\ShopDemo\\Auth\\Api\\" + projectName + ".csproj",
            "1 - Api", "WrongBc");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing project without correct BC ancestor");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying violation for missing BC ancestor");
        var violations = GetViolations(results);
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("Auth");
        violations[0].Message.ShouldContain("ancestral");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void SlnNotFound_ShouldReturnEmptyResults()
    {
        // Arrange
        LogArrange("Using temp directory without .sln file");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootDir);
        var compilations = CreateCompilation("ShopDemo.Auth.Api");

        try
        {
            // Act
            LogAct("Analyzing without .sln file");
            var results = rule.Analyze(compilations, rootDir);

            // Assert
            LogAssert("Verifying empty results");
            results.ShouldBeEmpty();
        }
        finally
        {
            Directory.Delete(rootDir, true);
        }
    }

    [Fact]
    public void ProjectNotFoundInSln_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Project exists in compilation but not in .sln");
        var rule = new IN018_CanonicalSolutionFoldersRule();
        var projectName = "ShopDemo.Auth.Api";
        var (rootDir, _) = CreateSlnWithProject("ShopDemo.Auth.Domain",
            "src\\ShopDemo\\Auth\\Domain\\ShopDemo.Auth.Domain.csproj",
            "3 - Domain", "Auth");
        var compilations = CreateCompilation(projectName);

        // Act
        LogAct("Analyzing project not in .sln");
        var results = rule.Analyze(compilations, rootDir);

        // Assert
        LogAssert("Verifying no results for missing project");
        results.ShouldBeEmpty();
    }

    #endregion

    #region Helpers

    private static List<Violation> GetViolations(IReadOnlyList<RuleAnalysisResult> results)
    {
        return results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
    }

    private static Dictionary<string, Compilation> CreateCompilation(string projectName)
    {
        var source = """
            namespace Some.Namespace
            {
                public class SomeClass { }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "TestFile.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        return new Dictionary<string, Compilation>
        {
            [projectName] = CSharpCompilation.Create(
                projectName,
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
        };
    }

    /// <summary>
    /// Creates a temp directory with a .sln file containing a single project
    /// nested under: bcFolderName > layerFolderName > project.
    /// </summary>
    private static (string RootDir, string SlnPath) CreateSlnWithProject(
        string projectName, string projectPath, string layerFolderName, string bcFolderName)
    {
        var srcGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var shopDemoGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var bcGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var layerGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var projGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();

        var slnContent = $$"""
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{{{srcGuid}}}"
            EndProject
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "ShopDemo", "ShopDemo", "{{{shopDemoGuid}}}"
            EndProject
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "{{bcFolderName}}", "{{bcFolderName}}", "{{{bcGuid}}}"
            EndProject
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "{{layerFolderName}}", "{{layerFolderName}}", "{{{layerGuid}}}"
            EndProject
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{{projectName}}", "{{projectPath}}", "{{{projGuid}}}"
            EndProject
            Global
                GlobalSection(NestedProjects) = preSolution
                    {{{shopDemoGuid}}} = {{{srcGuid}}}
                    {{{bcGuid}}} = {{{shopDemoGuid}}}
                    {{{layerGuid}}} = {{{bcGuid}}}
                    {{{projGuid}}} = {{{layerGuid}}}
                EndGlobalSection
            EndGlobal
            """;

        return WriteSlnToTempDir(slnContent);
    }

    /// <summary>
    /// Creates a temp directory with a .sln file containing a project nested under:
    /// bcFolderName > grandparentFolderName > parentFolderName > project.
    /// Used for Infra layers that require grandparent validation.
    /// </summary>
    private static (string RootDir, string SlnPath) CreateSlnWithNestedInfraProject(
        string projectName, string projectPath,
        string parentFolderName, string grandparentFolderName, string bcFolderName)
    {
        var srcGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var shopDemoGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var bcGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var grandparentGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var parentGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var projGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();

        var slnContent = $$"""
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{{{srcGuid}}}"
            EndProject
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "ShopDemo", "ShopDemo", "{{{shopDemoGuid}}}"
            EndProject
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "{{bcFolderName}}", "{{bcFolderName}}", "{{{bcGuid}}}"
            EndProject
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "{{grandparentFolderName}}", "{{grandparentFolderName}}", "{{{grandparentGuid}}}"
            EndProject
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "{{parentFolderName}}", "{{parentFolderName}}", "{{{parentGuid}}}"
            EndProject
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{{projectName}}", "{{projectPath}}", "{{{projGuid}}}"
            EndProject
            Global
                GlobalSection(NestedProjects) = preSolution
                    {{{shopDemoGuid}}} = {{{srcGuid}}}
                    {{{bcGuid}}} = {{{shopDemoGuid}}}
                    {{{grandparentGuid}}} = {{{bcGuid}}}
                    {{{parentGuid}}} = {{{grandparentGuid}}}
                    {{{projGuid}}} = {{{parentGuid}}}
                EndGlobalSection
            EndGlobal
            """;

        return WriteSlnToTempDir(slnContent);
    }

    /// <summary>
    /// Creates a .sln with a project that has no NestedProjects entry (orphan at root).
    /// </summary>
    private static (string RootDir, string SlnPath) CreateSlnWithOrphanProject(
        string projectName, string projectPath)
    {
        var projGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();

        var slnContent = $$"""
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{{projectName}}", "{{projectPath}}", "{{{projGuid}}}"
            EndProject
            Global
                GlobalSection(NestedProjects) = preSolution
                EndGlobalSection
            EndGlobal
            """;

        return WriteSlnToTempDir(slnContent);
    }

    private static (string RootDir, string SlnPath) WriteSlnToTempDir(string slnContent)
    {
        var rootDir = Path.Combine(Path.GetTempPath(), "IN018_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(rootDir);
        var slnPath = Path.Combine(rootDir, "Test.sln");
        File.WriteAllText(slnPath, slnContent);
        return (rootDir, slnPath);
    }

    #endregion
}
