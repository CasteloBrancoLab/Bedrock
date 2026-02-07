using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE021_PublicMethodsDelegateToInternalRuleTests : TestBase
{
    public DE021_PublicMethodsDelegateToInternalRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ChangeMethodWithCorrespondingInternal_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Change method having corresponding Internal");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public Order? ChangeName(string name)
                {
                    return null;
                }

                private bool ChangeNameInternal(string name) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Change method with corresponding Internal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Change method with corresponding Internal passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ChangeMethodCallingInternalMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Change method calling Internal");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public Product? ChangeName(string name)
                {
                    SomeOtherInternal();
                    return null;
                }

                private void SomeOtherInternal() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Change method calling Internal method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Change method calling Internal method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ChangeMethodWithoutInternal_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Change method without Internal");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public Customer? ChangeName(string name)
                {
                    // No Internal method called
                    return null;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Change method without Internal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Change method without Internal fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE021_PublicMethodsDelegateToInternal");
        typeResult.Violation.Message.ShouldContain("ChangeNameInternal");
    }

    [Fact]
    public void NonChangePublicMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with non-Change public method");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public void DoSomething()
                {
                    // No Internal method needed
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing non-Change public method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-Change public method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void PrivateChangeMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with private Change method");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                private void ChangeName(string name)
                {
                    // No Internal method needed for private methods
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing private Change method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying private Change method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public void ChangeName(string name)
                {
                    // Should be ignored
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class not inheriting EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class not inheriting EntityBase is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "RegularClass");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public void ChangeName(string name)
                {
                    // Should be ignored
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractEntity");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating entity to verify violation metadata");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public Entity? ChangeName(string name)
                {
                    return null;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Entity");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE021_PublicMethodsDelegateToInternal");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-021-metodos-publicos-vs-metodos-internos.md");
        violation.LlmHint.ShouldContain("Internal");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE021_PublicMethodsDelegateToInternalRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE021_PublicMethodsDelegateToInternal");
        rule.Description.ShouldContain("Internal");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-021-metodos-publicos-vs-metodos-internos.md");
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
