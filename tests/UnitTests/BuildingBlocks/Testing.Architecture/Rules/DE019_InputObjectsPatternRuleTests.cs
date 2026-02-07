using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE019_InputObjectsPatternRuleTests : TestBase
{
    public DE019_InputObjectsPatternRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void FactoryMethodWithReadOnlyRecordStructParameter_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with factory method using readonly record struct");
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public readonly record struct RegisterNewInput(string Name);
            public readonly record struct CreateFromExistingInfoInput(string Name);
            public sealed class Order : EntityBase<Order>
            {
                public static Order? RegisterNew(RegisterNewInput input)
                {
                    return new Order();
                }

                public static Order CreateFromExistingInfo(CreateFromExistingInfoInput input)
                {
                    return new Order();
                }

                private Order() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing factory method with readonly record struct");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying factory method with readonly record struct passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void FactoryMethodWithPrimitiveParameter_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with factory method using primitive parameter");
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public static Product? RegisterNew(string name)
                {
                    return new Product();
                }

                private Product() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing factory method with primitive parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying factory method with primitive parameter fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE019_InputObjectsPattern");
        typeResult.Violation.Message.ShouldContain("readonly record struct");
    }

    [Fact]
    public void FactoryMethodWithClassParameter_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with factory method using class parameter");
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public class InputClass { public string? Name { get; set; } }
            public sealed class Customer : EntityBase<Customer>
            {
                public static Customer? RegisterNew(InputClass input)
                {
                    return new Customer();
                }

                private Customer() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing factory method with class parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying factory method with class parameter fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void FactoryMethodWithRegularStructParameter_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with factory method using regular struct parameter");
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public struct InputStruct { public string? Name; }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public static Invoice? RegisterNew(InputStruct input)
                {
                    return new Invoice();
                }

                private Invoice() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing factory method with regular struct parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying factory method with regular struct parameter fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void FactoryMethodWithExecutionContextParameter_ShouldIgnoreIt()
    {
        // Arrange
        LogArrange("Creating entity with factory method having ExecutionContext parameter");
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public class ExecutionContext { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public readonly record struct RegisterNewInput(string Name);
            public sealed class Payment : EntityBase<Payment>
            {
                public static Payment? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
                {
                    return new Payment();
                }

                private Payment() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing factory method with ExecutionContext parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying ExecutionContext parameter is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithoutFactoryMethods_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without factory methods");
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                private Shipment() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without factory methods");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without factory methods passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public static RegularClass? RegisterNew(string name)
                {
                    return new RegularClass();
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
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public static AbstractEntity? RegisterNew(string name)
                {
                    return null;
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
        var rule = new DE019_InputObjectsPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public static Entity? RegisterNew(string name)
                {
                    return new Entity();
                }

                private Entity() { }
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

        violation.Rule.ShouldBe("DE019_InputObjectsPattern");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-019-input-objects-pattern.md");
        violation.LlmHint.ShouldContain("readonly record struct");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE019_InputObjectsPatternRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE019_InputObjectsPattern");
        rule.Description.ShouldContain("readonly record struct");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-019-input-objects-pattern.md");
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
