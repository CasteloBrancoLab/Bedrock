using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE046_EnumConventionsRuleTests : TestBase
{
    public DE046_EnumConventionsRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EnumWithByteTypeAndExplicitValues_ShouldPass()
    {
        // Arrange
        LogArrange("Creating enum with byte type and explicit values starting at 1");
        var rule = new DE046_EnumConventionsRule();
        var source = """
            #nullable enable
            public enum OrderStatus : byte
            {
                Pending = 1,
                Confirmed = 2,
                Shipped = 3
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing enum with proper conventions");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying enum with proper conventions passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "OrderStatus");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EnumWithEnumSuffix_ShouldFail()
    {
        // Arrange
        LogArrange("Creating enum with Enum suffix");
        var rule = new DE046_EnumConventionsRule();
        var source = """
            #nullable enable
            public enum StatusEnum : byte
            {
                Active = 1
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing enum with Enum suffix");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying enum with Enum suffix fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "StatusEnum");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE046_EnumConventions");
        typeResult.Violation.Message.ShouldContain("Enum");
    }

    [Fact]
    public void EnumWithIntUnderlyingType_ShouldFail()
    {
        // Arrange
        LogArrange("Creating enum with int underlying type");
        var rule = new DE046_EnumConventionsRule();
        var source = """
            #nullable enable
            public enum Priority
            {
                Low = 1,
                Medium = 2
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing enum with int underlying type");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying enum with int underlying type fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Priority");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("byte");
    }

    [Fact]
    public void EnumWithShortType_ShouldPass()
    {
        // Arrange
        LogArrange("Creating enum with short underlying type");
        var rule = new DE046_EnumConventionsRule();
        var source = """
            #nullable enable
            public enum LargeCategory : short
            {
                First = 1,
                Second = 2
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing enum with short underlying type");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying enum with short underlying type passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "LargeCategory");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EnumWithZeroValueNonStandard_ShouldFail()
    {
        // Arrange
        LogArrange("Creating enum with non-standard zero value");
        var rule = new DE046_EnumConventionsRule();
        var source = """
            #nullable enable
            public enum Color : byte
            {
                Red = 0,
                Green = 1
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing enum with non-standard zero value");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying enum with non-standard zero value fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Color");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("0");
    }

    [Fact]
    public void EnumWithZeroNoneValue_ShouldPass()
    {
        // Arrange
        LogArrange("Creating enum with None = 0");
        var rule = new DE046_EnumConventionsRule();
        var source = """
            #nullable enable
            public enum State : byte
            {
                None = 0,
                Active = 1
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing enum with None = 0");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying enum with None = 0 passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "State");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void NonEnumType_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating non-enum type");
        var rule = new DE046_EnumConventionsRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public string? Name { get; set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing non-enum type");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-enum type is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "RegularClass");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating enum to verify violation metadata");
        var rule = new DE046_EnumConventionsRule();
        var source = """
            #nullable enable
            public enum TypeEnumeration : byte
            {
                Value = 1
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "TypeEnumeration");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE046_EnumConventions");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-046-convencoes-enumeracoes-dominio.md");
        violation.LlmHint.ShouldContain("Enumeration");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE046_EnumConventionsRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE046_EnumConventions");
        rule.Description.ShouldContain("enum");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-046-convencoes-enumeracoes-dominio.md");
    }

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilations(string source)
    {
        return new Dictionary<string, Compilation>
        {
            ["TestProject"] = CreateSingleCompilation(source, "TestProject")
        };
    }

    private static Compilation CreateSingleCompilation(string source, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "TestFile.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        return CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));
    }

    #endregion
}
