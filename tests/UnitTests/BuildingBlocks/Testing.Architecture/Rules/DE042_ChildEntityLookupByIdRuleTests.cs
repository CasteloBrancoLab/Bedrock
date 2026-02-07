using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE042_ChildEntityLookupByIdRuleTests : TestBase
{
    public DE042_ChildEntityLookupByIdRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ProcessMethodWithGuidParameter_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Process*Internal method with Guid parameter");
        var rule = new DE042_ChildEntityLookupByIdRule();
        var source = """
            #nullable enable
            using System;
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class OrderItem : EntityBase<OrderItem> { }
            public sealed class Order : EntityBase<Order>
            {
                private readonly List<OrderItem> _items = new();
                private void ProcessOrderItemForChangeInternal(Guid itemId, string newValue) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Process*Internal method with Guid parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Process*Internal method with Guid parameter passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ProcessMethodWithoutGuidParameter_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Process*Internal method without Guid parameter");
        var rule = new DE042_ChildEntityLookupByIdRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class LineItem : EntityBase<LineItem> { }
            public sealed class Product : EntityBase<Product>
            {
                private readonly List<LineItem> _items = new();
                private void ProcessLineItemForChangeInternal(string value) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Process*Internal method without Guid parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Process*Internal method without Guid parameter fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE042_ChildEntityLookupById");
        typeResult.Violation.Message.ShouldContain("Guid");
    }

    [Fact]
    public void ProcessRegisterNewMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with Process*ForRegisterNew*Internal method");
        var rule = new DE042_ChildEntityLookupByIdRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class ChildItem : EntityBase<ChildItem> { }
            public sealed class Customer : EntityBase<Customer>
            {
                private readonly List<ChildItem> _items = new();
                private void ProcessChildItemForRegisterNewInternal(ChildItem item) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Process*ForRegisterNew*Internal method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Process*ForRegisterNew*Internal is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithoutChildCollection_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without child collection");
        var rule = new DE042_ChildEntityLookupByIdRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private readonly List<string> _tags = new();
                private void ProcessTagForChangeInternal(string tag) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without child collection");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without child collection passes");
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
        var rule = new DE042_ChildEntityLookupByIdRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public sealed class RegularClass
            {
                private readonly List<Child> _children = new();
                private void ProcessChildForChangeInternal(string value) { }
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
        var rule = new DE042_ChildEntityLookupByIdRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                private readonly List<Child> _children = new();
                private void ProcessChildForChangeInternal(string value) { }
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
        var rule = new DE042_ChildEntityLookupByIdRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public sealed class Entity : EntityBase<Entity>
            {
                private readonly List<Child> _children = new();
                private void ProcessChildForChangeInternal(string value) { }
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

        violation.Rule.ShouldBe("DE042_ChildEntityLookupById");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-042-localizacao-entidade-filha-por-id.md");
        violation.LlmHint.ShouldContain("Guid");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE042_ChildEntityLookupByIdRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE042_ChildEntityLookupById");
        rule.Description.ShouldContain("Id");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-042-localizacao-entidade-filha-por-id.md");
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
