using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE041_OperationSpecificChildValidationRuleTests : TestBase
{
    public DE041_OperationSpecificChildValidationRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EntityWithProcessAndValidateForMethods_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Process*Internal and Validate*For*Internal methods");
        var rule = new DE041_OperationSpecificChildValidationRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class OrderItem : EntityBase<OrderItem> { }
            public sealed class Order : EntityBase<Order>
            {
                private readonly List<OrderItem> _items = new();
                private void ProcessOrderItemForAddInternal(OrderItem item) { }
                private bool ValidateOrderItemForAddInternal(OrderItem item) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with Process*Internal and Validate*For*Internal methods");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with both methods passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithProcessButNoValidateForMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Process*Internal but no Validate*For*Internal method");
        var rule = new DE041_OperationSpecificChildValidationRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class LineItem : EntityBase<LineItem> { }
            public sealed class Product : EntityBase<Product>
            {
                private readonly List<LineItem> _items = new();
                private void ProcessLineItemForAddInternal(LineItem item) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with Process*Internal but no Validate*For*Internal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without Validate*For*Internal fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE041_OperationSpecificChildValidation");
        typeResult.Violation.Message.ShouldContain("Validate*For*Internal");
    }

    [Fact]
    public void EntityWithoutChildCollection_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without child collection");
        var rule = new DE041_OperationSpecificChildValidationRule();
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
        LogAct("Analyzing entity without child collection");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without child collection passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithChildCollectionButNoProcessMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with child collection but no Process*Internal (DE-040 handles this)");
        var rule = new DE041_OperationSpecificChildValidationRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class ChildEntity : EntityBase<ChildEntity> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private readonly List<ChildEntity> _children = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with child collection but no Process*Internal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without Process*Internal passes (DE-040 handles)");
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
        var rule = new DE041_OperationSpecificChildValidationRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public sealed class RegularClass
            {
                private readonly List<Child> _children = new();
                private void ProcessChildForAddInternal(Child item) { }
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
        var rule = new DE041_OperationSpecificChildValidationRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                private readonly List<Child> _children = new();
                private void ProcessChildForAddInternal(Child item) { }
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
        var rule = new DE041_OperationSpecificChildValidationRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public sealed class Entity : EntityBase<Entity>
            {
                private readonly List<Child> _children = new();
                private void ProcessChildForAddInternal(Child item) { }
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

        violation.Rule.ShouldBe("DE041_OperationSpecificChildValidation");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-041-validacao-entidade-filha-especifica-operacao.md");
        violation.LlmHint.ShouldContain("Validate*For*Internal");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE041_OperationSpecificChildValidationRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE041_OperationSpecificChildValidation");
        rule.Description.ShouldContain("Validate*For*Internal");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-041-validacao-entidade-filha-especifica-operacao.md");
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
