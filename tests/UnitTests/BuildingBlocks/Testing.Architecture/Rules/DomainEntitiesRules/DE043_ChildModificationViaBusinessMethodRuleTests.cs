using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE043_ChildModificationViaBusinessMethodRuleTests : TestBase
{
    public DE043_ChildModificationViaBusinessMethodRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ChildEntityWithBusinessMethodReturningNullableSelf_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with child having business method returning T?");
        var rule = new DE043_ChildModificationViaBusinessMethodRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class OrderItem : EntityBase<OrderItem>
            {
                public OrderItem? ChangeName(string name) => this;
            }
            public sealed class Order : EntityBase<Order>
            {
                private readonly List<OrderItem> _items = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing child entity with business method returning T?");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying child entity with business method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ChildEntityWithoutBusinessMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with child without business method");
        var rule = new DE043_ChildModificationViaBusinessMethodRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class LineItem : EntityBase<LineItem>
            {
                public string? Name { get; private set; }
            }
            public sealed class Product : EntityBase<Product>
            {
                private readonly List<LineItem> _items = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing child entity without business method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying child entity without business method fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE043_ChildModificationViaBusinessMethod");
        typeResult.Violation.Message.ShouldContain("LineItem");
    }

    [Fact]
    public void EntityWithoutChildEntityCollection_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without child entity collection");
        var rule = new DE043_ChildModificationViaBusinessMethodRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                private readonly List<string> _tags = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without child entity collection");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without child entity collection passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE043_ChildModificationViaBusinessMethodRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child>
            {
                public string? Name { get; private set; }
            }
            public sealed class RegularClass
            {
                private readonly List<Child> _children = new();
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
        var rule = new DE043_ChildModificationViaBusinessMethodRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child>
            {
                public string? Name { get; private set; }
            }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                private readonly List<Child> _children = new();
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
        var rule = new DE043_ChildModificationViaBusinessMethodRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child>
            {
                public string? Name { get; private set; }
            }
            public sealed class Entity : EntityBase<Entity>
            {
                private readonly List<Child> _children = new();
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

        violation.Rule.ShouldBe("DE043_ChildModificationViaBusinessMethod");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-043-modificacao-entidade-filha-via-metodo-negocio.md");
        violation.LlmHint.ShouldContain("Child?");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE043_ChildModificationViaBusinessMethodRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE043_ChildModificationViaBusinessMethod");
        rule.Description.ShouldContain("negocio");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-043-modificacao-entidade-filha-via-metodo-negocio.md");
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
