using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN003_DomainZeroExternalDependenciesRuleTests : TestBase
{
    public IN003_DomainZeroExternalDependenciesRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN003_DomainZeroExternalDependenciesRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN003_DomainZeroExternalDependencies");
        rule.Description.ShouldContain("Domain");
        rule.Description.ShouldContain("Domain.Entities");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.Category.ShouldBe("Infrastructure");
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-003-domain-projeto-separado.md");
    }

    #endregion

    #region IsDomainProject Tests

    [Theory]
    [InlineData("ShopDemo.Auth.Domain", true)]
    [InlineData("ShopDemo.Orders.Domain", true)]
    [InlineData("MyApp.Domain", true)]
    [InlineData("ShopDemo.Auth.Domain.Entities", false)]
    [InlineData("ShopDemo.Auth.Application", false)]
    [InlineData("ShopDemo.Auth.Infra.Data", false)]
    [InlineData("Bedrock.BuildingBlocks.Core", false)]
    [InlineData("Bedrock.BuildingBlocks.Domain.Entities", false)]
    public void IsDomainProject_ShouldClassifyCorrectly(string projectName, bool expected)
    {
        // Arrange
        LogArrange($"Classifying project '{projectName}'");

        // Act
        LogAct("Calling IsDomainProject");
        var result = IN003_DomainZeroExternalDependenciesRule.IsDomainProject(projectName);

        // Assert
        LogAssert($"Verifying result is {expected}");
        result.ShouldBe(expected);
    }

    #endregion

    #region Allowed ProjectReference Tests

    [Theory]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    [InlineData("Bedrock.BuildingBlocks.Domain")]
    [InlineData("Bedrock.BuildingBlocks.Security")]
    [InlineData("Bedrock.BuildingBlocks.Observability")]
    public void AllowedProjectReference_BedrockBuildingBlocks_ShouldPass(string reference)
    {
        // Arrange
        LogArrange($"Testing allowed BuildingBlocks reference: {reference}");
        var rule = new IN003_DomainZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain",
            projectReferences: [reference]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain");

            // Act
            LogAct("Analyzing allowed reference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying passed result");
            var typeResults = results.SelectMany(r => r.TypeResults).ToList();
            typeResults.Count.ShouldBe(1);
            typeResults[0].Status.ShouldBe(TypeAnalysisStatus.Passed);
            typeResults[0].TypeName.ShouldBe(reference);
            typeResults[0].Violation.ShouldBeNull();
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    [Theory]
    [InlineData("ShopDemo.Auth.Domain.Entities")]
    [InlineData("ShopDemo.Core.Entities")]
    [InlineData("ShopDemo.Shared.Entities")]
    public void AllowedProjectReference_DomainEntities_ShouldPass(string reference)
    {
        // Arrange
        LogArrange($"Testing allowed Domain.Entities/shared kernel reference: {reference}");
        var rule = new IN003_DomainZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain",
            projectReferences: [reference]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain");

            // Act
            LogAct("Analyzing allowed reference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying passed result for Domain.Entities reference");
            var typeResults = results.SelectMany(r => r.TypeResults).ToList();
            typeResults.Count.ShouldBe(1);
            typeResults[0].Status.ShouldBe(TypeAnalysisStatus.Passed);
            typeResults[0].TypeName.ShouldBe(reference);
            typeResults[0].Violation.ShouldBeNull();
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    [Fact]
    public void AllowedProjectReference_Configuration_ShouldPass()
    {
        // Arrange
        var reference = "ShopDemo.Auth.Infra.CrossCutting.Configuration";
        LogArrange($"Testing allowed Configuration reference: {reference}");
        var rule = new IN003_DomainZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain",
            projectReferences: [reference]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain");

            // Act
            LogAct("Analyzing allowed reference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying passed result for Configuration reference");
            var typeResults = results.SelectMany(r => r.TypeResults).ToList();
            typeResults.Count.ShouldBe(1);
            typeResults[0].Status.ShouldBe(TypeAnalysisStatus.Passed);
            typeResults[0].TypeName.ShouldBe(reference);
            typeResults[0].Violation.ShouldBeNull();
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region Forbidden ProjectReference Tests

    [Theory]
    [InlineData("ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Infra.Data")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql")]
    [InlineData("ShopDemo.Auth.Api")]
    [InlineData("SomeExternalLibrary")]
    public void ForbiddenProjectReference_ShouldGenerateViolation(string reference)
    {
        // Arrange
        LogArrange($"Testing forbidden reference: {reference}");
        var rule = new IN003_DomainZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain",
            projectReferences: [reference]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain");

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
            violations[0].Rule.ShouldBe("IN003_DomainZeroExternalDependencies");
            violations[0].Severity.ShouldBe(Severity.Error);
            violations[0].Project.ShouldBe("ShopDemo.Auth.Domain");
            violations[0].Message.ShouldContain("ShopDemo.Auth.Domain");
            violations[0].Message.ShouldContain(reference);
            violations[0].LlmHint.ShouldContain(reference);
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region PackageReference Tests

    [Theory]
    [InlineData("Newtonsoft.Json")]
    [InlineData("MediatR")]
    public void PackageReference_ShouldGenerateViolation(string package)
    {
        // Arrange
        LogArrange($"Testing forbidden PackageReference: {package}");
        var rule = new IN003_DomainZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain",
            packageReferences: [package]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain");

            // Act
            LogAct("Analyzing forbidden PackageReference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying violation for PackageReference");
            var violations = results
                .SelectMany(r => r.TypeResults)
                .Where(t => t.Violation is not null)
                .Select(t => t.Violation!)
                .ToList();
            violations.Count.ShouldBe(1);
            violations[0].Rule.ShouldBe("IN003_DomainZeroExternalDependencies");
            violations[0].Severity.ShouldBe(Severity.Error);
            violations[0].Project.ShouldBe("ShopDemo.Auth.Domain");
            violations[0].Message.ShouldContain("PackageReference");
            violations[0].Message.ShouldContain(package);
            violations[0].LlmHint.ShouldContain(package);
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    [Fact]
    public void MixedViolations_ProjectAndPackageReferences_ShouldReportAll()
    {
        // Arrange
        LogArrange("Testing mixed violations: allowed + forbidden ProjectReference + PackageReference");
        var rule = new IN003_DomainZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain",
            projectReferences: ["Bedrock.BuildingBlocks.Core", "ShopDemo.Auth.Application"],
            packageReferences: ["Newtonsoft.Json"]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain");

            // Act
            LogAct("Analyzing mixed references");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying 1 pass + 2 violations");
            var typeResults = results.SelectMany(r => r.TypeResults).ToList();
            typeResults.Count.ShouldBe(3);

            var passed = typeResults.Where(t => t.Status == TypeAnalysisStatus.Passed).ToList();
            passed.Count.ShouldBe(1);
            passed[0].TypeName.ShouldBe("Bedrock.BuildingBlocks.Core");

            var violations = typeResults.Where(t => t.Status == TypeAnalysisStatus.Failed).ToList();
            violations.Count.ShouldBe(2);
            violations.ShouldContain(v => v.TypeName == "ShopDemo.Auth.Application");
            violations.ShouldContain(v => v.TypeName == "Newtonsoft.Json");
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region Non-Domain Project Tests

    [Theory]
    [InlineData("ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Infra.Data")]
    [InlineData("ShopDemo.Auth.Domain.Entities")]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    public void NonDomainProject_ShouldBeIgnored(string projectName)
    {
        // Arrange
        LogArrange($"Testing non-Domain project: {projectName}");
        var rule = new IN003_DomainZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            projectName,
            projectReferences: ["SomeExternalProject"],
            packageReferences: ["Newtonsoft.Json"]);

        try
        {
            var compilations = CreateMinimalCompilations(projectName);

            // Act
            LogAct("Analyzing non-Domain project");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying no results (project ignored)");
            var typeResults = results.SelectMany(r => r.TypeResults).ToList();
            typeResults.ShouldBeEmpty();
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region IsAllowedProjectReference Tests

    [Theory]
    [InlineData("Bedrock.BuildingBlocks.Core", true)]
    [InlineData("Bedrock.BuildingBlocks.Domain", true)]
    [InlineData("Bedrock.BuildingBlocks.Security", true)]
    [InlineData("ShopDemo.Auth.Domain.Entities", true)]
    [InlineData("ShopDemo.Core.Entities", true)]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Configuration", true)]
    [InlineData("ShopDemo.Auth.Application", false)]
    [InlineData("ShopDemo.Auth.Infra.Data", false)]
    [InlineData("SomeExternalProject", false)]
    public void IsAllowedProjectReference_ShouldClassifyCorrectly(string reference, bool expected)
    {
        // Arrange
        LogArrange($"Classifying reference '{reference}'");

        // Act
        LogAct("Calling IsAllowedProjectReference");
        var result = IN003_DomainZeroExternalDependenciesRule.IsAllowedProjectReference(reference);

        // Assert
        LogAssert($"Verifying result is {expected}");
        result.ShouldBe(expected);
    }

    #endregion

    #region Helpers

    private static string CreateTempProjectStructure(
        string sourceProject,
        string[]? projectReferences = null,
        string[]? packageReferences = null)
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"in003_test_{Guid.NewGuid():N}");
        var srcDir = Path.Combine(tempRoot, "src");
        var projectDir = Path.Combine(srcDir, sourceProject.Replace('.', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(projectDir);

        var itemGroups = "";

        if (projectReferences is { Length: > 0 })
        {
            var refs = string.Join("\n    ",
                projectReferences.Select(r => $"""<ProjectReference Include="..\{r}\{r}.csproj" />"""));
            itemGroups += $"""
              <ItemGroup>
                {refs}
              </ItemGroup>
            """;
        }

        if (packageReferences is { Length: > 0 })
        {
            var pkgs = string.Join("\n    ",
                packageReferences.Select(p => $"""<PackageReference Include="{p}" Version="1.0.0" />"""));
            itemGroups += $"""
              <ItemGroup>
                {pkgs}
              </ItemGroup>
            """;
        }

        var csprojContent = $"""
            <Project Sdk="Microsoft.NET.Sdk">
            {itemGroups}
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
