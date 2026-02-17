using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN011_DataModelRepositoryImplementsBaseRuleTests : TestBase
{
    public IN011_DataModelRepositoryImplementsBaseRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN011_DataModelRepositoryImplementsBase");
        rule.Description.ShouldContain("DataModelRepositories");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-011-datamodel-repository-implementa-idatamodelrepository.md");
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
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();
        var source = """
            namespace Some.Namespace.DataModelsRepositories.Interfaces
            {
                public interface ISomeDataModelRepository { }
            }
            """;
        var compilations = CreateCompilationWithSource(projectName, source);

        // Act
        LogAct("Analyzing project");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying project is ignored");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Valid Interface

    [Fact]
    public void Interface_ImplementsIDataModelRepository_ShouldPass()
    {
        // Arrange
        LogArrange("Creating interface that implements IDataModelRepository transitively");
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces
            {
                public interface IDataModelRepository { }
                public interface IDataModelRepository<TDataModel> : IDataModelRepository
                    where TDataModel : class { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces
            {
                public interface IPostgreSqlDataModelRepository<TDataModel>
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces.IDataModelRepository<TDataModel>
                    where TDataModel : class { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces
            {
                public interface IUserDataModelRepository
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces.IPostgreSqlDataModelRepository<string>
                {
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing valid interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for valid interface");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();

        var passed = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.TypeName == "IUserDataModelRepository" && t.Status == TypeAnalysisStatus.Passed)
            .ToList();
        passed.Count.ShouldBe(1);
    }

    #endregion

    #region Interface Missing IDataModelRepository

    [Fact]
    public void Interface_WithoutIDataModelRepository_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating interface that does NOT implement IDataModelRepository");
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces
            {
                public interface IUserDataModelRepository
                {
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing interface without IDataModelRepository");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing IDataModelRepository");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Rule.ShouldBe("IN011_DataModelRepositoryImplementsBase");
        violations[0].Message.ShouldContain("nao herda de IDataModelRepository");
    }

    #endregion

    #region Valid Sealed Class With Base

    [Fact]
    public void SealedClass_InheritsDataModelRepositoryBase_ShouldPass()
    {
        // Arrange
        LogArrange("Creating sealed class inheriting DataModelRepositoryBase");
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories
            {
                public abstract class DataModelRepositoryBase<TDataModel> where TDataModel : class { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories
            {
                public sealed class UserDataModelRepository
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.DataModelRepositoryBase<string>
                {
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing valid sealed class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for valid class");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();

        var passed = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.TypeName == "UserDataModelRepository" && t.Status == TypeAnalysisStatus.Passed)
            .ToList();
        passed.Count.ShouldBe(1);
    }

    #endregion

    #region Class Not Sealed

    [Fact]
    public void Class_NotSealed_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-sealed class in DataModelsRepositories");
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories
            {
                public abstract class DataModelRepositoryBase<TDataModel> where TDataModel : class { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories
            {
                public class UserDataModelRepository
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.DataModelRepositoryBase<string>
                {
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-sealed class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-sealed class");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao e sealed");
    }

    #endregion

    #region Class Missing DataModelRepositoryBase

    [Fact]
    public void Class_WithoutDataModelRepositoryBase_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating sealed class without DataModelRepositoryBase inheritance");
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories
            {
                public sealed class UserDataModelRepository
                {
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing class without DataModelRepositoryBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing DataModelRepositoryBase");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao herda de DataModelRepositoryBase");
    }

    #endregion

    #region Class Not Sealed And Missing Base

    [Fact]
    public void Class_NotSealedAndMissingBase_ShouldGenerateViolationWithBothIssues()
    {
        // Arrange
        LogArrange("Creating non-sealed class without DataModelRepositoryBase");
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories
            {
                public class UserDataModelRepository
                {
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing class with multiple issues");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation contains both issues");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao e sealed");
        violations[0].Message.ShouldContain("nao herda de DataModelRepositoryBase");
    }

    #endregion

    #region DataModels Namespace Should Be Ignored

    [Fact]
    public void ClassInDataModelsNamespace_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class in DataModels namespace (not DataModelsRepositories)");
        var rule = new IN011_DataModelRepositoryImplementsBaseRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing class in DataModels namespace");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class in DataModels is ignored by this rule");
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
