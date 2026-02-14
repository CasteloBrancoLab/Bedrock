using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.PostgreSqlRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.PostgreSqlRules;

public class PG002_ConnectionValidatesConnectionStringRuleTests : TestBase
{
    public PG002_ConnectionValidatesConnectionStringRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new PG002_ConnectionValidatesConnectionStringRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("PG002_ConnectionValidatesConnectionString");
        rule.Description.ShouldContain("ThrowIfNullOrWhiteSpace");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/postgresql/PG-002-connection-validar-connectionstring.md");
        rule.Category.ShouldBe("PostgreSQL");
    }

    #endregion

    #region Non Infra.Data.Tech Projects Should Be Ignored

    [Theory]
    [InlineData("ShopDemo.Auth.Domain")]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    public void NonInfraDataTechProject_ShouldBeIgnored(string projectName)
    {
        // Arrange
        LogArrange($"Testing non-Infra.Data.Tech project '{projectName}'");
        var rule = new PG002_ConnectionValidatesConnectionStringRule();
        var source = CreateConnectionWithValidation();
        var compilations = CreateCompilationWithSource(projectName, source);

        // Act
        LogAct("Analyzing project");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying project is ignored");
        results.ShouldBeEmpty();
    }

    #endregion

    #region Connection With ThrowIfNullOrWhiteSpace Should Pass

    [Fact]
    public void Connection_WithThrowIfNullOrWhiteSpace_ShouldPass()
    {
        // Arrange
        LogArrange("Creating connection with ThrowIfNullOrWhiteSpace validation");
        var rule = new PG002_ConnectionValidatesConnectionStringRule();
        var source = CreateConnectionWithValidation();
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing connection with validation");
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

    #region Connection With ThrowIfNullOrEmpty Should Pass

    [Fact]
    public void Connection_WithThrowIfNullOrEmpty_ShouldPass()
    {
        // Arrange
        LogArrange("Creating connection with ThrowIfNullOrEmpty validation");
        var rule = new PG002_ConnectionValidatesConnectionStringRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public class Options { public void WithConnectionString(string s) { } }
                public abstract class PostgreSqlConnectionBase
                {
                    protected abstract void ConfigureInternal(Options options);
                }
                public sealed class AuthConnection : PostgreSqlConnectionBase
                {
                    protected override void ConfigureInternal(Options options)
                    {
                        string connectionString = "test";
                        System.ArgumentException.ThrowIfNullOrEmpty(connectionString);
                        options.WithConnectionString(connectionString);
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing connection with ThrowIfNullOrEmpty");
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

    #region Connection Without Validation Should Violate

    [Fact]
    public void Connection_WithoutValidation_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating connection without connection string validation");
        var rule = new PG002_ConnectionValidatesConnectionStringRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public class Options { public void WithConnectionString(string s) { } }
                public abstract class PostgreSqlConnectionBase
                {
                    protected abstract void ConfigureInternal(Options options);
                }
                public sealed class AuthConnection : PostgreSqlConnectionBase
                {
                    protected override void ConfigureInternal(Options options)
                    {
                        options.WithConnectionString("hardcoded");
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing connection without validation");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing validation");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("ThrowIfNullOrWhiteSpace");
        violations[0].Message.ShouldContain("AuthConnection");
    }

    #endregion

    #region Non-Connection Classes Should Be Ignored

    [Fact]
    public void NonConnectionClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class that does not inherit PostgreSqlConnectionBase");
        var rule = new PG002_ConnectionValidatesConnectionStringRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public sealed class ConnectionHelper
                {
                    public void DoSomething() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-connection class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class is ignored (no PostgreSqlConnectionBase inheritance)");
        var typeResults = results
            .SelectMany(r => r.TypeResults)
            .ToList();
        typeResults.ShouldBeEmpty();
    }

    #endregion

    #region Helpers

    private static string CreateConnectionWithValidation()
    {
        return """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public class Options { public void WithConnectionString(string s) { } }
                public abstract class PostgreSqlConnectionBase
                {
                    protected abstract void ConfigureInternal(Options options);
                }
                public sealed class AuthConnection : PostgreSqlConnectionBase
                {
                    protected override void ConfigureInternal(Options options)
                    {
                        string connectionString = "test";
                        System.ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
                        options.WithConnectionString(connectionString);
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
