using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN008_ConnectionImplementationSealedRuleTests : TestBase
{
    public IN008_ConnectionImplementationSealedRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN008_ConnectionImplementationSealedRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN008_ConnectionImplementationSealed");
        rule.Description.ShouldContain("sealed");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-008-conexao-implementacao-sealed-herda-base.md");
        rule.Category.ShouldBe("Infrastructure");
    }

    #endregion

    #region Valid Implementation

    [Fact]
    public void SealedClass_WithBaseAndMarker_ShouldPass()
    {
        // Arrange
        LogArrange("Creating valid sealed connection implementation");
        var rule = new IN008_ConnectionImplementationSealedRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces
            {
                public interface IConnection { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces
            {
                public interface IPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces.IConnection { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections
            {
                public abstract class PostgreSqlConnectionBase
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces.IPostgreSqlConnection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces
            {
                public interface IAuthPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces.IPostgreSqlConnection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public sealed class AuthPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.PostgreSqlConnectionBase,
                      ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces.IAuthPostgreSqlConnection
                { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing valid connection implementation");
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
    public void NonSealedConnection_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-sealed connection class");
        var rule = new IN008_ConnectionImplementationSealedRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces
            {
                public interface IConnection { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces
            {
                public interface IPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces.IConnection { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections
            {
                public abstract class PostgreSqlConnectionBase
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces.IPostgreSqlConnection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces
            {
                public interface IAuthPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces.IPostgreSqlConnection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public class AuthPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.PostgreSqlConnectionBase,
                      ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces.IAuthPostgreSqlConnection
                { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-sealed connection");
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
        violations[0].Rule.ShouldBe("IN008_ConnectionImplementationSealed");
    }

    #endregion

    #region Missing Base Class

    [Fact]
    public void ConnectionWithoutBaseClass_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating connection class without base class");
        var rule = new IN008_ConnectionImplementationSealedRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces
            {
                public interface IConnection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces
            {
                public interface IAuthPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces.IConnection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public sealed class AuthPostgreSqlConnection
                    : ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces.IAuthPostgreSqlConnection
                { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing connection without base class");
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
    public void ConnectionWithoutBcMarker_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating connection class without BC marker interface");
        var rule = new IN008_ConnectionImplementationSealedRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces
            {
                public interface IConnection { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces
            {
                public interface IPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces.IConnection { }
            }
            namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections
            {
                public abstract class PostgreSqlConnectionBase
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces.IPostgreSqlConnection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public sealed class AuthPostgreSqlConnection
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.PostgreSqlConnectionBase
                { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing connection without BC marker");
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

    #region Non-Connection Class Should Be Ignored

    [Fact]
    public void NonConnectionClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class in Connections namespace that doesn't implement IConnection");
        var rule = new IN008_ConnectionImplementationSealedRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public class SomeHelper { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-connection class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for non-connection class");
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
