using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN010_DataModelInheritsDataModelBaseRuleTests : TestBase
{
    public IN010_DataModelInheritsDataModelBaseRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN010_DataModelInheritsDataModelBaseRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN010_DataModelInheritsDataModelBase");
        rule.Description.ShouldContain("DataModelBase");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-010-datamodel-herda-datamodelbase.md");
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
        var rule = new IN010_DataModelInheritsDataModelBaseRule();
        var source = """
            namespace Some.Namespace.DataModels
            {
                public class SomeDataModel { }
            }
            """;
        var compilations = CreateCompilationWithSource(projectName, source);

        // Act
        LogAct("Analyzing project");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying project classes are ignored");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Valid DataModel

    [Fact]
    public void DataModel_InheritsDataModelBase_ShouldPass()
    {
        // Arrange
        LogArrange("Creating DataModel that inherits DataModelBase");
        var rule = new IN010_DataModelInheritsDataModelBaseRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels
            {
                public class DataModelBase { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels.DataModelBase
                {
                    public string Email { get; set; }
                    public string Username { get; set; }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing valid DataModel");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for valid DataModel");
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

    #region Missing DataModelBase Inheritance

    [Fact]
    public void DataModel_WithoutDataModelBase_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating DataModel without DataModelBase inheritance");
        var rule = new IN010_DataModelInheritsDataModelBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel
                {
                    public string Email { get; set; }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel without base class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing DataModelBase");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Rule.ShouldBe("IN010_DataModelInheritsDataModelBase");
        violations[0].Message.ShouldContain("nao herda de DataModelBase");
    }

    #endregion

    #region DataModel With Business Methods

    [Fact]
    public void DataModel_WithBusinessMethods_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating DataModel with business methods");
        var rule = new IN010_DataModelInheritsDataModelBaseRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels
            {
                public class DataModelBase { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels.DataModelBase
                {
                    public string Email { get; set; }
                    public bool IsActive() => true;
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with business methods");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for business methods");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("metodos de negocio");
        violations[0].Message.ShouldContain("IsActive");
    }

    #endregion

    #region DataModelsRepositories Namespace Should Be Ignored

    [Fact]
    public void ClassInDataModelsRepositoriesNamespace_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class in DataModelsRepositories namespace (not DataModels)");
        var rule = new IN010_DataModelInheritsDataModelBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories
            {
                public class UserDataModelRepository
                {
                    public void SomeMethod() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing class in DataModelsRepositories namespace");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class is ignored (wrong namespace)");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Indirect Inheritance

    [Fact]
    public void DataModel_InheritsIndirectly_ShouldPass()
    {
        // Arrange
        LogArrange("Creating DataModel with indirect DataModelBase inheritance");
        var rule = new IN010_DataModelInheritsDataModelBaseRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels
            {
                public class DataModelBase { }
                public class ExtendedDataModelBase : DataModelBase { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels.ExtendedDataModelBase
                {
                    public string Email { get; set; }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with indirect inheritance");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for indirect inheritance");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
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
