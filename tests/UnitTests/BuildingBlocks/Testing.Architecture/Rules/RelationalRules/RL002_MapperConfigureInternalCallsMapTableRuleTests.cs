using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;

public class RL002_MapperConfigureInternalCallsMapTableRuleTests : TestBase
{
    public RL002_MapperConfigureInternalCallsMapTableRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new RL002_MapperConfigureInternalCallsMapTableRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("RL002_MapperConfigureInternalCallsMapTable");
        rule.Description.ShouldContain("MapTable");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/relational/RL-002-mapper-configurar-maptable.md");
        rule.Category.ShouldBe("Relational");
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
        var rule = new RL002_MapperConfigureInternalCallsMapTableRule();
        var source = CreateMapperWithMapTable();
        var compilations = CreateCompilationWithSource(projectName, source);

        // Act
        LogAct("Analyzing project");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying project is ignored");
        results.ShouldBeEmpty();
    }

    #endregion

    #region Valid Mapper With MapTable

    [Fact]
    public void Mapper_WithMapTable_ShouldPass()
    {
        // Arrange
        LogArrange("Creating mapper with MapTable call in ConfigureInternal");
        var rule = new RL002_MapperConfigureInternalCallsMapTableRule();
        var source = CreateMapperWithMapTable();
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper with MapTable");
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

    #region Mapper Without MapTable

    [Fact]
    public void Mapper_WithoutMapTable_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating mapper without MapTable call");
        var rule = new RL002_MapperConfigureInternalCallsMapTableRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal(object options);
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal(object options)
                    {
                        // Nenhuma chamada a MapTable
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper without MapTable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing MapTable");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("MapTable");
        violations[0].Message.ShouldContain("UserDataModelMapper");
    }

    #endregion

    #region Non-Mapper Classes Should Be Ignored

    [Fact]
    public void NonMapperClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating non-mapper class in Mappers namespace");
        var rule = new RL002_MapperConfigureInternalCallsMapTableRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public sealed class SomeHelper
                {
                    public void DoSomething() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-mapper class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class is ignored (no DataModelMapperBase inheritance)");
        var typeResults = results
            .SelectMany(r => r.TypeResults)
            .ToList();
        typeResults.ShouldBeEmpty();
    }

    #endregion

    #region Mapper With MapTable In Fluent Chain

    [Fact]
    public void Mapper_WithMapTableInFluentChain_ShouldPass()
    {
        // Arrange
        LogArrange("Creating mapper with MapTable in fluent method chain");
        var rule = new RL002_MapperConfigureInternalCallsMapTableRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public class MapperOptions
                {
                    public MapperOptions MapTable(string schema, string name) => this;
                    public MapperOptions MapColumn(string name) => this;
                }
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal(MapperOptions options);
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal(MapperOptions options)
                    {
                        options
                            .MapTable("public", "users")
                            .MapColumn("Username");
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper with fluent MapTable chain");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Helpers

    private static string CreateMapperWithMapTable()
    {
        return """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public class MapperOptions
                {
                    public MapperOptions MapTable(string schema, string name) => this;
                }
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal(MapperOptions options);
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal(MapperOptions options)
                    {
                        options.MapTable("public", "users");
                    }
                }
            }
            """;
    }

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
