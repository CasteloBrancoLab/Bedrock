using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE037_PublicPropertyIReadOnlyListRuleTests : TestBase
{
    public DE037_PublicPropertyIReadOnlyListRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void PublicIReadOnlyListProperty_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with public IReadOnlyList property");
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                private readonly List<string> _items = new();
                public IReadOnlyList<string> Items => _items.AsReadOnly();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public IReadOnlyList property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public IReadOnlyList property passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void PublicListProperty_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with public List property");
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public List<string> Items { get; } = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public List property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public List property fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE037_PublicPropertyIReadOnlyList");
        typeResult.Violation.Message.ShouldContain("List");
    }

    [Fact]
    public void PublicIListProperty_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with public IList property");
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public IList<string> Orders { get; } = new List<string>();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public IList property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public IList property fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("IList");
    }

    [Fact]
    public void PublicIEnumerableProperty_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with public IEnumerable property");
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private readonly List<string> _items = new();
                public IEnumerable<string> Items => _items;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public IEnumerable property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public IEnumerable property fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("IEnumerable");
    }

    [Fact]
    public void PrivateListProperty_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with private List property");
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                private List<string> Items { get; } = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing private List property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying private List property passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void StaticListProperty_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with static List property");
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                public static List<string> AllShipments { get; } = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing static List property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying static List property is ignored");
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
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public sealed class RegularClass
            {
                public List<string> Items { get; } = new();
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
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public List<string> Items { get; } = new();
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
        var rule = new DE037_PublicPropertyIReadOnlyListRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public List<string> Items { get; } = new();
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

        violation.Rule.ShouldBe("DE037_PublicPropertyIReadOnlyList");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-037-propriedade-publica-ireadonlylist-asreadonly.md");
        violation.LlmHint.ShouldContain("IReadOnlyList");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE037_PublicPropertyIReadOnlyListRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE037_PublicPropertyIReadOnlyList");
        rule.Description.ShouldContain("IReadOnlyList");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-037-propriedade-publica-ireadonlylist-asreadonly.md");
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
