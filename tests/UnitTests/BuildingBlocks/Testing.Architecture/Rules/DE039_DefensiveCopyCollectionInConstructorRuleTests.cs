using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE039_DefensiveCopyCollectionInConstructorRuleTests : TestBase
{
    public DE039_DefensiveCopyCollectionInConstructorRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ConstructorWithDefensiveCopy_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with constructor making defensive copy");
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            using System.Linq;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                private readonly List<string> _items;
                private Order(IEnumerable<string> items)
                {
                    _items = items.ToList();
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing constructor with defensive copy");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying constructor with defensive copy passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ConstructorWithDirectAssignment_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with constructor doing direct assignment");
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                private readonly List<string> _items;
                private Product(List<string> items)
                {
                    _items = items;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing constructor with direct assignment");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying constructor with direct assignment fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE039_DefensiveCopyCollectionInConstructor");
        typeResult.Violation.Message.ShouldContain("items");
    }

    [Fact]
    public void ConstructorWithThisPrefixDirectAssignment_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with constructor doing direct assignment via this.");
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                private readonly List<string> _orders;
                private Customer(List<string> orders)
                {
                    this._orders = orders;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing constructor with this. prefix direct assignment");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying constructor with this. prefix direct assignment fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void ConstructorWithoutCollectionParameter_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with constructor without collection parameter");
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private Invoice(string name)
                {
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing constructor without collection parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying constructor without collection parameter passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ParameterlessConstructor_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with parameterless constructor");
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                private readonly List<string> _items = new();
                private Payment() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing parameterless constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying parameterless constructor is ignored");
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
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public sealed class RegularClass
            {
                private readonly List<string> _items;
                public RegularClass(List<string> items)
                {
                    _items = items;
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
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                private readonly List<string> _items;
                protected AbstractEntity(List<string> items)
                {
                    _items = items;
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
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                private readonly List<string> _data;
                private Entity(List<string> data)
                {
                    _data = data;
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

        violation.Rule.ShouldBe("DE039_DefensiveCopyCollectionInConstructor");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-039-defensive-copy-colecoes-construtor.md");
        violation.LlmHint.ShouldContain("copia defensiva");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE039_DefensiveCopyCollectionInConstructorRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE039_DefensiveCopyCollectionInConstructor");
        rule.Description.ShouldContain("defensiva");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-039-defensive-copy-colecoes-construtor.md");
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
