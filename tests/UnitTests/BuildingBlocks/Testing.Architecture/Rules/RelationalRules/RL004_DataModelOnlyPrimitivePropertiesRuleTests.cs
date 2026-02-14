using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;

public class RL004_DataModelOnlyPrimitivePropertiesRuleTests : TestBase
{
    public RL004_DataModelOnlyPrimitivePropertiesRuleTests(ITestOutputHelper outputHelper)
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
        var rule = new RL004_DataModelOnlyPrimitivePropertiesRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("RL004_DataModelOnlyPrimitiveProperties");
        rule.Description.ShouldContain("primitiv");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/relational/RL-004-datamodel-propriedades-primitivas.md");
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
        var rule = new RL004_DataModelOnlyPrimitivePropertiesRule();
        var source = """
            namespace Some.Namespace.DataModels
            {
                public class UserDataModel
                {
                    public System.Collections.Generic.List<string> Tags { get; set; }
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

    #region DataModel With Only Primitive Properties Should Pass

    [Fact]
    public void DataModel_WithOnlyPrimitiveProperties_ShouldPass()
    {
        // Arrange
        LogArrange("Creating DataModel with only primitive properties");
        var rule = new RL004_DataModelOnlyPrimitivePropertiesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel
                {
                    public string Username { get; set; }
                    public string Email { get; set; }
                    public int Age { get; set; }
                    public long EntityVersion { get; set; }
                    public System.Guid Id { get; set; }
                    public bool IsActive { get; set; }
                    public System.DateTimeOffset CreatedAt { get; set; }
                    public byte[] PasswordHash { get; set; }
                    public short Status { get; set; }
                    public decimal Balance { get; set; }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with primitive properties");
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

    #region DataModel With Complex Property Should Violate

    [Fact]
    public void DataModel_WithComplexProperty_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating DataModel with complex (List) property");
        var rule = new RL004_DataModelOnlyPrimitivePropertiesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class Address
                {
                    public string Street { get; set; }
                }
                public class UserDataModel
                {
                    public string Username { get; set; }
                    public Address HomeAddress { get; set; }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with complex property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for complex property");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("HomeAddress");
        violations[0].Message.ShouldContain("Address");
    }

    #endregion

    #region Nullable Primitives Should Be Allowed

    [Fact]
    public void DataModel_WithNullablePrimitives_ShouldPass()
    {
        // Arrange
        LogArrange("Creating DataModel with nullable primitive properties");
        var rule = new RL004_DataModelOnlyPrimitivePropertiesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public class UserDataModel
                {
                    public string? LastChangedBy { get; set; }
                    public System.DateTimeOffset? LastChangedAt { get; set; }
                    public System.Guid? CorrelationId { get; set; }
                    public long? OptionalVersion { get; set; }
                    public int? NullableAge { get; set; }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with nullable primitives");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for nullable primitives");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Enum Properties Should Be Allowed

    [Fact]
    public void DataModel_WithEnumProperty_ShouldPass()
    {
        // Arrange
        LogArrange("Creating DataModel with enum property");
        var rule = new RL004_DataModelOnlyPrimitivePropertiesRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels
            {
                public enum UserStatus { Active, Inactive }
                public class UserDataModel
                {
                    public string Username { get; set; }
                    public UserStatus Status { get; set; }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing DataModel with enum property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for enum property");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region DataModelBase Should Be Ignored

    [Fact]
    public void DataModelBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating DataModelBase class (should not be analyzed)");
        var rule = new RL004_DataModelOnlyPrimitivePropertiesRule();
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
