using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture;

public class RuleTests : TestBase
{
    public RuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Analyze_WithEmptyCompilations_ShouldReturnEmptyResults()
    {
        // Arrange
        LogArrange("Creating rule with empty compilations");
        var rule = new AlwaysPassRule();
        var compilations = new Dictionary<string, Compilation>();

        // Act
        LogAct("Analyzing empty compilations");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying empty results");
        results.ShouldBeEmpty();
    }

    [Fact]
    public void Analyze_WithPassingRule_ShouldReturnPassedResults()
    {
        // Arrange
        LogArrange("Creating compilation with a simple class");
        var rule = new AlwaysPassRule();
        var compilations = CreateCompilations("public class TestClass { }");

        // Act
        LogAct("Analyzing with always-pass rule");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying passed results");
        results.Count.ShouldBe(1);
        results[0].RuleName.ShouldBe("AlwaysPass");
        results[0].ProjectName.ShouldBe("TestProject");

        var passedTypes = results[0].TypeResults.Where(t => t.Status == TypeAnalysisStatus.Passed).ToList();
        passedTypes.ShouldNotBeEmpty();
    }

    [Fact]
    public void Analyze_WithFailingRule_ShouldReturnFailedResults()
    {
        // Arrange
        LogArrange("Creating compilation with a simple class");
        var rule = new AlwaysFailRule();
        var compilations = CreateCompilations("public class TestClass { }");

        // Act
        LogAct("Analyzing with always-fail rule");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying failed results");
        results.Count.ShouldBe(1);

        var failedTypes = results[0].TypeResults.Where(t => t.Status == TypeAnalysisStatus.Failed).ToList();
        failedTypes.ShouldNotBeEmpty();
        failedTypes[0].Violation.ShouldNotBeNull();
        failedTypes[0].Violation!.Rule.ShouldBe("AlwaysFail");
    }

    [Fact]
    public void Analyze_ShouldPopulateRuleMetadataInResults()
    {
        // Arrange
        LogArrange("Creating rule to verify metadata population");
        var rule = new AlwaysPassRule();
        var compilations = CreateCompilations("public class TestClass { }");

        // Act
        LogAct("Analyzing");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying rule metadata in results");
        results[0].RuleName.ShouldBe("AlwaysPass");
        results[0].RuleDescription.ShouldBe("Always passes");
        results[0].DefaultSeverity.ShouldBe(Severity.Error);
        results[0].AdrPath.ShouldBe("docs/adr/test.md");
    }

    [Fact]
    public void Analyze_ShouldPopulateTypeAnalysisResultFields()
    {
        // Arrange
        LogArrange("Creating compilation to check type result fields");
        var rule = new AlwaysPassRule();
        var compilations = CreateCompilations("public class MyEntity { }");

        // Act
        LogAct("Analyzing");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying type analysis result fields");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyEntity");
        typeResult.ShouldNotBeNull();
        typeResult.TypeFullName.ShouldContain("MyEntity");
        typeResult.Line.ShouldBeGreaterThan(0);
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void Analyze_WithMultipleTypes_ShouldAnalyzeEach()
    {
        // Arrange
        LogArrange("Creating compilation with multiple classes");
        var rule = new AlwaysPassRule();
        var source = """
            public class ClassA { }
            public class ClassB { }
            public class ClassC { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing multiple types");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying all types analyzed");
        var typeNames = results[0].TypeResults.Select(t => t.TypeName).ToList();
        typeNames.ShouldContain("ClassA");
        typeNames.ShouldContain("ClassB");
        typeNames.ShouldContain("ClassC");
    }

    [Fact]
    public void Analyze_WithMultipleProjects_ShouldReturnResultPerProject()
    {
        // Arrange
        LogArrange("Creating multiple compilations");
        var rule = new AlwaysPassRule();
        var compilations = new Dictionary<string, Compilation>
        {
            ["ProjectA"] = CreateSingleCompilation("public class TypeA { }", "ProjectA"),
            ["ProjectB"] = CreateSingleCompilation("public class TypeB { }", "ProjectB")
        };

        // Act
        LogAct("Analyzing multiple projects");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying result per project");
        results.Count.ShouldBe(2);
        results.Select(r => r.ProjectName).ShouldContain("ProjectA");
        results.Select(r => r.ProjectName).ShouldContain("ProjectB");
    }

    [Fact]
    public void Analyze_ShouldIgnoreAutoGeneratedProgramClass()
    {
        // Arrange
        LogArrange("Creating compilation with Program class");
        var rule = new AlwaysPassRule();
        var source = """
            public class Program { static void Main() { } }
            public class RealClass { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Program class is filtered out");
        var typeNames = results[0].TypeResults.Select(t => t.TypeName).ToList();
        typeNames.ShouldNotContain("Program");
        typeNames.ShouldContain("RealClass");
    }

    [Fact]
    public void Analyze_WithNestedTypes_ShouldIncludeNested()
    {
        // Arrange
        LogArrange("Creating compilation with nested types");
        var rule = new AlwaysPassRule();
        var source = """
            public class Outer
            {
                public class Inner { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying nested type included");
        var typeNames = results[0].TypeResults.Select(t => t.TypeName).ToList();
        typeNames.ShouldContain("Outer");
        typeNames.ShouldContain("Inner");
    }

    [Fact]
    public void Analyze_WithConditionalRule_ShouldMixPassAndFail()
    {
        // Arrange
        LogArrange("Creating compilation with sealed and non-sealed classes");
        var rule = new SealedOnlyRule();
        var source = """
            public sealed class SealedClass { }
            public class NonSealedClass { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing with sealed-only rule");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying mixed results");
        var sealedResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "SealedClass");
        var nonSealedResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "NonSealedClass");

        sealedResult.ShouldNotBeNull();
        sealedResult.Status.ShouldBe(TypeAnalysisStatus.Passed);

        nonSealedResult.ShouldNotBeNull();
        nonSealedResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        nonSealedResult.Violation.ShouldNotBeNull();
    }

    [Fact]
    public void Analyze_FailedViolation_ShouldContainCorrectMetadata()
    {
        // Arrange
        LogArrange("Creating compilation to check violation metadata");
        var rule = new AlwaysFailRule();
        var compilations = CreateCompilations("public class TestClass { }");

        // Act
        LogAct("Analyzing with always-fail rule");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "TestClass");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("AlwaysFail");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adr/test.md");
        violation.Project.ShouldBe("TestProject");
        violation.Message.ShouldNotBeNullOrEmpty();
        violation.LlmHint.ShouldNotBeNullOrEmpty();
    }

    #region Test Rule Implementations

    private sealed class AlwaysPassRule : Rule
    {
        public override string Name => "AlwaysPass";
        public override string Description => "Always passes";
        public override Severity DefaultSeverity => Severity.Error;
        public override string AdrPath => "docs/adr/test.md";

        protected override Violation? AnalyzeType(TypeContext context) => null;
    }

    private sealed class AlwaysFailRule : Rule
    {
        public override string Name => "AlwaysFail";
        public override string Description => "Always fails";
        public override Severity DefaultSeverity => Severity.Error;
        public override string AdrPath => "docs/adr/test.md";

        protected override Violation? AnalyzeType(TypeContext context) => new()
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = "Always fails",
            LlmHint = "This rule always fails"
        };
    }

    private sealed class SealedOnlyRule : Rule
    {
        public override string Name => "SealedOnly";
        public override string Description => "Classes must be sealed";
        public override Severity DefaultSeverity => Severity.Error;
        public override string AdrPath => "docs/adr/test.md";

        protected override Violation? AnalyzeType(TypeContext context)
        {
            if (context.Type.IsSealed)
                return null;

            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"{context.Type.Name} must be sealed",
                LlmHint = "Add the sealed modifier"
            };
        }
    }

    #endregion

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
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion
}
