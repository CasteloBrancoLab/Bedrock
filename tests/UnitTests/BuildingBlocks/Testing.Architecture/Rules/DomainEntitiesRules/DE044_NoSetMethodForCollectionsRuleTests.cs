using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE044_NoSetMethodForCollectionsRuleTests : TestBase
{
    public DE044_NoSetMethodForCollectionsRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EntityWithoutSetCollectionMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without Set* method for collections");
        var rule = new DE044_NoSetMethodForCollectionsRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                private readonly List<string> _items = new();
                public Order? AddItem(string item) => this;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without Set* method for collections");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without Set* method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithSetCollectionMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Set* method receiving collection");
        var rule = new DE044_NoSetMethodForCollectionsRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                private readonly List<string> _items = new();
                public void SetItems(List<string> items) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with Set* method receiving collection");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with Set* method fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE044_NoSetMethodForCollections");
        typeResult.Violation.Message.ShouldContain("SetItems");
    }

    [Fact]
    public void EntityWithSetIEnumerableMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Set* method receiving IEnumerable");
        var rule = new DE044_NoSetMethodForCollectionsRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                private readonly List<string> _tags = new();
                public void SetTags(IEnumerable<string> tags) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with Set* method receiving IEnumerable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with Set* IEnumerable method fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void EntityWithSetScalarMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Set* method for scalar value");
        var rule = new DE044_NoSetMethodForCollectionsRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public string? Name { get; private set; }
                public void SetName(string name) { Name = name; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with Set* method for scalar value");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with Set* scalar method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE044_NoSetMethodForCollectionsRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public sealed class RegularClass
            {
                private readonly List<string> _items = new();
                public void SetItems(List<string> items) { }
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
        var rule = new DE044_NoSetMethodForCollectionsRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public void SetItems(List<string> items) { }
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
        var rule = new DE044_NoSetMethodForCollectionsRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public void SetData(List<string> data) { }
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

        violation.Rule.ShouldBe("DE044_NoSetMethodForCollections");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-044-antipadrao-colecoes-sem-metodo-set.md");
        violation.LlmHint.ShouldContain("SetData");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE044_NoSetMethodForCollectionsRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE044_NoSetMethodForCollections");
        rule.Description.ShouldContain("Set");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-044-antipadrao-colecoes-sem-metodo-set.md");
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
