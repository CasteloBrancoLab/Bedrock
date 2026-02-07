using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE045_DuplicateValidationIgnoresSelfRuleTests : TestBase
{
    public DE045_DuplicateValidationIgnoresSelfRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ValidateForChangeMethodWithIntParameter_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Validate*ForChange*Internal with int parameter");
        var rule = new DE045_DuplicateValidationIgnoresSelfRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class OrderItem : EntityBase<OrderItem> { }
            public sealed class Order : EntityBase<Order>
            {
                private readonly List<OrderItem> _items = new();
                private bool ValidateItemForChangeInternal(int currentIndex, string value) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate*ForChange*Internal with int parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate*ForChange*Internal with int parameter passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ValidateForChangeMethodWithoutIntParameter_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Validate*ForChange*Internal without int parameter");
        var rule = new DE045_DuplicateValidationIgnoresSelfRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class LineItem : EntityBase<LineItem> { }
            public sealed class Product : EntityBase<Product>
            {
                private readonly List<LineItem> _items = new();
                private bool ValidateLineItemForChangeInternal(string value) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate*ForChange*Internal without int parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate*ForChange*Internal without int fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE045_DuplicateValidationIgnoresSelf");
        typeResult.Violation.Message.ShouldContain("currentIndex");
    }

    [Fact]
    public void EntityWithoutChildCollection_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without child collection");
        var rule = new DE045_DuplicateValidationIgnoresSelfRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                private readonly List<string> _tags = new();
                private bool ValidateTagForChangeInternal(string value) => true;
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
    public void ValidateForRegisterNewMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with Validate*ForRegisterNew*Internal (no index needed)");
        var rule = new DE045_DuplicateValidationIgnoresSelfRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class ChildItem : EntityBase<ChildItem> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private readonly List<ChildItem> _items = new();
                private bool ValidateItemForRegisterNewInternal(string value) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate*ForRegisterNew*Internal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate*ForRegisterNew*Internal is ignored");
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
        var rule = new DE045_DuplicateValidationIgnoresSelfRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public sealed class RegularClass
            {
                private readonly List<Child> _children = new();
                private bool ValidateChildForChangeInternal(string value) => true;
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
        var rule = new DE045_DuplicateValidationIgnoresSelfRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                private readonly List<Child> _children = new();
                private bool ValidateChildForChangeInternal(string value) => true;
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
        var rule = new DE045_DuplicateValidationIgnoresSelfRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Child : EntityBase<Child> { }
            public sealed class Entity : EntityBase<Entity>
            {
                private readonly List<Child> _children = new();
                private bool ValidateChildForChangeNameInternal(string value) => true;
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

        violation.Rule.ShouldBe("DE045_DuplicateValidationIgnoresSelf");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-045-validacao-duplicidade-ignora-propria-entidade.md");
        violation.LlmHint.ShouldContain("int currentIndex");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE045_DuplicateValidationIgnoresSelfRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE045_DuplicateValidationIgnoresSelf");
        rule.Description.ShouldContain("duplicidade");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-045-validacao-duplicidade-ignora-propria-entidade.md");
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
