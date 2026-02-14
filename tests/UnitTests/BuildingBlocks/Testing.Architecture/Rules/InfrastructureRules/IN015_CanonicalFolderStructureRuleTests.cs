using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN015_CanonicalFolderStructureRuleTests : TestBase
{
    public IN015_CanonicalFolderStructureRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN015_CanonicalFolderStructureRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN015_CanonicalFolderStructure");
        rule.Description.ShouldContain("pastas canonicas");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-015-estrutura-pastas-canonica-infra-data-tech.md");
        rule.Category.ShouldBe("Infrastructure");
    }

    #endregion

    #region Non Infra.Data.Tech Projects Should Be Ignored

    [Theory]
    [InlineData("ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Infra.Data")]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    public void NonInfraDataTechProject_ShouldBeIgnored(string projectName)
    {
        // Arrange
        LogArrange($"Testing non-Infra.Data.Tech project '{projectName}'");
        var rule = new IN015_CanonicalFolderStructureRule();
        var source = """
            namespace Some.Namespace.DataModels
            {
                public class UserDataModel { }
            }
            """;
        var compilations = CreateCompilationWithSource(projectName, source);

        // Act
        LogAct("Analyzing project");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying project is ignored");
        results.ShouldBeEmpty();
    }

    #endregion

    #region No DataModels Should Be Skipped

    [Fact]
    public void InfraDataTechProject_WithNoDataModels_ShouldNotRequireFolders()
    {
        // Arrange
        LogArrange("Creating Infra.Data.Tech project without DataModels");
        var rule = new IN015_CanonicalFolderStructureRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces
            {
                public interface IAuthPostgreSqlConnection { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing project without DataModels");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations (no DataModels = no requirement)");
        var typeResults = results
            .SelectMany(r => r.TypeResults)
            .ToList();
        typeResults.ShouldBeEmpty();
    }

    #endregion

    #region Complete Structure Should Pass

    [Fact]
    public void InfraDataTechProject_WithCompleteStructure_ShouldPass()
    {
        // Arrange
        LogArrange("Creating Infra.Data.Tech project with all canonical namespaces");
        var rule = new IN015_CanonicalFolderStructureRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces
            {
                public interface IAuthPostgreSqlConnection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IAuthPostgreSqlUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces
            {
                public interface IUserDataModelRepository { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories
            {
                public static class UserFactory
                {
                    public static object Create() => new();
                }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters
            {
                public static class UserDataModelAdapter
                {
                    public static object Adapt() => new();
                }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces
            {
                public interface IUserPostgreSqlRepository { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing project with complete structure");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();

        var passed = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Status == TypeAnalysisStatus.Passed)
            .ToList();
        passed.Count.ShouldBe(1);
    }

    #endregion

    #region Missing Single Namespace

    [Theory]
    [InlineData(".Connections.Interfaces")]
    [InlineData(".UnitOfWork.Interfaces")]
    [InlineData(".DataModelsRepositories.Interfaces")]
    [InlineData(".Factories")]
    [InlineData(".Adapters")]
    [InlineData(".Repositories.Interfaces")]
    public void InfraDataTechProject_MissingSingleNamespace_ShouldGenerateViolation(string missingSegment)
    {
        // Arrange
        LogArrange($"Creating project missing namespace '{missingSegment}'");
        var rule = new IN015_CanonicalFolderStructureRule();

        // Build all canonical namespaces except the missing one
        var allSegments = new[]
        {
            ".Connections.Interfaces",
            ".UnitOfWork.Interfaces",
            ".DataModelsRepositories.Interfaces",
            ".Factories",
            ".Adapters",
            ".Repositories.Interfaces"
        };

        var sourceBuilder = new System.Text.StringBuilder();
        sourceBuilder.AppendLine("""
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            """);

        foreach (var segment in allSegments)
        {
            if (segment == missingSegment)
                continue;

            var ns = $"ShopDemo.Auth.Infra.Data.PostgreSql{segment}";
            sourceBuilder.AppendLine($"namespace {ns} {{ public class Placeholder {{ }} }}");
        }

        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", sourceBuilder.ToString());

        // Act
        LogAct($"Analyzing project missing '{missingSegment}'");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert($"Verifying violation reports missing '{missingSegment}'");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain(missingSegment);
    }

    #endregion

    #region Missing Multiple Namespaces

    [Fact]
    public void InfraDataTechProject_MissingMultipleNamespaces_ShouldReportAll()
    {
        // Arrange
        LogArrange("Creating project with only DataModels namespace");
        var rule = new IN015_CanonicalFolderStructureRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing project with only DataModels");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying all missing namespaces are reported");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain(".Connections.Interfaces");
        violations[0].Message.ShouldContain(".UnitOfWork.Interfaces");
        violations[0].Message.ShouldContain(".DataModelsRepositories.Interfaces");
        violations[0].Message.ShouldContain(".Factories");
        violations[0].Message.ShouldContain(".Adapters");
        violations[0].Message.ShouldContain(".Repositories.Interfaces");
    }

    #endregion

    #region DataModelBase Should Not Trigger

    [Fact]
    public void DataModelBase_ShouldNotTriggerFolderRequirement()
    {
        // Arrange
        LogArrange("Creating project with only DataModelBase (not a real DataModel)");
        var rule = new IN015_CanonicalFolderStructureRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class DataModelBase { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing project with only DataModelBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations (DataModelBase is not a real DataModel)");
        var typeResults = results
            .SelectMany(r => r.TypeResults)
            .ToList();
        typeResults.ShouldBeEmpty();
    }

    #endregion

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilationWithSource(string projectName, string source)
    {
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

    #endregion
}
