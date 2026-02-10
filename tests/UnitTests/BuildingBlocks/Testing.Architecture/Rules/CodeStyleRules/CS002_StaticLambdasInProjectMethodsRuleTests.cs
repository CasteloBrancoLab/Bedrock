using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

public class CS002_StaticLambdasInProjectMethodsRuleTests : TestBase
{
    public CS002_StaticLambdasInProjectMethodsRuleTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    #region Pass Cases

    [Fact]
    public void StaticLambdaInProjectMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating static lambda passed to project method");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate TResult MyFunc<T1, TResult>(T1 arg);
            public sealed class Processor
            {
                public void Execute(MyFunc<int, int> func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute(static (x) => x + 1);
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing static lambda in project method");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying static lambda passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void LambdaInExternalMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating lambda passed to System.Linq method (external)");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            using System.Linq;
            namespace MyProject.Services;
            public sealed class Consumer
            {
                public void Run()
                {
                    var list = new int[] { 1, 2, 3 };
                    var result = list.Where(x => x > 1);
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing lambda in external method (System.Linq)");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying lambda in external method passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void MethodGroup_ShouldPass()
    {
        // Arrange
        LogArrange("Creating method group passed to project method");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute(Transform);
                }
                private static int Transform(int x) => x + 1;
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing method group in project method");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying method group passes (not a lambda)");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void TypeWithoutLambdas_ShouldPass()
    {
        // Arrange
        LogArrange("Creating type with no lambdas");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public sealed class SimpleService
            {
                public int Add(int a, int b) => a + b;
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/SimpleService.cs");

        // Act
        LogAct("Analyzing type without lambdas");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying type without lambdas passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "SimpleService");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void SuppressedWithDisableOnce_ShouldPass()
    {
        // Arrange
        LogArrange("Creating non-static lambda suppressed with CS002 disable once");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    // CS002 disable once : integração legada requer closure
                    processor.Execute((x) => x + 1);
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing suppressed lambda with disable once");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying suppressed lambda passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void SuppressedWithDisableRestoreBlock_ShouldPass()
    {
        // Arrange
        LogArrange("Creating non-static lambda in CS002 disable/restore block");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    // CS002 disable : bloco de integração legada
                    processor.Execute((x) => x + 1);
                    // CS002 restore
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing suppressed lambda in disable/restore block");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying suppressed lambda in block passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void StaticParenthesizedLambda_ShouldPass()
    {
        // Arrange
        LogArrange("Creating static parenthesized lambda");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int a, int b);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute(static (a, b) => a + b);
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing static parenthesized lambda");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying static parenthesized lambda passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    #endregion

    #region Fail Cases

    [Fact]
    public void NonStaticLambdaInProjectMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating non-static lambda in project method");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute((x) => x + 1);
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing non-static lambda in project method");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying non-static lambda fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("CS002_StaticLambdasInProjectMethods");
        typeResult.Violation.Message.ShouldContain("static");
    }

    [Fact]
    public void NonStaticAnonymousDelegate_ShouldFail()
    {
        // Arrange
        LogArrange("Creating non-static anonymous delegate in project method");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute(delegate(int x) { return x + 1; });
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing non-static anonymous delegate");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying non-static anonymous delegate fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    [Fact]
    public void NonStaticParenthesizedLambda_ShouldFail()
    {
        // Arrange
        LogArrange("Creating non-static parenthesized lambda in project method");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int a, int b);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute((a, b) => a + b);
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing non-static parenthesized lambda");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying non-static parenthesized lambda fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    [Fact]
    public void LambdaAfterRestoreComment_ShouldFail()
    {
        // Arrange
        LogArrange("Creating non-static lambda after CS002 restore (outside block)");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    // CS002 disable : bloco de integração legada
                    processor.Execute((x) => x + 1);
                    // CS002 restore
                    processor.Execute((x) => x + 2);
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing lambda after restore comment");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying lambda after restore fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    #endregion

    #region Violation Metadata

    [Fact]
    public void Violation_ShouldHaveCorrectMetadata()
    {
        // Arrange
        LogArrange("Creating non-static lambda to verify violation metadata");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject.Services;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute((x) => x + 1);
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/Consumer.cs");

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Consumer");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("CS002_StaticLambdasInProjectMethods");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/code-style/CS-002-lambdas-inline-devem-ser-static.md");
        violation.Project.ShouldBe("MyProject.Services");
        violation.Message.ShouldContain("static");
        violation.Message.ShouldContain("Processor");
        violation.Message.ShouldContain("Execute");
        violation.Message.ShouldContain("Consumer");
        violation.LlmHint.ShouldContain("static");
        violation.LlmHint.ShouldContain("Execute");
    }

    #endregion

    #region Rule Properties

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("CS002_StaticLambdasInProjectMethods");
        rule.Description.ShouldContain("static");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/code-style/CS-002-lambdas-inline-devem-ser-static.md");
    }

    #endregion

    #region Namespace Detection

    [Fact]
    public void DottedAssemblyName_ShouldExtractRootNamespace()
    {
        // Arrange
        LogArrange("Creating compilation with dotted assembly name");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace ShopDemo.Auth.Services;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute((x) => x + 1);
                }
            }
            """;
        var compilations = new Dictionary<string, Compilation>
        {
            ["ShopDemo.Auth.Services"] = CreateSingleCompilation(
                source, "ShopDemo.Auth.Services", "src/Services/Consumer.cs")
        };

        // Act
        LogAct("Analyzing with dotted assembly name (ShopDemo.Auth.Services)");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying root namespace ShopDemo is detected correctly");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    [Fact]
    public void SimpleAssemblyName_ShouldUseEntireNameAsRoot()
    {
        // Arrange
        LogArrange("Creating compilation with simple assembly name (no dots)");
        var rule = new CS002_StaticLambdasInProjectMethodsRule();
        var source = """
            namespace MyProject;
            public delegate int MyFunc(int arg);
            public sealed class Processor
            {
                public void Execute(MyFunc func) { }
            }
            public sealed class Consumer
            {
                public void Run()
                {
                    var processor = new Processor();
                    processor.Execute((x) => x + 1);
                }
            }
            """;
        var compilations = new Dictionary<string, Compilation>
        {
            ["MyProject"] = CreateSingleCompilation(
                source, "MyProject", "src/Consumer.cs")
        };

        // Act
        LogAct("Analyzing with simple assembly name (MyProject)");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying root namespace MyProject is used correctly");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Consumer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    #endregion

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilations(string source, string filePath)
    {
        return new Dictionary<string, Compilation>
        {
            ["MyProject.Services"] = CreateSingleCompilation(source, "MyProject.Services", filePath)
        };
    }

    private static Compilation CreateSingleCompilation(string source, string assemblyName, string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: filePath);

        var coreAssemblyDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(coreAssemblyDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(coreAssemblyDir, "System.Linq.dll")),
            MetadataReference.CreateFromFile(Path.Combine(coreAssemblyDir, "System.Collections.dll")),
        };

        return CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion
}
