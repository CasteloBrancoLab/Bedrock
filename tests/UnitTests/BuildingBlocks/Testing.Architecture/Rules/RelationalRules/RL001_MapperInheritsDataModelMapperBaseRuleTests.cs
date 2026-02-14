using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;

public class RL001_MapperInheritsDataModelMapperBaseRuleTests : TestBase
{
    public RL001_MapperInheritsDataModelMapperBaseRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("RL001_MapperInheritsDataModelMapperBase");
        rule.Description.ShouldContain("Mapper");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/relational/RL-001-mapper-herda-datamodelmapperbase.md");
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
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
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

    #region Valid Mapper

    [Fact]
    public void DataModel_WithValidMapper_ShouldPass()
    {
        // Arrange
        LogArrange("Creating DataModel with valid sealed mapper inheriting DataModelMapperBase");
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal();
                    public abstract void MapBinaryImporter();
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal() { }
                    public override void MapBinaryImporter() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with valid mapper");
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

    #region Missing Mapper

    [Fact]
    public void DataModel_MissingMapper_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating DataModel without mapper");
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel without mapper");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing mapper");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("UserDataModelMapper");
        violations[0].Message.ShouldContain("nao encontrado");
    }

    #endregion

    #region Mapper Not Sealed

    [Fact]
    public void Mapper_NotSealed_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-sealed mapper class");
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal();
                    public abstract void MapBinaryImporter();
                }
                public class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal() { }
                    public override void MapBinaryImporter() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-sealed mapper");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-sealed mapper");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao e sealed");
    }

    #endregion

    #region Mapper Does Not Inherit DataModelMapperBase

    [Fact]
    public void Mapper_NotInheritingBase_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating mapper that does not inherit DataModelMapperBase");
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public sealed class UserDataModelMapper
                {
                    protected void ConfigureInternal() { }
                    public void MapBinaryImporter() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper without base class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing base class");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao herda de DataModelMapperBase");
    }

    #endregion

    #region Mapper Missing ConfigureInternal

    [Fact]
    public void Mapper_MissingConfigureInternal_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating mapper without ConfigureInternal override");
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal();
                    public abstract void MapBinaryImporter();
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    public override void MapBinaryImporter() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper without ConfigureInternal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing ConfigureInternal");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("ConfigureInternal");
    }

    #endregion

    #region Mapper Missing MapBinaryImporter

    [Fact]
    public void Mapper_MissingMapBinaryImporter_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating mapper without MapBinaryImporter override");
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal();
                    public abstract void MapBinaryImporter();
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper without MapBinaryImporter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing MapBinaryImporter");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("MapBinaryImporter");
    }

    #endregion

    #region Multiple Issues

    [Fact]
    public void Mapper_WithMultipleIssues_ShouldReportAll()
    {
        // Arrange
        LogArrange("Creating mapper with multiple violations (not sealed, no base, no overrides)");
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public class UserDataModelMapper
                {
                    public void SomeMethod() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper with multiple issues");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying all violations are reported");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao e sealed");
        violations[0].Message.ShouldContain("nao herda de DataModelMapperBase");
        violations[0].Message.ShouldContain("ConfigureInternal");
        violations[0].Message.ShouldContain("MapBinaryImporter");
    }

    #endregion

    #region DataModelBase Should Be Ignored

    [Fact]
    public void DataModelBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating DataModelBase class (should not require mapper)");
        var rule = new RL001_MapperInheritsDataModelMapperBaseRule();
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
