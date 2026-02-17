using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN007_UnitOfWorkMarkerInterfaceRuleTests : TestBase
{
    public IN007_UnitOfWorkMarkerInterfaceRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new IN007_UnitOfWorkMarkerInterfaceRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN007_UnitOfWorkMarkerInterface");
        rule.Description.ShouldContain("marker interface de UnitOfWork");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-007-unitofwork-marker-interface-herda-iunitofwork.md");
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
        var rule = new IN007_UnitOfWorkMarkerInterfaceRule();
        var compilations = CreateCompilationWithSource(projectName, "public class Dummy { }");

        // Act
        LogAct("Analyzing project");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying project is ignored");
        results.ShouldBeEmpty();
    }

    #endregion

    #region Valid Marker Interface

    [Fact]
    public void ValidMarkerInterface_InheritsIUnitOfWork_ShouldPass()
    {
        // Arrange
        LogArrange("Creating project with valid marker interface that inherits IUnitOfWork");
        var rule = new IN007_UnitOfWorkMarkerInterfaceRule();
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
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IAuthPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces.IPostgreSqlUnitOfWork { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing valid marker interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations");
        var typeResults = results.SelectMany(r => r.TypeResults).ToList();
        typeResults.ShouldNotBeEmpty();
        var violations = typeResults.Where(t => t.Violation is not null).ToList();
        violations.ShouldBeEmpty();
        typeResults.ShouldContain(t => t.TypeName == "IAuthPostgreSqlUnitOfWork" && t.Status == TypeAnalysisStatus.Passed);
    }

    #endregion

    #region Missing Marker Interface

    [Fact]
    public void MissingMarkerInterface_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating Infra.Data.Tech project without UnitOfWork marker interface");
        var rule = new IN007_UnitOfWorkMarkerInterfaceRule();
        var source = "public class SomeClass { }";
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing project without marker interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing interface");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Rule.ShouldBe("IN007_UnitOfWorkMarkerInterface");
        violations[0].Message.ShouldContain("nao declara nenhuma interface");
        violations[0].Message.ShouldContain("UnitOfWork.Interfaces");
    }

    #endregion

    #region Interface Does Not Inherit IUnitOfWork

    [Fact]
    public void InterfaceWithoutIUnitOfWorkInheritance_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating interface that does not inherit IUnitOfWork");
        var rule = new IN007_UnitOfWorkMarkerInterfaceRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IAuthPostgreSqlUnitOfWork { }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing interface without IUnitOfWork inheritance");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing inheritance");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao herda de IUnitOfWork");
    }

    #endregion

    #region Non-Marker Interface

    [Fact]
    public void InterfaceWithMembers_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating interface with members (non-marker)");
        var rule = new IN007_UnitOfWorkMarkerInterfaceRule();
        var source = """
            namespace Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces
            {
                public interface IUnitOfWork { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces
            {
                public interface IAuthPostgreSqlUnitOfWork
                    : Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces.IUnitOfWork
                {
                    void ExtraMethod();
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-marker interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-marker interface");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao e um marker");
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
