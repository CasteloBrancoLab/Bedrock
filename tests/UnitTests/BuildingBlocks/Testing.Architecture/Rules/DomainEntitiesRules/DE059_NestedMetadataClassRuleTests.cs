using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE059_NestedMetadataClassRuleTests : TestBase
{
    public DE059_NestedMetadataClassRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void NestedMetadataClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with nested metadata class");
        var rule = new DE059_NestedMetadataClassRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public string? Name { get; private set; }

                public static class OrderMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
                    public static bool NameIsRequired { get; private set; } = true;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with nested metadata class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with nested metadata class passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void SeparateMetadataClass_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with metadata class as separate type");
        var rule = new DE059_NestedMetadataClassRule();
        var entitySource = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public string? Name { get; private set; }
            }
            """;
        var metadataSource = """
            #nullable enable
            public static class ProductMetadata
            {
                public static int NameMaxLength { get; private set; } = 100;
                public static bool NameIsRequired { get; private set; } = true;
            }
            """;
        var compilations = CreateCompilationsFromMultipleSources(entitySource, metadataSource);

        // Act
        LogAct("Analyzing entity with separate metadata class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with separate metadata class fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE059_NestedMetadataClass");
        typeResult.Violation.Message.ShouldContain("ProductMetadata");
        typeResult.Violation.Message.ShouldContain("aninhada");
    }

    [Fact]
    public void EntityWithoutMetadataClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without any metadata class");
        var rule = new DE059_NestedMetadataClassRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public string? Name { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without metadata class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without metadata class passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase with separate metadata");
        var rule = new DE059_NestedMetadataClassRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public string? Name { get; set; }
            }

            public static class RegularClassMetadata
            {
                public static int NameMaxLength { get; private set; } = 100;
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
        LogArrange("Creating abstract class with separate metadata");
        var rule = new DE059_NestedMetadataClassRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public string? Name { get; protected set; }
            }

            public static class AbstractEntityMetadata
            {
                public static int NameMaxLength { get; private set; } = 100;
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
        var rule = new DE059_NestedMetadataClassRule();
        var entitySource = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public string? Name { get; private set; }
            }
            """;
        var metadataSource = """
            #nullable enable
            public static class EntityMetadata
            {
                public static int NameMaxLength { get; private set; } = 100;
            }
            """;
        var compilations = CreateCompilationsFromMultipleSources(entitySource, metadataSource);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Entity");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE059_NestedMetadataClass");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-059-metadata-deve-ser-classe-aninhada.md");
        violation.LlmHint.ShouldContain("aninhada");
        violation.LlmHint.ShouldContain("EntityMetadata");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE059_NestedMetadataClassRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE059_NestedMetadataClass");
        rule.Description.ShouldContain("aninhada");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-059-metadata-deve-ser-classe-aninhada.md");
    }

    [Fact]
    public void EntityWithBothNestedAndSeparateMetadata_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with both nested and separate metadata class");
        var rule = new DE059_NestedMetadataClassRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public string? Name { get; private set; }

                public static class CustomerMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
                }
            }

            public static class CustomerMetadata
            {
                public static int NameMaxLength { get; private set; } = 200;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with both nested and separate metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with separate metadata class still fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilations(string source)
    {
        return new Dictionary<string, Compilation>
        {
            ["TestProject"] = CreateSingleCompilation(source, "TestProject")
        };
    }

    private static Dictionary<string, Compilation> CreateCompilationsFromMultipleSources(
        params string[] sources)
    {
        var syntaxTrees = sources.Select((s, i) =>
            CSharpSyntaxTree.ParseText(s, path: $"TestFile{i}.cs")).ToArray();

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestProject",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        return new Dictionary<string, Compilation>
        {
            ["TestProject"] = compilation
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
