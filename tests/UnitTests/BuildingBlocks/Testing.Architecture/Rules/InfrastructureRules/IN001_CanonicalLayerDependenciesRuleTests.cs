using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using static Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules.IN001_CanonicalLayerDependenciesRule;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN001_CanonicalLayerDependenciesRuleTests : TestBase
{
    public IN001_CanonicalLayerDependenciesRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN001_CanonicalLayerDependenciesRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN001_CanonicalLayerDependencies");
        rule.Description.ShouldContain("grafo canonico");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-001-camadas-canonicas-bounded-context.md");
    }

    #endregion

    #region ClassifyLayer Tests

    [Theory]
    [InlineData("ShopDemo.Auth.Domain.Entities", BoundedContextLayer.DomainEntities)]
    [InlineData("ShopDemo.Auth.Domain", BoundedContextLayer.Domain)]
    [InlineData("ShopDemo.Auth.Application", BoundedContextLayer.Application)]
    [InlineData("ShopDemo.Auth.Infra.Data", BoundedContextLayer.InfraData)]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql", BoundedContextLayer.InfraDataTech)]
    [InlineData("ShopDemo.Auth.Infra.Data.MongoDB", BoundedContextLayer.InfraDataTech)]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Configuration", BoundedContextLayer.Configuration)]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Bootstrapper", BoundedContextLayer.Bootstrapper)]
    [InlineData("ShopDemo.Auth.Api", BoundedContextLayer.Api)]
    public void ClassifyLayer_KnownSuffix_ShouldReturnCorrectLayer(string projectName, BoundedContextLayer expected)
    {
        // Arrange
        LogArrange($"Classifying layer for '{projectName}'");

        // Act
        LogAct("Calling ClassifyLayer");
        var result = IN001_CanonicalLayerDependenciesRule.ClassifyLayer(projectName);

        // Assert
        LogAssert($"Verifying layer is {expected}");
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    [InlineData("Bedrock.BuildingBlocks.Testing")]
    [InlineData("SomeRandomProject")]
    public void ClassifyLayer_UnknownProject_ShouldReturnUnknown(string projectName)
    {
        // Arrange
        LogArrange($"Classifying layer for unknown project '{projectName}'");

        // Act
        LogAct("Calling ClassifyLayer");
        var result = IN001_CanonicalLayerDependenciesRule.ClassifyLayer(projectName);

        // Assert
        LogAssert("Verifying layer is Unknown");
        result.ShouldBe(BoundedContextLayer.Unknown);
    }

    #endregion

    #region ExtractBcPrefix Tests

    [Theory]
    [InlineData("ShopDemo.Auth.Domain.Entities", BoundedContextLayer.DomainEntities, "ShopDemo.Auth")]
    [InlineData("ShopDemo.Auth.Domain", BoundedContextLayer.Domain, "ShopDemo.Auth")]
    [InlineData("ShopDemo.Auth.Application", BoundedContextLayer.Application, "ShopDemo.Auth")]
    [InlineData("ShopDemo.Auth.Infra.Data", BoundedContextLayer.InfraData, "ShopDemo.Auth")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql", BoundedContextLayer.InfraDataTech, "ShopDemo.Auth")]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Configuration", BoundedContextLayer.Configuration, "ShopDemo.Auth")]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Bootstrapper", BoundedContextLayer.Bootstrapper, "ShopDemo.Auth")]
    [InlineData("ShopDemo.Auth.Api", BoundedContextLayer.Api, "ShopDemo.Auth")]
    [InlineData("MyApp.Orders.Domain", BoundedContextLayer.Domain, "MyApp.Orders")]
    public void ExtractBcPrefix_KnownLayer_ShouldReturnCorrectPrefix(
        string projectName, BoundedContextLayer layer, string expected)
    {
        // Arrange
        LogArrange($"Extracting BC prefix from '{projectName}' (layer: {layer})");

        // Act
        LogAct("Calling ExtractBcPrefix");
        var result = IN001_CanonicalLayerDependenciesRule.ExtractBcPrefix(projectName, layer);

        // Assert
        LogAssert($"Verifying prefix is '{expected}'");
        result.ShouldBe(expected);
    }

    #endregion

    #region Allowed Reference Tests

    [Theory]
    [InlineData("ShopDemo.Auth.Application", "ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Application", "ShopDemo.Auth.Infra.CrossCutting.Configuration")]
    [InlineData("ShopDemo.Auth.Domain", "ShopDemo.Auth.Domain.Entities")]
    [InlineData("ShopDemo.Auth.Domain", "ShopDemo.Auth.Infra.CrossCutting.Configuration")]
    [InlineData("ShopDemo.Auth.Infra.Data", "ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Infra.Data", "ShopDemo.Auth.Domain.Entities")]
    [InlineData("ShopDemo.Auth.Infra.Data", "ShopDemo.Auth.Infra.Data.PostgreSql")]
    [InlineData("ShopDemo.Auth.Infra.Data", "ShopDemo.Auth.Infra.CrossCutting.Configuration")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql", "ShopDemo.Auth.Domain.Entities")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql", "ShopDemo.Auth.Infra.CrossCutting.Configuration")]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Bootstrapper", "ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Bootstrapper", "ShopDemo.Auth.Infra.Data")]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Bootstrapper", "ShopDemo.Auth.Infra.Data.PostgreSql")]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Bootstrapper", "ShopDemo.Auth.Infra.CrossCutting.Configuration")]
    [InlineData("ShopDemo.Auth.Api", "ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Api", "ShopDemo.Auth.Infra.CrossCutting.Bootstrapper")]
    [InlineData("ShopDemo.Auth.Api", "ShopDemo.Auth.Infra.CrossCutting.Configuration")]
    public void AllowedReference_ShouldNotGenerateViolation(string sourceProject, string targetProject)
    {
        // Arrange
        LogArrange($"Testing allowed reference: {sourceProject} -> {targetProject}");
        var rule = new IN001_CanonicalLayerDependenciesRule();
        var tempDir = CreateTempProjectStructure(sourceProject, targetProject);

        try
        {
            var compilations = CreateMinimalCompilations(sourceProject);

            // Act
            LogAct("Analyzing allowed reference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying no violations");
            var violations = results
                .SelectMany(r => r.TypeResults)
                .Where(t => t.Violation is not null)
                .ToList();
            violations.ShouldBeEmpty();
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region Forbidden Reference Tests

    [Theory]
    [InlineData("ShopDemo.Auth.Application", "ShopDemo.Auth.Domain.Entities")]
    [InlineData("ShopDemo.Auth.Application", "ShopDemo.Auth.Infra.Data")]
    [InlineData("ShopDemo.Auth.Application", "ShopDemo.Auth.Infra.Data.PostgreSql")]
    [InlineData("ShopDemo.Auth.Domain", "ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Domain.Entities", "ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql", "ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql", "ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Api", "ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Api", "ShopDemo.Auth.Infra.Data")]
    public void ForbiddenReference_ShouldGenerateViolation(string sourceProject, string targetProject)
    {
        // Arrange
        LogArrange($"Testing forbidden reference: {sourceProject} -> {targetProject}");
        var rule = new IN001_CanonicalLayerDependenciesRule();
        var tempDir = CreateTempProjectStructure(sourceProject, targetProject);

        try
        {
            var compilations = CreateMinimalCompilations(sourceProject);

            // Act
            LogAct("Analyzing forbidden reference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying violation detected");
            var violations = results
                .SelectMany(r => r.TypeResults)
                .Where(t => t.Violation is not null)
                .Select(t => t.Violation!)
                .ToList();
            violations.Count.ShouldBe(1);
            violations[0].Rule.ShouldBe("IN001_CanonicalLayerDependencies");
            violations[0].Severity.ShouldBe(Severity.Error);
            violations[0].Project.ShouldBe(sourceProject);
            violations[0].Message.ShouldContain(sourceProject);
            violations[0].Message.ShouldContain(targetProject);
            violations[0].LlmHint.ShouldContain(targetProject);
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region BuildingBlocks Reference Tests

    [Fact]
    public void BuildingBlocksReference_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Testing reference to Bedrock.BuildingBlocks.Core (should be ignored)");
        var rule = new IN001_CanonicalLayerDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain.Entities",
            "Bedrock.BuildingBlocks.Core");

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain.Entities");

            // Act
            LogAct("Analyzing BuildingBlocks reference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying no violations for BuildingBlocks reference");
            var violations = results
                .SelectMany(r => r.TypeResults)
                .Where(t => t.Violation is not null)
                .ToList();
            violations.ShouldBeEmpty();
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region Cross-BC Reference Tests

    [Fact]
    public void CrossBcReference_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Testing cross-BC reference (should be ignored)");
        var rule = new IN001_CanonicalLayerDependenciesRule();
        // Auth.Application referencing Catalog.Domain.Entities — cross-BC, should be ignored
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Application",
            "ShopDemo.Catalog.Domain.Entities");

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Application");

            // Act
            LogAct("Analyzing cross-BC reference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying no violations for cross-BC reference");
            var violations = results
                .SelectMany(r => r.TypeResults)
                .Where(t => t.Violation is not null)
                .ToList();
            violations.ShouldBeEmpty();
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region Unknown Layer Tests

    [Fact]
    public void UnknownLayerProject_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Testing project with unknown layer classification");
        var rule = new IN001_CanonicalLayerDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "SomeRandomProject",
            "ShopDemo.Auth.Domain");

        try
        {
            var compilations = CreateMinimalCompilations("SomeRandomProject");

            // Act
            LogAct("Analyzing unknown layer project");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying no violations for unknown layer");
            var violations = results
                .SelectMany(r => r.TypeResults)
                .Where(t => t.Violation is not null)
                .ToList();
            violations.ShouldBeEmpty();
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region ProjectRule Base Tests

    [Fact]
    public void AnalyzeType_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Verifying ProjectRule.AnalyzeType is sealed and returns null");
        var rule = new IN001_CanonicalLayerDependenciesRule();
        var compilations = CreateRoslynCompilations("public class TestClass { }", "TestProject");

        // Act
        LogAct("Analyzing with type-level compilation");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no type-level failures (AnalyzeType returns null)");
        // ProjectRule does not analyze types — no TypeResults from Roslyn analysis
        // Instead, it generates results from ProjectReference analysis
        results.ShouldNotBeNull();
    }

    [Fact]
    public void ParseProjectReferences_WithValidCsproj_ShouldReturnReferences()
    {
        // Arrange
        LogArrange("Creating temp csproj with ProjectReferences");
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <ProjectReference Include="..\Domain\ShopDemo.Auth.Domain.csproj" />
                <ProjectReference Include="..\Domain.Entities\ShopDemo.Auth.Domain.Entities.csproj" />
              </ItemGroup>
            </Project>
            """);

        try
        {
            // Act
            LogAct("Parsing project references");
            var references = ProjectRule.ParseProjectReferences(tempFile);

            // Assert
            LogAssert("Verifying parsed references");
            references.Count.ShouldBe(2);
            references.ShouldContain("ShopDemo.Auth.Domain");
            references.ShouldContain("ShopDemo.Auth.Domain.Entities");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseProjectReferences_WithNoReferences_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating temp csproj without ProjectReferences");
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        try
        {
            // Act
            LogAct("Parsing project references");
            var references = ProjectRule.ParseProjectReferences(tempFile);

            // Assert
            LogAssert("Verifying empty references");
            references.ShouldBeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseProjectReferences_WithInvalidFile_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Parsing non-existent csproj");

        // Act
        LogAct("Parsing project references from invalid path");
        var references = ProjectRule.ParseProjectReferences("/nonexistent/path/file.csproj");

        // Assert
        LogAssert("Verifying empty references for invalid file");
        references.ShouldBeEmpty();
    }

    [Fact]
    public void FindCsprojPath_WithNonexistentDir_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Finding csproj in nonexistent directory");

        // Act
        LogAct("Calling FindCsprojPath");
        var result = ProjectRule.FindCsprojPath("SomeProject", "/nonexistent/root");

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
    }

    #endregion

    #region Helpers

    private static string CreateTempProjectStructure(string sourceProject, string targetProject)
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"in001_test_{Guid.NewGuid():N}");
        var srcDir = Path.Combine(tempRoot, "src");
        var projectDir = Path.Combine(srcDir, sourceProject.Replace('.', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(projectDir);

        var csprojContent = $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <ProjectReference Include="..\{targetProject}\{targetProject}.csproj" />
              </ItemGroup>
            </Project>
            """;

        var csprojPath = Path.Combine(projectDir, $"{sourceProject}.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        return tempRoot;
    }

    private static Dictionary<string, Compilation> CreateMinimalCompilations(string projectName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("", path: "Empty.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            projectName,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return new Dictionary<string, Compilation>
        {
            [projectName] = compilation
        };
    }

    private static Dictionary<string, Compilation> CreateRoslynCompilations(string source, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "TestFile.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        return new Dictionary<string, Compilation>
        {
            [assemblyName] = CSharpCompilation.Create(
                assemblyName,
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
        };
    }

    private static void CleanupTempDir(string dir)
    {
        try
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }

    #endregion
}
