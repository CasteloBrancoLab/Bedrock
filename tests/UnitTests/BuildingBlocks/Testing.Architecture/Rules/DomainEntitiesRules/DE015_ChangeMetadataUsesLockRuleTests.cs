using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE015_ChangeMetadataUsesLockRuleTests : TestBase
{
    public DE015_ChangeMetadataUsesLockRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ChangeMetadataMethodWithLock_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Change*Metadata method using lock");
        var rule = new DE015_ChangeMetadataUsesLockRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public string? Name { get; private set; }

                public static class OrderMetadata
                {
                    private static readonly object _lockObject = new();
                    public static int NameMaxLength { get; private set; } = 100;

                    public static void ChangeNameMetadata(int maxLength)
                    {
                        lock (_lockObject)
                        {
                            NameMaxLength = maxLength;
                        }
                    }
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Change*Metadata method with lock");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Change*Metadata method with lock passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ChangeMetadataMethodWithoutLock_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Change*Metadata method without lock");
        var rule = new DE015_ChangeMetadataUsesLockRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public string? Name { get; private set; }

                public static class ProductMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;

                    public static void ChangeNameMetadata(int maxLength)
                    {
                        NameMaxLength = maxLength;
                    }
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Change*Metadata method without lock");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Change*Metadata method without lock fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE015_ChangeMetadataUsesLock");
        typeResult.Violation.Message.ShouldContain("lock");
    }

    [Fact]
    public void MetadataWithoutChangeMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with metadata but no Change*Metadata method");
        var rule = new DE015_ChangeMetadataUsesLockRule();
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
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing metadata without Change*Metadata method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying metadata without Change*Metadata method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithoutMetadataClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without metadata class");
        var rule = new DE015_ChangeMetadataUsesLockRule();
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
    public void NonChangeMetadataMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with regular method in metadata");
        var rule = new DE015_ChangeMetadataUsesLockRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                public string? Name { get; private set; }

                public static class PaymentMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;

                    public static void DoSomething()
                    {
                        // No lock needed for non-Change*Metadata methods
                    }
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing non-Change*Metadata method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-Change*Metadata method is ignored");
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
        var rule = new DE015_ChangeMetadataUsesLockRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public string? Name { get; set; }

                public static class RegularClassMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;

                    public static void ChangeNameMetadata(int maxLength)
                    {
                        NameMaxLength = maxLength; // No lock but should be ignored
                    }
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
        var rule = new DE015_ChangeMetadataUsesLockRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public string? Name { get; protected set; }

                public static class AbstractEntityMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;

                    public static void ChangeNameMetadata(int maxLength)
                    {
                        NameMaxLength = maxLength; // No lock but should be ignored
                    }
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
        var rule = new DE015_ChangeMetadataUsesLockRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public string? Name { get; private set; }

                public static class EntityMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;

                    public static void ChangeNameMetadata(int maxLength)
                    {
                        NameMaxLength = maxLength;
                    }
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

        violation.Rule.ShouldBe("DE015_ChangeMetadataUsesLock");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-015-customizacao-de-metadados-apenas-no-startup.md");
        violation.LlmHint.ShouldContain("lock");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE015_ChangeMetadataUsesLockRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE015_ChangeMetadataUsesLock");
        rule.Description.ShouldContain("lock");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-015-customizacao-de-metadados-apenas-no-startup.md");
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
