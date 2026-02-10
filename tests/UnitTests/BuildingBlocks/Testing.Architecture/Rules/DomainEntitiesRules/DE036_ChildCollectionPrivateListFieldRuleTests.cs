using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE036_ChildCollectionPrivateListFieldRuleTests : TestBase
{
    public DE036_ChildCollectionPrivateListFieldRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void PrivateListField_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with private List field");
        var rule = new DE036_ChildCollectionPrivateListFieldRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                private readonly List<string> _items = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing private List field");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying private List field passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void PublicListField_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with public List field");
        var rule = new DE036_ChildCollectionPrivateListFieldRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public List<string> Items = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public List field");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public List field fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE036_ChildCollectionPrivateListField");
        typeResult.Violation.Message.ShouldContain("Items");
    }

    [Fact]
    public void InternalListField_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with internal List field");
        var rule = new DE036_ChildCollectionPrivateListFieldRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                internal List<string> _orders = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing internal List field");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying internal List field fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void PublicICollectionProperty_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with public ICollection property");
        var rule = new DE036_ChildCollectionPrivateListFieldRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public ICollection<string> Items { get; } = new List<string>();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public ICollection property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public ICollection property fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("ICollection");
    }

    [Fact]
    public void StaticListField_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with static List field");
        var rule = new DE036_ChildCollectionPrivateListFieldRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                public static List<string> AllPayments = new();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing static List field");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying static List field is ignored");
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
        var rule = new DE036_ChildCollectionPrivateListFieldRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public sealed class RegularClass
            {
                public List<string> Items = new();
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
        var rule = new DE036_ChildCollectionPrivateListFieldRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public List<string> Items = new();
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
        var rule = new DE036_ChildCollectionPrivateListFieldRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public List<string> _items = new();
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

        violation.Rule.ShouldBe("DE036_ChildCollectionPrivateListField");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-036-colecoes-entidades-filhas-field-privado-list.md");
        violation.LlmHint.ShouldContain("private");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE036_ChildCollectionPrivateListFieldRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE036_ChildCollectionPrivateListField");
        rule.Description.ShouldContain("List");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-036-colecoes-entidades-filhas-field-privado-list.md");
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
