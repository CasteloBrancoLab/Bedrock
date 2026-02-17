using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN014_DataModelAdapterRuleTests : TestBase
{
    public IN014_DataModelAdapterRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN014_DataModelAdapterRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN014_DataModelAdapter");
        rule.Description.ShouldContain("Adapter");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-014-adapter-atualizacao-datamodel-existente.md");
        rule.Category.ShouldBe("Infrastructure");
    }

    #endregion

    #region Non Infra.Data.Tech Projects Should Be Ignored

    [Theory]
    [InlineData("ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Infra.Data")]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql.Migrations")]
    public void NonInfraDataTechProject_ShouldBeIgnored(string projectName)
    {
        // Arrange
        LogArrange($"Testing non-Infra.Data.Tech project '{projectName}'");
        var rule = new IN014_DataModelAdapterRule();
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

    #region Valid Adapter

    [Fact]
    public void DataModel_WithValidAdapter_ShouldPass()
    {
        // Arrange
        LogArrange("Creating DataModel with valid static adapter having Adapt method");
        var rule = new IN014_DataModelAdapterRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel
                {
                    public string Email { get; set; }
                }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters
            {
                public static class UserDataModelAdapter
                {
                    public static object Adapt(object dataModel, object entity) => dataModel;
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with valid adapter");
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

    #region Missing Adapter

    [Fact]
    public void DataModel_MissingAdapter_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating DataModel without adapter");
        var rule = new IN014_DataModelAdapterRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel without adapter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing adapter");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("UserDataModelAdapter");
        violations[0].Message.ShouldContain("nao encontrado");
    }

    #endregion

    #region Adapter Not Static

    [Fact]
    public void Adapter_NotStatic_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-static adapter class");
        var rule = new IN014_DataModelAdapterRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters
            {
                public class UserDataModelAdapter
                {
                    public static object Adapt(object dataModel, object entity) => dataModel;
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-static adapter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-static adapter");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao e static");
    }

    #endregion

    #region Adapter Missing Adapt Method

    [Fact]
    public void Adapter_MissingAdaptMethod_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating adapter without Adapt method");
        var rule = new IN014_DataModelAdapterRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters
            {
                public static class UserDataModelAdapter
                {
                    public static object Update(object dataModel, object entity) => dataModel;
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing adapter without Adapt method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing Adapt method");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("Adapt");
    }

    #endregion

    #region DataModelBase Should Be Ignored

    [Fact]
    public void DataModelBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating DataModelBase class (should not require adapter)");
        var rule = new IN014_DataModelAdapterRule();
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
