using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE052_ProtectedConstructorsInAbstractClassesRuleTests : TestBase
{
    public DE052_ProtectedConstructorsInAbstractClassesRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void AbstractClassWithProtectedConstructor_ShouldPass()
    {
        // Arrange
        LogArrange("Creating abstract class with protected constructor");
        var rule = new DE052_ProtectedConstructorsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractOrder : EntityBase<AbstractOrder>
            {
                protected AbstractOrder() { }
                protected AbstractOrder(string name) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with protected constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with protected constructor passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractOrder");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClassWithPublicConstructor_ShouldFail()
    {
        // Arrange
        LogArrange("Creating abstract class with public constructor");
        var rule = new DE052_ProtectedConstructorsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractProduct : EntityBase<AbstractProduct>
            {
                public AbstractProduct() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with public constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with public constructor fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractProduct");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE052_ProtectedConstructorsInAbstractClasses");
        typeResult.Violation.Message.ShouldContain("protected");
    }

    [Fact]
    public void AbstractClassWithPrivateConstructor_ShouldFail()
    {
        // Arrange
        LogArrange("Creating abstract class with private constructor");
        var rule = new DE052_ProtectedConstructorsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractCustomer : EntityBase<AbstractCustomer>
            {
                private AbstractCustomer() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with private constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with private constructor fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractCustomer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void ConcreteClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating concrete class with public constructor");
        var rule = new DE052_ProtectedConstructorsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public Order() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing concrete class with public constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying concrete class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class not inheriting EntityBase");
        var rule = new DE052_ProtectedConstructorsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class AbstractClass
            {
                public AbstractClass() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class not inheriting EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class not inheriting EntityBase is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractClass");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating abstract class to verify violation metadata");
        var rule = new DE052_ProtectedConstructorsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                internal AbstractEntity(string name) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "AbstractEntity");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE052_ProtectedConstructorsInAbstractClasses");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-052-construtores-protegidos-em-classes-abstratas.md");
        violation.LlmHint.ShouldContain("protected");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE052_ProtectedConstructorsInAbstractClassesRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE052_ProtectedConstructorsInAbstractClasses");
        rule.Description.ShouldContain("Construtores");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-052-construtores-protegidos-em-classes-abstratas.md");
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
