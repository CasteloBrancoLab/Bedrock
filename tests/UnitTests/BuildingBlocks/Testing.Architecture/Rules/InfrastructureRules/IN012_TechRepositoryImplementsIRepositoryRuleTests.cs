using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN012_TechRepositoryImplementsIRepositoryRuleTests : TestBase
{
    public IN012_TechRepositoryImplementsIRepositoryRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN012_TechRepositoryImplementsIRepositoryRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN012_TechRepositoryImplementsIRepository");
        rule.Description.ShouldContain("IRepository");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-012-repositorio-tech-implementa-irepository.md");
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
        var rule = new IN012_TechRepositoryImplementsIRepositoryRule();
        var source = """
            namespace Some.Namespace.Repositories.Interfaces
            {
                public interface ISomeRepository { }
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
    public void Interface_ImplementsIRepository_ShouldPass()
    {
        // Arrange
        LogArrange("Creating interface that implements IRepository transitively");
        var rule = new IN012_TechRepositoryImplementsIRepositoryRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Domain.Repositories.Interfaces
            {
                public interface IRepository { }
                public interface IRepository<TAggregateRoot> : IRepository { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces
            {
                public interface IPostgreSqlRepository<TAggregateRoot>
                    : Bedrock.BuildingBlocks.Domain.Repositories.Interfaces.IRepository<TAggregateRoot>
                { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces
            {
                public interface IUserPostgreSqlRepository
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces.IPostgreSqlRepository<string>
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
            .Where(t => t.TypeName == "IUserPostgreSqlRepository" && t.Status == TypeAnalysisStatus.Passed)
            .ToList();
        passed.Count.ShouldBe(1);
    }

    #endregion

    #region Interface Missing IRepository

    [Fact]
    public void Interface_WithoutIRepository_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating interface that does NOT implement IRepository");
        var rule = new IN012_TechRepositoryImplementsIRepositoryRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces
            {
                public interface IUserPostgreSqlRepository { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing interface without IRepository");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing IRepository");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Rule.ShouldBe("IN012_TechRepositoryImplementsIRepository");
        violations[0].Message.ShouldContain("nao herda de IRepository");
    }

    #endregion

    #region Valid Sealed Class

    [Fact]
    public void SealedClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating sealed class in Repositories namespace");
        var rule = new IN012_TechRepositoryImplementsIRepositoryRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories
            {
                public sealed class UserPostgreSqlRepository { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing valid sealed class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for sealed class");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();

        var passed = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.TypeName == "UserPostgreSqlRepository" && t.Status == TypeAnalysisStatus.Passed)
            .ToList();
        passed.Count.ShouldBe(1);
    }

    #endregion

    #region Class Not Sealed

    [Fact]
    public void Class_NotSealed_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-sealed class in Repositories namespace");
        var rule = new IN012_TechRepositoryImplementsIRepositoryRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories
            {
                public class UserPostgreSqlRepository { }
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

    #region DataModelsRepositories Should Be Ignored

    [Fact]
    public void InterfaceInDataModelsRepositories_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating interface in DataModelsRepositories namespace (IN-011 territory)");
        var rule = new IN012_TechRepositoryImplementsIRepositoryRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces
            {
                public interface IUserDataModelRepository { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing interface in DataModelsRepositories namespace");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying interface in DataModelsRepositories is ignored");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    [Fact]
    public void ClassInDataModelsRepositories_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class in DataModelsRepositories namespace (IN-011 territory)");
        var rule = new IN012_TechRepositoryImplementsIRepositoryRule();
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
