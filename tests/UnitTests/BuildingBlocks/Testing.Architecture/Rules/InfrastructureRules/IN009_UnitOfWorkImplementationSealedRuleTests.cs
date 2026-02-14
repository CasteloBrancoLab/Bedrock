using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN009_UnitOfWorkImplementationSealedRuleTests : TestBase
{
    public IN009_UnitOfWorkImplementationSealedRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN009_UnitOfWorkImplementationSealedRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN009_UnitOfWorkImplementationSealed");
        rule.Description.ShouldContain("sealed");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-009-unitofwork-implementacao-sealed-herda-base.md");
        rule.Category.ShouldBe("Infrastructure");
    }

    #endregion

    #region Valid Implementation

    [Fact]
    public void SealedClass_WithBaseAndMarker_ShouldPass()
    {
        // Arrange
        LogArrange("Creating valid sealed UnitOfWork implementation");
        var rule = new IN009_UnitOfWorkImplementationSealedRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces
            {
                public interface IUnitOfWork { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces.IUnitOfWork { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork
            {
                public abstract class PostgreSqlUnitOfWorkBase
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces.IPostgreSqlUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IAuthPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces.IPostgreSqlUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork
            {
                public sealed class AuthPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.PostgreSqlUnitOfWorkBase,
                      ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces.IAuthPostgreSqlUnitOfWork
                { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing valid UnitOfWork implementation");
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

    #region Not Sealed

    [Fact]
    public void NonSealedUnitOfWork_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-sealed UnitOfWork class");
        var rule = new IN009_UnitOfWorkImplementationSealedRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces
            {
                public interface IUnitOfWork { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces.IUnitOfWork { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork
            {
                public abstract class PostgreSqlUnitOfWorkBase
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces.IPostgreSqlUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IAuthPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces.IPostgreSqlUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork
            {
                public class AuthPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.PostgreSqlUnitOfWorkBase,
                      ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces.IAuthPostgreSqlUnitOfWork
                { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-sealed UnitOfWork");
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
        violations[0].Rule.ShouldBe("IN009_UnitOfWorkImplementationSealed");
    }

    #endregion

    #region Missing Base Class

    [Fact]
    public void UnitOfWorkWithoutBaseClass_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating UnitOfWork class without base class");
        var rule = new IN009_UnitOfWorkImplementationSealedRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces
            {
                public interface IUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IAuthPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces.IUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork
            {
                public sealed class AuthPostgreSqlUnitOfWork
                    : ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces.IAuthPostgreSqlUnitOfWork
                { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing UnitOfWork without base class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing base class");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao herda de uma base class tecnologica");
    }

    #endregion

    #region Missing BC Marker

    [Fact]
    public void UnitOfWorkWithoutBcMarker_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating UnitOfWork class without BC marker interface");
        var rule = new IN009_UnitOfWorkImplementationSealedRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces
            {
                public interface IUnitOfWork { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces.IUnitOfWork { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork
            {
                public abstract class PostgreSqlUnitOfWorkBase
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces.IPostgreSqlUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork
            {
                public sealed class AuthPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.PostgreSqlUnitOfWorkBase
                { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing UnitOfWork without BC marker");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing BC marker");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao implementa a marker interface");
    }

    #endregion

    #region Non-UnitOfWork Class Should Be Ignored

    [Fact]
    public void NonUnitOfWorkClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class in UnitOfWork namespace that doesn't implement IUnitOfWork");
        var rule = new IN009_UnitOfWorkImplementationSealedRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork
            {
                public class SomeHelper { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-UnitOfWork class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for non-UnitOfWork class");
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
