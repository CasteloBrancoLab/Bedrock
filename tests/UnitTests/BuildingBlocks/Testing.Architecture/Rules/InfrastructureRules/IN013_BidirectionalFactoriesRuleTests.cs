using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN013_BidirectionalFactoriesRuleTests : TestBase
{
    public IN013_BidirectionalFactoriesRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN013_BidirectionalFactoriesRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN013_BidirectionalFactories");
        rule.Description.ShouldContain("Factory");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-013-factories-bidirecionais-datamodel-entidade.md");
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
        var rule = new IN013_BidirectionalFactoriesRule();
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

    #region Valid Bidirectional Factories

    [Fact]
    public void DataModel_WithBothFactories_ShouldPass()
    {
        // Arrange
        LogArrange("Creating DataModel with both static factories having Create method");
        var rule = new IN013_BidirectionalFactoriesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel
                {
                    public string Email { get; set; }
                }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories
            {
                public static class UserFactory
                {
                    public static string Create(object dataModel) => "";
                }
                public static class UserDataModelFactory
                {
                    public static object Create(string entity) => null;
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with valid factories");
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
            .Where(t => t.TypeName == "UserDataModel" && t.Status == TypeAnalysisStatus.Passed)
            .ToList();
        passed.Count.ShouldBe(1);
    }

    #endregion

    #region Missing Entity Factory

    [Fact]
    public void DataModel_MissingEntityFactory_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating DataModel with only DataModelFactory (missing EntityFactory)");
        var rule = new IN013_BidirectionalFactoriesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories
            {
                public static class UserDataModelFactory
                {
                    public static object Create(string entity) => null;
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel missing EntityFactory");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing EntityFactory");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("UserFactory");
        violations[0].Message.ShouldContain("nao encontrada");
    }

    #endregion

    #region Missing DataModel Factory

    [Fact]
    public void DataModel_MissingDataModelFactory_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating DataModel with only EntityFactory (missing DataModelFactory)");
        var rule = new IN013_BidirectionalFactoriesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories
            {
                public static class UserFactory
                {
                    public static string Create(object dataModel) => "";
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel missing DataModelFactory");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing DataModelFactory");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("UserDataModelFactory");
        violations[0].Message.ShouldContain("nao encontrada");
    }

    #endregion

    #region Both Factories Missing

    [Fact]
    public void DataModel_MissingBothFactories_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating DataModel with no factories at all");
        var rule = new IN013_BidirectionalFactoriesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with no factories");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation mentions both missing factories");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("UserFactory");
        violations[0].Message.ShouldContain("UserDataModelFactory");
    }

    #endregion

    #region Factory Not Static

    [Fact]
    public void Factory_NotStatic_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-static factory class");
        var rule = new IN013_BidirectionalFactoriesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories
            {
                public class UserFactory
                {
                    public static string Create(object dataModel) => "";
                }
                public static class UserDataModelFactory
                {
                    public static object Create(string entity) => null;
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-static factory");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-static factory");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("UserFactory");
        violations[0].Message.ShouldContain("nao e static");
    }

    #endregion

    #region Factory Missing Create Method

    [Fact]
    public void Factory_MissingCreateMethod_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating factory without Create method");
        var rule = new IN013_BidirectionalFactoriesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories
            {
                public static class UserFactory
                {
                    public static string Build(object dataModel) => "";
                }
                public static class UserDataModelFactory
                {
                    public static object Create(string entity) => null;
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing factory without Create method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing Create method");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("UserFactory");
        violations[0].Message.ShouldContain("Create");
    }

    #endregion

    #region DataModelBase Should Be Ignored

    [Fact]
    public void DataModelBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating DataModelBase class (should not require factories)");
        var rule = new IN013_BidirectionalFactoriesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class DataModelBase { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModelBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying DataModelBase is ignored");
        var typeResults = results
            .SelectMany(r => r.TypeResults)
            .ToList();
        typeResults.ShouldBeEmpty();
    }

    #endregion

    #region DataModelsRepositories Namespace Should Be Ignored

    [Fact]
    public void ClassInDataModelsRepositories_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class in DataModelsRepositories namespace (not DataModels)");
        var rule = new IN013_BidirectionalFactoriesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories
            {
                public class UserDataModelRepository { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing class in DataModelsRepositories namespace");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class in DataModelsRepositories is ignored");
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
