using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;

public class RL003_NoSqlLiteralsOutsideMappersRuleTests : TestBase
{
    public RL003_NoSqlLiteralsOutsideMappersRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new RL003_NoSqlLiteralsOutsideMappersRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("RL003_NoSqlLiteralsOutsideMappers");
        rule.Description.ShouldContain("SQL");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/relational/RL-003-proibir-sql-fora-de-mapper.md");
        rule.Category.ShouldBe("Relational");
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
        var rule = new RL003_NoSqlLiteralsOutsideMappersRule();
        var source = """
            namespace Some.Namespace.Repositories
            {
                public class UserRepository
                {
                    private const string Sql = "SELECT * FROM users WHERE id = @id";
                }
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

    #region SQL In Mappers Namespace Should Be Allowed

    [Fact]
    public void SqlInMappersNamespace_ShouldBeAllowed()
    {
        // Arrange
        LogArrange("Creating class in Mappers namespace with SQL literals");
        var rule = new RL003_NoSqlLiteralsOutsideMappersRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public sealed class UserDataModelMapper
                {
                    private const string Sql = "SELECT id, name FROM users WHERE tenant = @tenant";
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing SQL in Mappers namespace");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying SQL in Mappers is allowed (no types outside Mappers)");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region SQL Outside Mappers Should Violate

    [Fact]
    public void SqlOutsideMappers_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating class outside Mappers with SQL literal");
        var rule = new RL003_NoSqlLiteralsOutsideMappersRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories
            {
                public sealed class UserRepository
                {
                    private const string Sql = "SELECT id, name FROM users WHERE tenant = @tenant";
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing SQL outside Mappers");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for SQL outside Mappers");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("SQL");
        violations[0].Message.ShouldContain("UserRepository");
    }

    #endregion

    #region Single SQL Keyword Should Be Allowed

    [Fact]
    public void SingleSqlKeyword_ShouldBeAllowed()
    {
        // Arrange
        LogArrange("Creating class with only one SQL keyword (not enough for detection)");
        var rule = new RL003_NoSqlLiteralsOutsideMappersRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories
            {
                public sealed class UserRepository
                {
                    private const string Message = "SELECT your option";
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing single SQL keyword");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying single keyword is allowed");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region No SQL Literals Should Pass

    [Fact]
    public void NoSqlLiterals_ShouldPass()
    {
        // Arrange
        LogArrange("Creating class with no SQL literals");
        var rule = new RL003_NoSqlLiteralsOutsideMappersRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories
            {
                public sealed class UserRepository
                {
                    private const string Name = "user_repository";
                    public void DoSomething() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing class without SQL literals");
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

    #region Multiple SQL Keywords Should Violate

    [Fact]
    public void MultipleSqlKeywords_ShouldViolate()
    {
        // Arrange
        LogArrange("Creating class with INSERT INTO VALUES SQL literal");
        var rule = new RL003_NoSqlLiteralsOutsideMappersRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories
            {
                public sealed class UserDataModelRepository
                {
                    private const string InsertSql = "INSERT INTO users (id, name) VALUES (@id, @name)";
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing INSERT SQL literal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for INSERT SQL");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
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
