using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN002_DomainEntitiesZeroExternalDependenciesRuleTests : TestBase
{
    public IN002_DomainEntitiesZeroExternalDependenciesRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN002_DomainEntitiesZeroExternalDependenciesRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN002_DomainEntitiesZeroExternalDependencies");
        rule.Description.ShouldContain("zero dependencias externas");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.Category.ShouldBe("Infrastructure");
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-002-domain-entities-projeto-separado.md");
    }

    #endregion

    #region Allowed ProjectReference Tests

    [Theory]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    [InlineData("Bedrock.BuildingBlocks.Domain")]
    [InlineData("Bedrock.BuildingBlocks.Domain.Entities")]
    [InlineData("Bedrock.BuildingBlocks.Testing")]
    public void AllowedProjectReference_BedrockBuildingBlocks_ShouldPass(string reference)
    {
        // Arrange
        LogArrange($"Testing allowed BuildingBlocks reference: {reference}");
        var rule = new IN002_DomainEntitiesZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain.Entities",
            projectReferences: [reference]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain.Entities");

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
    [InlineData("ShopDemo.Core.Entities")]
    [InlineData("ShopDemo.Shared.Entities")]
    public void AllowedProjectReference_OtherEntitiesProject_ShouldPass(string reference)
    {
        // Arrange
        LogArrange($"Testing allowed shared kernel reference: {reference}");
        var rule = new IN002_DomainEntitiesZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain.Entities",
            projectReferences: [reference]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain.Entities");

            // Act
            LogAct("Analyzing allowed reference");
            var results = rule.Analyze(compilations, tempDir);

            // Assert
            LogAssert("Verifying passed result for shared kernel reference");
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
    [InlineData("ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Infra.Data")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql")]
    [InlineData("SomeExternalLibrary")]
    public void ForbiddenProjectReference_ShouldGenerateViolation(string reference)
    {
        // Arrange
        LogArrange($"Testing forbidden reference: {reference}");
        var rule = new IN002_DomainEntitiesZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain.Entities",
            projectReferences: [reference]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain.Entities");

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
            violations[0].Rule.ShouldBe("IN002_DomainEntitiesZeroExternalDependencies");
            violations[0].Severity.ShouldBe(Severity.Error);
            violations[0].Project.ShouldBe("ShopDemo.Auth.Domain.Entities");
            violations[0].Message.ShouldContain("ShopDemo.Auth.Domain.Entities");
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
    [InlineData("FluentValidation")]
    public void PackageReference_ShouldGenerateViolation(string package)
    {
        // Arrange
        LogArrange($"Testing forbidden PackageReference: {package}");
        var rule = new IN002_DomainEntitiesZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain.Entities",
            packageReferences: [package]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain.Entities");

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
            violations[0].Rule.ShouldBe("IN002_DomainEntitiesZeroExternalDependencies");
            violations[0].Severity.ShouldBe(Severity.Error);
            violations[0].Project.ShouldBe("ShopDemo.Auth.Domain.Entities");
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
        LogArrange("Testing mixed violations: forbidden ProjectReference + PackageReference");
        var rule = new IN002_DomainEntitiesZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            "ShopDemo.Auth.Domain.Entities",
            projectReferences: ["Bedrock.BuildingBlocks.Core", "ShopDemo.Auth.Domain"],
            packageReferences: ["Newtonsoft.Json"]);

        try
        {
            var compilations = CreateMinimalCompilations("ShopDemo.Auth.Domain.Entities");

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
            violations.ShouldContain(v => v.TypeName == "ShopDemo.Auth.Domain");
            violations.ShouldContain(v => v.TypeName == "Newtonsoft.Json");
        }
        finally
        {
            CleanupTempDir(tempDir);
        }
    }

    #endregion

    #region Non-Domain.Entities Project Tests

    [Theory]
    [InlineData("ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Infra.Data")]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    public void NonDomainEntitiesProject_ShouldBeIgnored(string projectName)
    {
        // Arrange
        LogArrange($"Testing non-Domain.Entities project: {projectName}");
        var rule = new IN002_DomainEntitiesZeroExternalDependenciesRule();
        var tempDir = CreateTempProjectStructure(
            projectName,
            projectReferences: ["SomeExternalProject"],
            packageReferences: ["Newtonsoft.Json"]);

        try
        {
            var compilations = CreateMinimalCompilations(projectName);

            // Act
            LogAct("Analyzing non-Domain.Entities project");
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
    [InlineData("Bedrock.BuildingBlocks.Domain.Entities", true)]
    [InlineData("ShopDemo.Core.Entities", true)]
    [InlineData("ShopDemo.Auth.Domain", false)]
    [InlineData("ShopDemo.Auth.Application", false)]
    [InlineData("SomeExternalProject", false)]
    public void IsAllowedProjectReference_ShouldClassifyCorrectly(string reference, bool expected)
    {
        // Arrange
        LogArrange($"Classifying reference '{reference}'");

        // Act
        LogAct("Calling IsAllowedProjectReference");
        var result = IN002_DomainEntitiesZeroExternalDependenciesRule.IsAllowedProjectReference(reference);

        // Assert
        LogAssert($"Verifying result is {expected}");
        result.ShouldBe(expected);
    }

    #endregion

    #region ParsePackageReferences Tests

    [Fact]
    public void ParsePackageReferences_WithValidCsproj_ShouldReturnPackages()
    {
        // Arrange
        LogArrange("Creating temp csproj with PackageReferences");
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
                <PackageReference Include="MediatR" Version="12.0.0" />
              </ItemGroup>
            </Project>
            """);

        try
        {
            // Act
            LogAct("Parsing package references");
            var packages = ProjectRule.ParsePackageReferences(tempFile);

            // Assert
            LogAssert("Verifying parsed packages");
            packages.Count.ShouldBe(2);
            packages.ShouldContain("Newtonsoft.Json");
            packages.ShouldContain("MediatR");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParsePackageReferences_WithNoPackages_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating temp csproj without PackageReferences");
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
            LogAct("Parsing package references");
            var packages = ProjectRule.ParsePackageReferences(tempFile);

            // Assert
            LogAssert("Verifying empty packages");
            packages.ShouldBeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParsePackageReferences_WithInvalidFile_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Parsing non-existent csproj");

        // Act
        LogAct("Parsing package references from invalid path");
        var packages = ProjectRule.ParsePackageReferences("/nonexistent/path/file.csproj");

        // Assert
        LogAssert("Verifying empty packages for invalid file");
        packages.ShouldBeEmpty();
    }

    #endregion

    #region Helpers

    private static string CreateTempProjectStructure(
        string sourceProject,
        string[]? projectReferences = null,
        string[]? packageReferences = null)
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"in002_test_{Guid.NewGuid():N}");
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
